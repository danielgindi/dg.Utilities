#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;
using System.Threading;
using System;
using System.Runtime.Remoting.Messaging;

namespace dg.Utilities.Threading
{
    public delegate IEnumerable<T> ProductionLine<T>(object state);

    public class Producer<T> : IDisposable
    {
        private delegate void InternalProductionCycle<Type>(ProductionLine<T> productionLine, object state);

        private Queue<T> _queue;
        private bool _waitingForEmpty;
        private bool _disposed;
        private InternalProductionCycle<T> _productionCycle;
        private object _emptySignal;
        private object _disposingLock;
        private int _activeConsumers;
        private int _consumedProducts;
        private AutoResetEvent _wakeOneSignal;
        private ManualResetEvent _wakeAllSignal;

        public int QueuedProducts
        {
            get { return _queue.Count; }
        }

        public int ConsumedProducts
        {
            get { return _consumedProducts; }
        }

        public int ActiveConsumers
        {
            get { return _activeConsumers; }
        }

        public Producer()
        {
            _queue = new Queue<T>();
            _wakeOneSignal = new AutoResetEvent(false);
            _wakeAllSignal = new ManualResetEvent(false);
            _emptySignal = new object();
            _disposingLock = new object();
            _disposed = false;
        }

        internal IEnumerable<T> GetConsumerChannel(CancelableAsyncResult cancelableResult, int millisecondsTimeout)
        {
            ValidateNotDisposed();

            WaitHandle[] waitEvents;

            if (cancelableResult != null)
                waitEvents = new WaitHandle[] { _wakeOneSignal, _wakeAllSignal, cancelableResult.CancelSignal };
            else
                waitEvents = new WaitHandle[] { _wakeOneSignal, _wakeAllSignal };

            Interlocked.Increment(ref _activeConsumers);

            while (!_disposed)
            {
                T consumed = default(T);
                bool dequeued = false;

                if (_queue.Count == 0)
                {
                    // alert anyone waiting for producer to be empty
                    if (_waitingForEmpty)
                    {
                        lock (_emptySignal)
                        {
                            Monitor.PulseAll(_emptySignal);
                        }
                    }

                    int waitResult = WaitHandle.WaitAny(waitEvents, millisecondsTimeout, true);

                    // no waitevents fired before the timeout
                    if (waitResult == WaitHandle.WaitTimeout)
                        break;
                }

                if (cancelableResult != null && cancelableResult.Canceling)
                    break;

                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        consumed = _queue.Dequeue();
                        dequeued = true;

                        Interlocked.Increment(ref _consumedProducts);
                    }
                }

                // must yield out of lock
                if (dequeued)
                    yield return consumed;
            }

            Interlocked.Decrement(ref _activeConsumers);
            yield break;
        }

        public IEnumerable<T> GetConsumerChannel(int millisecondsTimeout)
        {
            return GetConsumerChannel(null, millisecondsTimeout);
        }

        public IEnumerable<T> GetConsumerChannel()
        {
            return GetConsumerChannel(Timeout.Infinite);
        }

        public void Produce(T item)
        {
            lock (_queue)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);

                _queue.Enqueue(item);

                _wakeOneSignal.Set();
            }
            //Monitor.Pulse(_queue);
        }

        public void Produce(ProductionLine<T> productionLine, object obj)
        {
            InternalProductionCycle<T> internalCycle = new InternalProductionCycle<T>(ProductionCycle);

            internalCycle(productionLine, obj);
        }

        public IAsyncResult BeginProduce(ProductionLine<T> productionLine, object obj, AsyncCallback callback, object state)
        {
            ValidateNotDisposed();

            if (productionLine == null)
                throw new ArgumentNullException("productionLine", "ProductionLine delegate cannot be null");

            _productionCycle = new InternalProductionCycle<T>(ProductionCycle);
            return _productionCycle.BeginInvoke(productionLine, state, callback, obj);
        }

        public void EndProduce(IAsyncResult result)
        {
            _productionCycle.EndInvoke(result);
        }

        public void WaitUntilEmpty()
        {
            if (_queue.Count == 0)
                return;

            _waitingForEmpty = true;

            lock (_emptySignal)
            {
                Monitor.Wait(_emptySignal);
            }

            _waitingForEmpty = false;
        }

        private void ProductionCycle(ProductionLine<T> productionLine, object state)
        {
            // add products to the buffer until the method breaks
            foreach (T product in productionLine(state))
            {
                Produce(product);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_queue)
                {
                    _disposed = true;

                    if (_queue != null)
                        _queue.Clear();

                    if (_wakeAllSignal != null)
                        _wakeAllSignal.Set();
                }
            }

            if (_wakeOneSignal != null)
                _wakeOneSignal.Close();

            if (_wakeAllSignal != null)
                _wakeAllSignal.Close();
        }

        ~Producer()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
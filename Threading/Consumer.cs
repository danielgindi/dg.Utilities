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

namespace dg.Utilities.Threading
{
    public delegate void ConsumptionCycle<T>(IEnumerable<T> consumptionLine, object state);

    public class Consumer<T>
    {
        private Producer<T> _producer;
        private ConsumptionCycle<T> _consumptionCycle;

        public Producer<T> Producer
        {
            get { return _producer; }
        }

        public Consumer(Producer<T> producer)
        {
            _producer = producer;
        }

        public IAsyncResult BeginConsume(ConsumptionCycle<T> consumptionCycle, object obj, AsyncCallback callback, object state)
        {
            if (consumptionCycle == null)
                throw new ArgumentNullException("consumptionCycle", "ConsumptionCycle delegate cannot be null");

            CancelableAsyncResult cancelableResult = new CancelableAsyncResult();

            // wrap user callback delegate to pass cancelable result
            AsyncCallback fixedCallback = null;
            if (callback != null)
            {
                fixedCallback = delegate
                {
                    callback(cancelableResult);
                };
            }

            _consumptionCycle = consumptionCycle;
            cancelableResult.Result = _consumptionCycle.BeginInvoke(_producer.GetConsumerChannel(cancelableResult, Timeout.Infinite), obj, fixedCallback, state);

            return cancelableResult;
        }

        public void EndConsume(IAsyncResult result)
        {
            CancelableAsyncResult cancelableResult = (CancelableAsyncResult)result;

            _consumptionCycle.EndInvoke(cancelableResult.Result);
            cancelableResult.CancelSignal.Close();
        }

        public void Consume(ConsumptionCycle<T> consumptionCycle, object obj)
        {
            Consume(consumptionCycle, obj, Timeout.Infinite);
        }

        public void Consume(ConsumptionCycle<T> consumptionCycle, object obj, int millisecondsTimeout)
        {
            if (consumptionCycle == null)
                throw new ArgumentNullException("consumptionCycle", "ConsumptionCycle delegate cannot be null");

            consumptionCycle(_producer.GetConsumerChannel(null, millisecondsTimeout), obj);
        }
    }
}
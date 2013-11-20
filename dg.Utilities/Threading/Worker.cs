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

using System;
using System.Threading;
using System.IO;

namespace dg.Utilities.Threading
{
    /// <summary>
    /// Skeleton for a worker thread. Another thread would typically set up
    /// an instance with some work to do, and invoke the Run method (eg with
    /// new Thread(new ThreadStart(job.Run)).Start())
    /// </summary>
    public class Worker
    {
        public event ErrorEventHandler Error;

        /// <summary>
        /// Lock covering stopping and stopped
        /// </summary>
        private readonly object _stopLock = new object();
        /// <summary>
        /// Whether or not the worker thread has been asked to stop
        /// </summary>
        private bool _stopping = false;
        /// <summary>
        /// Whether or not the worker thread has stopped
        /// </summary>
        private bool _stopped = false;

        private readonly Thread _thread;
        private readonly Action _action;
        private TimeSpan _interval;
        private DateTime? _startTime;
        private DateTime _lastActionStart;

        /// <summary>
        /// Returns whether the worker thread has been asked to stop.
        /// This continues to return true even after the thread has stopped.
        /// </summary>
        public bool Stopping
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopping;
                }
            }
        }

        /// <summary>
        /// Returns whether the worker thread has stopped.
        /// </summary>
        public bool Stopped
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopped;
                }
            }
        }

        /// <summary>
        /// Returns the Thread that will do the work.
        /// </summary>
        public Thread Thread
        {
            get { return _thread; }
        }

        public Worker(Action action, TimeSpan interval)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _thread = new Thread(new ThreadStart(Run));
            _action = action;
            _interval = interval;
        }

        /// <summary>
        /// Start immediately.
        /// </summary>
        public void Start()
        {
            _thread.Start();
        }

        /// <summary>
        /// Start in the next interval from the sp
        /// </summary>
        /// <param name="startTime"></param>
        public void Start(DateTime startTime)
        {
            _startTime = startTime;
            _thread.Start();
        }

        /// <summary>
        /// Tells the worker thread to stop after completing its current work item.
        /// </summary>
        public void Stop()
        {
            lock (_stopLock)
            {
                _stopping = true;
                Monitor.Pulse(_stopLock);
            }
        }

        /// <summary>
        /// Tells the worker thread to stop after completing its current work item
        /// and then waits until it has finished.
        /// </summary>
        public void StopWait()
        {
            Stop();
            _thread.Join();
        }

        /// <summary>
        /// Called by the worker thread to indicate when it has stopped.
        /// </summary>
        private void SetStopped()
        {
            lock (_stopLock)
            {
                _stopped = true;
            }
        }

        private void OnError(Exception ex)
        {
            ErrorEventHandler e = Error;

            if (e != null)
                e(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        private void Run()
        {
            try
            {
                while (WaitInterval())
                {
                    _lastActionStart = DateTime.Now;

                    try
                    {
                        _action();
                    }
                    catch (Exception e)
                    {
                        OnError(e);

                        throw;
                    }

                };
            }
            finally
            {
                SetStopped();
            }
        }

        private bool WaitInterval()
        {
            // waits for the calculated amount of time or until pulsed
            // return value determines whether to stop work
            // run immediately if no start time has been specified
            if (_startTime != null)
            {
                lock (_stopLock)
                {
                    if (Stopping)
                        return false;

                    TimeSpan timeout = CalculateWaitUntilNextRun();

                    Monitor.Wait(_stopLock, timeout);
                }
            }

            return !Stopping;
        }

        private TimeSpan CalculateWaitUntilNextRun()
        {
            if (_startTime == null)
                throw new InvalidOperationException("Start time must have been set to calculate the next run time.");

            DateTime now = DateTime.Now;
            DateTime startTime = _startTime.Value;

            if (startTime == now)
            {
                // start now
                return TimeSpan.Zero;
            }
            else if (startTime > now)
            {
                // time until start
                return startTime - now;
            }
            else
            {
                // next interval time
                TimeSpan timeSinceStart = now - startTime;
                decimal intervalsSinceStart = (decimal)timeSinceStart.Ticks / (decimal)_interval.Ticks;

                int nextIntervalCount = (int)Math.Ceiling(intervalsSinceStart);
                decimal intervalFractionUntilNext = nextIntervalCount - intervalsSinceStart;

                TimeSpan timeUntilNextInterval = new TimeSpan((long)(intervalFractionUntilNext * (decimal)_interval.Ticks));

                return timeUntilNextInterval;
            }
        }
    }
}

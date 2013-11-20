using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace dg.Utilities.Threading
{
    /// <summary>
    /// Written by Daniel Cohen Gindi (danielgindi@gmail.com)
    /// </summary>
    public class SingleThreadedTimeout : IDisposable
    {
        SingleThreadedTimer timer = null;

        public SingleThreadedTimeout()
        {
        }

        /// <summary>
        /// Will Start() automatically.
        /// </summary>
        /// <param name="Milliseconds">Time for each Timer rotation</param>
        /// <param name="Elapsed">Event to call when Timer elapses</param>
        public SingleThreadedTimeout(int Milliseconds, EventHandler Elapsed)
        {
            if (Milliseconds == 0) Milliseconds = 1; // There's no real "0" for the Timer.
            TimerAction += Elapsed;
            timer = new SingleThreadedTimer(Milliseconds, CallTimerAction);
        }
        public SingleThreadedTimeout(int Milliseconds, EventHandler Elapsed, bool AutoDisposeWhenDone)
        {
            this.AutoDisposeWhenDone = AutoDisposeWhenDone;
            if (Milliseconds == 0) Milliseconds = 1; // There's no real "0" for the Timer.
            TimerAction += Elapsed;
            timer = new SingleThreadedTimer(Milliseconds, CallTimerAction);
        } 
        public static void SetTimeout(int Milliseconds, EventHandler Elapsed)
        {
            new SingleThreadedTimeout(Milliseconds, Elapsed, true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            // Now clean up Native Resources (Pointers)
        }

        public void Stop()
        {
            lock (this)
            {
                if (timer != null)
                {
                    SingleThreadedTimer old = timer;
                    timer = null;
                    old.Dispose();
                }
            }
        }

        private bool AutoDisposeWhenDone = false;
        private event EventHandler TimerAction;
        private void CallTimerAction(Object sender, EventArgs e)
        {
            if (TimerAction != null)
            {
                TimerAction(sender, e);
            }
            if (AutoDisposeWhenDone)
            {
                AutoDisposeWhenDone = false;
                this.Dispose();
            }
            else
            {
                Stop();
            }
        }
    }
}

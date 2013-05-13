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

using System.Threading;
using System;

namespace dg.Utilities.Threading
{
    public sealed class CancelableAsyncResult : IAsyncResult
    {
        private IAsyncResult _result;

        private ManualResetEvent _cancelSignal;
        private bool _canceling;

        internal IAsyncResult Result
        {
            get { return _result; }
            set { _result = value; }
        }

        internal ManualResetEvent CancelSignal
        {
            get { return _cancelSignal; }
        }

        internal bool Canceling
        {
            get { return _canceling; }
        }

        public CancelableAsyncResult()
        {
            _cancelSignal = new ManualResetEvent(false);
        }

        ~CancelableAsyncResult()
        {
            _cancelSignal.Close();
        }

        public void Cancel()
        {
            _canceling = true;
            _cancelSignal.Set();
        }

        public object AsyncState
        {
            get { return _result.AsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _result.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return _result.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return _result.IsCompleted; }
        }
    }
}
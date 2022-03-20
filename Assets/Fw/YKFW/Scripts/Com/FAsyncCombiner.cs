using System;
using System.Collections.Generic;

namespace FW
{
    public class FAsyncCombiner
    {
        public class AsyncHandle
        {
            private readonly FAsyncCombiner _combiner;
            private readonly int _handle;

            protected internal AsyncHandle(FAsyncCombiner combiner, int handle)
            {
                _combiner = combiner;
                _handle = handle;
            }

            public FAsyncCombiner Combiner
            {
                get { return _combiner; }
            }

            public void Finish()
            {
                _combiner._asyncHandles[_handle] = true;
                _combiner.RefreshAsyncHandles();
            }
        }

        private List<CallBack> _completionCallBacks = new List<CallBack>();
        private List<bool> _asyncHandles = new List<bool>();


        public void AddCompletionCall(CallBack call)
        {
            _completionCallBacks.Add(call);
        }

        public AsyncHandle CreateAsyncHandle()
        {
            _asyncHandles.Add(false);
            return new AsyncHandle(this, _asyncHandles.Count - 1);
        }

        public void RefreshAsyncHandles()
        {
            bool finish = true;
            for (int i = 0; i < _asyncHandles.Count; i++)
            {
                if (!_asyncHandles[i])
                {
                    finish = false;
                    break;
                }
            }

            if (finish)
            {
                for (int i = 0; i < _completionCallBacks.Count; i++)
                {
                    _completionCallBacks[i]();

                }

                _completionCallBacks.Clear();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CMMProgram
{
    public class EactThread
    {
        Thread thread;
        Func<object> _task;
        bool _isOk = false;
        object result = 0;
        public EactThread()
        {
            thread = new Thread(new ThreadStart(Process));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public object Run(Func<object> a)
        {
            _isOk = true;
            _task = a;
            while (true)
            {
                if (!_isOk)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            return result;
        }

        public void Stop()
        {
            thread.Join();
        }
        private void Process()
        {
            Console.WriteLine(string.Format("Main  CurrentThread:{0}", Thread.CurrentThread.ManagedThreadId));
            while (true)
            {
                try
                {
                    if (_task != null)
                    {
                        result = _task();
                        _isOk = false;
                        _task = null;
                    }
                }
                catch (Exception ex)
                {
                    _isOk = false;
                    _task = null;
                    Console.WriteLine(ex);
                }
            }
        }
    }
}

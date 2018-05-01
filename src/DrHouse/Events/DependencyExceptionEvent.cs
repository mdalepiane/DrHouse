using System;

namespace DrHouse.Events
{
    public class DependencyExceptionEvent : EventArgs
    {
        public Exception Exception { private set; get; }

        public DependencyExceptionEvent(Exception exception)
        {
            this.Exception = exception;
        }
    }
}

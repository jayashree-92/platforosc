using System;

namespace HM.Operations.Secure.Middleware
{
    public class UnhandledWireMessageException : Exception
    {
        public UnhandledWireMessageException()
            : base()
        {
        }
        public UnhandledWireMessageException(string message)
            : base(message)
        {
        }
        public UnhandledWireMessageException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
        public UnhandledWireMessageException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }
    }
}

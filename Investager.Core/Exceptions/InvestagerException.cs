using System;

namespace Investager.Core.Exceptions
{
    public class InvestagerException : Exception
    {
        public InvestagerException()
        {
        }

        public InvestagerException(string message)
            : base(message)
        {
        }

        public InvestagerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

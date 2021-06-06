using System;

namespace Investager.Core.Exceptions
{
    public class InvalidBearerTokenException : InvestagerException
    {
        public InvalidBearerTokenException()
        {
        }

        public InvalidBearerTokenException(string message)
            : base(message)
        {
        }

        public InvalidBearerTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

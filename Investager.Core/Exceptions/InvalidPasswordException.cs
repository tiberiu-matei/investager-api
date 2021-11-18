using System;

namespace Investager.Core.Exceptions;

public class InvalidPasswordException : InvestagerException
{
    public InvalidPasswordException()
    {
    }

    public InvalidPasswordException(string message)
        : base(message)
    {
    }

    public InvalidPasswordException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

using System;

namespace Compiler.Extensions.Exceptions
{
    public class UnexpectedValueException : Exception
    {
        public UnexpectedValueException(string message)
            : base(message)
        {
        }

        public UnexpectedValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
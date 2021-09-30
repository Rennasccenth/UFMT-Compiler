using System;

namespace Compiler.Extensions.Exceptions
{
    public class SyntacticException : Exception
    {
        public SyntacticException(string message)
            : base(message)
        {
        }

        public SyntacticException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
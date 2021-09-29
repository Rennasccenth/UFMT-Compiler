using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public record Token
    {
        public int Line { get; }
        public TokenType Type { get; }
        public string? TokenValue { get; }

        public Token(string? tokenValue, TokenType type)
        {
            TokenValue = tokenValue;
            Type = type;
        }

        public override string ToString()
        {
            return $"Token '{TokenValue}' is a {Type}.";
        }
    }

    public enum TokenType
    {
        INTEGER=1,
        FLOAT=2,
        IDENTIFIER=3,
        OPERATOR=4,
        KEYWORD=5,
        ATRIBUITOR=6,
        TYPEDECLARATION=7,
        SYMBOL=8,
        SEMICOLON=9,
        RELATIONAL=10
    }
}

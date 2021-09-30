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
            TokenValue = tokenValue ?? "NULL";
            Type = type;
        }

        public override string ToString()
        {
            return $"Token '{TokenValue}' is a {Type}.";
        }
    }

    public enum TokenType
    {
        Integer=1,
        Float=2,
        Identifier=3,
        Operator=4,
        Keyword=5,
        Attributor=6,
        TypeDeclaration=7,
        Symbol=8,
        Semicolon=9,
        Relational=10
    }
}

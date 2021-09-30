using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Extensions
{
    public static class NullableCharExtensionBehavior
    {
        public static bool IsLetter(this char? currentChar)
        {
            return currentChar is not null && char.IsLetter((char)currentChar);
        }
        public static bool IsZero(this char? currentChar)
        {
            return currentChar is '0';
        }
        public static bool IsNumberAndNotIsZero(this char? currentChar)
        {
            return currentChar is not null && char.IsNumber((char)currentChar) && currentChar is not '0';
        }
        public static bool IsNumber(this char? currentChar)
        {
            return currentChar is not null && char.IsNumber((char)currentChar);
        }
        public static bool IsLetterOrDigit(this char? currentChar)
        {
            return currentChar is not null && char.IsLetterOrDigit((char)currentChar);
        }
        public static bool IsWhiteSpace(this char? currentChar)
        {
            return currentChar is not null && char.IsWhiteSpace((char)currentChar);
        }
        public static bool IsSemiColon(this char? currentChar)
        {
            return currentChar is ';';
        }
        public static bool IsColon(this char? currentChar)
        {
            return currentChar is ':';
        }
        public static bool IsMathOperator(this char? currentChar){
            return currentChar is '+' or '-' or '/' or '*';
        }
        public static bool IsEqualSymbol(this char? currentChar)
        {
            return currentChar is '=';
        }
        public static bool IsSymbol(this char? currentChar)
        {
            return currentChar is '(' or ')'  or ',' or '.' or '$';
        }
        public static bool IsDot(this char? currentChar)
        {
            return currentChar is '.';
        }
        public static bool IsRelational(this char? currentChar)
        {
            return currentChar is '>' or '<';
        }
    }
}

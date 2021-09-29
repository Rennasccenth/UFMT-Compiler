using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Compiler.Extensions;

namespace Compiler
{
    public class LexAnalyzer
    {
        private State CurrentState { get; set; } 
        private string? CurrentBuffer { get; set; }
        private Token? CurrentToken { get; set; }
        private int Position { get; set; }

        private List<string?> Keywords => new()
        {
            "program",
            "begin",
            "end",
            "real",
            "integer",
            "read",
            "write",
            "if",
            "then",
            "else",
        };

        private string Input { get; }

        public List<Token> TokensAlreadyFound => new();

        public LexAnalyzer(string inputFileName)
        {
            try
            {
                var contentPath = EnvironmentExt.GetContentPath();
                using var sr = new StreamReader( string.Concat(contentPath, inputFileName));
                
                Input = sr.ReadToEnd() + " ";
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Token? NextToken()
        {
            ResetStateAndBuffer();
            ResetToken();

            while (CurrentChar is not null)
            {
                switch (CurrentState)
                {
                    case State.Initial:
                        {
                            if (CurrentChar.IsLetter())
                            {
                                CurrentState = State.ExpectingLetterOrNumber;
                                CurrentBuffer += CurrentChar;
                                GoNext();
                            }else if (CurrentChar.IsNumber())
                            {
                                CurrentState = State.ExpectingNumber;
                                CurrentBuffer += CurrentChar.ToString();
                                GoNext();
                            }else if (CurrentChar.IsWhiteSpace())
                            {
                                GoNext();
                            }else if (CurrentChar.IsMathOperator())
                            {
                                var mathOperatorFound = CurrentChar.ToString();
                                GoNext();
                                return new Token(mathOperatorFound, TokenType.OPERATOR);
                            }else if (CurrentChar.IsColon())
                            {
                                CurrentState = State.MaybeEqualsFromColon;
                                CurrentBuffer += CurrentChar;
                                GoNext();
                            }else if (CurrentChar.IsSymbol())
                            {
                                ResetStateAndBuffer();
                                CurrentBuffer += CurrentChar;
                                GoNext();
                                return new Token(CurrentBuffer, TokenType.SYMBOL);
                            }else if (CurrentChar.IsSemiColon())
                            {
                                ResetStateAndBuffer();
                                CurrentBuffer += CurrentChar;
                                GoNext();
                                return new Token(CurrentBuffer, TokenType.SEMICOLON);
                            }else if (CurrentChar.IsRelational())
                            {
                                CurrentState = State.MaybeEqualsFromRelational;
                                CurrentBuffer += CurrentChar;
                                GoNext();
                            }
                            // FALTA PEGAR OS OPERADORES RELACIONAIS
                            break;
                        }

                    case State.ExpectingLetterOrNumber:
                        {
                            if (CurrentChar.IsLetterOrDigit())
                            {
                                CurrentBuffer += CurrentChar.ToString();
                                GoNext();
                            }
                            else
                            {
                                ResetState();
                                if (Keywords.Contains(CurrentBuffer))
                                {
                                    CurrentToken = new Token(CurrentBuffer, TokenType.KEYWORD);
                                    TokensAlreadyFound.Add(CurrentToken);
                                    return CurrentToken;
                                }
                                CurrentToken = new Token(CurrentBuffer, TokenType.IDENTIFIER);
                                TokensAlreadyFound.Add(CurrentToken);
                                return CurrentToken;
                            }
                            break;
                        }

                    case State.ExpectingNumber:
                        {
                            if (CurrentChar.IsNumber())
                            {
                                CurrentBuffer += CurrentChar.ToString();
                                GoNext();
                                break;
                            }
                            if (CurrentChar.IsDot())
                            {
                                CurrentState = State.ExpectingOneNumberAfterDot;
                                CurrentBuffer += CurrentChar.ToString();
                                GoNext();
                                break;
                            }
                            if (CurrentChar.IsLetter())
                            {
                                throw new InvalidOperationException($"Unexpected token was found. Found a letter '{CurrentChar}' after a Number"); 
                            }
                            CurrentToken = new Token(CurrentBuffer, TokenType.INTEGER);
                            ResetStateAndBuffer();
                            TokensAlreadyFound.Add(CurrentToken);
                            return CurrentToken;
                        }
                    
                    case State.ExpectingOneNumberAfterDot:
                        if (!CurrentChar.IsNumber()) 
                            throw new InvalidOperationException("Unexpected token was found.");
                        CurrentState = State.ExpectingNumberAfterDot;
                        CurrentBuffer += CurrentChar.ToString();
                        GoNext();
                        break;
                        
                    case State.ExpectingNumberAfterDot:
                    {
                        if (CurrentChar.IsNumber())
                        {
                            CurrentBuffer += CurrentChar.ToString();
                            GoNext();
                            break;
                        }
                        if(CurrentChar.IsLetter())
                            throw new InvalidOperationException($"Unexpected token was found. Expected Math Operators or Relational Operators but found '{CurrentChar}'");
                            
                        CurrentToken = new Token(CurrentBuffer, TokenType.FLOAT);
                        ResetStateAndBuffer();
                        TokensAlreadyFound.Add(CurrentToken);
                        return CurrentToken;
                    }

                    case State.MaybeEqualsFromColon:
                    {
                        if (CurrentChar.IsEqualSymbol())
                        {
                            CurrentBuffer += CurrentChar;
                            CurrentToken = new Token(CurrentBuffer, TokenType.ATRIBUITOR);
                            ResetStateAndBuffer();
                            TokensAlreadyFound.Add(CurrentToken);
                            GoNext();
                            return CurrentToken; 
                        }
                        if (CurrentChar.IsNumber())
                        {
                            throw new InvalidOperationException($"Unexpected token was found. Expected letters buf found '{CurrentChar}'");                                                
                        }
                        CurrentToken = new Token(CurrentBuffer, TokenType.TYPEDECLARATION);
                        ResetStateAndBuffer();
                        TokensAlreadyFound.Add(CurrentToken);
                        return CurrentToken;
                    }
                    case State.MaybeEqualsFromRelational:
                    {
                        if (CurrentChar.IsEqualSymbol())
                        {
                            CurrentBuffer += CurrentChar;
                            CurrentToken = new Token(CurrentBuffer, TokenType.RELATIONAL);
                            ResetStateAndBuffer();
                            TokensAlreadyFound.Add(CurrentToken);
                            GoNext();
                            return CurrentToken; 
                        }
                        CurrentToken = new Token(CurrentBuffer, TokenType.RELATIONAL);
                        ResetStateAndBuffer();
                        TokensAlreadyFound.Add(CurrentToken);
                        GoNext();
                        return CurrentToken;
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        private bool IsEof()
        {
            return Input.Length == Position;
        }

        private char? CurrentChar => IsEof() ? null : Input[Position];

        private void GoBack() => Position--;

        private void GoNext() => Position++;
        private void ResetState() => CurrentState = State.Initial;
        private void ResetBuffer() => CurrentBuffer = string.Empty;
        private void ResetStateAndBuffer()
        {
            ResetState();
            ResetBuffer();
        }

        private void ResetToken()
        {
            CurrentToken = null;
        }
    }

    public enum State
    {
        Initial = 1,
        MaybeEqualsFromRelational = 2,
        ExpectingLetterOrNumber = 3,
        ExpectingNumber = 4,
        ExpectingNumberAfterDot = 5,
        ExpectingOneNumberAfterDot = 6,
        MaybeEqualsFromColon = 7,

    }
    
    
}

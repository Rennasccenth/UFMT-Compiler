using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Extensions.Exceptions;

namespace Compiler
{
    public class Syntactic
    {
        
        #region Properties and Constructor

        private readonly LexAnalyzer _lexScanner;
        private Token? _token;
        
        private int _buffer = 1;
        private int _currentLine;
        
        private readonly Dictionary<string , Symbol> Symbols = new();
        
        private string _generatedCode = new string("operator; argument1; argument2; result\n");

        private List<string> CStack { get; set; }
        private int s { get; set; }
        
        public Syntactic(string fileName)
        {
            _lexScanner = new LexAnalyzer(fileName);
            CStack = new List<string>();
            s = -1;
        }

        #endregion

        #region Auxiliary Functions

        private string PopFromCStack()
        {
            var lastItem = CStack.Last();
            
            CStack.RemoveAt(CStack.IndexOf(lastItem));
            
            return lastItem;
        }
        
        private void PushOnCStack(string item)
        {
            CStack.Add(item);
        }
        
        /// <summary>
        ///     Starts file parsing
        /// </summary>
        /// <exception cref="SyntacticException"> Throw when founds any unexpected token </exception>
        public void Analysis()
        {
            GetToken();
            Programa();
            if (_token is null)
            {
                foreach (var item in CStack)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                throw new SyntacticException($"Expected final of program, but found {_token}");
            }
        }
        
        private string? GenerateBuffer()
        {
            return $"T{_buffer++}";
        }

        private void IncrementGeneratedCode(string? operation, string argument1, string argument2, string? result)
        {
            _generatedCode += $"{operation};  {argument1}  ;  {argument2};  {result}\n";
            _currentLine++;
        }

        private bool VariableAlreadyDeclared(Token? variableToken)
        {
            var key = variableToken?.TokenValue;
            if (key is null || _token is null) throw new UnexpectedValueException("Cannot verify null value of a token.");
            return Symbols.ContainsKey(key);
        }

        private TokenType ResolveTokenType()
        {
            var key = _token?.TokenValue;
            if (key is null || _token is null) throw new UnexpectedValueException("Cannot register null value of a token.");
            Symbols.TryGetValue(key, out var bufferSymbol);

            var previousDeclaredType = bufferSymbol?._token.Type;
            return previousDeclaredType ?? _token.Type;
        }

        private void GetToken()
        {
            _token = _lexScanner.NextToken();
            if (_token is not null)
            {
                Console.WriteLine(_token);
            }
        }
        private void BackToken()
        {
            _lexScanner.UndoToken();
        }

        private void RegisterThisToken(TokenType type)
        {
            var key = _token?.TokenValue;
            if (key is null || _token is null) throw new UnexpectedValueException("Cannot register null value of a token.");

            s++;
            var bufferToken = new Token(key, type);
            var symbol = new Symbol(bufferToken, s);
            Symbols.Add(key, symbol);
        }

        /// <summary>
        ///     Verify the current Token to match expected values.
        /// </summary>
        /// <param name="expectedTerms">
        ///    List of expected terms that we're looking for
        /// </param>
        private bool ValidateTokenValue(params string[] expectedTerms)
        {
            return expectedTerms
                .Select(term => _token?.TokenValue?.Equals(term))
                .Any(match => match is true);
        }
        
        /// <summary>
        ///     Verify the current Token to match expected value.
        /// </summary>
        /// <param name="expected">
        ///    Expected term that we're looking for
        /// </param>
        private bool ValidateTokenValue(string expected)
        {
            return string.Equals(expected, _token?.TokenValue);
        }
        
        /// <summary>
        ///     Validate the current token type against a list of
        /// expected types.
        /// </summary>
        /// <param name="expectedTypes">
        ///     List of TokenTypes that we're trying to match
        /// </param>
        private bool ValidateTokenType(params TokenType?[] expectedTypes)
        {
            return expectedTypes.ToList().Contains(_token?.Type);
        }
        
        /// <summary>
        ///     Validate the current token type
        /// to match expected type.
        /// </summary>
        /// <param name="expectedToken">
        ///     The TokenType that we're trying to match
        /// </param>
        private bool ValidateTokenType(TokenType? expectedToken)
        {
            return expectedToken.Equals(_token?.Type);
        }
        
        private void ReplaceLastOccurence(string oldString)
        {
            var lastIndex = _generatedCode.LastIndexOf(oldString, StringComparison.Ordinal);

            string beginString = _generatedCode[..lastIndex];
            string endString = _generatedCode[(lastIndex + oldString.Length)..];

            _generatedCode = beginString + _currentLine + endString;
        }
        
        #endregion
        
        /// <summary>
        ///     ´programa´ -> program IDENTIFIER ´corpo´ .
        /// </summary>
        private void Programa()
        {
            if (ValidateTokenValue("program"))
            {
                GetToken();
                if (ValidateTokenType(TokenType.Identifier))
                {
                    PushOnCStack("INPP");
                    Corpo();
                    GetToken();
                    
                    if (ValidateTokenValue(".") is false)
                        throw new SyntacticException($"Syntactic error found, expected '.' but found {_token}.");
                    
                    PushOnCStack("PARA");
                    GetToken();
                    if (_token is not null) 
                        throw new SyntacticException($"Syntactic error found, this language doesn't support any instructions after the end of 'program' scope.");
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected a IDENTIFIER but found a {_token?.Type.ToString().ToUpper()}.");   
                }
            }
            else
            {
                throw new SyntacticException($"Syntactic error found, expected 'program' but found {_token}.");
            }
        }

        /// <summary>
        ///     ´corpo´ -> ´declaracao´ begin ´comandos´ end
        /// </summary>
        private void Corpo()
        {
            Declaracao();
            if (ValidateTokenValue("begin"))
            {
                Comandos();
                if (!ValidateTokenValue("end"))
                {
                    throw new SyntacticException($"Syntactic error found, expected 'end' but found {_token}.");
                }
            }
            else
            {
                throw new SyntacticException($"Syntactic error found, expected 'begin' but found {_token}.");
            }
        }
        
        /// <summary>
        ///     ´declaracao´ -> ´declaracao_variavel´ ´mais_declaracao´ | λ
        /// </summary>
        private void Declaracao()
        {
            GetToken();
            if (ValidateTokenValue("begin") is not false) 
                return;
            DeclaracaoVariavel();
            MaisDeclaracao();
        }

        /// <summary>
        ///     ´declaracao_variavel´ ->  ´tipo_var´ : ´variaveis´
        /// </summary>
        private void DeclaracaoVariavel()
        {
            var variableType = TipoVar();
            
            GetToken();
            if (ValidateTokenValue(":"))
            {
                Variaveis(variableType);
            }
            else
            {
                throw new SyntacticException($"Syntactic error found, expected ':' but found {_token}.");
            }
        }

        /// <summary>
        ///     ´tipo_var´ -> real | integer
        /// </summary>
        /// <returns>
        ///     Resolved enum of respective variable type
        /// </returns>
        private TokenType TipoVar()
        {
            return _token?.TokenValue switch
            {
                "real" => TokenType.Float,
                "integer" => TokenType.Integer,
                _ => throw new SyntacticException($"Syntactic error found, expected a valid type declaration but found {_token}.")
            };
        }

        /// <summary>
        ///     ´variaveis´ -> IDENTIFIER ´mais_var´
        /// </summary>
        /// <param name="variablesType">
        ///     Type of variables
        /// </param>
        private void Variaveis(TokenType variablesType)
        {
            GetToken();

            if (ValidateTokenType(TokenType.Identifier))
            {
                if (VariableAlreadyDeclared(_token))
                {
                    throw new SyntacticException(
                        $"Syntactic error found, variable '{_token?.TokenValue}' was already declared before.");
                }
                
                RegisterThisToken(variablesType);
                PushOnCStack("ALME 1");
                MaisVar(variablesType);
            }
            else
            {
                throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' was already declared before.");
            }
        }
        
        /// <summary>
        ///     ´mais_var´ -> , ´variaveis´ | λ 
        /// </summary>
        /// <param name="variablesType">
        ///     Type of the another variables
        /// </param>
        private void MaisVar(TokenType variablesType)
        {
            GetToken();
            if (ValidateTokenValue(","))
            {
                Variaveis(variablesType);
            }
        }
        
        /// <summary>
        ///     ´mais_dc´ -> ; ´dc´ | λ 
        /// </summary>
        private void MaisDeclaracao()
        {
            if (ValidateTokenValue(";"))
            {
                Declaracao();
            }
        }
        
        /// <summary>
        ///     ´comandos´ -> ´comando´ ´mais_comandos´
        /// </summary>
        private void Comandos()
        {
            
            Comando();
            MaisComandos(ValidateTokenValue(";"));
        }
        
        /// <summary>
        ///     ´comando´ -> read (IDENTIFIER)
        ///     ´comando´ -> write (IDENTIFIER)
        ///     ´comando´ -> ident := ´expressao´
        ///     ´comando´ -> if ´condicao´ then ´comandos´ ´pfalsa´ $
        ///     ´comando´ -> while ´condicao´ do ´comandos´ $
        /// </summary>
        private void Comando()
        {
            GetToken();
            if (ValidateTokenValue("read"))
            {
                GetToken();
                if (ValidateTokenValue("("))
                {
                    GetToken();
                    if (ValidateTokenType(TokenType.Identifier))
                    {
                        var bufferIdentifier = _token?.TokenValue;
                        if (VariableAlreadyDeclared(_token) is not true)
                        {
                            throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' wasn't declared before.");
                        }
                        
                        GetToken();
                        if (!ValidateTokenValue(")"))
                        {
                            throw new SyntacticException($"Syntactic error found, expected ')' but found '{_token?.TokenValue}'.");
                        }

                        s++;
                        PushOnCStack("LEIT");
                        PushOnCStack($"ARMZ {Symbols[bufferIdentifier]._relativePosition}");
                        
                    }
                    else
                    {
                        throw new SyntacticException($"Syntactic error found, expected a 'IDENTIFIER' but found '{_token?.Type}'.");    
                    }
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected '(' but found '{_token?.TokenValue}'.");
                }
            }
            else if (ValidateTokenValue("write"))
            {
                GetToken();
                if (ValidateTokenValue("("))
                {
                    GetToken();
                    if (ValidateTokenType(TokenType.Identifier))
                    {
                        var bufferIdentifier = _token?.TokenValue;
                        if (VariableAlreadyDeclared(_token) is not true)
                        {
                            throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' wasn't declared before.");
                        }
                        
                        GetToken();
                        if (!ValidateTokenValue(")"))
                        {
                            throw new SyntacticException($"Syntactic error found, expected ')' but found '{_token?.TokenValue}'.");
                        }
                        
                        s++;
                        PushOnCStack("IMPR");
                        PushOnCStack($"CRVL {Symbols[bufferIdentifier]._relativePosition}");
                    }
                    else
                    {
                        throw new SyntacticException($"Syntactic error found, expected a 'IDENTIFIER' but found '{_token?.Type}'.");    
                    }
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected '(' but found '{_token?.TokenValue}'.");
                }
            }
            else if (ValidateTokenType(TokenType.Identifier))
            {
                var tokenBuffer = _token?.TokenValue; 
                if (VariableAlreadyDeclared(_token) is not true)
                {
                    throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' wasn't declared before.");
                }
                
                GetToken();
                if (ValidateTokenValue(":="))
                {
                    Expressao();
                    PushOnCStack($"ARMZ {Symbols[tokenBuffer]._relativePosition}");
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected ':=' but found '{_token?.TokenValue}'.");
                }
            }
            else if (ValidateTokenValue("if"))
            {   
                var condition = Condicao();
                if (ValidateTokenValue("then"))
                {
                    var TempDSVFPosition = CStack.Count;
                    PushOnCStack("DSVF TempDSVF");
                    
                    Comandos();
                    
                    var TempDSVIPosition = CStack.Count;
                    PushOnCStack("DSVI TempDSVI");
                    var lineOfElse = CStack.Count;
                    Pfalsa();

                    if (CStack.Count == lineOfElse)
                    {
                        PopFromCStack();
                        CStack[TempDSVFPosition] = CStack[TempDSVFPosition].Replace("TempDSVF", CStack.Count.ToString());
                    }
                    else
                    {
                        CStack[TempDSVFPosition] = CStack[TempDSVFPosition].Replace("TempDSVF", lineOfElse.ToString());
                        CStack[TempDSVIPosition] = CStack[TempDSVIPosition].Replace("TempDSVI", CStack.Count.ToString());
                    }
                    
                    if (ValidateTokenValue("$") is not true)
                    {
                        throw new SyntacticException($"Syntactic error found, expected '$' but found '{_token?.TokenValue}'.");
                    }
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected 'then' but found '{_token?.TokenValue}'.");
                }
            }
            else if (ValidateTokenValue("while"))
            {
                var condLineTemp = CStack.Count;
                
                Condicao();
                
                var DSVFTempLine = CStack.Count;
                PushOnCStack("DSVF DSVFTempLine");
                
                if (ValidateTokenValue("do"))
                {
                    
                    Comandos();
                    
                    PushOnCStack($"DSVF {condLineTemp.ToString()}");

                    CStack[DSVFTempLine] = CStack[DSVFTempLine].Replace("DSVFTempLine", CStack.Count.ToString()); 
                    
                    if (ValidateTokenValue("$") is not true)
                    {
                        throw new SyntacticException($"Syntactic error found, expected '$' but found '{_token?.TokenValue}'.");
                    }
                }
                else
                {
                    throw new SyntacticException($"Syntactic error found, expected 'do' but found '{_token?.TokenValue}'.");
                }
            }
            else
            {
                throw new SyntacticException($"Syntactic error found, unsupported expression, expected 'read', 'write', 'IDENTIFIER', 'if' or 'while' but found '{_token?.TokenValue}' instead.");
            }
        }
        
        /// <summary>
        ///     ´mais_comandos´ -> ; ´comandos´ | λ 
        /// </summary>
        private void MaisComandos(bool previouslyGetSemiColon)
        {
            if (!previouslyGetSemiColon)
                GetToken();
            if (ValidateTokenValue(";"))
            {
                Comandos();
            }
        }

        /// <summary>
        ///     ´expressao´ -> ´termo´ ´outros_termos´
        /// </summary>
        private string? Expressao()
        {
            var termo = Termo();
            if (termo is "") return termo;
            var outrosTermos = OutrosTermos(termo);

            return outrosTermos;
        }

        /// <summary>
        ///     ´outros_termos´ -> ´op_ad´ ´termo´ ´outros_termos´ | λ
        /// </summary>
        private string? OutrosTermos(string? outrosTermosEsq)
        {
            if (ValidateTokenValue("+", "-"))
            {
                var opAdDir = OpAd();
                GetToken();
                var bufferVar = _token?.TokenValue;
                var termoDir = Termo();
                if (opAdDir == "+")
                    PushOnCStack("SOMA");
                else
                    PushOnCStack("SUBT");
                
                if (ValidateTokenValue(";")) termoDir = bufferVar;

                return OutrosTermos(termoDir);
            }
            return outrosTermosEsq;
        }
        

        /// <summary>
        ///     ´condicao´ -> ´expressao´ ´relacao´ ´expressao´ 
        /// </summary>
        /// <returns>
        ///     Returns the semantic of the operation
        /// </returns>
        private string? Condicao()
        {
            var expressaoDir = Expressao();
            var relacaoDir = Relacao();
            var expressaoLinhaDir = Expressao();

            switch (relacaoDir)
            {
                case "<=":
                    PushOnCStack("CPMI");
                    break;
                case "=":
                    PushOnCStack("CPIG");
                    break;
                case ">=":
                    PushOnCStack("CMAI");
                    break;
                case "<>":
                    PushOnCStack("CDES");
                    break;
                case "<":
                    PushOnCStack("CPME");
                    break;
                case ">":
                    PushOnCStack("CPMA");
                    break;
            }
            
            return string.Empty;
        }

        /// <summary>
        ///     'pfalsa' -> else 'comandos' | λ 
        /// </summary>
        private void Pfalsa()
        {
            if (ValidateTokenValue("else"))
            {
                Comandos();
            }
        }

        /// <summary>
        ///     ´relacao´ -> = | <> | >= | <= | > | <
        /// </summary>
        /// <exception cref="SyntacticException"></exception>
        private string? Relacao()
        {
            if (!ValidateTokenValue("=", "<>", "<>", ">=", "<=", ">", "<"))
            {
                throw new SyntacticException($"Syntactic error, expected relational operator but found '{_token?.TokenValue}'");
            }

            return _token?.TokenValue;
        }

        /// <summary>
        ///     ´termo´ -> ´op_un´ ´fator´ ´mais_fatores´ 
        /// </summary>
        /// <returns></returns>
        private string? Termo()
        {
            var opUn = OpUn();
            var fatorDir = Fator(opUn);
            var maisFatoresDir = MaisFatores(fatorDir);

            return maisFatoresDir;
        }

        /// <summary>
        ///     ´fator´ -> ident | numero_int | numero_real | (´expressao´)
        /// </summary>
        /// <param name="fatorEsq"></param>
        /// <exception cref="SyntacticException"></exception>
        private string? Fator(string? fatorEsq)
        {
            if (fatorEsq == "")
            {
                return null;
            } 
            
            if (ValidateTokenType(TokenType.Identifier))
            {
                var identifier = _token?.TokenValue;
                if (VariableAlreadyDeclared(_token) is false)
                {
                    throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' wasn't declared before.");
                }

                Symbols.TryGetValue(_token!.TokenValue!, out var previouslyRegisteredSymbol);
                
                if (ResolveTokenType() != previouslyRegisteredSymbol?._token.Type)
                {
                    throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' have a different type declaration.");
                }

                PushOnCStack($"CRVL {Symbols[identifier!]._relativePosition}");
                
                if (fatorEsq != "-") return  identifier;
                
                var bufferRegister = GenerateBuffer();
                PushOnCStack($"INVE");
                return bufferRegister;

            }
            if (ValidateTokenType(TokenType.Integer, TokenType.Float))
            {
    
                Symbols.TryGetValue(_token!.TokenValue!, out var previouslyRegisteredToken);
                
                PushOnCStack($"CRCT {_token!.TokenValue}");
                
                if (fatorEsq != "-") return _token.TokenValue;
                
                PushOnCStack($"INVE");

                var t = GenerateBuffer();
                return t;
            }
            if (ValidateTokenValue("("))
            {
                var expressaoDir = Expressao();
                
                GetToken();
                if (ValidateTokenValue(")") is false)
                {
                    throw new SyntacticException($"Syntactic error, expected ')' operator but found '{_token?.TokenValue}'");
                }

                if (fatorEsq != "-") return expressaoDir;
                var t = GenerateBuffer();
                PushOnCStack($"INVE");
                return t;
            }
            
            return "";
        }

         
        /// <summary>
        ///     ´mais_fatores´ -> ´op_mul´ ´fator´ ´mais_fatores´ | λ
        /// </summary>
        /// <param name="maisFatoresEsq"></param>
        private string? MaisFatores(string? maisFatoresEsq)
        {
            if (maisFatoresEsq != "") GetToken();
            
            if (ValidateTokenValue("*", "/"))
            {
                var opMulDir = OpMul();
                GetToken();
                var fatorDir = Fator(opMulDir);

                var bufferRegister = GenerateBuffer();
                if (opMulDir == "*")
                {
                    PushOnCStack("MULT");
                }
                else
                {
                    PushOnCStack("DIVI");
                }

                fatorDir = bufferRegister;
                return MaisFatores(fatorDir);
            }
            return maisFatoresEsq;
        }

        
        /// <summary>
        ///     ´op_un´ -> - | λ
        /// </summary>
        /// <returns></returns>
        private string? OpUn()
        {
            GetToken();
            if (ValidateTokenValue("$"))
            {
                BackToken();
                return null;
            }
            return ValidateTokenValue("-") ? "-" : null;
        }
        
        /// <summary>
        ///     ´op_ad´ -> + | -
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnexpectedValueException"></exception>
        private string? OpAd()
        {
            var value = _token?.TokenValue;
            if (!ValidateTokenValue("+", "-")) throw new UnexpectedValueException($"Expected '+' or '-' buf found '{_token?.TokenValue}'");
            return value;
        }
        
        /// <summary>
        ///     ´op_mul´ -> * | / 
        /// </summary>
        /// <returns></returns>
        private string? OpMul()
        {   
            if (ValidateTokenValue("*", "/"))
                return _token?.TokenValue;
            throw new SyntacticException($"Syntactic error, expected '*' or '/' operators but found '{_token?.TokenValue}'");
        }

        private class Symbol
        {
            public Token _token { get; set; }
            public int _relativePosition { get; set; }

            public Symbol(Token token, int relativePosition)
            {
                _token = token;
                _relativePosition = relativePosition;
            }
        }
    }
}
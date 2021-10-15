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
        
        private readonly Dictionary<string , Token> _tokenTable = new();
        
        private string _generatedCode = new string("operator; argument1; argument2; result\n");

        public Syntactic(string fileName)
        {
            _lexScanner = new LexAnalyzer(fileName);
        }

        #endregion

        #region Auxiliary Functions
        
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
                Console.WriteLine(_generatedCode);
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
            return _tokenTable.ContainsKey(key);
        }

        private TokenType ResolveTokenType()
        {
            var key = _token?.TokenValue;
            if (key is null || _token is null) throw new UnexpectedValueException("Cannot register null value of a token.");
            _tokenTable.TryGetValue(key, out var bufferToken);

            var previousDeclaredType = bufferToken?.Type;
            return previousDeclaredType ?? _token.Type;
        }
        
        private string GetThisTokenInitialValue()
        {
            return _token?.Type switch
            {
                TokenType.Float => "0.0",
                TokenType.Integer => "0",
                _ => ""
            };
        }
        
        private void GetToken()
        {
            _token = _lexScanner.NextToken();
        }
        private void BackToken()
        {
            _lexScanner.BackOneToken();
        }

        private void RegisterThisToken(TokenType type)
        {
            var key = _token?.TokenValue;
            if (key is null || _token is null) throw new UnexpectedValueException("Cannot register null value of a token.");
            var bufferToken = new Token(key, type);
            _tokenTable.Add(key, bufferToken);
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
                    Corpo();
                    GetToken();
                    
                    if (ValidateTokenValue(".") is false)
                        throw new SyntacticException($"Syntactic error found, expected '.' but found {_token}.");
                    
                    IncrementGeneratedCode("PARA", "", "", "");
                    
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
                IncrementGeneratedCode("ALME", GetThisTokenInitialValue(), "", _token?.TokenValue ?? "");
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

                        IncrementGeneratedCode("read", "", "", bufferIdentifier);
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

                        IncrementGeneratedCode("write", "", "", bufferIdentifier);
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
                    var expression = Expressao();
                    IncrementGeneratedCode(":=", expression, "", tokenBuffer);
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
                    IncrementGeneratedCode("JF", condition, "JF_line", "");
                    Comandos();
                    IncrementGeneratedCode("goto", "goto_line", "", "");
                    ReplaceLastOccurence("JF_line");
                    Pfalsa();
                    ReplaceLastOccurence("goto_line");
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
            else
            {
                throw new SyntacticException($"Syntactic error found, unsupported expression, expected 'read', 'write', 'IDENTIFIER' or 'if' but found '{_token?.TokenValue}' instead.");
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
                if (ValidateTokenValue(";")) termoDir = bufferVar;
                

                var generatedBuffer = GenerateBuffer();
                IncrementGeneratedCode(opAdDir, outrosTermosEsq, termoDir, generatedBuffer);
                termoDir = generatedBuffer;
                
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
            var t = GenerateBuffer();
            IncrementGeneratedCode(relacaoDir, expressaoDir, expressaoLinhaDir, t);
            return t;
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

                _tokenTable.TryGetValue(_token!.TokenValue!, out var previouslyRegisteredToken);
                
                if (ResolveTokenType() != previouslyRegisteredToken?.Type)
                {
                    throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' have a different type declaration.");
                }

                if (fatorEsq != "-") return  identifier;
                
                var bufferRegister = GenerateBuffer();
                IncrementGeneratedCode("minus",  identifier, "", bufferRegister);
                return bufferRegister;

            }
            if (ValidateTokenType(TokenType.Integer, TokenType.Float))
            {
    
                _tokenTable.TryGetValue(_token!.TokenValue!, out var previouslyRegisteredToken);
                
                // if (_token.Type != previouslyRegisteredToken?.Type)
                // {
                //     throw new SyntacticException($"Syntactic error found, variable '{_token?.TokenValue}' have a different type declaration.");
                // }

                if (fatorEsq != "-") return _token.TokenValue;
                
                var t = GenerateBuffer();
                IncrementGeneratedCode("minus", _token.TokenValue!, "", t);
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
                IncrementGeneratedCode("minus", expressaoDir, "", t);
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
                    IncrementGeneratedCode("*", maisFatoresEsq, fatorDir, bufferRegister);
                }
                else
                {
                    IncrementGeneratedCode("/", maisFatoresEsq, fatorDir, bufferRegister);
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
    }
}
using CompilerProject.Models.DataStructures;
using CompilerProject.Models.Helpers;
using System.Collections.Generic;

namespace CompilerProject.Models.Phases
{
    internal class LexicalAnalyzer
    {
        private DFATransitionMatrix _dfa;
        private TokenDictionary _tokenDict;
        private ErrorHandler _errorHandler;

        public LexicalAnalyzer(
            DFATransitionMatrix dfa,
            TokenDictionary tokenDict,
            ErrorHandler errorHandler)
        {
            _dfa = dfa;
            _tokenDict = tokenDict;
            _errorHandler = errorHandler;
        }

        // Main lexical analysis function
        public List<Token> Analyze(string source, List<ErrorModel> errors)
        {
            List<Token> tokens = new List<Token>();

            int i = 0;
            int line = 1;
            int column = 1;

            while (i < source.Length)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(source[i]))
                {
                    if (source[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }

                    i++;
                    continue;
                }

                // Skip single-line comments: // comment
                if (i + 1 < source.Length && source[i] == '/' && source[i + 1] == '/')
                {
                    i = SkipSingleLineComment(source, i, ref line, ref column);
                    continue;
                }

                // Skip multi-line comments: /* comment */
                if (i + 1 < source.Length && source[i] == '/' && source[i + 1] == '*')
                {
                    i = SkipMultiLineComment(source, i, ref line, ref column, errors);
                    continue;
                }

                // Read string literals
                if (source[i] == '"' || source[i] == '\'')
                {
                    Token strToken = ReadString(source, ref i, ref line, ref column, errors);

                    if (strToken != null)
                    {
                        tokens.Add(strToken);
                    }

                    continue;
                }

                // DFA traversal using Maximal Munch
                Token token = DFATraversal(source, ref i, ref line, ref column, errors);

                if (token != null)
                {
                    tokens.Add(token);
                }
            }

            // Add EOF token at the end
            tokens.Add(new Token(TokenType.EOF, "EOF", line, column));

            return tokens;
        }

        // Runs the DFA over the input using Maximal Munch
        private Token DFATraversal(
            string source,
            ref int i,
            ref int line,
            ref int column,
            List<ErrorModel> errors)
        {
            int currentState = DFATransitionMatrix.STATE_START;
            int startColumn = column;
            string lexeme = "";

            while (i < source.Length)
            {
                char c = source[i];

                int category = _dfa.GetCharCategory(c);
                int nextState = _dfa.GetNextState(currentState, category);

                if (nextState == DFATransitionMatrix.STATE_ERROR)
                {
                    break;
                }

                lexeme += c;
                currentState = nextState;

                i++;
                column++;
            }

            if (lexeme == "")
            {
                errors.Add(new ErrorModel(
                    ErrorType.Lexical,
                    _errorHandler.GetErrorMessage("LEX001") + $": '{source[i]}'",
                    line,
                    column,
                    ErrorSeverity.Critical
                ));

                i++;
                column++;

                return null;
            }

            return TokenClassification(lexeme, currentState, line, startColumn, errors);
        }

        // Classifies a lexeme into a token according to the final DFA state
        private Token TokenClassification(
            string lexeme,
            int finalState,
            int line,
            int column,
            List<ErrorModel> errors)
        {
            if (finalState == DFATransitionMatrix.STATE_IDENTIFIER)
            {
                TokenType type = _tokenDict.GetTokenType(lexeme);
                return new Token(type, lexeme, line, column);
            }

            if (finalState == DFATransitionMatrix.STATE_NUMBER)
            {
                return new Token(TokenType.NumberLiteral, lexeme, line, column);
            }

            if (finalState == DFATransitionMatrix.STATE_OPERATOR ||
                finalState == DFATransitionMatrix.STATE_PUNCTUATION)
            {
                if (_tokenDict.ContainsToken(lexeme))
                {
                    TokenType type = _tokenDict.GetTokenType(lexeme);
                    return new Token(type, lexeme, line, column);
                }

                errors.Add(new ErrorModel(
                    ErrorType.Lexical,
                    _errorHandler.GetErrorMessage("LEX001") + $": '{lexeme}'",
                    line,
                    column,
                    ErrorSeverity.Critical
                ));

                return null;
            }

            errors.Add(new ErrorModel(
                ErrorType.Lexical,
                _errorHandler.GetErrorMessage("LEX001") + $": '{lexeme}'",
                line,
                column,
                ErrorSeverity.Critical
            ));

            return null;
        }

        // Reads a string literal until the closing quote
        private Token ReadString(
            string source,
            ref int i,
            ref int line,
            ref int column,
            List<ErrorModel> errors)
        {
            char quote = source[i];

            int startLine = line;
            int startColumn = column;

            string value = "" + quote;

            i++;
            column++;

            while (i < source.Length && source[i] != quote)
            {
                if (source[i] == '\n')
                {
                    errors.Add(new ErrorModel(
                        ErrorType.Lexical,
                        _errorHandler.GetErrorMessage("LEX002"),
                        startLine,
                        startColumn,
                        ErrorSeverity.Critical
                    ));

                    return null;
                }

                value += source[i];

                i++;
                column++;
            }

            if (i >= source.Length)
            {
                errors.Add(new ErrorModel(
                    ErrorType.Lexical,
                    _errorHandler.GetErrorMessage("LEX002"),
                    startLine,
                    startColumn,
                    ErrorSeverity.Critical
                ));

                return null;
            }

            // Add closing quote
            value += source[i];

            i++;
            column++;

            return new Token(TokenType.StringLiteral, value, startLine, startColumn);
        }

        // Skips a single-line comment
        private int SkipSingleLineComment(
            string source,
            int i,
            ref int line,
            ref int column)
        {
            while (i < source.Length && source[i] != '\n')
            {
                i++;
                column++;
            }

            if (i < source.Length && source[i] == '\n')
            {
                i++;
                line++;
                column = 1;
            }

            return i;
        }

        // Skips a multi-line comment
        private int SkipMultiLineComment(
            string source,
            int i,
            ref int line,
            ref int column,
            List<ErrorModel> errors)
        {
            int startLine = line;
            int startColumn = column;

            // Skip /*
            i += 2;
            column += 2;

            while (i + 1 < source.Length)
            {
                if (source[i] == '\n')
                {
                    i++;
                    line++;
                    column = 1;
                    continue;
                }

                if (source[i] == '*' && source[i + 1] == '/')
                {
                    i += 2;
                    column += 2;
                    return i;
                }

                i++;
                column++;
            }

            errors.Add(new ErrorModel(
                ErrorType.Lexical,
                _errorHandler.GetErrorMessage("LEX004"),
                startLine,
                startColumn,
                ErrorSeverity.Critical
            ));

            return source.Length;
        }
    }
}
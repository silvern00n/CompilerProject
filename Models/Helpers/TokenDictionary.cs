using CompilerProject.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.Helpers
{
    internal class TokenDictionary
    {
        // Token dictionary — maps a string value to a TokenType
        private Dictionary<string, TokenType> _tokenDict;

        public TokenDictionary()
        {
            _tokenDict = new Dictionary<string, TokenType>();
            InitializeTokenDictionary();
        }

        private void InitializeTokenDictionary()
        {
            // Keywords
            string[] keywords =
            {
                "let", "const", "var",
                "if", "else", "switch", "case", "default",
                "for", "while", "do", "break", "continue",
                "function", "return",
                "try", "catch", "finally",
                "class", "extends", "new", "super",
                "import", "export",
                "typeof", "instanceof", "in"
            };

            foreach (string keyword in keywords)
            {
                _tokenDict[keyword] = TokenType.Keyword;
            }

            // Literals
            _tokenDict["true"] = TokenType.BooleanLiteral;
            _tokenDict["false"] = TokenType.BooleanLiteral;
            _tokenDict["null"] = TokenType.NullLiteral;
            _tokenDict["undefined"] = TokenType.NullLiteral;

            // Operators
            Dictionary<string, TokenType> operators = new Dictionary<string, TokenType>
            {
                { "+", TokenType.ArithmeticOperator },
                { "-", TokenType.ArithmeticOperator },
                { "*", TokenType.ArithmeticOperator },
                { "/", TokenType.ArithmeticOperator },
                { "%", TokenType.ArithmeticOperator },
                { "**", TokenType.ArithmeticOperator },

                { "++", TokenType.IncrementOperator },
                { "--", TokenType.IncrementOperator },

                { "=", TokenType.AssignmentOperator },
                { "+=", TokenType.AssignmentOperator },
                { "-=", TokenType.AssignmentOperator },
                { "*=", TokenType.AssignmentOperator },
                { "/=", TokenType.AssignmentOperator },
                { "%=", TokenType.AssignmentOperator },
                { "**=", TokenType.AssignmentOperator },

                { "==", TokenType.ComparisonOperator },
                { "!=", TokenType.ComparisonOperator },
                { "===", TokenType.ComparisonOperator },
                { "!==", TokenType.ComparisonOperator },
                { ">", TokenType.ComparisonOperator },
                { "<", TokenType.ComparisonOperator },
                { ">=", TokenType.ComparisonOperator },
                { "<=", TokenType.ComparisonOperator },

                { "&&", TokenType.LogicalOperator },
                { "||", TokenType.LogicalOperator },
                { "!", TokenType.LogicalOperator },

                { "&", TokenType.BitwiseOperator },
                { "|", TokenType.BitwiseOperator },
                { "^", TokenType.BitwiseOperator },
                { "~", TokenType.BitwiseOperator },
                { "<<", TokenType.BitwiseOperator },
                { ">>", TokenType.BitwiseOperator },
                { ">>>", TokenType.BitwiseOperator }
            };

            foreach (var op in operators)
            {
                _tokenDict[op.Key] = op.Value;
            }

            // Punctuation / separators
            Dictionary<string, TokenType> punctuation = new Dictionary<string, TokenType>
            {
                { "(", TokenType.LeftParenthesis },
                { ")", TokenType.RightParenthesis },
                { "{", TokenType.LeftBrace },
                { "}", TokenType.RightBrace },
                { "[", TokenType.LeftBracket },
                { "]", TokenType.RightBracket },

                { ";", TokenType.Semicolon },
                { ",", TokenType.Comma },
                { ".", TokenType.Dot },
                { ":", TokenType.Colon },
                { "?", TokenType.QuestionMark }
            };

            foreach (var p in punctuation)
            {
                _tokenDict[p.Key] = p.Value;
            }
        }

        public TokenType GetTokenType(string value)
        {
            if (_tokenDict.ContainsKey(value))
            {
                return _tokenDict[value];
            }

            return TokenType.Identifier;
        }

        public bool IsKeyword(string value)
        {
            return _tokenDict.ContainsKey(value) && _tokenDict[value] == TokenType.Keyword;
        }

        public bool ContainsToken(string value)
        {
            return _tokenDict.ContainsKey(value);
        }
    }
}

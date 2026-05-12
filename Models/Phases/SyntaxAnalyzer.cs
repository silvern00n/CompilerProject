using CompilerProject.Models.DataStructures;
using CompilerProject.Models.Helpers;
using System.Collections.Generic;

namespace CompilerProject.Models.Phases
{
    internal class SyntaxAnalyzer
    {
        private List<Token> _tokens;
        private int _current;
        private List<ErrorModel> _errors;
        private ErrorHandler _errorHandler;

        public SyntaxAnalyzer(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
            _tokens = new List<Token>();
            _errors = new List<ErrorModel>();
            _current = 0;
        }

        // Main syntax analysis function
        public ASTNode Analyze(List<Token> tokens, List<ErrorModel> errors)
        {
            _tokens = tokens;
            _errors = errors;
            _current = 0;

            ASTNode root = new ASTNode(ASTNodeType.Program);

            while (!IsAtEnd())
            {
                ASTNode statement = ParseStatement();

                if (statement != null)
                {
                    root.AddChild(statement);
                }
                else
                {
                    SyntaxErrorRecovery("SYN003");
                }
            }

            return root;
        }

        // Parses a statement according to the current token
        private ASTNode ParseStatement()
        {
            Token current = CurrentToken();

            if (current == null)
            {
                return null;
            }

            if (current.Type == TokenType.Keyword)
            {
                switch (current.Value)
                {
                    case "let":
                    case "const":
                    case "var":
                        return ParseVariableDeclaration();

                    case "if":
                        return ParseIfStatement();

                    case "while":
                        return ParseWhileStatement();

                    case "for":
                        return ParseForStatement();

                    case "do":
                        return ParseDoWhileStatement();

                    case "function":
                        return ParseFunctionDeclaration();

                    case "return":
                        return ParseReturnStatement();

                    case "switch":
                        return ParseSwitchStatement();

                    default:
                        return ParseExpressionStatement();
                }
            }

            if (current.Type == TokenType.Identifier && PeekValue(1) == "=")
            {
                return ParseAssignmentStatement();
            }

            return ParseExpressionStatement();
        }

        // Parses variable declaration: let/const/var name = value;
        private ASTNode ParseVariableDeclaration()
        {
            ASTNode node = new ASTNode(ASTNodeType.VariableDeclaration);

            Token keyword = Consume();
            node.AddChild(new ASTNode(ASTNodeType.Keyword, keyword.Value));

            Token name = Expect(TokenType.Identifier, "SYN003");

            if (name != null)
            {
                node.AddChild(new ASTNode(ASTNodeType.Identifier, name.Value));
            }

            if (Match("="))
            {
                Consume();

                ASTNode value = ParseExpression();

                if (value != null)
                {
                    node.AddChild(value);
                }
            }

            Expect(";", "SYN002");

            return node;
        }

        // Parses assignment statement: name = value;
        private ASTNode ParseAssignmentStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.Assignment);

            Token name = Expect(TokenType.Identifier, "SYN003");

            if (name != null)
            {
                node.AddChild(new ASTNode(ASTNodeType.Identifier, name.Value));
            }

            Expect("=", "SYN003");

            ASTNode value = ParseExpression();

            if (value != null)
            {
                node.AddChild(value);
            }

            Expect(";", "SYN002");

            return node;
        }

        // Parses if statement
        private ASTNode ParseIfStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.IfStatement);

            Consume(); // if

            Expect("(", "SYN001");

            ASTNode condition = ParseExpression();
            if (condition != null)
            {
                node.AddChild(condition);
            }

            Expect(")", "SYN001");

            ASTNode block = ParseBlock();
            if (block != null)
            {
                node.AddChild(block);
            }

            if (Match("else"))
            {
                Consume();

                if (Match("if"))
                {
                    ASTNode elseIfNode = ParseIfStatement();
                    if (elseIfNode != null)
                    {
                        node.AddChild(elseIfNode);
                    }
                }
                else
                {
                    ASTNode elseBlock = ParseBlock();
                    if (elseBlock != null)
                    {
                        node.AddChild(elseBlock);
                    }
                }
            }

            return node;
        }

        // Parses while loop
        private ASTNode ParseWhileStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.WhileStatement);

            Consume(); // while

            Expect("(", "SYN001");

            ASTNode condition = ParseExpression();
            if (condition != null)
            {
                node.AddChild(condition);
            }

            Expect(")", "SYN001");

            ASTNode block = ParseBlock();
            if (block != null)
            {
                node.AddChild(block);
            }

            return node;
        }

        // Parses for loop
        private ASTNode ParseForStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.ForStatement);

            Consume(); // for

            Expect("(", "SYN001");

            ASTNode init = ParseStatement();
            if (init != null)
            {
                node.AddChild(init);
            }

            ASTNode condition = ParseExpression();
            if (condition != null)
            {
                node.AddChild(condition);
            }

            Expect(";", "SYN002");

            ASTNode update = ParseExpression();
            if (update != null)
            {
                node.AddChild(update);
            }

            Expect(")", "SYN001");

            ASTNode block = ParseBlock();
            if (block != null)
            {
                node.AddChild(block);
            }

            return node;
        }

        // Parses do while loop
        private ASTNode ParseDoWhileStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.DoWhileStatement);

            Consume(); // do

            ASTNode block = ParseBlock();
            if (block != null)
            {
                node.AddChild(block);
            }

            Expect("while", "SYN003");
            Expect("(", "SYN001");

            ASTNode condition = ParseExpression();
            if (condition != null)
            {
                node.AddChild(condition);
            }

            Expect(")", "SYN001");
            Expect(";", "SYN002");

            return node;
        }

        // Parses function declaration
        private ASTNode ParseFunctionDeclaration()
        {
            ASTNode node = new ASTNode(ASTNodeType.FunctionDeclaration);

            Consume(); // function

            Token name = Expect(TokenType.Identifier, "SYN003");

            if (name != null)
            {
                node.AddChild(new ASTNode(ASTNodeType.Identifier, name.Value));
            }

            Expect("(", "SYN001");

            ASTNode parameters = ParseParameters();
            if (parameters != null)
            {
                node.AddChild(parameters);
            }

            Expect(")", "SYN001");

            ASTNode block = ParseBlock();
            if (block != null)
            {
                node.AddChild(block);
            }

            return node;
        }

        // Parses function parameters
        private ASTNode ParseParameters()
        {
            ASTNode node = new ASTNode(ASTNodeType.Parameters);

            while (!Match(")") && !IsAtEnd())
            {
                Token param = Expect(TokenType.Identifier, "SYN003");

                if (param != null)
                {
                    node.AddChild(new ASTNode(ASTNodeType.Parameter, param.Value));
                }

                if (Match(","))
                {
                    Consume();
                }
                else
                {
                    break;
                }
            }

            return node;
        }

        // Parses return statement
        private ASTNode ParseReturnStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.ReturnStatement);

            Consume(); // return

            if (!Match(";"))
            {
                ASTNode value = ParseExpression();

                if (value != null)
                {
                    node.AddChild(value);
                }
            }

            Expect(";", "SYN002");

            return node;
        }

        // Parses switch statement
        private ASTNode ParseSwitchStatement()
        {
            ASTNode node = new ASTNode(ASTNodeType.SwitchStatement);

            Consume(); // switch

            Expect("(", "SYN001");

            ASTNode expression = ParseExpression();
            if (expression != null)
            {
                node.AddChild(expression);
            }

            Expect(")", "SYN001");
            Expect("{", "SYN001");

            while (!Match("}") && !IsAtEnd())
            {
                if (Match("case"))
                {
                    Consume();

                    ASTNode caseNode = new ASTNode(ASTNodeType.Case);

                    ASTNode caseValue = ParseExpression();
                    if (caseValue != null)
                    {
                        caseNode.AddChild(caseValue);
                    }

                    Expect(":", "SYN003");

                    while (!Match("case") && !Match("default") && !Match("}") && !IsAtEnd())
                    {
                        ASTNode statement = ParseStatement();

                        if (statement != null)
                        {
                            caseNode.AddChild(statement);
                        }
                    }

                    node.AddChild(caseNode);
                }
                else if (Match("default"))
                {
                    Consume();

                    Expect(":", "SYN003");

                    ASTNode defaultNode = new ASTNode(ASTNodeType.Default);

                    while (!Match("}") && !IsAtEnd())
                    {
                        ASTNode statement = ParseStatement();

                        if (statement != null)
                        {
                            defaultNode.AddChild(statement);
                        }
                    }

                    node.AddChild(defaultNode);
                }
                else
                {
                    SyntaxErrorRecovery("SYN003");
                }
            }

            Expect("}", "SYN001");

            return node;
        }

        // Parses a block: { statements }
        private ASTNode ParseBlock()
        {
            ASTNode node = new ASTNode(ASTNodeType.Block);

            Expect("{", "SYN001");

            while (!Match("}") && !IsAtEnd())
            {
                ASTNode statement = ParseStatement();

                if (statement != null)
                {
                    node.AddChild(statement);
                }
                else
                {
                    SyntaxErrorRecovery("SYN003");
                }
            }

            Expect("}", "SYN001");

            return node;
        }

        // Parses expression
        private ASTNode ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private ASTNode ParseComparison()
        {
            ASTNode left = ParseAddition();

            while (Match("==") || Match("!=") || Match("===") || Match("!==") ||
                   Match(">") || Match("<") || Match(">=") || Match("<="))
            {
                string op = Consume().Value;

                ASTNode right = ParseAddition();

                ASTNode node = new ASTNode(ASTNodeType.BinaryExpression, op);
                node.AddChild(left);
                node.AddChild(right);

                left = node;
            }

            return left;
        }

        private ASTNode ParseAssignmentExpression()
        {
            if (CurrentToken() != null &&
                CurrentToken().Type == TokenType.Identifier &&
                PeekValue(1) == "=")
            {
                ASTNode node = new ASTNode(ASTNodeType.Assignment);

                Token name = Expect(TokenType.Identifier, "SYN003");

                if (name != null)
                {
                    node.AddChild(new ASTNode(ASTNodeType.Identifier, name.Value));
                }

                Expect("=", "SYN003");

                ASTNode value = ParseExpression();

                if (value != null)
                {
                    node.AddChild(value);
                }

                return node;
            }

            return ParseComparison();
        }

        private ASTNode ParseAddition()
        {
            ASTNode left = ParseMultiplication();

            while (Match("+") || Match("-"))
            {
                string op = Consume().Value;

                ASTNode right = ParseMultiplication();

                ASTNode node = new ASTNode(ASTNodeType.BinaryExpression, op);
                node.AddChild(left);
                node.AddChild(right);

                left = node;
            }

            return left;
        }

        private ASTNode ParseMultiplication()
        {
            ASTNode left = ParsePrimary();

            while (Match("*") || Match("/") || Match("%"))
            {
                string op = Consume().Value;

                ASTNode right = ParsePrimary();

                ASTNode node = new ASTNode(ASTNodeType.BinaryExpression, op);
                node.AddChild(left);
                node.AddChild(right);

                left = node;
            }

            return left;
        }

        // Parses primary values: number, string, boolean, identifier, parentheses
        private ASTNode ParsePrimary()
        {
            Token current = CurrentToken();

            if (current == null)
            {
                return null;
            }

            if (current.Type == TokenType.NumberLiteral)
            {
                Consume();
                return new ASTNode(ASTNodeType.NumberLiteral, current.Value);
            }

            if (current.Type == TokenType.StringLiteral)
            {
                Consume();
                return new ASTNode(ASTNodeType.StringLiteral, current.Value);
            }

            if (current.Type == TokenType.BooleanLiteral)
            {
                Consume();
                return new ASTNode(ASTNodeType.BooleanLiteral, current.Value);
            }

            if (current.Type == TokenType.NullLiteral)
            {
                Consume();
                return new ASTNode(ASTNodeType.NullLiteral, current.Value);
            }

            if (current.Type == TokenType.Identifier)
            {
                Consume();
                return new ASTNode(ASTNodeType.Identifier, current.Value);
            }

            if (Match("("))
            {
                Consume();

                ASTNode expression = ParseExpression();

                Expect(")", "SYN001");

                return expression;
            }

            SyntaxErrorRecovery("SYN003");

            return null;
        }

        private ASTNode ParseExpressionStatement()
        {
            ASTNode expression = ParseExpression();

            Expect(";", "SYN002");

            return expression;
        }

        // Helper functions

        private Token CurrentToken()
        {
            if (_current < _tokens.Count)
            {
                return _tokens[_current];
            }

            return null;
        }

        private string PeekValue(int offset)
        {
            int index = _current + offset;

            if (index < _tokens.Count)
            {
                return _tokens[index].Value;
            }

            return "";
        }

        private Token Consume()
        {
            Token token = CurrentToken();

            if (!IsAtEnd())
            {
                _current++;
            }

            return token;
        }

        private bool Match(string value)
        {
            Token token = CurrentToken();

            return token != null && token.Value == value;
        }

        private bool Match(TokenType type)
        {
            Token token = CurrentToken();

            return token != null && token.Type == type;
        }

        private bool IsAtEnd()
        {
            Token token = CurrentToken();

            return token == null || token.Type == TokenType.EOF;
        }

        private Token Expect(string value, string errorCode)
        {
            Token token = CurrentToken();

            if (token != null && token.Value == value)
            {
                return Consume();
            }

            AddSyntaxError(errorCode, $"Expected '{value}'");

            return null;
        }

        private Token Expect(TokenType type, string errorCode)
        {
            Token token = CurrentToken();

            if (token != null && token.Type == type)
            {
                return Consume();
            }

            AddSyntaxError(errorCode, $"Expected token type '{type}'");

            return null;
        }

        private void AddSyntaxError(string errorCode, string extraMessage = "")
        {
            Token token = CurrentToken();

            string message = _errorHandler.GetErrorMessage(errorCode);

            if (extraMessage != "")
            {
                message += " - " + extraMessage;
            }

            _errors.Add(new ErrorModel(
                ErrorType.Syntax,
                message,
                token != null ? token.Line : 0,
                token != null ? token.Column : 0,
                ErrorSeverity.Critical
            ));
        }

        // Error recovery: skips tokens until a safe point
        private void SyntaxErrorRecovery(string errorCode)
        {
            AddSyntaxError(errorCode);

            while (!IsAtEnd() && !Match(";") && !Match("}"))
            {
                Consume();
            }

            if (Match(";"))
            {
                Consume();
            }
        }
    }
}
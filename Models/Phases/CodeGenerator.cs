using CompilerProject.Models.DataStructures;
using CompilerProject.Models.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace CompilerProject.Models.Phases
{
    internal class CodeGenerator
    {
        private Dictionary<string, SymbolEntry> _symbolTable;
        private List<ErrorModel> _errors;
        private ErrorHandler _errorHandler;
        private int _indentLevel;

        public CodeGenerator(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
            _symbolTable = new Dictionary<string, SymbolEntry>();
            _errors = new List<ErrorModel>();
            _indentLevel = 0;
        }

        // Main code generation function
        public string Generate(
            ASTNode tree,
            Dictionary<string, SymbolEntry> symbolTable,
            List<ErrorModel> errors)
        {
            _symbolTable = symbolTable;
            _errors = errors;
            _indentLevel = 0;

            return TreeTraversal(tree);
        }

        // Traverses the AST and generates Rust code
        private string TreeTraversal(ASTNode node)
        {
            if (node == null)
            {
                return "";
            }

            switch (node.NodeType)
            {
                case ASTNodeType.Program:
                    return GenerateProgram(node);

                case ASTNodeType.VariableDeclaration:
                    return GenerateVariableDeclaration(node);

                case ASTNodeType.Assignment:
                    return GenerateAssignment(node);

                case ASTNodeType.IfStatement:
                    return GenerateIfStatement(node);

                case ASTNodeType.WhileStatement:
                    return GenerateWhileStatement(node);

                case ASTNodeType.ForStatement:
                    return GenerateForStatement(node);

                case ASTNodeType.DoWhileStatement:
                    return GenerateDoWhileStatement(node);

                case ASTNodeType.FunctionDeclaration:
                    return GenerateFunctionDeclaration(node);

                case ASTNodeType.ReturnStatement:
                    return GenerateReturnStatement(node);

                case ASTNodeType.SwitchStatement:
                    return GenerateSwitchStatement(node);

                case ASTNodeType.Block:
                    return GenerateBlock(node);

                case ASTNodeType.BinaryExpression:
                    return GenerateBinaryExpression(node);

                case ASTNodeType.NumberLiteral:
                case ASTNodeType.StringLiteral:
                case ASTNodeType.BooleanLiteral:
                case ASTNodeType.NullLiteral:
                    return GenerateLiteral(node);

                case ASTNodeType.Identifier:
                    return node.Value;

                default:
                    AddGenerationError("GEN001", $"Unsupported AST node type: {node.NodeType}");

                    string result = "";

                    foreach (ASTNode child in node.Children)
                    {
                        result += TreeTraversal(child);
                    }

                    return result;
            }
        }

        private string GenerateProgram(ASTNode node)
        {
            string code = "fn main() {\n";

            _indentLevel++;

            foreach (ASTNode child in node.Children)
            {
                code += TreeTraversal(child);
            }

            _indentLevel--;

            code += "}\n";

            return code;
        }

        // Translates JS variable declaration into Rust
        private string GenerateVariableDeclaration(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid variable declaration node");
                return "";
            }

            string keyword = node.Children[0].Value;
            string name = node.Children[1].Value;

            // JS → Rust:
            // let   → let mut
            // var   → let mut
            // const → let
            string rustKeyword = keyword == "const" ? "let" : "let mut";

            string line = Indent() + $"{rustKeyword} {name}";

            if (node.Children.Count > 2)
            {
                string value = TreeTraversal(node.Children[2]);
                line += $" = {value}";
            }

            line += ";\n";

            return line;
        }

        // Translates JS assignment into Rust assignment
        private string GenerateAssignment(ASTNode node)
        {
            return Indent() + GenerateAssignmentExpression(node) + ";\n";
        }

        private string GenerateAssignmentExpression(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid assignment node");
                return "";
            }

            string name = node.Children[0].Value;
            string value = TreeTraversal(node.Children[1]);

            return Indent() + $"{name} = {value}";
        }

        // Translates if statement
        private string GenerateIfStatement(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid if statement node");
                return "";
            }

            string condition = TreeTraversal(node.Children[0]);

            string code = Indent() + $"if {condition} ";
            code += TreeTraversal(node.Children[1]);

            if (node.Children.Count > 2)
            {
                ASTNode elseNode = node.Children[2];

                code += Indent() + "else ";

                if (elseNode.NodeType == ASTNodeType.IfStatement)
                {
                    code += TreeTraversal(elseNode).TrimStart();
                }
                else
                {
                    code += TreeTraversal(elseNode).TrimStart();
                }
            }

            return code;
        }

        // Translates while loop
        private string GenerateWhileStatement(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid while statement node");
                return "";
            }

            string condition = TreeTraversal(node.Children[0]);

            string code = Indent() + $"while {condition} ";
            code += TreeTraversal(node.Children[1]);

            return code;
        }

        // Translates JS for loop into Rust while loop
        private string GenerateForStatement(ASTNode node)
        {
            if (node.Children.Count < 4)
            {
                AddGenerationError("GEN001", "Invalid for statement node");
                return "";
            }

            string init = TreeTraversal(node.Children[0]);
            string condition = TreeTraversal(node.Children[1]);
            ASTNode stepNode = node.Children[2];
            ASTNode bodyNode = node.Children[3];

            string code = init;
            code += Indent() + $"while {condition} {{\n";

            _indentLevel++;

            // bodyNode is a Block node, but we do NOT want GenerateBlock()
            // because GenerateBlock() adds another pair of { }.
            foreach (ASTNode statement in bodyNode.Children)
            {
                code += TreeTraversal(statement);
            }

            if (stepNode.NodeType == ASTNodeType.Assignment)
            {
                code += Indent() + GenerateAssignmentExpression(stepNode) + ";\n";
            }
            else
            {
                string step = TreeTraversal(stepNode).Trim();

                if (step.EndsWith(";"))
                {
                    code += Indent() + step + "\n";
                }
                else
                {
                    code += Indent() + step + ";\n";
                }
            }

            _indentLevel--;

            code += Indent() + "}\n";

            return code;
        }

        // Translates do-while loop into Rust loop with break
        private string GenerateDoWhileStatement(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid do-while statement node");
                return "";
            }

            string body = TreeTraversal(node.Children[0]);
            string condition = TreeTraversal(node.Children[1]);

            string code = Indent() + "loop {\n";

            _indentLevel++;

            code += body;
            code += Indent() + $"if !({condition}) {{ break; }}\n";

            _indentLevel--;

            code += Indent() + "}\n";

            return code;
        }

        // Translates function declaration
        private string GenerateFunctionDeclaration(ASTNode node)
        {
            if (node.Children.Count < 3)
            {
                AddGenerationError("GEN001", "Invalid function declaration node");
                return "";
            }

            string name = node.Children[0].Value;
            ASTNode parametersNode = node.Children[1];

            string parametersList = string.Join(
                ", ",
                parametersNode.Children.Select(parameter => parameter.Value + ": i64")
            );

            string code = Indent() + $"fn {name}({parametersList}) {{\n";

            _indentLevel++;

            code += TreeTraversal(node.Children[2]);

            _indentLevel--;

            code += Indent() + "}\n";

            return code;
        }

        // Translates return statement
        private string GenerateReturnStatement(ASTNode node)
        {
            if (node.Children.Count == 0)
            {
                return Indent() + "return;\n";
            }

            string value = TreeTraversal(node.Children[0]);

            return Indent() + $"return {value};\n";
        }

        // Translates JS switch into Rust match
        private string GenerateSwitchStatement(ASTNode node)
        {
            if (node.Children.Count < 1)
            {
                AddGenerationError("GEN001", "Invalid switch statement node");
                return "";
            }

            string expression = TreeTraversal(node.Children[0]);

            string code = Indent() + $"match {expression} {{\n";

            _indentLevel++;

            for (int i = 1; i < node.Children.Count; i++)
            {
                ASTNode child = node.Children[i];

                if (child.NodeType == ASTNodeType.Case)
                {
                    if (child.Children.Count < 1)
                    {
                        AddGenerationError("GEN001", "Invalid case node");
                        continue;
                    }

                    string caseValue = TreeTraversal(child.Children[0]);

                    code += Indent() + $"{caseValue} => {{\n";

                    _indentLevel++;

                    for (int j = 1; j < child.Children.Count; j++)
                    {
                        code += TreeTraversal(child.Children[j]);
                    }

                    _indentLevel--;

                    code += Indent() + "}\n";
                }
                else if (child.NodeType == ASTNodeType.Default)
                {
                    code += Indent() + "_ => {\n";

                    _indentLevel++;

                    foreach (ASTNode statement in child.Children)
                    {
                        code += TreeTraversal(statement);
                    }

                    _indentLevel--;

                    code += Indent() + "}\n";
                }
            }

            _indentLevel--;

            code += Indent() + "}\n";

            return code;
        }

        // Translates block
        private string GenerateBlock(ASTNode node)
        {
            string code = "{\n";

            _indentLevel++;

            foreach (ASTNode child in node.Children)
            {
                code += TreeTraversal(child);
            }

            _indentLevel--;

            code += Indent() + "}\n";

            return code;
        }

        // Translates binary expression
        private string GenerateBinaryExpression(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddGenerationError("GEN001", "Invalid binary expression node");
                return "";
            }

            string left = TreeTraversal(node.Children[0]);
            string right = TreeTraversal(node.Children[1]);
            string op = TranslateOperator(node.Value);

            if (node.Value == "**")
            {
                return $"{left}.pow({right})";
            }

            return $"({left} {op} {right})";
        }

        // Translates literals
        private string GenerateLiteral(ASTNode node)
        {
            if (node.NodeType == ASTNodeType.StringLiteral)
            {
                return node.Value;
            }

            if (node.NodeType == ASTNodeType.BooleanLiteral)
            {
                return node.Value.ToLower();
            }

            if (node.NodeType == ASTNodeType.NullLiteral)
            {
                return "None";
            }

            return node.Value;
        }

        // Translates JS operators into Rust operators
        private string TranslateOperator(string jsOperator)
        {
            Dictionary<string, string> operatorMap = new Dictionary<string, string>
            {
                { "===", "==" },
                { "!==", "!=" },
                { "==",  "==" },
                { "!=",  "!=" },
                { "&&",  "&&" },
                { "||",  "||" },
                { ">>>", ">>" }
            };

            if (operatorMap.ContainsKey(jsOperator))
            {
                return operatorMap[jsOperator];
            }

            return jsOperator;
        }

        private void AddGenerationError(string errorCode, string extraMessage)
        {
            string message = _errorHandler.GetErrorMessage(errorCode);

            if (extraMessage != "")
            {
                message += " - " + extraMessage;
            }

            _errors.Add(new ErrorModel(
                ErrorType.CodeGeneration,
                message,
                0,
                0,
                ErrorSeverity.Critical
            ));
        }

        private string Indent()
        {
            return new string(' ', _indentLevel * 4);
        }
    }
}
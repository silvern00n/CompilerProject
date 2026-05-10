using CompilerProject.Models.DataStructures;
using CompilerProject.Models.Helpers;
using System.Collections.Generic;

namespace CompilerProject.Models.Phases
{
    internal class SemanticAnalyzer
    {
        private Dictionary<string, SymbolEntry> _symbolTable;
        private List<ErrorModel> _errors;
        private ErrorHandler _errorHandler;
        private int _scopeLevel;

        public SemanticAnalyzer(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
            _symbolTable = new Dictionary<string, SymbolEntry>();
            _errors = new List<ErrorModel>();
            _scopeLevel = 0;
        }

        // Main semantic analysis function
        public (ASTNode enrichedTree, Dictionary<string, SymbolEntry> symbolTable)
            Analyze(ASTNode tree, List<ErrorModel> errors)
        {
            _errors = errors;
            _symbolTable = new Dictionary<string, SymbolEntry>();
            _scopeLevel = 0;

            TreeTraversal(tree);

            return (tree, _symbolTable);
        }

        // DFS traversal over the AST
        private void TreeTraversal(ASTNode node)
        {
            if (node == null)
            {
                return;
            }

            switch (node.NodeType)
            {
                case ASTNodeType.Program:
                    foreach (ASTNode child in node.Children)
                    {
                        TreeTraversal(child);
                    }
                    break;

                case ASTNodeType.Block:
                    EnterScope();

                    foreach (ASTNode child in node.Children)
                    {
                        TreeTraversal(child);
                    }

                    ExitScope();
                    break;

                case ASTNodeType.VariableDeclaration:
                    HandleVariableDeclaration(node);
                    break;

                case ASTNodeType.FunctionDeclaration:
                    HandleFunctionDeclaration(node);
                    break;

                case ASTNodeType.Identifier:
                    HandleIdentifierUsage(node);
                    break;

                case ASTNodeType.BinaryExpression:
                    HandleBinaryExpression(node);
                    break;

                case ASTNodeType.NumberLiteral:
                    node.InferredType = "number";
                    break;

                case ASTNodeType.StringLiteral:
                    node.InferredType = "string";
                    break;

                case ASTNodeType.BooleanLiteral:
                    node.InferredType = "bool";
                    break;

                case ASTNodeType.NullLiteral:
                    node.InferredType = "null";
                    break;

                default:
                    foreach (ASTNode child in node.Children)
                    {
                        TreeTraversal(child);
                    }
                    break;
            }
        }

        // Handles variable declarations: let / const / var
        private void HandleVariableDeclaration(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                AddSemanticError("SEM004", "Invalid variable declaration");
                return;
            }

            string keyword = node.Children[0].Value; // let / const / var
            string name = node.Children[1].Value;    // variable name

            string key = BuildSymbolKey(name, _scopeLevel);

            // Checks if the variable already exists in the current scope
            if (_symbolTable.ContainsKey(key))
            {
                AddSemanticError(
                    "SEM004",
                    $"Variable '{name}' is already declared in the current scope"
                );

                return;
            }

            SymbolKind kind = keyword == "const"
                ? SymbolKind.Constant
                : SymbolKind.Variable;

            string inferredType = "unknown";
            bool isInitialized = false;

            // If there is an initial value, analyze it and infer its type
            if (node.Children.Count > 2)
            {
                ASTNode valueNode = node.Children[2];

                TreeTraversal(valueNode);

                inferredType = valueNode.InferredType;
                isInitialized = true;
            }

            SymbolEntry entry = new SymbolEntry(
                name,
                inferredType,
                kind,
                _scopeLevel
            );

            entry.IsInitialized = isInitialized;

            SymbolTableManager(key, entry);

            node.InferredType = inferredType;
        }

        // Handles function declarations
        private void HandleFunctionDeclaration(ASTNode node)
        {
            if (node.Children.Count < 3)
            {
                AddSemanticError("SEM003", "Invalid function declaration");
                return;
            }

            string name = node.Children[0].Value;
            string key = BuildSymbolKey(name, _scopeLevel);

            if (_symbolTable.ContainsKey(key))
            {
                AddSemanticError(
                    "SEM004",
                    $"Function '{name}' is already declared in the current scope"
                );

                return;
            }

            SymbolEntry functionEntry = new SymbolEntry(
                name,
                "function",
                SymbolKind.Function,
                _scopeLevel
            );

            ASTNode parametersNode = node.Children[1];

            foreach (ASTNode parameter in parametersNode.Children)
            {
                functionEntry.ParameterTypes.Add("unknown");
            }

            SymbolTableManager(key, functionEntry);

            EnterScope();

            // Adds function parameters to the function scope
            foreach (ASTNode parameter in parametersNode.Children)
            {
                string paramKey = BuildSymbolKey(parameter.Value, _scopeLevel);

                SymbolEntry parameterEntry = new SymbolEntry(
                    parameter.Value,
                    "unknown",
                    SymbolKind.Parameter,
                    _scopeLevel
                );

                parameterEntry.IsInitialized = true;

                SymbolTableManager(paramKey, parameterEntry);
            }

            // Analyze function body
            TreeTraversal(node.Children[2]);

            ExitScope();

            node.InferredType = "function";
        }

        // Checks that an identifier was declared before use
        private void HandleIdentifierUsage(ASTNode node)
        {
            string name = node.Value;

            SymbolEntry entry = LookupSymbol(name);

            if (entry == null)
            {
                AddSemanticError(
                    "SEM001",
                    $"Variable '{name}' was not declared"
                );

                node.InferredType = "unknown";
                return;
            }

            node.InferredType = entry.DataType;
        }

        // Checks binary expression types
        private void HandleBinaryExpression(ASTNode node)
        {
            if (node.Children.Count < 2)
            {
                node.InferredType = "unknown";
                return;
            }

            TreeTraversal(node.Children[0]);
            TreeTraversal(node.Children[1]);

            string leftType = node.Children[0].InferredType;
            string rightType = node.Children[1].InferredType;

            node.InferredType = TypeChecker(leftType, rightType, node.Value);
        }

        // Checks type compatibility and returns the final expression type
        private string TypeChecker(string leftType, string rightType, string op)
        {
            if (leftType == "unknown" || rightType == "unknown")
            {
                return "unknown";
            }

            if (leftType == rightType)
            {
                if (op == "==" || op == "!=" || op == "===" || op == "!==" ||
                    op == ">" || op == "<" || op == ">=" || op == "<=")
                {
                    return "bool";
                }

                return leftType;
            }

            // JavaScript allows string concatenation with other types
            if (op == "+" && (leftType == "string" || rightType == "string"))
            {
                return "string";
            }

            AddSemanticWarning(
                "SEM002",
                $"Type mismatch between '{leftType}' and '{rightType}'"
            );

            return "unknown";
        }

        // Adds or updates an entry in the symbol table
        private void SymbolTableManager(string key, SymbolEntry entry)
        {
            _symbolTable[key] = entry;
        }

        // Searches for an identifier from the current scope outward
        private SymbolEntry LookupSymbol(string name)
        {
            for (int scope = _scopeLevel; scope >= 0; scope--)
            {
                string key = BuildSymbolKey(name, scope);

                if (_symbolTable.ContainsKey(key))
                {
                    return _symbolTable[key];
                }
            }

            return null;
        }

        private string BuildSymbolKey(string name, int scopeLevel)
        {
            return name + "_" + scopeLevel;
        }

        private void EnterScope()
        {
            _scopeLevel++;
        }

        private void ExitScope()
        {
            if (_scopeLevel > 0)
            {
                _scopeLevel--;
            }
        }

        private void AddSemanticError(string errorCode, string extraMessage)
        {
            string message = _errorHandler.GetErrorMessage(errorCode);

            if (extraMessage != "")
            {
                message += " - " + extraMessage;
            }

            _errors.Add(new ErrorModel(
                ErrorType.Semantic,
                message,
                0,
                0,
                ErrorSeverity.Critical
            ));
        }

        private void AddSemanticWarning(string errorCode, string extraMessage)
        {
            string message = _errorHandler.GetErrorMessage(errorCode);

            if (extraMessage != "")
            {
                message += " - " + extraMessage;
            }

            _errors.Add(new ErrorModel(
                ErrorType.Semantic,
                message,
                0,
                0,
                ErrorSeverity.Warning
            ));
        }
    }
}
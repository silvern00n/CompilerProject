using CompilerProject.Models.DataStructures;
using CompilerProject.Models.Helpers;
using CompilerProject.Models.Phases;
using System.Collections.Generic;

namespace CompilerProject.Models
{
    internal class CompilerModel
    {
        // Helper objects
        private DFATransitionMatrix _dfa;
        private TokenDictionary _tokenDict;
        private FileManager _fileManager;
        private ErrorHandler _errorHandler;

        // Compiler phases
        private LexicalAnalyzer _lexicalAnalyzer;
        private SyntaxAnalyzer _syntaxAnalyzer;
        private SemanticAnalyzer _semanticAnalyzer;
        private CodeGenerator _codeGenerator;

        // Main data structures
        private List<Token> _tokens;
        private List<ErrorModel> _errors;
        private ASTNode _syntaxTree;
        private ASTNode _enrichedTree;
        private Dictionary<string, SymbolEntry> _symbolTable;
        private string _outputCode;
        private string _inputFilePath;
        private string _outputDirectory;

        public CompilerModel()
        {
            _dfa = new DFATransitionMatrix();
            _tokenDict = new TokenDictionary();
            _fileManager = new FileManager();
            _errorHandler = new ErrorHandler();

            _lexicalAnalyzer = new LexicalAnalyzer(_dfa, _tokenDict, _errorHandler);
            _syntaxAnalyzer = new SyntaxAnalyzer(_errorHandler);
            _semanticAnalyzer = new SemanticAnalyzer(_errorHandler);
            _codeGenerator = new CodeGenerator(_errorHandler);

            _tokens = new List<Token>();
            _errors = new List<ErrorModel>();
            _symbolTable = new Dictionary<string, SymbolEntry>();

            _syntaxTree = null;
            _enrichedTree = null;

            _outputCode = "";
            _inputFilePath = "";
            _outputDirectory = "";
        }

        // Main compiler algorithm
        public int RunCompiler(string inputFilePath, string outputDirectory)
        {
            _inputFilePath = inputFilePath;
            _outputDirectory = outputDirectory; 
            _errors.Clear();
            _tokens.Clear();
            _symbolTable.Clear();
            _outputCode = "";


            // Stage 1 — check file extension
            if (!_fileManager.IsJavaScriptFile(inputFilePath))
            {
                _errors.Add(new ErrorModel(
                    ErrorType.File,
                    "Unsupported file type. Please choose a JavaScript file with a .js extension.",
                    0,
                    0,
                    ErrorSeverity.Critical
                ));

                return 1;
            }

            // Stage 2 — read source file
            string sourceCode = _fileManager.ReadSourceFile(inputFilePath);

            if (string.IsNullOrEmpty(sourceCode))
            {
                _errors.Add(new ErrorModel(
                    ErrorType.File,
                    "File was not found or is empty.",
                    0,
                    0,
                    ErrorSeverity.Critical
                ));

                return 1;
            }

            // Stage 3 — lexical analysis
            _tokens = _lexicalAnalyzer.Analyze(sourceCode, _errors);

            if (_errorHandler.CheckErrors(_errors) == 1)
            {
                return 1;
            }

            // Stage 4 — syntax analysis
            _syntaxTree = _syntaxAnalyzer.Analyze(_tokens, _errors);

            if (_errorHandler.CheckErrors(_errors) == 1)
            {
                return 1;
            }

            // Stage 5 — semantic analysis
            (_enrichedTree, _symbolTable) =
                _semanticAnalyzer.Analyze(_syntaxTree, _errors);

            if (_errorHandler.CheckErrors(_errors) == 1)
            {
                return 1;
            }

            // Stage 6 — Rust code generation
            _outputCode = _codeGenerator.Generate(
                _enrichedTree,
                _symbolTable,
                _errors
            );

            if (_errorHandler.CheckErrors(_errors) == 1)
            {
                return 1;
            }

            // Stage 7 — write output file
            bool written = _fileManager.WriteOutputFile(_outputCode, inputFilePath, outputDirectory);

            if (!written)
            {
                _errors.Add(new ErrorModel(
                    ErrorType.CodeGeneration,
                    "Error while writing the output file.",
                    0,
                    0,
                    ErrorSeverity.Critical
                ));

                return 1;
            }

            return 0;
        }

        // Getters for the Controller
        public List<ErrorModel> GetErrors()
        {
            return _errors;
        }

        public string GetOutputCode()
        {
            return _outputCode;
        }

        public string GetOutputPath()
        {
            return _fileManager.GetOutputPath(_inputFilePath, _outputDirectory);
        }

        public List<Token> GetTokens()
        {
            return _tokens;
        }

        public ASTNode GetSyntaxTree()
        {
            return _syntaxTree;
        }

        public ASTNode GetEnrichedTree()
        {
            return _enrichedTree;
        }

        public Dictionary<string, SymbolEntry> GetSymbolTable()
        {
            return _symbolTable;
        }
        public void PrintBothFiles(string inputFilePath, string outputDirectory)
        {
            _fileManager.PrintBothFiles(inputFilePath, outputDirectory);
        }
    }
}
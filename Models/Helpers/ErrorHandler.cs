using CompilerProject.Models.DataStructures;
using System;
using System.Collections.Generic;

namespace CompilerProject.Models.Helpers
{
    internal class ErrorHandler
    {
        // Error dictionary — maps an error code to a clear error message
        private Dictionary<string, string> _errorMessages;
        private int _lastCheckedErrorIndex = 0;

        public ErrorHandler()
        {
            _errorMessages = new Dictionary<string, string>();
            InitializeErrorMessages();
        }

        private void InitializeErrorMessages()
        {
            // Lexical errors
            _errorMessages["LEX001"] = "Illegal character";
            _errorMessages["LEX002"] = "Unclosed string literal";
            _errorMessages["LEX003"] = "Invalid number format";
            _errorMessages["LEX004"] = "Unclosed multi-line comment";

            // Syntax errors
            _errorMessages["SYN001"] = "Unclosed parentheses";
            _errorMessages["SYN002"] = "Missing semicolon";
            _errorMessages["SYN003"] = "Invalid syntax structure";
            _errorMessages["SYN004"] = "Unexpected token";

            // Semantic errors
            _errorMessages["SEM001"] = "Variable was not declared";
            _errorMessages["SEM002"] = "Type mismatch";
            _errorMessages["SEM003"] = "Function was not declared";
            _errorMessages["SEM004"] = "Variable is already declared in the current scope";

            // Code generation errors
            _errorMessages["GEN001"] = "Cannot generate code for this node";
            _errorMessages["GEN002"] = "Error while generating Rust code";
        }

        // Returns the error message that matches the given error code
        public string GetErrorMessage(string errorCode)
        {
            if (_errorMessages.ContainsKey(errorCode))
            {
                return _errorMessages[errorCode];
            }

            return "Unknown error";
        }

        // Returns 1 if there is a critical error, otherwise returns 0
        public int CheckErrors(List<ErrorModel> errors)
        {
            for (int i = _lastCheckedErrorIndex; i < errors.Count; i++)
            {
                if (errors[i].Severity == ErrorSeverity.Critical)
                {
                    _lastCheckedErrorIndex = errors.Count;
                    return 1;
                }
            }

            _lastCheckedErrorIndex = errors.Count;
            return 0;
        }

        public void ResetErrorCheck()
        {
            _lastCheckedErrorIndex = 0;
        }

        public void PrintErrors(List<ErrorModel> errors)
        {
            foreach (ErrorModel error in errors)
            {
                Console.WriteLine(error.ToString());
            }
        }
    }
}
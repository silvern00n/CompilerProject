using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal class ErrorModel
    {
        public ErrorType Type { get; private set; }
        public string Message { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public ErrorSeverity Severity { get; private set; }

        public ErrorModel(ErrorType type, string message, int line, int column, ErrorSeverity severity)
        {
            Type = type;
            Message = message;
            Line = line;
            Column = column;
            Severity = severity;
        }

        public override string ToString()
        {
            return $"[{Severity}][{Type}] {Message} (Line {Line}, Column {Column})";
        }
    }
}
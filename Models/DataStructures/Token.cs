using CompilerProject.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal class Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"[{Type}] '{Value}' (Line {Line}, Column {Column})";
        }
    }
}
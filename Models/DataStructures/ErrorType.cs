using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal enum ErrorType
    {
        File,
        Lexical,
        Syntax,
        Semantic,
        CodeGeneration,
        Unknown
    }
}

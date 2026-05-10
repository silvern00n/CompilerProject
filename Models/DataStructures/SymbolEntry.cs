using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal class SymbolEntry
    {
        public string Name { get; private set; }
        public string DataType { get; set; }
        public SymbolKind Kind { get; private set; }
        public int ScopeLevel { get; private set; }
        public bool IsInitialized { get; set; }

        // Relevant mainly for functions
        public List<string> ParameterTypes { get; private set; }
        public string ReturnType { get; set; }

        public SymbolEntry(string name, string dataType, SymbolKind kind, int scopeLevel)
        {
            Name = name;
            DataType = dataType;
            Kind = kind;
            ScopeLevel = scopeLevel;
            IsInitialized = false;
            ParameterTypes = new List<string>();
            ReturnType = "";
        }

        public override string ToString()
        {
            return $"[{Kind}] {Name} : {DataType} (scope {ScopeLevel})";
        }
    }
}
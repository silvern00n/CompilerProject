using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal class ASTNode
    {
        public ASTNodeType NodeType { get; private set; }
        public string Value { get; set; }
        public List<ASTNode> Children { get; private set; }

        // Added during semantic analysis
        public string InferredType { get; set; }

        public ASTNode(ASTNodeType nodeType, string value = "")
        {
            NodeType = nodeType;
            Value = value;
            Children = new List<ASTNode>();
            InferredType = "";
        }

        public void AddChild(ASTNode child)
        {
            Children.Add(child);
        }

        public override string ToString()
        {
            return $"[{NodeType}] '{Value}' (InferredType: {InferredType})";
        }
    }
}
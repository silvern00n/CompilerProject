using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal enum ASTNodeType
    {
        Program,
        Block,
        VariableDeclaration,
        Assignment,
        Keyword,
        Identifier,
        NumberLiteral,
        StringLiteral,
        BooleanLiteral,
        NullLiteral,
        BinaryExpression,
        UnaryExpression,
        IfStatement,
        ElseStatement,
        WhileStatement,
        ForStatement,
        DoWhileStatement,
        FunctionDeclaration,
        FunctionCall,
        Parameters,
        Parameter,
        ReturnStatement,
        SwitchStatement,
        Case,
        Default,
        Unknown
    }
}
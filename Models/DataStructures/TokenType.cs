using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.DataStructures
{
    internal enum TokenType
    {
        // Special tokens
        EOF,
        Unknown,

        // Names and keywords
        Identifier,
        Keyword,

        // Literals
        NumberLiteral,
        StringLiteral,
        BooleanLiteral,
        NullLiteral,

        // Operators
        AssignmentOperator,     // =
        ArithmeticOperator,     // + - * / %
        ComparisonOperator,     // == === != !== < > <= >=
        LogicalOperator,        // && || !
        IncrementOperator,      // ++ --
        BitwiseOperator,        // & | ^ ~ << >> >>>

        // Separators / punctuation
        Semicolon,              // ;
        Comma,                  // ,
        Dot,                    // .
        Colon,                  // :
        QuestionMark,           // ?

        // Brackets
        LeftParenthesis,        // (
        RightParenthesis,       // )
        LeftBrace,              // {
        RightBrace,             // }
        LeftBracket,            // [
        RightBracket            // ]
    }
}

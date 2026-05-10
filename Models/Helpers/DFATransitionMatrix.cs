using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerProject.Models.Helpers
{
    internal class DFATransitionMatrix
    {
        private int[,] _matrix;
        private int _numStates;
        private int _numCategories;

        // DFA states
        public const int STATE_START = 0;
        public const int STATE_IDENTIFIER = 1;
        public const int STATE_NUMBER = 2;
        public const int STATE_OPERATOR = 3;
        public const int STATE_PUNCTUATION = 4;
        public const int STATE_ERROR = -1;

        // Character categories
        public const int CAT_LETTER = 0;
        public const int CAT_DIGIT = 1;
        public const int CAT_OPERATOR = 2;
        public const int CAT_PUNCTUATION = 3;
        public const int CAT_WHITESPACE = 4;
        public const int CAT_OTHER = 5;

        public DFATransitionMatrix()
        {
            _numStates = 5;
            _numCategories = 6;
            _matrix = new int[_numStates, _numCategories];

            InitializeMatrix();
        }

        private void InitializeMatrix()
        {
            // Default: error
            for (int i = 0; i < _numStates; i++)
            {
                for (int j = 0; j < _numCategories; j++)
                {
                    _matrix[i, j] = STATE_ERROR;
                }
            }

            // From START state
            _matrix[STATE_START, CAT_LETTER] = STATE_IDENTIFIER;
            _matrix[STATE_START, CAT_DIGIT] = STATE_NUMBER;
            _matrix[STATE_START, CAT_OPERATOR] = STATE_OPERATOR;
            _matrix[STATE_START, CAT_PUNCTUATION] = STATE_PUNCTUATION;
            _matrix[STATE_START, CAT_WHITESPACE] = STATE_START;

            // Identifier: letters and digits continue the identifier
            _matrix[STATE_IDENTIFIER, CAT_LETTER] = STATE_IDENTIFIER;
            _matrix[STATE_IDENTIFIER, CAT_DIGIT] = STATE_IDENTIFIER;

            // Number: digits continue the number
            _matrix[STATE_NUMBER, CAT_DIGIT] = STATE_NUMBER;

            // Operator: another operator may continue it
            // Examples: ==, ===, !=, !==, <=, >=, &&, ||, ++, --
            _matrix[STATE_OPERATOR, CAT_OPERATOR] = STATE_OPERATOR;
        }

        public int GetNextState(int currentState, int charCategory)
        {
            if (currentState < 0 || currentState >= _numStates)
            {
                return STATE_ERROR;
            }

            if (charCategory < 0 || charCategory >= _numCategories)
            {
                return STATE_ERROR;
            }

            return _matrix[currentState, charCategory];
        }

        public int GetCharCategory(char c)
        {
            if (char.IsLetter(c) || c == '_' || c == '$')
            {
                return CAT_LETTER;
            }

            if (char.IsDigit(c))
            {
                return CAT_DIGIT;
            }

            if ("+-*/%=<>!&|^~".Contains(c))
            {
                return CAT_OPERATOR;
            }

            if ("(){}[];,.:?".Contains(c))
            {
                return CAT_PUNCTUATION;
            }

            if (char.IsWhiteSpace(c))
            {
                return CAT_WHITESPACE;
            }

            return CAT_OTHER;
        }

        public bool IsAcceptingState(int state)
        {
            return state == STATE_IDENTIFIER ||
                   state == STATE_NUMBER ||
                   state == STATE_OPERATOR ||
                   state == STATE_PUNCTUATION;
        }
    }
}
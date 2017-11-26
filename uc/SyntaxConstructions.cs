﻿using System.Collections.Generic;

namespace Translator
{
    public interface IExpression
    {
    }

    public class Expression : IExpression
    {
        public SourcePosition SourcePosition;
        public List<Token> Tokens;

        public Expression(List<Token> resultingExpression)
        {
            this.Tokens = resultingExpression;
        }
    }

    public class CodeBlock : IExpression
    {
    }

    public class Node
	{
        public Token Token;

		public Node Left;
		public Node Right;

        public Node(Token token)
        {
            Token = token;
        }
    }
}

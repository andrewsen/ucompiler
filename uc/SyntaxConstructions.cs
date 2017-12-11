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
        public List<IExpression> Children = new List<IExpression>();
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

    public class If : IExpression
    {
        public Node Condition;
        public CodeBlock Block;
    }

    public class Statement : IExpression
    {
        public Node Root;

        public Statement(Node root)
        {
            Root = root;
            if (Root != null && (!Root.Token.IsOp() || Root.Token.Operation.Type != OperationType.Assign))
                InfoProvider.AddError("Assignment expected", ExceptionType.IllegalType, new SourcePosition("", Root.Token.Line, Root.Token.Position, "<stdin>"));
        }
    }
}

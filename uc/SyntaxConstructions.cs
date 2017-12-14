using System;
using System.Collections.Generic;
using System.Linq;

namespace Translator
{
    public interface IExpression
    {
    }

    public interface INamedDataContainer
    {
        bool HasDeclaration(Token variable);

        bool HasDeclarationRecursively(Token variable);

        INamedDataElement FindDeclaration(Token token);

        INamedDataElement FindDeclarationRecursively(Token token);

        INamedDataElement findDeclarationClosure(Token token);
    }

    public class Expression : IExpression
    {
        public SourcePosition SourcePosition;
        public Node ExpressionRoot;

        public Expression(Node rootNode, SourcePosition position)
        {
            ExpressionRoot = rootNode;
            SourcePosition = position;
        }
    }

    public class CodeBlock : IExpression, INamedDataContainer
    {
        public ClassType ClassContext;
        public Method MethodContext;
        public INamedDataContainer Parent;
        public List<Variable> Locals;
        public List<IExpression> Expressions;

        public CodeBlock()
        {
            Locals = new List<Variable>();
            Expressions = new List<IExpression>();
        }

        public CodeBlock(CodeBlock parent)
            : this()
        {
            Parent = parent;
            ClassContext = parent.ClassContext;
            MethodContext = parent.MethodContext;
        }

        public CodeBlock(INamedDataContainer parent, ClassType context, Method methodContext)
            : this()
        {
            Parent = parent;
            ClassContext = context;
            MethodContext = methodContext;
        }

        /// <summary>
        /// Checks if variable is declared in this block
        /// </summary>
        /// <returns><c>true</c>, if local was declared here, <c>false</c> otherwise.</returns>
        /// <param name="variable">Variable to be checked</param>
        public bool HasDeclaration(Token variable)
        {
            return Locals.Exists(loc => loc.Name == variable.Representation);
        }

        public bool HasDeclarationRecursively(Token variable)
        {
            return HasDeclaration(variable) || (Parent != null && Parent.HasDeclarationRecursively(variable));
        }

        public INamedDataElement FindDeclaration(Token token)
        {
            return Locals.Find(loc => loc.Name == token.Representation);
        }

        public INamedDataElement FindDeclarationRecursively(Token token)
        {
            return FindDeclaration(token) ?? Parent?.findDeclarationClosure(token) ?? MethodContext?.FindParameter(token);
        }

        public INamedDataElement findDeclarationClosure(Token token)
        {
            return FindDeclaration(token) ?? Parent?.findDeclarationClosure(token);
        }
    }

    public class Node
	{
        public Token Token;

        public IType Type;

        public INamedDataElement RelatedNamedData;

        public List<Node> Children;

        public Node Left 
        {
            get
            {
                return Children.Count > 0 ? Children.Last() : null;
            }
            set
            {
                if (Children.Count > 0)
                    Children[Children.Count - 1] = value;
                else
                    Children.Add(value);
            }
        }

        public Node Right 
        {
            get
            {
                return Children.Count > 1 ? Children[Children.Count - 2] : null;
            }
            set
            {
                if (Children.Count > 1)
                    Children[Children.Count - 2] = value;
                else if(Children.Count == 1)
                    Children.Insert(0, value);
                else
                {
                    Children.Add(value);
                    Children.Add(null);
                }
            }
        }

        public Node(Token token)
        {
            Token = token;
            Children = new List<Node>();
        }

        public void AddRight(Node node)
        {
            Children.Add(node);
        }
    }
}

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
            return MethodContext.HasParameter(variable) || HasDeclarationRecursively(variable);
        }

        public bool HasDeclarationRecursively(Token variable)
        {
            return HasDeclarationLocal(variable) || (Parent != null && Parent.HasDeclarationRecursively(variable));
        }

        public bool HasDeclarationLocal(Token token)
        {
            return Locals.Exists(loc => loc.Name == token.Representation);
        }

        public INamedDataElement FindDeclaration(Token token)
        {
            return FindDeclarationLocal(token) ?? Parent?.FindDeclarationRecursively(token) ?? MethodContext?.FindParameter(token);
        }

        public INamedDataElement FindDeclarationLocal(Token token)
        {
            return Locals.Find(loc => loc.Name == token.Representation);
        }

        public INamedDataElement FindDeclarationRecursively(Token token)
        {
            return FindDeclarationLocal(token) ?? Parent?.FindDeclarationRecursively(token);
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

    public class If : IExpression
    {
        public ConditionalPart MasterIf;
        public List<ConditionalPart> ElseIfList;
        public IExpression ElsePart;

        public CodeBlock Parent;

        public If(CodeBlock parent)
        {
            Parent = parent;
            ElseIfList = new List<ConditionalPart>();
        }
    }

    public class ConditionalPart
    {
        public Expression Condition;
        public IExpression Body;
    }

    public class Return : IExpression
    {
        public CodeBlock Parent;
        public Expression Expression;

        public Return(CodeBlock parent)
        {
            Parent = parent;
        }
    }

    public class For : IExpression
    {
        public CodeBlock Parent;
        public CodeBlock Scope;

        public Expression Condition;
        public Expression Iteration;

        public IExpression Body;
        public IExpression ElsePart;

        public For(CodeBlock parent)
        {
            Parent = parent;
            Scope = new CodeBlock(parent);
        }
    }

    public class While : IExpression
    {
        public CodeBlock Parent;

        public Expression Condition;

        public IExpression Body;
        public IExpression ElsePart;

        public While(CodeBlock parent)
        {
            Parent = parent;
        }
    }

    public class DoWhile : IExpression
    {
        public CodeBlock Parent;

        public Expression Condition;

        public IExpression Body;

        public DoWhile(CodeBlock parent)
        {
            Parent = parent;
        }
    }
}

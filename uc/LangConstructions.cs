using System.Collections.Generic;


namespace Translator
{
    public class CommonClassEntry
    {
        public Scope Scope;
        public IType Type;
        public ClassEntryModifiers Modifiers;
        public string Name;
        public SourcePosition DeclarationPosition;

        public bool HasVoidType => Type.Type == DataTypes.Void;
    }

    public interface IClassElement
    {
        string Name
        {
            get;
            set;
        }

        IType Type
        {
            get;
            set;
        }

        SourcePosition DeclarationPosition
        {
            get;
            set;
        }
    }

    public class Variable
    {
        public string Name;
        public IType Type;
    }

    public class ParameterList : List<Variable>
    {
        
    }

    public class Field : IClassElement
    {
        public Scope Scope;
        public int InitialExpressionPosition;

        public string Name
        {
            get;
            set;
        }

        public IType Type
        {
            get;
            set;
        }

        public SourcePosition DeclarationPosition
        {
            get;
            set;
        }

        public void FromClassEntry(CommonClassEntry entry)
        {
            this.Name = entry.Name;
            this.Type = entry.Type;
            this.Scope = entry.Scope;
            this.DeclarationPosition = entry.DeclarationPosition;
        }
    }

    public class Property : Field
    {
        public Method Getter;
        public Method Setter;
    }

    public class Method : Field
    {
        public ParameterList Parameters;
        public CodeBlock Body;
        public int Begin;
    }
}

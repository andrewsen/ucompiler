using System.Collections.Generic;


namespace Translator
{
    public class CommonClassEntry
    {
        public AttributeList AttributeList;
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
        public override string ToString()
        {
            var result = "";
            foreach (var variable in this)
            {
                result += variable.Type + " " + variable.Name + ", ";
            }
            return result.TrimEnd(',', ' ');
        }
    }

    public class Field : IClassElement
    {
        public Scope Scope;
		public AttributeList AttributeList;
		public ClassEntryModifiers Modifiers;
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
            this.AttributeList = entry.AttributeList;
			this.Name = entry.Name;
			this.Type = entry.Type;
            this.Modifiers = entry.Modifiers;
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

﻿using System;
using System.Collections.Generic;
using uc;

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
        public ClassType Class;

        public bool HasVoidType => Type.Type == DataTypes.Void;
    }

    public interface INamedDataElement
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

    public interface IClassElement : INamedDataElement
    {
        
    }

    public class Variable : INamedDataElement
    {
        public bool IsAssigned;
        public bool IsUsed;

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

        public Variable()
        {
            IsAssigned = false;
            IsUsed = false;
        }

        public static bool operator!=(Variable v1, Variable v2) => !(v1 == v2);

        public static bool operator==(Variable v1, Variable v2)
        {
            if (v1.Name != v2.Name)
                return false;
            return v1.Type.Equals(v2.Type);
        }
    }

    public class Parameter : Variable
    {
        public override string ToString()
        {
            return $"{Type.ToString()} {Name}";
        }
    }

    public class ParameterList : List<Parameter>
    {
        public override string ToString()
        {
            var result = string.Join(", ", this);
            return result;
        }
    }

    public class Field : IClassElement
    {
        public Scope Scope;
		public AttributeList AttributeList;
		public ClassEntryModifiers Modifiers;
        public ClassType Class;
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
            this.Class = entry.Class;
        }
    }

    public class Property : Field
    {
        public Method Getter;
        public Method Setter;
    }

    public class Method : Field
    {
        private int localCount;

		public DeclarationForm DeclarationForm;
        public ParameterList Parameters;

        public CodeBlock Body;
        public Dictionary<Variable, int> DeclaredLocals;

        /// <summary>
        /// Points on position before `{` in full form or after `->` in short one
        /// </summary>
        public TokenStream BodyStream;

        public string Signature => $"{Type.ToString()} {Class.Name}::{Name} ({Parameters.ToString()})";

        public Method()
        {
            localCount = 0;
            DeclaredLocals = new Dictionary<Variable, int>();
        }
		
		public void AddLocal(Variable localVar)
		{
            DeclaredLocals.Add(localVar, localCount++);
		}

        public bool ParametersFitsArgs(List<IType> argList)
        {
            if (Parameters.Count != argList.Count)
                return false;
            for (int i = 0; i < argList.Count; ++i)
            {
                var parameter = Parameters[i];
                var argument = argList[i];
                if (!parameter.Type.Equals(argument) && !Compiler.CanCast(argument, parameter.Type))
                {
                    return false;
                }
            }
            return true;
        }
		
        public bool ParametersFitsValues(List<Node> argList)
        {
            if (Parameters.Count != argList.Count)
                return false;
            for (int i = 0; i < argList.Count; ++i)
            {
                var parameter = Parameters[i];
                var argument = argList[i];
                if (!parameter.Type.Equals(argument) && !Compiler.CanCast(argument.Type, parameter.Type))
                {
                    // TODO: Finish
                    //if(argument.IsConst && TypesHelper.IsIntegerType(argument.Type))
                    //{
                    //    var constantToken = Compiler.FoldConstants(argument);
                    //}
                    return false;
                }
            }
            return true;
		}

        public INamedDataElement FindParameter(Token token)
        {
            return Parameters.Find(p => p.Name == token);
        }

        public bool HasParameter(Token variable)
        {
            return Parameters.Exists(p => p.Name == variable);
        }
    }
}

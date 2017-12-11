using System;
using System.Collections.Generic;
using System.Linq;

namespace Translator
{
    public interface IType : IEquatable<IType>
    {
        DataTypes Type
        {
            get;
        }
    }

    public class PlainType : IType
    {
        public DataTypes Type
        {
            get;
            set;
        }

        public PlainType(string plain)
        {
            Type = FromString(plain);
        }

        public PlainType(DataTypes plain)
        {
            Type = plain;
        }

        public static DataTypes FromString(string type)
        {
            switch (type)
            {
                case "byte":
                    return DataTypes.UI8;
                case "sbyte":
                    return DataTypes.I8;
                case "ushort":
                    return DataTypes.UI16;
                case "short":
                    return DataTypes.I16;
                case "char":
                    return DataTypes.Char;
                case "uint":
                    return DataTypes.UI32;
                case "int":
                    return DataTypes.I32;
                case "ulong":
                    return DataTypes.UI64;
                case "long":
                    return DataTypes.I64;
                case "double":
                    return DataTypes.Double;
                case "bool":
                    return DataTypes.Bool;
                case "string":
                    return DataTypes.String;
                case "object":
                    return DataTypes.Object;
                //case "null":     // TODO: Hmmm...
                //    return DataTypes.Null;
                //case "array":    // TODO: Hmmm...[2]
                //    return DataTypes.Array;
                case "void":
                    return DataTypes.Void;
                default:         // TODO: Hmmm...[3]
                    return DataTypes.Null;
            }
        }

        public override string ToString()
        {
            return string.Format(Type.ToString().ToLower());
        }

        public bool Equals(IType other)
        {
            if (!(other is PlainType))
                return false;
            return Type == other.Type;
        }
    }

    public class ClassType : IType, INamedDataContainer
    {
        public string Name;
        public Scope Scope;
        public ClassType Parent;
        public AttributeList AttributeList;
        public ClassSymbolTable SymbolTable;
        public SourcePosition DeclarationPosition;

        public DataTypes Type => DataTypes.Class;

        public ClassType(string name)
        {
            Name = name;

            SymbolTable = new ClassSymbolTable();
            AttributeList = new AttributeList();

            var fieldConstructor = new Method();
            fieldConstructor.Name = name + "$fldCtor";
            fieldConstructor.Type = new PlainType(DataTypes.Void);
            fieldConstructor.Parameters = new ParameterList();
            fieldConstructor.Scope = Scope.Private;

            SymbolTable.Add(fieldConstructor);
		}

		public override string ToString()
		{
			return string.Format(Name);
        }

        public bool Equals(IType other)
        {
            if (!(other is ClassType clazz))
                return false;
            return clazz.Name == Name;
        }

        public bool HasDeclaration(Token variable)
        {
            return SymbolTable.Contains(variable.Representation);
        }

        public bool HasDeclarationRecursively(Token variable)
        {
            return HasDeclaration(variable) || (Parent != null && Parent.HasDeclarationRecursively(variable));
        }

        public INamedDataElement FindDeclaration(Token token)
        {
            return SymbolTable.Find(token.Representation);
        }

        public INamedDataElement FindDeclarationRecursively(Token token)
        {
            return FindDeclaration(token) ?? Parent?.FindDeclarationRecursively(token);
        }

        public INamedDataElement FindMethod(Token token, List<IType> paramList)
        {
            var methods = SymbolTable.Methods;
            var suitted = methods.Where(m => m.Name == token.Representation && m.ParametersFitsArgs(paramList));
        }
    }

    public class ArrayType : IType
    {
        public int Dimensions;
        public IType Inner;

        public DataTypes Type => DataTypes.Array;

        public ArrayType(IType inner, int dimens)
        {
            Inner = inner;
            Dimensions = dimens;
		}

		public override string ToString()
		{
            string dimens = "";
            for (int i = 0; i < Dimensions; ++i)
                dimens += "[]";
            return string.Format(Inner.ToString() + dimens);
		}

        public bool Equals(IType other)
        {
            if (!(other is ArrayType array))
                return false;
            return array.Dimensions == Dimensions && array.Inner.Equals(Inner);
        }
    }
}


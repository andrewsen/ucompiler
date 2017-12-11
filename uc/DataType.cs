using System;
using System.Collections.Generic;
using System.Linq;

namespace Translator
{
    public interface IType
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

        public int PointerDimension = 0;
        public int ArraySize = -1;

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
                case "float":
                    return DataTypes.Float;
                default:         // TODO: Hmmm...[3]
                    return DataTypes.Null;
            }
        }

        public override string ToString()
        {
            return new string('*', PointerDimension) + string.Format(Type.ToString().ToLower()) + (ArraySize == -1 ? "" : $"[{ArraySize}]");
        }

        public bool Equals(DataTypes type)
        {
            return Type == type && ArraySize == -1 && PointerDimension == 0;
        }

        public bool In(params DataTypes[] types)
        {
            return types.Contains(Type) && ArraySize == -1 && PointerDimension == 0;
        }

        public bool Equals(PlainType other)
        {
            return Type == other.Type && ArraySize == other.ArraySize && PointerDimension == other.PointerDimension;
        }
    }

    public class ClassType : IType
    {
        public string Name;
        public Scope Scope;
        public ClassType Parent;
        public AttributeList AttributeList;
        public ClassSymbolTable SymbolTable;

        public DataTypes Type
        {
            get
            {
                return DataTypes.Class;
            }
        }

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
    }

    public class ArrayType : IType
    {
        public int Dimensions;
        public IType Inner;

        public DataTypes Type
        {
            get
            {
                return DataTypes.Array;
            }
        }

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
    }
}


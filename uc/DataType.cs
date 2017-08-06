using System;
using System.Collections.Generic;

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
    }

    public class ClassType : IType
    {
        public string Name;
        public Scope Scope;
        public ClassType Parent;
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
            FieldConstructor = new Method();
            FieldConstructor.Name = name + "$fldCtor";
            FieldConstructor.Type = new PlainType(DataTypes.Void);
            FieldConstructor.Parameters = new ParameterList();
            FieldConstructor.Scope = Scope.Private;
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
    }
}


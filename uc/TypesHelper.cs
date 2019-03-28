using System;
using System.Collections.Generic;
using System.Linq;
using Translator;

namespace uc
{
    public static class TypesHelper
    {
        public static bool Is(IType val, DataTypes type)
        {
            return val.Type == type;
        }

        public static bool IsPlainType(string type, bool includeVoid = false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }

        public static bool IsIntegerType(DataTypes type)
        {
            return type <= DataTypes.I64;
        }

        public static bool IsNumericType(DataTypes type)
        {
            return type <= DataTypes.Double;
        }

        public static bool IsIntegerType(IType type)
        {
            return type.Type <= DataTypes.I64;
        }

        public static bool IsNumericType(IType type)
        {
            return type.Type <= DataTypes.Double;
        }

        public static int SizeOf(DataTypes type)
        {
            switch (type)
            {
                case DataTypes.Char:
                case DataTypes.UI8:
                case DataTypes.Bool:
                case DataTypes.I8:
                    return 1;
                case DataTypes.UI16:
                case DataTypes.I16:
                    return 2;
                case DataTypes.UI32:
                case DataTypes.I32:
                    return 4;
                case DataTypes.UI64:
                case DataTypes.I64:
                case DataTypes.Double:
                    return 8;
                case DataTypes.String:
                    return 4;
                case DataTypes.Object:
                    break;
                case DataTypes.Array:
                case DataTypes.Class:
                    return 4;
            }
            throw new InternalException($"Can't get size of {type.ToString()}");
        }

        public static bool SmallerEquals(DataTypes whatType, DataTypes thenType)
        {
            return SizeOf(whatType) <= SizeOf(thenType);
        }
    }
}

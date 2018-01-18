using System;
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
    }
}

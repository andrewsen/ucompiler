using System;

namespace Compiler
{
    public enum TokenType
    {
        ImplicitIdentifier, Identifier, Keyword, Delimiter, OperatorAssign,
        Operator, Constant, Unknown, EOF, Endl, Semicolon,
    }

    [Flags]
    public enum OperationType
    {
        Inc = 0x0, // 000
        Dec = 0x1, // 001
        PreMask = 0x2, // 100
        PreInc = 0x2, // 010
        PreDec = 0x3, // 011
        PostMask = 0x4, // 100
        PostInc = 0x4,  // 100
        PostDec = 0x5,  // 101
        UnaryPlus, UnaryMinus,
        Add, Sub, Mul, Div, Mod,
        BinOr, BinAnd, Xor, Inv, ShiftLeft, ShiftRight,
        Not, And, Or, Equals, NotEquals, GreaterEquals, LowerEquals, Greater, Lower,
        Cast,
        ArrayGet, ArrayMutate,
        Getter, Setter,
        New,
        NewObj,
        NewArr,
        Assign,
        AssignAdd, AssignSub, AssignMul, AssignDiv, AssignMod, AssignBinOr, AssignBinAnd, AssignXor, AssignShiftLeft, AssignShiftRight,
        FunctionCall,
        MemberAccess,
    }

    public enum Association
    {
        Left, Right
    }

    // NOTE: Andrew Senko: ConstantType and DataTypes must stay in same numerical order
    public enum ConstantType : byte
    {
        Char, UI8, I8, UI16, I16, UI32, I32, UI64, I64, Double, String, Null, Bool
    }

    public enum DataTypes : byte
    {
        Char, UI8, I8, UI16, I16, UI32, I32, UI64, I64, Double, String, Null, Bool, Void, Object, Array, Class
    }

    public enum Scope : byte
    {
        Private, Public, Protected, Internal, None
    }

    public enum BinaryType
    {
        Executable, Library
    }

    public enum InfoType
    {
        Info, Warning, Error
    }

    public enum ExceptionType
    {
        None,
        AdditionalInfo,
        IdentifierInUse,
        UninitedConstant,
        IllegalModifier,
        PropertyRedefinition,
        ImplicitVariable,
        InvProperty,
        ConstModifier,
        DuplicateValue,
        ColonExpected,
        MetaKeyExists,
        UnknownLocale,
        FunctionRedefinition,
        ClassRedefinition,
        UnknownOp,
        BadExpression,
        Import,
        FlowError,
        lValueExpected,
        WhileExpected,
        IllegalEscapeSequence,
        Brace,
        IllegalToken,
        IllegalCast,
        CommaExpected,
        IllegalType,
        EosExpexted,
        ExcessToken,
        NonNumericValue,
        AttributeException,
        ImpossibleError,
        MultipleGetters,
        MultipleSetters,
        MissingParenthesis,
        SemicolonExpected,
        UnexpectedComma,
        UnexpectedToken,
        NotImplemented,
        FunctionName,
        InvalidMemberAccess,
        InternalError,
        UndeclaredIdentifier,
        ClassNotFound,
        AssignmentExpected,
        ConsructorExpected,
        ArraySpec,
        AmbigiousOverload,
        MethodNotFound,
        ConstructorName,
        VariableRedefinition,
        NamingViolation,
        UnboundElse,
        UnboundCase,
        UnboundDefault,
        KeywordExpected,
        OverflowWarning,
        UndefinedType,
        MemberNotFound
    }

    public enum DeclarationForm
    {
        Full, Short
    }

    public enum ParsingPolicy
    {
        Brackets, SyncTokens, Both
    }

    [Flags]
    public enum ClassEntryModifiers
    {
        None, Const, Static, Native,
        Constructor
    }

    [Flags]
    public enum ValueAction
    {
        Load, Store
    }

    [Flags]
    public enum TypeReaderConf
    {
        None, IncludeVoid, IncludeVar, Soft
    }

    public enum OpCodes
    {
        NOP,
        INV,
        DUP,
        ADD,
        SUB,
        MUL,
        DIV,
        REM,
        NEG,
        CONV_UI8,
        CONV_I8,
        CONV_UI16,
        CONV_I16,
        CONV_CHR,
        CONV_I32,
        CONV_UI32,
        CONV_I64,
        CONV_UI64,
        CONV_F,
        CONV_BOOL,
        JMP,
        JZ,
        JFALSE,
        JNZ,
        JTRUE,
        JNULL,
        JNNULL,
        CALL,
        CALLS,
        NEWARR,
        NEWOBJ,
        LDLOC,
        LDLOC_0,
        LDLOC_1,
        LDLOC_2,
        STLOC,
        STLOC_0,
        STLOC_1,
        STLOC_2,
        LDELEM,
        LDELEM_0,
        LDELEM_1,
        LDELEM_2,
        LD_AREF,
        LD_BYREF,
        STELEM,
        STELEM_0,
        STELEM_1,
        STELEM_2,
        ST_BYREF,
        LDARG,
        LDARG_0,
        LDARG_1,
        LDARG_2,
        STARG,
        STARG_0,
        STARG_1,
        STARG_2,
        LDFLD,
        LDFLD_0,
        LDFLD_1,
        LDFLD_2,
        STFLD,
        STFLD_0,
        STFLD_1,
        STFLD_2,
        LDSFLD,
        LDSFLD_0,
        LDSFLD_1,
        LDSFLD_2,
        STSFLD,
        STSFLD_0,
        STSFLD_1,
        STSFLD_2,
        LDC_STR,
        LDC_UI8,
        LDC_I8,
        LDC_UI16,
        LDC_I16,
        LDC_CHR,
        LDC_I32,
        LDC_UI32,
        LDC_I64,
        LDC_UI64,
        LDC_F,
        LDC_TRUE,
        LDC_FALSE,
        LDC_NULL,
        AND,
        OR,
        EQ,
        NEQ,
        NOT,
        XOR,
        INC,
        DEC,
        SHL,
        SHR,
        POP,
        GT,
        GTE,
        LT,
        LTE,
        SIZEOF,
        TYPEOF,
        RET,

        // Service opcodes
        LABEL,
        CONV,
        LDC_BOOL,
    }
}

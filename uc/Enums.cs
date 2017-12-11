﻿using System;


namespace Lab4
{
    public enum TokenType {
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
    public enum ConstantType : byte {
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
        DoubledotExpected, //FIXME: LOOK UP FOR NORMAL ENGLISH TRANSLATION OF `:`
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
        InternalError
    }

    public enum DeclarationForm
    {
        Full, Short
    }

    [Flags]
    public enum ParsingPolicy
    {
        Brackets = 0x1, 
        Semicolon = 0x2, 
        Both = 0x3
    }

    [Flags]
    public enum ClassEntryModifiers
    {
        None, Const, Static, Native
    }

}

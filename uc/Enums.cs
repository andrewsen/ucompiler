﻿using System;


namespace Translator
{
    public enum TokenType {
        ImplicitIdentifier, Identifier, Keyword, Delimiter, OperatorAssign, 
        Operator, Constant, Unknown, EOF, Endl, Semicolon,
    }

    public enum OperationType 
    {
        UnaryPlus, UnaryMinus,
        Inc, Dec, PreInc, PreDec, PostInc, PostDec,
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
    }

    public enum Association
    {
        Left, Right
    }

    public enum ConstantType : int {
        Char, UI8, I8, UI16, I16, UI32, I32, UI64, I64, Double, String, Null, Bool
    }

    public enum DataTypes : byte
    {
        //Null, Void, Byte, Char, Short, Uint, Int, Ulong, Long, Bool, Double, String, Array, Class
        Void, Char, UI8, I8, UI16, I16, UI32, I32, UI64, I64, Double, String, Null, Bool, Object, Array, Class
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
        UnexpectedComma
    }

    [Flags]
    public enum ClassEntryModifiers
    {
        None, Const, Static, Native
    }

}

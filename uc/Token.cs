﻿﻿//
//  Token.cs
//
//  Author:
//       Andrew Senko <andrewsen98@gmail.com>
//
//  Copyright (c) 2014 Andrew Senko
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using uc;

namespace Translator
{
    //delegate void NewLineDelegate(TokenStream ts);

    public class Token
    {
        private static readonly Token eof = new Token{ Type = TokenType.EOF, Representation = "" };

        public TokenType Type;
        public ConstantType ConstType;
        public Operation Operation;
        public string Representation;
        public SourcePosition Position;

        public string Quoted
        {
            get
            {
                return "\""+Representation+"\"";
            }
        }

        public string Unquoted
        {
            get
            {
                if(Representation.First() == '"' && Representation.Last() == '"')
                    return Representation.Substring(1, Representation.Length-2);    // TODO: Shitty
                return Representation;
            }
        }

        public static Token EOF => eof;

        public SourcePosition TailPosition => new SourcePosition(Position.Source, Position.Line, Position.LineNum, Position.TokenPos + Representation.Length, Position.File);

        public static implicit operator string(Token tok)
        {
            return tok.Representation;
        }

        public bool IsOneOf(params string[] vals)
        {
            return vals.Contains(this.Representation);
        }

        public bool IsOneOf(TokenType type, params string[] vals)
        {
            return vals.Contains(this.Representation) && Type == type;
        }

        public static bool IsNumeric(ConstantType type)
        {
            return type <= ConstantType.Double;
        }

        public static bool IsInteger(ConstantType type)
        {
            return type <= ConstantType.I64;
        }

        #region Operations on constants

        public bool IsConstant()
        {
            return Type == TokenType.Constant;
        }

        public bool IsNumeric()
        {
            return Type == TokenType.Constant && IsNumeric(ConstType);
        }

        public bool IsInteger()
        {
            return Type == TokenType.Constant && IsInteger(ConstType);
        }

        public bool IsDouble()
        {
            return Type == TokenType.Constant && ConstType == ConstantType.Double;
        }

        public bool IsBoolean()
        {
            return Type == TokenType.Constant && ConstType == ConstantType.Bool;
        }

        public bool IsString()
        {
            return Type == TokenType.Constant && ConstType == ConstantType.String;
        }

        public bool IsNull()
        {
            return Type == TokenType.Constant && ConstType == ConstantType.Null;
        }

        #endregion

        public bool IsIdentifier()
        {
            return Type == TokenType.Identifier;
        }

        public bool IsOp()
        {
            return Type == TokenType.Operator || Type == TokenType.OperatorAssign;
        }

        public bool IsOp(OperationType type)
        {
            return (Type == TokenType.Operator || Type == TokenType.OperatorAssign) && Operation.Type == type;
        }

        public bool IsDelim()
        {
            return Type == TokenType.Delimiter;
        }

        public bool IsSemicolon()
        {
            return Type == TokenType.Semicolon;
        }

        public ConstantType GetMinimalIntType()
        {
            if (long.TryParse(Representation, out long longVal))
            {
                if (longVal >= sbyte.MinValue && longVal <= sbyte.MaxValue)
                    return ConstantType.I8;
                if (longVal >= short.MinValue && longVal <= short.MaxValue)
                    return ConstantType.I16;
                if (longVal >= int.MinValue && longVal <= int.MaxValue)
                    return ConstantType.I32;
                return ConstantType.I64;
            }
            if (ulong.TryParse(Representation, out ulong ulongVal))
            {
                if (ulongVal >= byte.MinValue && ulongVal <= byte.MaxValue)
                    return ConstantType.UI8;
                if (ulongVal >= ushort.MinValue && ulongVal <= ushort.MaxValue)
                    return ConstantType.UI16;
                if (ulongVal >= uint.MinValue && ulongVal <= uint.MaxValue)
                    return ConstantType.UI32;
                return ConstantType.UI64;
            }
            return ConstantType.Null; // TODO: May fail
        }

        public bool IsInRangeOf(ConstantType val)
        {
            switch (val)
            {
                case ConstantType.UI16:
                case ConstantType.Char:
                    return ushort.TryParse(Representation, out ushort _);
                case ConstantType.UI8:
                    return byte.TryParse(Representation, out byte _);
                case ConstantType.I8:
                    return sbyte.TryParse(Representation, out sbyte _);
                case ConstantType.I16:
                    return short.TryParse(Representation, out short _);
                case ConstantType.UI32:
                    return uint.TryParse(Representation, out uint _);
                case ConstantType.I32:
                    return int.TryParse(Representation, out int _);
                case ConstantType.UI64:
                    return ulong.TryParse(Representation, out ulong _);
                case ConstantType.I64:
                    return long.TryParse(Representation, out long _);
                case ConstantType.Double:
                    return double.TryParse(Representation, NumberStyles.Any, CultureInfo.InvariantCulture, out double _);
                case ConstantType.Bool:
                    return Representation == "true" || Representation == "false";
                case ConstantType.Null:
                    return Representation == "null";
                case ConstantType.String:
                    return true;
                default:
                    return false;
            }
        }

        #region Constant getters
        public byte GetUI8()
        {
            return byte.Parse(Representation);
        }
        public sbyte GetI8()
        {
            return sbyte.Parse(Representation);
        }
        public ushort GetUI16()
        {
            return ushort.Parse(Representation);
        }
        public short GetI16()
        {
            return short.Parse(Representation);
        }
        public uint GetUI32()
        {
            return uint.Parse(Representation);
        }
        public int GetI32()
        {
            return int.Parse(Representation);
        }
        public ulong GetUI64()
        {
            return ulong.Parse(Representation);
        }
        public long GetI64()
        {
            return long.Parse(Representation);
        }
        public double GetDouble()
        {
            return double.Parse(Representation, NumberStyles.Any, CultureInfo.InvariantCulture);
        }
        public string GetString()
        {
            return Representation;
        }
        public bool GetBool()
        {
            return bool.Parse(Representation);
        }
        public char GetChar()
        {
            return char.Parse(Representation);
        }
        #endregion

        public override string ToString()
        {
            return Representation;
        }

        public object GetValue()
        {
            if (!IsConstant())
                throw new InternalException("Trying to get value of non-constant token");
            switch (ConstType)
            {
                case ConstantType.Char:
                    return GetChar();
                case ConstantType.UI8:
                    return GetUI8();
                case ConstantType.I8:
                    return GetI8();
                case ConstantType.UI16:
                    return GetUI16();
                case ConstantType.I16:
                    return GetI16();
                case ConstantType.UI32:
                    return GetUI32();
                case ConstantType.I32:
                    return GetI32();
                case ConstantType.UI64:
                    return GetUI64();
                case ConstantType.I64:
                    return GetI64();
                case ConstantType.Double:
                    return GetDouble();
                case ConstantType.String:
                    return GetString();
                case ConstantType.Null:
                    return null;
                case ConstantType.Bool:
                    return GetBool();
            }
            throw new InternalException($"Method error: {nameof(GetValue)}");
        }
    }

    class TypedToken : Token
    {
        public readonly IType BoundType;

        public TypedToken(IType type)
        {
            BoundType = type;
        }
    }
}
/* 
 * // [0] - program stack
 * // [1] - array stack
 * // [2] - call stack
 * // [3] - instruction stack
 * 
 * .import "std"
 * .import "std.runtime"
 * .using Std::
 * .using Std::Runtime::
 * 
 * prot int main (int, char**)
 *
 * prot void ftest (void)
 * 
 * fun int main (int, char**)
 *      p2ui [1]
 *      mov vra, vr1
 *      add vr1, 1
 *      ui2p vra
 *      push vra
 *      call printf (char*, ...)
 *      pop
 *      pop
 *      push vr1
 *      call printf (char*, ..)
 *      pop
 *      pop
 *      call ftest (void)
 *      ret 0
 * end
 * 
 * fun void ftest (void)
 *      push "test.mod"
 *      call ldmod (char*)
 *      swst [0]
 *      
 * end
 * 
 *  _____________________________
 * |    |ch**|    |    |    |    |
 * 
 * 
 * 
 * import Std;
 * 
 * using Std;
 * 
 * int main(int argc, char** argv) {
 * 
 * }
 * 
 * void ftest () {
 * 
 * }
 * 
 */
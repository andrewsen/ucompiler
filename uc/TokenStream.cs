using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using uc;

namespace Translator
{
    public class SourcePosition
    {
        public string File;
        public string Line;
        public int LineNum;
        public int TokenPos;

        public SourcePosition(string line, int lineNum, int pos, string file)
        {
            Line = line;
            LineNum = lineNum;
            TokenPos = pos;
            File = file;
        }

        public override string ToString()
        {
            return string.Format("`{0}`", Line);
        }
    }

    public class TokenStream
    {

        const string UINT_SUFFIX = "u";
        const string ULONG_SUFFIX = "ul";
        const string LONG_SUFFIX = "l";
        const string FLOAT_SUFFIX = "f";

        string source;
        string file;
        int prevPos;
        int pos;
        List<int> lines;
        Token cur, prev;

        public TokenStream(string source, string file)
        {
            this.source = source + "\n";
            this.file = file;
            lines = new List<int>();
            cur = new Token();
            cur.Type = TokenType.Unknown;
            prev = new Token();
            prev.Type = TokenType.Unknown;
            lines.Add(0);
        }

        public int TokenPosition
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public string Line
        {
            get
            {
                return LineAt(lines.Last());
            }
        }

        public int LineNum
        {
            get
            {
                return lines.Count;
            }
        }

        public int LinePosition
        {
            get
            {
                return pos - lines.Last();
            }
        }

        public SourcePosition SourcePosition
        {
            get
            {
                return new SourcePosition(Line, LineNum, LinePosition, file);
            }
        }

        public bool Eof => cur.Type == TokenType.EOF;

        public bool NextEof()
        {
            return Next().Type == TokenType.EOF;
        }

        public TokenStream Pass()
        {
            Next();
            return this;
        }

        public Token Next()
        {
            if(cur.Type == TokenType.EOF)
                return cur;

            try
            {
                prevPos = pos;                
                prev = cur;
                return nextToken();
            }
            catch (IndexOutOfRangeException)
            {
                cur = new Token();
                cur.Type = TokenType.EOF;
                return cur;
            }
        }

        public override string ToString()
        {
            //return string.Format("[TokenStream: Line={0}, LineNum={1}, Position={2}]", Line, LineNum, Position);
            return cur.Representation;
        }

        private Token nextToken()
        {
            prev = cur;
            cur = new Token();
            cur.Type = TokenType.Unknown;
            string temp = "";

            while (" \t\n".Contains("" + source[pos])) // Throws OutOfRange
            {
                if (source[pos] == '\n')
                    lines.Add(++pos);
                else
                    ++pos;
            }

            if (source[pos] + "" + source[pos + 1] == "//")
            {
                while (source[pos++] != '\n')
                {
                }
                lines.Add(pos);
                return nextToken();
            }

            if (source[pos] + "" + source[pos + 1] == "/*")
            {
                pos += 2;
                while (source[pos] + "" + source[++pos] != "*/")
                {
                    if (source[pos] == '\n')
                        lines.Add(pos + 1);
                }
                ++pos;
                return nextToken();
            }

            if (isValidIdentStartSymbol(source[pos]))
            {
                temp += source[pos];
                while (isValidIdentSymbol(source[++pos]))
					temp += source[pos];
				if (temp == "true" || temp == "false")
				{
					cur.Type = TokenType.Constant;
					cur.ConstType = ConstantType.Bool;
				}
				if (temp == "and" || temp == "or")
				{
                    cur.Type = TokenType.Operator;
                    cur.Operation = Operation.From(temp);
				}
                else if (temp == "null")
                {
                    cur.Type = TokenType.Constant;
                    cur.ConstType = ConstantType.Null;
                }
                else
                    cur.Type = TokenType.Identifier;
                cur.Representation = temp;
            }
            else if (isDigit(source[pos]))
            {
                cur.Type = TokenType.Constant;
                temp += source[pos++];
                switch (source[pos])
                {
                    case 'x':
                        temp = parseHexDigit();
                        break;
                    case 'o':
                        temp = parseOctDigit();
                        break;
                    case 'b':
                        temp = parseBinDigit();                       
                        break;
                    default:
                        --pos;
                        temp = parseDecDigit();
                        break;
                }
                if (source[pos] + "" + source[pos + 1] == ULONG_SUFFIX)
                {
                    pos += 2;
                    cur.ConstType = ConstantType.UI64;
                }
                else if ("" + source[pos] == UINT_SUFFIX)
                {
                    ++pos;
                    cur.ConstType = ConstantType.UI32;
                }
                else if ("" + source[pos] == LONG_SUFFIX)
                {
                    ++pos;
                    cur.ConstType = ConstantType.I64;
                }
                else if ("" + source[pos] == FLOAT_SUFFIX)
                {
                    ++pos;
                    cur.ConstType = ConstantType.Double;
                }
                else if (temp.Contains("."))
                {
                    cur.ConstType = ConstantType.Double;
                }
                else
                {
                    cur.ConstType = ConstantType.I32;
                }

                if(isValidIdentStartSymbol(source[pos]))
                {
                    InfoProvider.AddError("Illegal numeric identifier", ExceptionType.NonNumericValue, SourcePosition);
                }
                cur.Representation = temp;
			}
			// Enable after Lab4
			//else if ("{}[]():,?@".Contains("" + source[pos]))
			else if ("{}[](),?@".Contains("" + source[pos]))
            {
                cur.Type = TokenType.Delimiter;
                cur.Representation = "" + source[pos++];
            }
            else if (source[pos] == ';')
            {
                cur.Type = TokenType.Semicolon;
                cur.Representation = ";";
                ++pos;
            }
            else if ("+-*/.:!&|%^~<>=".Contains("" + source[pos]))
            {
                cur.Type = TokenType.Operator;
                temp += source[pos];
                string dop = source[pos] + "" + source[pos + 1];
                // Enable after Lab4
				//if (new List<string> { "++", "--", "==", "!=", "<=", ">=", "->", "<-", "=>", ">>", "<<", "||", "&&", }.Contains(dop))
				if (new List<string>{ ":=", "<>", "<=", ">=", "=>", ">>", "<<" }.Contains(dop))
                {
                    temp = dop;
                    cur.Operation = Operation.From(dop);
                    pos += 2;
                }
                else if (new List<string>{ "+=", "-=", "*=", "/=", "|=", "&=", "^=", "~=", "%=" }.Contains(dop))
                {
					temp = dop;
					cur.Operation = Operation.From(dop);
                    pos += 2;
                    cur.Type = TokenType.OperatorAssign;
                }
                else if (new List<string>{ ">>=", "<<=" }.Contains(dop + source[pos + 2]))
                {
					temp = dop + source[pos + 2];
					pos += 3;
					cur.Operation = Operation.From(temp);
                    cur.Type = TokenType.OperatorAssign;
                }
                else
				{
					cur.Operation = Operation.From(temp);
                    ++pos;
                }
                cur.Representation = temp;
            }
            // Enable after Lab4
            //else if ("" + source[pos] == "#")
            //{
            //    cur.Type = TokenType.ImplicitIdentifier;
            //    cur.Representation = "#";
            //    ++pos;
            //}
            if (source[pos] == '"')
            {
                temp = "\"";
                while (source[++pos] != '"')
                    temp += source[pos];
                ++pos;
                cur.Representation = temp + "\"";
                cur.Type = TokenType.Constant;
                cur.ConstType = ConstantType.String;
            }
            if (source[pos] == '\'')
            {
                temp = "'";
                while (source[++pos] != '\'')
                    temp += source[pos];
                ++pos;
                cur.Representation = temp + "'";
                cur.Type = TokenType.Constant;
                cur.ConstType = ConstantType.Char;
            }
            if(cur.Type == TokenType.Unknown)
            {
                InfoProvider.AddError($"Unknown symbol `{source[pos]}`", ExceptionType.IllegalToken, SourcePosition);
                ++pos;
            }

            cur.Line = lines.Last();//6[lines.Count-1];
            cur.Position = pos - cur.Line;
            return cur;
        }

        public Token Current
        {            
            get
            {
                return cur;
            }
        }

        public Token Previous()
        {
            return prev;
        }

        public void PushBack()
        {
            pos = prevPos;
            cur = prev;
        }

        public void SkipTo(string str, bool include=false)
        {
            while(cur.Representation != str)
                Next();
            if(include)
                Next();
        }

        public void SkipBraced(char openBrc, char closeBrc)
        {
            int brcIndex = 1;
            while (brcIndex != 0)
            {
                if (source[pos] == closeBrc)
                {
                    --brcIndex;
                    ++pos;
                }
                else if (source[pos] == openBrc)
                {
                    ++brcIndex;
                    ++pos;
                }
                else if (source[pos] + "" + source[pos + 1] == "//")
                {
                    while (source[pos++] != '\n')
                    {
                    }
                    lines.Add(pos);
                }
                else if (source[pos] + "" + source[pos + 1] == "/*")
                {
                    pos += 2;
                    while (source[pos] + "" + source[++pos] != "*/")
                    {
                        if (source[pos] == '\n')
                            lines.Add(pos + 1);
                    }
                    ++pos;
                }
                else if (source[pos] == '"')
                {
                    while (source[++pos] != '"')
                    {
                        if (source[pos] == '\n')
                            lines.Add(pos + 1);
                    }
                    // Currently points on close quote. Must increment pos
                    ++pos;
                }
                else if (source[pos] == '\'')
                {
                    pos += 3;
                }
                else if (source[pos] == '\n')
                    lines.Add(++pos);
                else
                    ++pos;
            }
        }

        public string LineAt(int num)
        {
            string line = "";
            while(num < source.Length && source[num] != '\n')
                line += source[num++];
            return line;
        }

        static bool isValidIdentStartSymbol(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '_' || ch == '$';
        }

        static bool isValidIdentSymbol(char ch)
        {
            return isValidIdentStartSymbol(ch) || isDigit(ch);                
        }

        static bool isDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        static bool isHexDigit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
        }

        static bool isOctDigit(char ch)
        {
            return ch >= '0' && ch <= '7';
        }

        static bool isBinDigit(char ch)
        {
            return ch == '0' || ch == '1';
        }

        string parseHexDigit()
        {
            string res = "";
            while(isHexDigit(source[++pos]))
                res += source[pos];

            return ""+Convert.ToUInt64(res, 16);
        }

        string parseOctDigit()
        {
            string res = "";
            while(isOctDigit(source[++pos]))
                res += source[pos];

            return ""+Convert.ToUInt64(res, 8);
        }

        string parseBinDigit()
        {
            string res = "";
            while(isBinDigit(source[++pos]))
                res += source[pos];

            return ""+Convert.ToUInt64(res, 2);
        }

        string parseDecDigit()
        {
            string res = "";
            bool floatingPoint = false;

            if(!isDigit(source[pos]))
                return res;
            res += source[pos++];
            while(isDigit(source[pos]) || (source[pos] == '.' && !floatingPoint))
            {
                if(source[pos] == '.')
                    floatingPoint = true;
                res += source[pos++];
            }

            return res;
        }
    }
}


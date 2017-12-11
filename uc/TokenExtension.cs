using System;

namespace Lab4
{
    public static class TokenExtension
    {
        public static string GetIdentifier(this Reader toks)
        {
            var id = toks.Current;
            if (id.Type != TokenType.Identifier)
                CompilerLog.AddError("Identifier expected", ExceptionType.IllegalToken, toks.SourcePosition);
            return id.ToString();
        }

        public static string GetIdentifierNext(this Reader toks)
        {
            var id = toks.Next();
            if (id.Type != TokenType.Identifier)
                CompilerLog.AddError("Identifier expected", ExceptionType.IllegalToken, toks.SourcePosition);
            return id.ToString();
        }

        public static bool Is(this Reader toks, string str)
        {
            return toks.Current == str;
        }

        public static bool Is(this Reader toks, string str, TokenType tokType)
        {
            return toks.Current == str && toks.Current.Type == tokType;
        }

        public static bool Is(this Reader toks, string str, ConstantType constType)
        {
            return toks.Current == str && toks.Current.ConstType == constType;
        }

        public static bool IsNext(this Reader toks, string str)
        {
            return toks.Next() == str;
        }

        public static bool IsNext(this Reader toks, string str, TokenType tokType)
        {
            return toks.Next() == str && toks.Current.Type == tokType;
        }

        public static bool IsNext(this Reader toks, string str, ConstantType constType)
        {
            return toks.Next() == str && toks.Current.ConstType == constType;
        }

        public static void CheckNext(this Reader toks, string str, ExceptionType extype)
        {
            if(!toks.IsNext(str))
                CompilerLog.AddError("`"+str+"` expected", extype, toks.SourcePosition);
        }

        public static void Check(this Reader toks, string str, ExceptionType extype)
        {
            if(!toks.Is(str))
                CompilerLog.AddError("`"+str+"` expected", extype, toks.SourcePosition);
        }

        public static string CollectUntil(this Reader toks, TokenType tokType, bool include=true)
        {
            string result = "";

            while (toks.Current.Type != tokType)
            {
                result += toks.Current.Representation + " ";
                toks.Next();
            }

            if (!include)
            {
                toks.PushBack();
                return result.TrimEnd();
            }
            return result + toks.Current.Representation;
        }

        public static string CollectNextUntil(this Reader toks, TokenType tokType, bool include = true)
        {
            string result = "";

            while (toks.Next().Type != tokType)
            {
                result += toks.Current.Representation + " ";
            }

            if (!include)
            {
                toks.PushBack();
                return result.TrimEnd();
            }
            return result + toks.Current.Representation;
        }

        public static IType CurrentType(this Reader toks, bool includeVoid) 
        {
            string identifier = toks.GetIdentifier();
            return readType(toks, identifier, includeVoid);
        }

        public static IType NextType(this Reader toks, bool includeVoid)
        {
            string identifier = toks.GetIdentifierNext();
            return readType(toks, identifier, includeVoid);
        }

        private static IType readType(Reader toks, string identifier, bool includeVoid)
        {
            if(identifier == "void" && !includeVoid)
                CompilerLog.AddError("Unexpected `void` type", ExceptionType.IllegalType, toks.SourcePosition);

            int dimens = 0;

            while (!toks.Eof)
            {
                if (toks.IsNext("["))
                    dimens++;
                else
                {
                    toks.PushBack();
                    break;
                }
                toks.CheckNext("]", ExceptionType.Brace);
            }

            if(identifier == "void" && dimens > 0)
                CompilerLog.AddError("Unexpected `void` typed array", ExceptionType.IllegalType, toks.SourcePosition);

            IType result;
            if(Compiler.IsPlainType(identifier))
                result = new PlainType(identifier);
            else
                result = new ClassType(identifier);

            if(dimens != 0)
                return new ArrayType(result, dimens);
            return result;
        }

        /*public static DataType GetTypeNext(TokenStream toks)
        {
            string pod = toks.Next();
            if (!isPODType(pod))
                throw new CompilerException(ExceptionType.IllegalType, "Illegal type in `foreach`", toks);
            if (toks.Next() != "[")
            {
                toks.PushBack();
                return new DataType(getPODType(pod));
            }

            int dimens = 0;
            while (toks.ToString() == "[")
            {
                ++dimens;
                checkNextToken(toks, "]");
                toks.Next();
            }

            toks.PushBack();
            return new DataType(DataTypes.Array, new DataType(getPODType(pod)), dimens); 
        }

        private void checkEos(TokenStream toks)
        {
            if (!isEos(toks))
                throw new CompilerException(ExceptionType.EosExpexted, "End of statement expected (;)", tokens);
        }

        private void checkEosNext(TokenStream toks)
        {
            if (!isEosNext(toks))
                throw new CompilerException(ExceptionType.EosExpexted, "End of statement expected (;)", tokens);
        }

        void checkNextToken(TokenStream toks, string str)
        {
            if(toks.Next() != str)
                throw new CompilerException(ExceptionType.IllegalToken, "`" + str + "` expected", toks);
        }

        private bool isEos(TokenStream toks)
        {
            if (toks.ToString() != ";" && toks.Type != TokenType.Separator)
                return false;
            return true;
        }

        private bool isEosNext(TokenStream toks)
        {
            if (toks.Next() != ";" && toks.Type != TokenType.Separator)
                return false;
            return true;
        }

        private bool isPODType(string str, bool @void = false)
        {
            if (CommandArgs.lang == SyntaxLang.English)
                switch (str)
                {
                    case "bool":
                    case "byte":
                    case "char":
                    case "short":
                    case "int":
                    case "uint":
                    case "long":
                    case "ulong":
                    case "string":
                    case "double":
                        return true;
                    case "void":
                        if (@void)
                            return true;
                        else
                            return false;
                    case "array":
                        if(CommandArgs.allowBuiltins)
                            return true;
                        else 
                            return false;
                    default:
                        return false;
                }
            else
                switch (str)
                {
                    case "буль":
                    case "байт":
                    case "зкоротке":
                    case "зціле":
                    case "ціле":
                    case "здовге":
                    case "довге":
                    case "рядок":
                    case "подвійне":
                        return true;
                    case "воід":
                        if (@void)
                            return true;
                        else
                            return false;
                    default:
                        return false;
                }
        }*/
    }
}


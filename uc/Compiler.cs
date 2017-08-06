using System;
using System.Linq;
using System.IO;

namespace Translator
{
    public class Compiler
    {
        CompilerConfig compilerConfig;
        AttributeList bindedAttributeList = new AttributeList();
        DirectiveList directiveList = new DirectiveList();
        MetadataList metadataList = new MetadataList();

        public Compiler(CompilerConfig config)
        {
            compilerConfig = config;
        }

        public void Compile()
        {
            foreach (var src in compilerConfig.Sources)
            {
                parseGlobalScope(src);
            }

            foreach (var src in compilerConfig.Sources)
            {
                compileFile(src);
            }
        }

        void parseGlobalScope(string src)
        {
            TokenStream gStream = new TokenStream(File.ReadAllText(src), src);

            Token token = gStream.Next();
            while (!gStream.Eof)
            {
                switch (token)
                {
                    case "@":
                        parseAttribute(gStream);
                        break;
                    case "import":
                        parseImport(gStream);
                        break;
                    case "public":
                        parseClass(gStream, Scope.Public);
                        break;
                    case "private":
                    case "class":
                        parseClass(gStream, Scope.Private);
                        break;
                }
            }
        }

        void parseAttribute(TokenStream gStream)
        {
            AttributeReader reader = new AttributeReader(gStream);

            var attr = reader.Read();

            switch (attr.Name)
            {
                case "AllowBuiltins":
                    compilerConfig.AllowBuiltins = true;
                    break;
                case "AddMeta":
                case "Info":
                case "Define":
                    {
                        if ((attr.Data.Count > 2 || attr.Data.Count < 1 || !(attr.Data[0].Value is string)) && !attr.Data[0].IsOptional)
                        {
                            InfoProvider.AddError("@AddMeta: Incorrect data", ExceptionType.AttributeException, gStream.SourcePosition);
                            //throw new AttributeException("AddMeta", "Incorrect data");
                        }

                        Metadata md = new Metadata();
                        if (attr.Data[0].IsOptional)
                        {
                            md.Key = attr.Data[0].Key;
                            md.Value = attr.Data[0].Value;
                            md.Type = attr.Data[0].Type;
                        }
                        else
                        {
                            md.Key = attr.Data[0].Value as string;
                            if (attr.Data.Count == 2)
                            {
                                md.Value = attr.Data[1].Value;
                                md.Type = attr.Data[1].Type;
                            }
                            else
                                md.Type = DataTypes.Void;
                        }
                        var mlist = from m in metadataList
                                      where m.Key == md.Key
                                        select m;
                        if (mlist.ToList().Count != 0)
                            InfoProvider.AddError("Meta key " + md.Key + " exists", ExceptionType.MetaKeyExists, gStream.SourcePosition);
                            //throw new CompilerException(ExceptionType.MetaKeyExists, "Meta key " + md.key + " in module " + CommandArgs.source + " exists", tokens);

                        metadataList.Add(md);
                    }
                    break;
                case "Module":
                    if (attr.Data.Count != 1 && !(attr.Data[0].Value is string))
                        InfoProvider.AddError("@Module: Incorrect name", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("Module", "Incorrect module name");
                    if (compilerConfig.OutBinaryFile == null)
                        compilerConfig.OutBinaryFile = (attr.Data[0].Value as string) + ".vas"; // TODO: FIXME
                    break;
                case "RuntimeInternal":
                    if (attr.Data.Count != 0)
                        InfoProvider.AddError("@RuntimeInternal: Too many arguments", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("RuntimeInternal", "Too many arguments");
                    if (!attr.Binded)
                        InfoProvider.AddError("@RuntimeInternal must be binded to method (check `;`)", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("RuntimeInternal", "`@RuntimeInternal` must be binded to function (check `;`)");
                    bindedAttributeList.Add(attr);
                    break;
                case "Entry":
                    if (attr.Data.Count != 0)
                        InfoProvider.AddError("@Entry: Too many arguments", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("Entry", "Too many arguments");
                    if (!attr.Binded)
                        InfoProvider.AddError("@Entry must be binded to method (check `;`)", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("Entry", "`@Entry` must be binded to function (check `;`)");
                    bindedAttributeList.Add(attr);
                    break;
                case "Debug:Set":
                    {
                        if (!compilerConfig.DebugBuild)
                            goto default; // YAAAY ;D
                        if ((attr.Data.Count > 2 || attr.Data.Count < 1 || !(attr.Data[0].Value is string)) && !attr.Data[0].IsOptional)
                        {
                            InfoProvider.AddError("@Debug:Set: Incorrect data", ExceptionType.AttributeException, gStream.SourcePosition);
                            //throw new AttributeException("Debug:Set", "Incorrect data");
                        }
                        RuntimeMetadata md = new RuntimeMetadata();                       
                        md.Prefix = attr.Name;
                        if (attr.Data[0].IsOptional)
                        {
                            md.Key = attr.Data[0].Key;
                            md.Value = attr.Data[0].Value;
                            md.Type = attr.Data[0].Type;
                        }
                        else
                        {
                            md.Key = attr.Data[0].Value as string;
                            if (attr.Data.Count == 2)
                            {
                                md.Value = attr.Data[1].Value;
                                md.Type = attr.Data[1].Type;
                            }
                            else
                                md.Type = DataTypes.Void;
                        }
                        var mlist = from m in metadataList
                                      where m.Key == md.Key
                                          select m;
                        if (mlist.ToList().Count != 0)
                        {
                            foreach (var m in mlist)
                            {
                                metadataList.Remove(m);
                            }
                        }

                        metadataList.Add(md);
                    }
                    break;
                default:
                    InfoProvider.AddWarning("Unknown attribute `@" + attr.Name + "`", ExceptionType.AttributeException, gStream.SourcePosition);
                    //Console.WriteLine("Warning: unknown attribute `" + attr.Name + "`");
                    break;
            }
        }

        void parseImport(TokenStream gStream)
        {
            throw new NotImplementedException();
        }

        void parseClass(TokenStream gStream, Scope classScope)
        {
            ClassType clazz = new ClassType(gStream.GetIdentifierNext());
            clazz.Scope = classScope;

            gStream.CheckNext("{", ExceptionType.Brace);

            while (true)
            {
                var entry = readCommonClassEntry(gStream);

                gStream.Next();
                if (gStream.Is("{") || gStream.Is("->"))
                {
                    // Property
                    if (entry.HasVoidType)
                        InfoProvider.AddError("Void type not allowed for properties", ExceptionType.IllegalType, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native))
                        InfoProvider.AddError("Properties can't be native", ExceptionType.IllegalModifier, gStream.SourcePosition);
                }
                else if (gStream.Is("("))
                {
                    // Method
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                        InfoProvider.AddError("Methods can't be const", ExceptionType.IllegalModifier, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native) && !entry.Modifiers.HasFlag(ClassEntryModifiers.Static))
                        InfoProvider.AddError("Native methods must be declared as `static`", ExceptionType.IllegalModifier, gStream.SourcePosition);

                    Method method = parseMethod(entry, gStream);

                    clazz.SymbolTable.Add(method);
                }
                else
                {
                    // Field
                    if (entry.HasVoidType)
                        InfoProvider.AddError("Void type not allowed for fields", ExceptionType.IllegalType, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native))
                        InfoProvider.AddError("Fields can't be native", ExceptionType.IllegalModifier, gStream.SourcePosition);

                    Field field = parseField(entry, gStream);

                    clazz.SymbolTable.Add(field);
                }
            }
        }

        Field parseField(CommonClassEntry entry, TokenStream gStream)
        {
            Field field = new Field();
            if (gStream.Is(";") && entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                InfoProvider.AddError("Const field must be initialized immediately", ExceptionType.UninitedConstant, gStream.SourcePosition);
            field.FromClassEntry(entry);
            if (gStream.Is("="))
            {
                gStream.Next();
                field.InitialExpressionPosition = gStream.TokenPosition;
                gStream.SkipTo(";", true);
            }
            return field;
        }

        Method parseMethod(CommonClassEntry entry, TokenStream gStream)
        {
            Method method = new Method();
            method.FromClassEntry(entry);

            ParameterList paramList = readParams(gStream);

            if (gStream.Next() == "->")
            {
                method.Begin = gStream.TokenPosition;
            }
            else if (gStream.Current == "{")
            {
                method.Begin = gStream.TokenPosition - 1;
                gStream.SkipBraced('{', '}');
            }
        }

        ParameterList readParams(TokenStream gStream)
        {
            var plist = new ParameterList();

            while (true)
            {
                var param = new Variable();

                param.Name = gStream.GetIdentifier();
                param.Type = gStream.NextType(false);

                plist.Add(param);

                if(gStream.Next() == ")")
                    break;
                if(gStream.Current != ",")
                    InfoProvider.AddError("`,` or `)` expected in parameter declaration", ExceptionType.IllegalToken, gStream.SourcePosition);
            }

            return plist;
        }

        CommonClassEntry readCommonClassEntry(TokenStream gStream)
        {               
            CommonClassEntry entry = new CommonClassEntry(); 

            entry.Scope = readScope(gStream);
            entry.DeclarationPosition = gStream.SourcePosition;
            entry.Modifiers = readModifiers(gStream);
            entry.Type = gStream.NextType(true);
            entry.Name = gStream.GetIdentifierNext();

            return entry;
        }

        Scope readScope(TokenStream gStream)
        {
            var token = gStream.Next();
            Scope scope = Scope.Private;
            switch (token.ToString())
            {
                case "public":
                    scope = Scope.Public;
                    break;
                case "protected":
                    scope = Scope.Protected;
                    break;
                case "private":
                    scope = Scope.Private;
                    break;
                default:
                    gStream.PushBack();
                    break;
            }
            return scope;
        }

        ClassEntryModifiers readModifiers(TokenStream gStream)
        {
            var token = gStream.Next();
            ClassEntryModifiers modifiers = ClassEntryModifiers.None;
            if(token == "static")
                modifiers |= ClassEntryModifiers.Static;
            if(token == "native")
                modifiers |= ClassEntryModifiers.Native;
            if(token == "const")
                modifiers |= ClassEntryModifiers.Const;
            return modifiers;
        }

        void compileFile(string src)
        {

        }

        public static bool IsPlainType(string type, bool includeVoid=false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }
    }
}


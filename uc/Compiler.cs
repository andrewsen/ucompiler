using System;
using System.Linq;
using System.IO;

using static Translator.Alphabet;

namespace Translator
{
    public class Compiler
    {
        CompilerConfig compilerConfig;
        AttributeList bindedAttributeList = new AttributeList();
        DirectiveList directiveList = new DirectiveList();
        MetadataList metadataList = new MetadataList();
        ClassList classList = new ClassList();

        public Compiler(CompilerConfig config)
        {
            compilerConfig = config;
        }

        public void Compile()
        {
            foreach (var src in compilerConfig.Sources)
            {
                // Parsing classes and their content (fields, props, methods)
                parseGlobalScope(src);
            }

            foreach (var src in compilerConfig.Sources)
            {
                // Compiling 
                // TODO: Rework it
                compileFile(src);
            }
        }

        void parseGlobalScope(string src)
        {
            TokenStream gStream = new TokenStream(File.ReadAllText(src), src);

            Token token = gStream.Next();
            while (!gStream.Eof)
            {
                // On most top level we have only attributres, classes and imports
                switch (token)
                {
                    case ATTR:
                        parseAttribute(gStream);
                        break;
                    case IMPORT:
                        parseImport(gStream);
                        break;
                    case PUBLIC:
                        parseClass(gStream, Scope.Public);
                        break;
                    case PRIVATE:
                    case CLASS:
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
                        // Making fallthru if debug option is disabled
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
            clazz.AttributeList = bindedAttributeList;
            bindedAttributeList.Clear();

            gStream.CheckNext(BLOCK_O, ExceptionType.Brace);

            while (!gStream.Eof)
            {
                var entry = readCommonClassEntry(gStream);

                gStream.Next();
                //TODO: Move attributes parsing to readCommonClassEntry()
                if(gStream.Is(ATTR))
				{
					parseAttribute(gStream);
                }
                else if (gStream.Is(BLOCK_O) || gStream.Is(SHORT_FUNC_DECL))
                {
                    // Property
                    if (entry.HasVoidType)
                        InfoProvider.AddError("Void type not allowed for properties", ExceptionType.IllegalType, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native))
                        InfoProvider.AddError("Properties can't be native", ExceptionType.IllegalModifier, gStream.SourcePosition);
                    
                    Property property = parseProperty(entry, gStream);

                    clazz.SymbolTable.Add(property);
                }
                else if (gStream.Is(PAR_O))
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
            if (gStream.Is(STAT_SEP) && entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                InfoProvider.AddError("Const field must be initialized immediately", ExceptionType.UninitedConstant, gStream.SourcePosition);
            field.FromClassEntry(entry);
            if (gStream.Is(ASSIGN))
            {
                gStream.Next();
                field.InitialExpressionPosition = gStream.TokenPosition;
                gStream.SkipTo(STAT_SEP, false);
            }
            return field;
        }

        Property parseProperty(CommonClassEntry entry, TokenStream gStream)
        {
            Property property = new Property();
            property.FromClassEntry(entry);

            // Readonly property (starts with '->')
            if (gStream.Is(SHORT_FUNC_DECL))
            {
                // Short readonly properties must contain expression, not block
                Method getter = new Method();
                getter.FromClassEntry(entry);
                getter.Begin = gStream.TokenPosition;

                gStream.SkipTo(STAT_SEP, false);

                property.Getter = getter;
            }
            else if (gStream.Is(BLOCK_O))
            {
                while (!gStream.Eof)
                {
					if (gStream.Next() == GETTER_DECL)
					{
						if (property.Getter != null)
							InfoProvider.AddError("Getter for this property already exists", ExceptionType.MultipleGetters, gStream.SourcePosition);
						property.Getter = parsePropertyFunction(entry, gStream);
					}
					else if (gStream.Is(SETTER_DECL))
					{
						if (property.Setter != null)
							InfoProvider.AddError("Setter for this property already exists", ExceptionType.MultipleSetters, gStream.SourcePosition);
						property.Setter = parsePropertyFunction(entry, gStream);
					}
                    else if (gStream.Is(BLOCK_C))
                        break;
                    // TODO: Reporting illegal token
                }
            }
            return property;
        }

        Method parsePropertyFunction(CommonClassEntry entry, TokenStream gStream)
        {
            Method function = new Method();
            function.FromClassEntry(entry);
            function.Begin = gStream.TokenPosition;

            if (gStream.Next() == BLOCK_O)
                gStream.SkipBraced(BLOCK_CHAR_O, BLOCK_CHAR_C);
            else
            {
                gStream.PushBack();
                gStream.SkipTo(STAT_SEP, true);
            }

            return function;
        }

        Method parseMethod(CommonClassEntry entry, TokenStream gStream)
        {
            Method method = new Method();
            method.FromClassEntry(entry);

            ParameterList paramList = readParams(gStream);
            method.Parameters = paramList;

            if (gStream.Next() == SHORT_FUNC_DECL)
			{
				// Saving method start token (points after '->' symbol)
				method.Begin = gStream.TokenPosition;
            }
            else if (gStream.Is(BLOCK_O))
            {
                // Saving method start token (points on '{' symbol)
                method.Begin = gStream.TokenPosition - BLOCK_O.Length;

                // Skipping method body
                gStream.SkipBraced(BLOCK_CHAR_O, BLOCK_CHAR_C);
            }

            return method;
        }

        ParameterList readParams(TokenStream gStream)
        {
            var plist = new ParameterList();
            gStream.Next();

            while (!gStream.Eof)
            {
                if(gStream.Is(PAR_C))
                    break;

                var param = new Variable();

                param.Name = gStream.GetIdentifier();
                param.Type = gStream.NextType(false);

                plist.Add(param);

                if(gStream.Current != SEP)
                    InfoProvider.AddError("`,` or `)` expected in parameter declaration", ExceptionType.IllegalToken, gStream.SourcePosition);
            }

            return plist;
        }

        CommonClassEntry readCommonClassEntry(TokenStream gStream)
        {               
            CommonClassEntry entry = new CommonClassEntry();

            entry.AttributeList = bindedAttributeList;
            entry.Scope = readScope(gStream);
            entry.DeclarationPosition = gStream.SourcePosition;
            entry.Modifiers = readModifiers(gStream);
            entry.Type = gStream.CurrentType(true);
            entry.Name = gStream.GetIdentifierNext();

            bindedAttributeList.Clear();

            return entry;
        }

        Scope readScope(TokenStream gStream)
        {
            var token = gStream.Next();
            Scope scope = Scope.Private;
            switch (token.ToString())
            {
                case PUBLIC:
                    scope = Scope.Public;
                    break;
                case PROTECTED:
                    scope = Scope.Protected;
                    break;
                case PRIVATE:
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
            // [static [native|const]]
            var token = gStream.Next();
            ClassEntryModifiers modifiers = ClassEntryModifiers.None;
            if (token == STATIC)
            {
                modifiers |= ClassEntryModifiers.Static;
                token = gStream.Next();
            }

            if (token == NATIVE)
            {
                modifiers |= ClassEntryModifiers.Native;
                gStream.Next();
            }
            else if(token == CONST) 
            {
                modifiers |= ClassEntryModifiers.Const;
                gStream.Next();
            }
            return modifiers;
        }

        void compileFile(string src)
        {
            foreach(var clazz in classList)
			{
				Console.WriteLine("Class `{0}`", clazz.Name);
				Console.WriteLine("\tFields:");
				foreach (var field in clazz.SymbolTable.Fields)
					Console.WriteLine("\t\t{0}, {1} `{2}` of type `{3}`", field.Modifiers, field.Scope, field.Name, field.Type.ToString());
				Console.WriteLine("\tProperties:");
				foreach (var prop in clazz.SymbolTable.Properties)
					Console.WriteLine("\t\t{0}, {1}, `{2}` of type `{3}`", prop.Modifiers, prop.Scope, prop.Name, prop.Type.ToString());
				Console.WriteLine("\tMethods:");
                foreach (var method in clazz.SymbolTable.Methods)
                    Console.WriteLine("\t\t{0}, {1} `{2}` of type `{3}` with params `{4}`", 
                                      method.Modifiers, method.Scope, method.Name, method.Type.ToString(), method.Parameters.ToString());
            }
        }

        public static bool IsPlainType(string type, bool includeVoid=false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }
    }
}


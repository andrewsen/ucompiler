﻿using System;
using System.Linq;
using System.IO;

using static Translator.Alphabet;
using System.Collections.Generic;
using uc;

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

            while (!gStream.NextEof())
            {
                // On most top level we have only attributres, classes and imports
                Token token = gStream.Current;
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

                if(gStream.IsNext(BLOCK_C))
                    break;
                gStream.PushBack();
            }

            classList.Add(clazz);
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
            //gStream.Next();
            if(gStream.IsNext(PAR_C))
                return plist;

            while (!gStream.Eof)
            {

                var param = new Variable();
                
                param.Type = gStream.CurrentType(false);
                param.Name = gStream.GetIdentifierNext();

                plist.Add(param);

                if(gStream.IsNext(PAR_C))
                    break;
                else if(gStream.Is(SEP))
                    gStream.Next();
                else
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
            InfoProvider.Print();
        }

        public static bool IsPlainType(string type, bool includeVoid=false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }

        //
        // Parse
        //

        public CodeBlock parseBlock(TokenStream gStream)
        {
            var block = new CodeBlock();
            while (!gStream.Eof)
            {
                var entryToken = gStream.Next();
                if (entryToken.Type == TokenType.EOF)
                    InfoProvider.AddError("Missing end of block statement (`end;`)", ExceptionType.FlowError, gStream.SourcePosition, true);
                if (entryToken.Type == TokenType.Unknown)
                    InfoProvider.AddError("Bad expression", ExceptionType.BadExpression, gStream.SourcePosition, true);
                switch (entryToken.Representation.ToLower())
                {
                    case IF:
                        evalIf(block, gStream);
                        break;
                    case BLOCK_O:
                        block.Children.Add(parseBlock(gStream));
                        break;
                    case BLOCK_C:
                        if (gStream.Next().Type != TokenType.Semicolon)
                            gStream.PushBack();
                        //gStream.CheckNext(STAT_SEP, ExceptionType.SemicolonExpected);
                        return block;
                    default:
                        gStream.PushBack();
                        block.Children.Add(new Statement(buildAST(parseExpression(gStream, TokenType.Semicolon))));
                        break;
                }
            }
            InfoProvider.AddError("Block closing statement is missing", ExceptionType.Brace, gStream.SourcePosition);
            return block;
        }

        public void evalIf(CodeBlock block, TokenStream gStream)
        {
            // Shit. Rewrite
            var ifExpr = new If();

            //gStream.CheckNext(PAR_O, ExceptionType.MissingParenthesis);

            string collectedExpr = "";
            int parCounter = 1;
            while(gStream.Current.Representation.ToLower() != "then")
            {
                if (gStream.Eof || gStream.Next().Type == TokenType.EOF)
                {
                    InfoProvider.AddError("Flow error", ExceptionType.Brace, gStream.SourcePosition, true);
                }
                //if (gStream.Is(PAR_O))
                //parCounter++;
                if (gStream.Current.Representation.ToLower() == "then")
                    break;
                    //parCounter++;
                //else if (gStream.Is(PAR_C))
                //{
                    //parCounter--;
                    //if (parCounter == 0)
                        //break;
                //}
                collectedExpr += gStream.Current.Representation + " ";
            }
            gStream.PushBack();
            if (collectedExpr.Length == 0)
                InfoProvider.AddError("Empty statement", ExceptionType.BadExpression, gStream.SourcePosition);

            ifExpr.Condition = buildAST(parseExpression(new TokenStream(collectedExpr, gStream.SourcePosition.File)));
            if (ifExpr.Condition.Token.IsOp() && ifExpr.Condition.Token.Operation.Type == OperationType.Assign)
                InfoProvider.AddError("Assignment is forbidden in conditions", ExceptionType.IllegalToken, gStream.SourcePosition);

            block.Children.Add(ifExpr);

            gStream.CheckNext(THEN, ExceptionType.BadExpression, true);
            gStream.CheckNext(BLOCK_O, ExceptionType.BadExpression, true);
            ifExpr.Block = parseBlock(gStream);
        }

        public Expression parseExpression(TokenStream gStream, TokenType stopToken = TokenType.EOF)
        {
            // If (a>b) then begin end;
            // if (a>b) {}
            // TODO: Andrew Senko: No type control. No variable declaration control. Implement all of this

            List<Token> resultingExpression = new List<Token>();
            Stack<Token> opStack = new Stack<Token>();
            bool isLastOp = true;
            bool isLastBrace = false;
            bool isLastIdent = false;

            while(gStream.Next().Type != stopToken)
            {
                //if(gStream.Current.Type == TokenType.EOF)
                //{
                //    InfoProvider.AddError("Semicilion `;` is missed", ExceptionType.SemicolonExpected, gStream.SourcePosition);
                //    break;
                //}
                var token = gStream.Current;
                if(token.IsConstant() || token.IsIdentifier())
                {
                    if (token.Representation.ToLower() == "end")
                    {
                        gStream.PushBack();
                        break;
                    }
                    if (isLastBrace)
                        InfoProvider.AddError("Unexpected identifier after brace", ExceptionType.Brace, gStream.SourcePosition);
                    if (isLastIdent)
                        InfoProvider.AddError("Unexpected identifier", ExceptionType.BadExpression, gStream.SourcePosition);
                    resultingExpression.Add(token);
                    isLastOp = false;
                    isLastBrace = false;
                    isLastIdent = true;
                }
                else if (token.IsOp())
                {
                    /*
                     * Пока присутствует на вершине стека токен оператор op2, и
                     * Либо оператор op1 лево-ассоциативен и его приоритет меньше, чем у оператора op2 либо равен,
                     * или оператор op1 право-ассоциативен и его приоритет меньше, чем у op2,
                     * переложить op2 из стека в выходную очередь;
                    */
                    while (opStack.Count > 0 && opStack.Peek().IsOp())
                        if ((token.Operation.Association == Association.Left && token.Operation.Priority < opStack.Peek().Operation.Priority)
                           || (token.Operation.Association == Association.Right && token.Operation.Priority <= opStack.Peek().Operation.Priority))
                        {
                            resultingExpression.Add(opStack.Pop());
                        }
                        else
                            break;
                    opStack.Push(token);
                    isLastOp = true;
                    isLastBrace = false;
                    isLastIdent = false;
                }
                else if (token == INDEXER_O)
                {
                    if (isLastOp)
                        InfoProvider.AddError("Unexpected `[`", ExceptionType.Brace, gStream.SourcePosition);
                    opStack.Push(token);
                    isLastIdent = false;
                    isLastOp = true;
                }
                else if (token == INDEXER_C)
                {
                    // a = b[c + 4];
                    // a b[c + 4] - 6 =
                    // a b c 4 + [] 6 - =
                    if (isLastOp)
                        InfoProvider.AddError("Unexpected `]`", ExceptionType.Brace, gStream.SourcePosition);

                    while (opStack.Count > 0 && opStack.Peek() != INDEXER_O)
                    {
                        if ((opStack.Peek() == PAR_O))
                            InfoProvider.AddError("`)` is missed", ExceptionType.MissingParenthesis, gStream.SourcePosition);
                        resultingExpression.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0)
                        InfoProvider.AddError("`[` is missed", ExceptionType.Brace, gStream.SourcePosition);
                    else
                        opStack.Pop();
                    token.Representation = "[]";
                    token.Type = TokenType.Operator;
                    token.Operation = Operation.From("[]");
                    resultingExpression.Add(token);
                    isLastBrace = true;
                }
                else if (token == PAR_O)
                {
                    if (!isLastOp)
                        InfoProvider.AddError("Unexpected `(`", ExceptionType.Brace, gStream.SourcePosition);
                    opStack.Push(token);
                }
                else if (token == PAR_C)
                {
                    if (isLastOp)
                        InfoProvider.AddError("Unexpected `)`", ExceptionType.Brace, gStream.SourcePosition);
                    while (opStack.Count > 0 && opStack.Peek() != PAR_O)
                        resultingExpression.Add(opStack.Pop());
                    if (opStack.Count == 0)
                        InfoProvider.AddError("`(` is missed", ExceptionType.MissingParenthesis, gStream.SourcePosition);
                    else
                        opStack.Pop();
                    isLastBrace = true;
                }
                else
                    InfoProvider.AddError("Unexpected token", ExceptionType.IllegalToken, gStream.SourcePosition);
            }
            while(opStack.Count > 0)
            {
                var op = opStack.Pop();
                if (op == PAR_O)
                    InfoProvider.AddError("`)` is missed", ExceptionType.MissingParenthesis, gStream.SourcePosition);
                if (op == INDEXER_O)
                    InfoProvider.AddError("`]` is missed", ExceptionType.Brace, gStream.SourcePosition);
                resultingExpression.Add(op);
            }
            return new Expression(resultingExpression);
        }

        public Node buildAST(Expression expr)
        {
            // a b c * 4 e * + =
            if (expr.Tokens.Count == 0)
                return null;
            var count = expr.Tokens.Count;
            return buildNode(new Stack<Token>(expr.Tokens), expr.SourcePosition);
        }

        public Node buildNode(Stack<Token> rpn, SourcePosition position)
        {
            if (rpn.Count == 0)
                return null;
            var tok = rpn.Pop();

            Node result = new Node(tok);
            if(tok.IsOp())
            {
                result.Right = buildNode(rpn, position);
                result.Left = buildNode(rpn, position);

                if (result.Left == null && result.Right == null)
                    InfoProvider.AddError($"Both arguments of {tok} is missed", ExceptionType.BadExpression, position);
                else if (result.Left == null || result.Right == null)
                    InfoProvider.AddError($"Argument of {tok} is missed", ExceptionType.BadExpression, position);
                else if (tok.Operation.Type == OperationType.Assign && result.Left.Token.IsConstant())
                    InfoProvider.AddError("rvalue can't be on left side of assignment operator", ExceptionType.lValueExpected, position);
                // Pascal specific
                else if (result.Left.Token.Operation?.Type == OperationType.Assign || result.Right.Token.Operation?.Type == OperationType.Assign)
                    InfoProvider.AddError($"Assignment operator can be only as a root node", ExceptionType.BadExpression, position);
            }

            return result;
        }
    }
}


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
        public CompilerConfig Config;
        private AttributeList bindedAttributeList = new AttributeList();
        private DirectiveList directiveList = new DirectiveList();
        private MetadataList metadataList = new MetadataList();
        private ClassList classList = new ClassList();

        private List<Token> defaultSyncList = new List<Token>
        {
            new Token { Representation = SEMICOLON, Type = TokenType.Semicolon },
            new Token { Representation = BLOCK_C, Type = TokenType.Delimiter },
        };

        private List<string> keywords = new List<string>
        {
            VAR, IF, ELSE, FOR, WHILE, FOREACH, DO, BREAK, CONTINUE, RETURN, SWITCH, CASE,
            DEFATULT, IMPORT, CLASS, STATIC, NATIVE, CONST, PUBLIC, PRIVATE, PROTECTED, NEW, IS, AS
        };

        public Compiler(CompilerConfig config)
        {
            Config = config;
        }

        public void Compile()
        {
            foreach (var src in Config.Sources)
            {
                // Parsing classes and their content (fields, props, methods)
                parseGlobalScope(src);
            }

            foreach (var clazz in classList)
            {
                compileClass(clazz);
            }

            foreach (var src in Config.Sources)
            {
                // Compiling 
                // TODO: Rework it
                compileFile(src);
            }

            CodeGen gen = new CodeGen(Config, directiveList, metadataList, classList);
            gen.Generate();
        }

        private void compileClass(ClassType clazz)
        {
            foreach (var method in clazz.SymbolTable.Methods)
            {
                evalMethod(method);
            }
        }

        private void parseGlobalScope(string src)
        {
            // FIXME: Only for testing
            //TokenStream gStream = new TokenStream(File.ReadAllText(src), src);
            TokenStream gStream = new TokenStream(src, "<testing>");

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

        private void parseAttribute(TokenStream gStream)
        {
            AttributeReader reader = new AttributeReader(gStream);

            var attr = reader.Read();

            switch (attr.Name)
            {
                case "AllowBuiltins":
                    Config.AllowBuiltins = true;
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
                    if (Config.OutBinaryFile == null)
                        Config.OutBinaryFile = (attr.Data[0].Value as string) + ".vas"; // TODO: FIXME
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
                        if (!Config.DebugBuild)
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

        private void parseImport(TokenStream gStream)
        {
            throw new NotImplementedException();
        }

        private void parseClass(TokenStream gStream, Scope classScope)
        {
            ClassType clazz = new ClassType(gStream.GetIdentifierNext());
            clazz.DeclarationPosition = gStream.SourcePosition;
            clazz.Scope = classScope;
            clazz.AttributeList = bindedAttributeList;
            bindedAttributeList.Clear();

            gStream.CheckNext(BLOCK_O, TokenType.Delimiter, ExceptionType.Brace);

            while (!gStream.Eof)
            {
                var entry = readCommonClassEntry(clazz, gStream);
                entry.Class = clazz;

                gStream.Next();
                //TODO: Move attributes parsing to readCommonClassEntry()
                if (gStream.Is(ATTR))
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

                if (gStream.IsNext(BLOCK_C))
                    break;
                gStream.PushBack();
            }

            classList.Add(clazz);
        }

        private Field parseField(CommonClassEntry entry, TokenStream gStream)
        {
            Field field = new Field();
            if (gStream.Is(SEMICOLON) && entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                InfoProvider.AddError("Const field must be initialized immediately", ExceptionType.UninitedConstant, gStream.SourcePosition);
            field.FromClassEntry(entry);
            if (gStream.Is(ASSIGN))
            {
                gStream.Next();
                field.InitialExpressionPosition = gStream.TokenPosition;
                gStream.SkipTo(SEMICOLON, false);
            }
            return field;
        }

        private Property parseProperty(CommonClassEntry entry, TokenStream gStream)
        {
            Property property = new Property();
            property.FromClassEntry(entry);

            // Readonly property (starts with '->')
            if (gStream.Is(SHORT_FUNC_DECL))
            {
                // Short readonly properties must contain expression, not block
                Method getter = new Method();
                getter.FromClassEntry(entry);
                getter.BodyStream = gStream.Fork();

                gStream.SkipTo(SEMICOLON, false);

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
                    else
                        InfoProvider.AddError("Unexpected symbol", ExceptionType.UnexpectedToken, gStream.SourcePosition);
                    // TODO: Reporting illegal token
                }
            }
            return property;
        }

        private Method parsePropertyFunction(CommonClassEntry entry, TokenStream gStream)
        {
            Method function = new Method();
            function.FromClassEntry(entry);
            function.BodyStream = gStream.Fork();

            if (gStream.Next() == BLOCK_O)
                gStream.SkipBraced(BLOCK_CHAR_O, BLOCK_CHAR_C);
            else
            {
                gStream.PushBack();
                gStream.SkipTo(SEMICOLON, true);
            }

            return function;
        }

        private Method parseMethod(CommonClassEntry entry, TokenStream gStream)
        {
            Method method = new Method();
            method.FromClassEntry(entry);

            ParameterList paramList = readParams(gStream);
            method.Parameters = paramList;

            if (gStream.Next() == SHORT_FUNC_DECL)
            {
                method.DeclarationForm = DeclarationForm.Short;
                // Saving method start token (points after '->' symbol)
                method.BodyStream = gStream.Fork();
            }
            else if (gStream.Is(BLOCK_O))
            {
                method.DeclarationForm = DeclarationForm.Full;
                // Saving method start token (points on '{' symbol)
                method.BodyStream = gStream.Fork(); //gStream.TokenPosition - BLOCK_O.Length;

                // Skipping method body
                gStream.SkipBraced(BLOCK_CHAR_O, BLOCK_CHAR_C);
            }
            else
            {
                InfoProvider.AddError("Unexpected symbol", ExceptionType.UnexpectedToken, gStream.SourcePosition);
            }

            return method;
        }

        private ParameterList readParams(TokenStream gStream)
        {
            var plist = new ParameterList();
            //gStream.Next();
            if (gStream.IsNext(PAR_C))
                return plist;

            while (!gStream.Eof)
            {

                var param = new Parameter();

                param.Type = gStream.CurrentType();
                param.DeclarationPosition = gStream.SourcePosition;
                param.Name = gStream.GetIdentifierNext();

                plist.Add(param);

                if (gStream.IsNext(PAR_C))
                    break;
                else if (gStream.Is(COMMA))
                    gStream.Next();
                else
                    InfoProvider.AddError("`,` or `)` expected in parameter declaration", ExceptionType.IllegalToken, gStream.SourcePosition);
            }

            return plist;
        }

        private CommonClassEntry readCommonClassEntry(ClassType clazz, TokenStream gStream)
        {
            CommonClassEntry entry = new CommonClassEntry();

            entry.AttributeList = bindedAttributeList;
            entry.Scope = readScope(gStream);
            entry.DeclarationPosition = gStream.SourcePosition;
            entry.Modifiers = readModifiers(gStream);
            entry.Type = gStream.CurrentType(TypeReaderConf.IncludeVoid);
            if(gStream.Next().Type == TokenType.Identifier)
                entry.Name = gStream.Current;
            else if(gStream.Is(PAR_O, TokenType.Delimiter))
            {
                if (entry.Type.Type != DataTypes.Class)
                    InfoProvider.AddError("Constructor name must be same with class name", ExceptionType.ConstructorName, gStream.SourcePosition);

                var classType = entry.Type as ClassType;
                if (classType.Name != clazz.Name)
                    InfoProvider.AddError("Constructor name must be same with class name", ExceptionType.ConstructorName, gStream.SourcePosition);

                entry.Name = clazz.Name;
                entry.Type = null;
                gStream.PushBack();
            }

            bindedAttributeList.Clear();

            return entry;
        }

        private Scope readScope(TokenStream gStream)
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

        private ClassEntryModifiers readModifiers(TokenStream gStream)
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
            else if (token == CONST)
            {
                modifiers |= ClassEntryModifiers.Const;
                gStream.Next();
            }
            return modifiers;
        }

        private void evalMethod(Method method)
        {
            if (method.DeclarationForm == DeclarationForm.Full)
            {
                method.Body = evalBlock(new CodeBlock(method.Class, method.Class, method), method.BodyStream);
                method.Body.Parent = method.Class;
                method.Body.ClassContext = method.Class;
            }
            else
                InfoProvider.AddError("Short form is unsupported now", ExceptionType.NotImplemented, method.DeclarationPosition);
        }

        private CodeBlock evalBlock(CodeBlock parent, TokenStream lStream)
        {
            CodeBlock block = new CodeBlock(parent);

            while (!lStream.Eof)
            {
                var entryToken = lStream.Next();

                if (!(entryToken.IsIdentifier() || entryToken.IsSemicolon() || entryToken.IsOp() || (entryToken.IsOneOf(TokenType.Delimiter, BLOCK_C))))
                    InfoProvider.AddError("Identifier, control keyword, type or block expected", ExceptionType.IllegalToken, lStream.SourcePosition);

                switch (entryToken.Representation)
                {
                    case IF:
                        evalIf(block, lStream);
                        break;
                    case FOR:
                        evalFor(block, lStream);
                        break;
                    case WHILE:
                        evalWhile(block, lStream);
                        break;
                    case DO:
                        evalDoWhile(block, lStream);
                        break;
                    case BREAK:
                        evalBreak(block, lStream);
                        break;
                    case CONTINUE:
                        evalContinue(block, lStream);
                        break;
                    case SWITCH:
                        throw new InternalException("Unimplemented yet");
                        break;
                    case FOREACH:
                        throw new InternalException("Unimplemented yet");
                        break;
                    case RETURN:
                        // TODO: Andrew Senko: Add control flow control (lol)
                        evalReturn(block, lStream);
                        break;
                    case BLOCK_O:
                        evalBlock(parent, lStream);
                        break;
                    case BLOCK_C:
                        return block;
                    case ELSE:
                        InfoProvider.AddError("Unbound `else` statement", ExceptionType.UnboundElse, lStream.SourcePosition);
                        break;
                    case CASE:
                        InfoProvider.AddError("Unbound `case` statement", ExceptionType.UnboundCase, lStream.SourcePosition);
                        break;
                    case DEFATULT:
                        InfoProvider.AddError("Unbound `default` statement", ExceptionType.UnboundDefault, lStream.SourcePosition);
                        break;
                    default:
                        evalStatement(block, lStream);
                        break;
                }
            }

            return block;
        }

        private IExpression evalBlockOrStatement(CodeBlock parent, TokenStream lStream)
        {
            if (lStream.Is(BLOCK_O, TokenType.Delimiter))
                return evalBlock(parent, lStream);
            return evalExpression(parent, lStream);
        }

        private void evalIf(CodeBlock parent, TokenStream lStream)
        {
            If ifStatement = new If(parent);
            ifStatement.MasterIf = evalConditionalPart(parent, lStream);
            evalIfTail(ifStatement, parent, lStream);
            parent.Expressions.Add(ifStatement);
        }

        private ConditionalPart evalConditionalPart(CodeBlock parent, TokenStream lStream)
        {
            ConditionalPart ifPart = new ConditionalPart();
            var position = lStream.SourcePosition;

            // Now token stream points on next token `if` (should be `(` token)
            if(!lStream.IsNext(PAR_O, TokenType.Delimiter))
            {
                InfoProvider.AddError("`(` expected after `if` keyword as a beginning of condition statement", ExceptionType.MissingParenthesis, lStream.SourcePosition);
                lStream.PushBack(); // Doing rollback because that symbol might be a start of expression
            }

            // Pointing to start of the condition expression
            lStream.Next();
            var condition = evalExpression(parent, lStream, ParsingPolicy.Both);
            if(condition.ExpressionRoot.Type.Type != DataTypes.Bool)
                InfoProvider.AddError("Condition in `if` clause should have boolean type, but it does not", ExceptionType.IllegalType, position);

            ifPart.Condition = condition;
            if(lStream.IsNext(BLOCK_O, TokenType.Delimiter))
            {
                // Block'ed if
                // if(...) { ... }
                ifPart.Body = evalBlock(parent, lStream);
            }
            else
            {
                // Statement'ed if
                // if(...) ...;
                ifPart.Body = evalExpression(parent, lStream);
            }
            return ifPart;
        }

        private void evalIfTail(If ifStatement, CodeBlock parent, TokenStream lStream)
        {
            while(lStream.IsNext(ELSE, TokenType.Identifier))
            {
                if (lStream.IsNext(IF, TokenType.Identifier))
                {
                    ifStatement.ElseIfList.Add(evalConditionalPart(parent, lStream));
                }
                else
                {
                    ifStatement.ElsePart = evalBlockOrStatement(parent, lStream);

                    // Stopping
                    return;
                }
            }
            lStream.PushBack();
        }

        private void evalFor(CodeBlock parent, TokenStream lStream)
        {
            For forStatement = new For(parent);
            var position = lStream.SourcePosition;

            // Now token stream points on next token `if` (should be `(` token)
            if (!lStream.IsNext(PAR_O, TokenType.Delimiter))
            {
                InfoProvider.AddError("`(` expected after `for` keyword as a beginning of init statement", ExceptionType.MissingParenthesis, lStream.SourcePosition);
                lStream.PushBack(); // Doing rollback because that symbol might be a start of expression
            }

            evalStatement(forStatement.Scope, lStream.Pass());
            var condition = evalExpression(forStatement.Scope, lStream.Pass());

            if (condition.ExpressionRoot.Type.Type != DataTypes.Bool)
                InfoProvider.AddError("Condition in `for` clause should have boolean type, but it does not", ExceptionType.IllegalType, position);

            forStatement.Condition = condition;
            forStatement.Iteration = evalExpression(forStatement.Scope, lStream.Pass(), ParsingPolicy.Both);

            forStatement.Body = evalBlockOrStatement(parent, lStream.Pass());

            if (lStream.IsNext(ELSE, TokenType.Identifier))
                forStatement.ElsePart = evalBlockOrStatement(parent, lStream);
        }

        private void evalWhile(CodeBlock parent, TokenStream lStream)
        {
            While whileStatement = new While(parent);
            var position = lStream.SourcePosition;

            // Now token stream points on next token `if` (should be `(` token)
            if (!lStream.IsNext(PAR_O, TokenType.Delimiter))
            {
                InfoProvider.AddError("`(` expected after `while` keyword as a beginning of condition statement", ExceptionType.MissingParenthesis, lStream.SourcePosition);
                lStream.PushBack(); // Doing rollback because that symbol might be a start of expression
            }

            // Pointing to start of the condition expression
            var condition = evalExpression(parent, lStream.Pass(), ParsingPolicy.Both);

            if (condition.ExpressionRoot.Type.Type != DataTypes.Bool)
                InfoProvider.AddError("Condition in `while` clause should have boolean type, but it does not", ExceptionType.IllegalType, position);

            whileStatement.Condition = condition;

            whileStatement.Body = evalBlockOrStatement(parent, lStream.Pass());

            if (lStream.IsNext(ELSE, TokenType.Identifier))
                whileStatement.ElsePart = evalBlockOrStatement(parent, lStream.Pass());
        }

        private void evalDoWhile(CodeBlock parent, TokenStream lStream)
        {
            DoWhile whileStatement = new DoWhile(parent);
            var position = lStream.SourcePosition;

            lStream.CheckNext(BLOCK_O, TokenType.Delimiter, ExceptionType.Brace);

            whileStatement.Body = evalBlock(parent, lStream);
                
            lStream.CheckNext(WHILE, TokenType.Identifier, ExceptionType.KeywordExpected);

            // Now token stream points on next token `if` (should be `(` token)
            if (!lStream.IsNext(PAR_O, TokenType.Delimiter))
            {
                InfoProvider.AddError("`(` expected after `while` keyword as a beginning of condition statement", ExceptionType.MissingParenthesis, lStream.SourcePosition);
                lStream.PushBack(); // Doing rollback because that symbol might be a start of expression
            }

            // Pointing to start of the condition expression
            var condition = evalExpression(parent, lStream.Pass(), ParsingPolicy.Both);

            if (condition.ExpressionRoot.Type.Type != DataTypes.Bool)
                InfoProvider.AddError("Condition in `while` clause should have boolean type, but it does not", ExceptionType.IllegalType, position);

            whileStatement.Condition = condition;
            lStream.CheckNext(SEMICOLON, TokenType.Semicolon, ExceptionType.Brace);

            // No `else` case for now. Sorry :(
            //lStream.Next();
            //if (lStream.IsNext(ELSE, TokenType.Identifier))
            //    whileStatement.ElsePart = evalBlockOrStatement(parent, lStream);
        }

        private void evalBreak(CodeBlock parent, TokenStream lStream)
        {
            lStream.CheckNext(SEMICOLON, TokenType.Semicolon, ExceptionType.Brace);
            parent.Expressions.Add(new Break());
        }

        private void evalContinue(CodeBlock parent, TokenStream lStream)
        {
            lStream.CheckNext(SEMICOLON, TokenType.Semicolon, ExceptionType.Brace);
            parent.Expressions.Add(new Continue());
        }

        private void evalReturn(CodeBlock parent, TokenStream lStream)
        {
            Return retStatement = new Return(parent);
            var position = lStream.SourcePosition;

            lStream.Next();
            var expr = evalExpression(parent, lStream);

            var retType = parent.MethodContext.Type;
            if (!retType.Equals(expr.ExpressionRoot.Type))
            {
                var result = tryCreateImplicitCast(expr.ExpressionRoot, retType);
                if (result == null)
                    InfoProvider.AddFatal($"Can't cast return type {expr.ExpressionRoot.Type.ToString()} to method's type {retType.ToString()}",
                                          ExceptionType.IllegalType, position);
                expr.ExpressionRoot = result;
            }

            retStatement.Expression = expr;
            parent.Expressions.Add(retStatement);
        }

        public void evalStatement(CodeBlock parent, TokenStream lStream)
        {
            Variable localVar = null;
            if (isRegisteredType(lStream.Current))
            {
                localVar = evalLocalVarDeclaration(parent, lStream);
                parent.AddLocal(localVar);

                if (lStream.IsNext(SEMICOLON, TokenType.Semicolon))
                    return;
                if (!lStream.Is(ASSIGN, TokenType.Operator))
                    InfoProvider.AddError("Assignment operator expected after variable declaration", ExceptionType.AssignmentExpected, lStream.SourcePosition);
                lStream.PushBack();
            }

            var expr = evalExpression(parent, lStream);
            parent.Expressions.Add(expr);
        }

        private Expression evalExpression(CodeBlock parent, TokenStream lStream, ParsingPolicy parsingPolicy = ParsingPolicy.SyncTokens)
        {
            var pos = lStream.SourcePosition;
            var expr = buildPostfixForm(lStream, parsingPolicy);

            if(expr.Count == 0)
                return new Expression(new Node(null) { Type = new PlainType(DataTypes.Void) }, pos);
            
            var rootNode = expressionToAST(expr);

            // If errors were detected during syntax alanysis - handle it and stop compilation
            if (InfoProvider.HasErrors)
                InfoProvider.InvokeErrorHandler();
            assignTypes(rootNode, parent);

            var result = new Expression(rootNode, pos);
            return result;
        }

        private Variable evalLocalVarDeclaration(CodeBlock parent, TokenStream lStream)
        {
            var declPosition = lStream.SourcePosition;
            var type = lStream.CurrentType(TypeReaderConf.IncludeVar);
            var name = lStream.GetIdentifierNext();

            checkNamingUsage(lStream.Current, parent);

            if (type is ClassType clazz)
            {
                type = classList.Find(clazz.Name, lStream.SourcePosition);
            }
            else if (type is ArrayType array && array.Inner is ClassType innerClazz)
            {
                array.Inner = classList.Find(innerClazz.Name, lStream.SourcePosition);
            }

            Variable local = new Variable()
            {
                Name = name,
                Type = type,
                DeclarationPosition = declPosition,
            };

            return local;
        }

        private void checkNamingUsage(Token nameToken, CodeBlock parent)
        {
            if(!nameToken.IsIdentifier())
                InfoProvider.AddFatal($"Identifier expected as variable name", ExceptionType.NamingViolation, nameToken.Position);
            else if(keywords.Contains(nameToken.Representation))
                InfoProvider.AddFatal($"Forbidden to use reserved words as identifiers", ExceptionType.NamingViolation, nameToken.Position);
            else if(parent.HasDeclaration(nameToken))
            {
                var previousDecl = parent.FindDeclaration(nameToken);
                InfoProvider.AddError($"Identifier is already in use. Redefinition of {previousDecl.DeclarationPosition}", ExceptionType.VariableRedefinition, nameToken.Position);
            }
        }

        public List<Token> buildPostfixForm(TokenStream stream, ParsingPolicy parsingPolicy = ParsingPolicy.SyncTokens, List<Token> syncTokens = null)
        {
            // TODO: Andrew Senko: Add unary +/- support

            List<Token> resultingExpression = new List<Token>();
            Stack<int> functionArgCounter = new Stack<int>();
            Stack<Token> opStack = new Stack<Token>();
            Token lastToken = Token.EOF;

            int bracketsCount = 0;

            // Lol below
            syncTokens = syncTokens ?? defaultSyncList;

            /// <summary>
            /// Satisfieses the policy.
            /// </summary>
            /// <returns><c>true</c>, if policy was satisfiesed, <c>false</c> otherwise.</returns>
            /// <param name="token">Token.</param>
            bool satisfiesPolicy(Token token)
            {
                if (token == Token.EOF)
                    return false;
                switch (parsingPolicy)
                {
                    case ParsingPolicy.Brackets:
                        return bracketsCount >= 0;
                    case ParsingPolicy.SyncTokens:
                        return !syncTokens.Any(t => t.Representation == token.Representation && t.Type == token.Type);
                    case ParsingPolicy.Both:
                        return bracketsCount >= 0 && !syncTokens.Any(t => t.Representation == token.Representation && t.Type == token.Type);
                }
                throw new ArgumentOutOfRangeException(nameof(parsingPolicy), "Invalid parsing policy");
            }

            while (satisfiesPolicy(stream.Current))
            {
                var token = stream.Current;
                if (token.IsConstant() || token.IsIdentifier())
                {
                    if (lastToken.IsOneOf(TokenType.Delimiter, PAR_C, INDEXER_C))
                        InfoProvider.AddError("Unexpected identifier after brace", ExceptionType.Brace, stream.SourcePosition);
                    if (lastToken.IsIdentifier())
                        InfoProvider.AddError("Unexpected identifier", ExceptionType.BadExpression, stream.SourcePosition);
                    resultingExpression.Add(token);
                    lastToken = token;
                }
                else if (token.IsOp())
                {
                    /*
                     * Пока присутствует на вершине стека токен оператор op2, и
                     * Либо оператор op1 лево-ассоциативен и его приоритет меньше, чем у оператора op2 либо равен,
                     * или оператор op1 право-ассоциативен и его приоритет меньше, чем у op2,
                     * переложить op2 из стека в выходную очередь;
                    */

                    // Determine if it is prefix or postfix 
                    if (token.Operation.Is(OperationType.Inc, OperationType.Dec))
                    {
                        if (lastToken == Token.EOF || lastToken.IsOp() || lastToken.IsOneOf(TokenType.Delimiter, INDEXER_O, PAR_O))
                        {
                            token.Operation.Type = token.Operation.Type | OperationType.PreMask;
                            token.Operation.Priority++;
                        }
                        else
                        {
                            token.Operation.Type = token.Operation.Type | OperationType.PostMask;
                            token.Operation.Association = Association.Right;
                        }
                    }
                    else if(token.Operation.Is(OperationType.New))
                    {
                        var sourcePosition = stream.SourcePosition;
                        var tokenPosition = stream.TokenPosition;
                        var type = stream.NextType(TypeReaderConf.Soft);

                        if(stream.IsNext(PAR_O, TokenType.Delimiter))
                        {
                            if (!(type is ClassType))
                                InfoProvider.AddError("Operator `new` not appliable to this type", ExceptionType.IllegalType, sourcePosition);
                            token.Operation.Type = OperationType.NewObj;
                            stream.TokenPosition = tokenPosition; // Next token will be identifier - constructor call
                        }
                        else if(stream.Is(INDEXER_O, TokenType.Delimiter))
                        {
                            stream.PushBack();
                            token.Operation.Type = OperationType.NewArr;
                            token.Representation = "new[]";
                            resultingExpression.Add(new TypedToken(type) { Position = sourcePosition });
                        }
                    }

                    while (opStack.Count > 0 && opStack.Peek().IsOp())
                    {
                        if ((token.Operation.Association == Association.Left && token.Operation.Priority <= opStack.Peek().Operation.Priority)
                           || (token.Operation.Association == Association.Right && token.Operation.Priority <= opStack.Peek().Operation.Priority))
                        {
                            resultingExpression.Add(opStack.Pop());
                        }
                        else
                            break;
                    }
                    opStack.Push(token);
                    lastToken = token;
                }
                else if (token == INDEXER_O)
                {
                    if (lastToken.IsOp() && !lastToken.IsOp(OperationType.NewArr))
                        InfoProvider.AddError("Unexpected `[`", ExceptionType.Brace, stream.SourcePosition);
                    opStack.Push(token);
                    lastToken = token;
                }
                else if (token == INDEXER_C)
                {
                    // a = b[c + 4];
                    // a b[c + 4] - 6 =
                    // a b c 4 + [] 6 - =
                    if (lastToken.IsOp())
                        InfoProvider.AddError("Unexpected `]`", ExceptionType.Brace, stream.SourcePosition);

                    while (opStack.Count > 0 && opStack.Peek() != INDEXER_O)
                    {
                        if (opStack.Peek() == PAR_O)
                            InfoProvider.AddError("`)` is missed", ExceptionType.MissingParenthesis, stream.SourcePosition);
                        resultingExpression.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0)
                        InfoProvider.AddError("`[` is missed", ExceptionType.Brace, stream.SourcePosition);
                    else
                        opStack.Pop();
                    token.Representation = "[]";
                    token.Type = TokenType.Operator;
                    token.Operation = Operation.From("[]");
                    resultingExpression.Add(token);
                    lastToken = token;
                }
                else if (token == PAR_O)
                {
                    bracketsCount++;
                    // Process as function
                    // x = fun(1, g(5, y*(3-i), 8));

                    // x = f(1, 2+3, a*(b-c), true);
                    // x f ()=

                    if (lastToken.IsIdentifier())
                    {
                        // Function call operator
                        var callToken = new Token();
                        callToken.Representation = "()";
                        callToken.Type = TokenType.Operator;
                        callToken.Operation = Operation.From("()");

                        // Counting function arguments by commas
                        var lookupStream = stream.Fork();
                        if (lookupStream.IsNext(PAR_C, TokenType.Delimiter))
                            callToken.Operation.ArgumentCount = 0;
                        else
                        {
                            callToken.Operation.ArgumentCount = 1;
                            int callBracketsCount = 1;
                            while (!lookupStream.Eof && callBracketsCount > 0)
                            {
                                if (lookupStream.IsNext(PAR_C, TokenType.Delimiter))
                                    callBracketsCount--;
                                else if (lookupStream.Is(PAR_O))
                                    callBracketsCount++;
                                else if (callBracketsCount == 1 && lookupStream.Is(COMMA, TokenType.Delimiter))
                                    callToken.Operation.ArgumentCount++;
                            }
                        }
                        if (opStack.Count != 0 && !opStack.Peek().IsOp(OperationType.NewObj))
                        {
                            while (opStack.Count > 0 && opStack.Peek().IsOp())
                            {
                                if ((callToken.Operation.Association == Association.Left && callToken.Operation.Priority <= opStack.Peek().Operation.Priority)
                                    || (callToken.Operation.Association == Association.Right && callToken.Operation.Priority <= opStack.Peek().Operation.Priority))
                                {
                                    resultingExpression.Add(opStack.Pop());
                                }
                                else
                                    break;
                            }
                        }

                        opStack.Push(callToken);
                        functionArgCounter.Push(0);
                    }
                    else if (!lastToken.IsOp())
                        InfoProvider.AddError("Unexpected `(`", ExceptionType.Brace, stream.SourcePosition);
                    opStack.Push(token);
                    lastToken = token;
                }
                else if (token == PAR_C)
                {
                    bool isFuncCall = false;
                    bracketsCount--;
                    if (bracketsCount < 0 && (parsingPolicy == ParsingPolicy.Both || parsingPolicy == ParsingPolicy.Brackets))
                        break;

                    if (lastToken.IsOp() && lastToken.Operation.Type != OperationType.FunctionCall)
                        InfoProvider.AddError("Unexpected `)`", ExceptionType.Brace, stream.SourcePosition);
                    while (opStack.Count > 0 && opStack.Peek() != PAR_O)
                    {
                        var op = opStack.Peek();
                        if (op.Operation.Type == OperationType.FunctionCall)
                        {
                            isFuncCall = true;
                        }
                        else
                        {
                            isFuncCall = false;
                        }
                        resultingExpression.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0 && !isFuncCall)
                        InfoProvider.AddError("`(` is missed", ExceptionType.MissingParenthesis, stream.SourcePosition);
                    //else if(!isFuncCall)
                    opStack.Pop();
                }
                else if (token == COMMA)
                {
                    if (!(lastToken.IsConstant() || lastToken.IsIdentifier() || lastToken == PAR_C || lastToken == INDEXER_C))
                        InfoProvider.AddError("Unexpected comma", ExceptionType.UnexpectedComma, stream.SourcePosition);
                    functionArgCounter.Push(functionArgCounter.Pop() + 1);
                    while (opStack.Count != 0 && opStack.Peek() != PAR_O)
                        resultingExpression.Add(opStack.Pop());
                    lastToken = token;
                    //isUnary = true;
                }

                token = stream.Next();
                if(token == BLOCK_C)
                {
                    InfoProvider.AddError("Semicolon expected", ExceptionType.SemicolonExpected, lastToken.TailPosition);
                }
            }
            while (opStack.Count > 0)
            {
                var op = opStack.Pop();
                if (op == PAR_O)
                    InfoProvider.AddError("`)` is missed", ExceptionType.MissingParenthesis, stream.SourcePosition);
                if (op == INDEXER_O)
                    InfoProvider.AddError("`]` is missed", ExceptionType.Brace, stream.SourcePosition);
                resultingExpression.Add(op);
            }
            return resultingExpression;
        }

        public Node expressionToAST(List<Token> expr)
        {
            var root = makeASTNode(new Stack<Token>(expr));

            if(root.Token.Type == TokenType.OperatorAssign)
            {
                var newRoot = new Node(new Token 
                { 
                    Type = TokenType.Operator, 
                    Representation = "=", 
                    Operation = Operation.From("="), 
                    Position = root.Token.Position 
                });

                newRoot.Left = root.Left;
                newRoot.Right = root;
                root.Token = new Token
                {
                    Type = TokenType.Operator,
					Representation = root.Token.Representation.TrimEnd('='),
                    Operation = Operation.From(root.Token.Representation.TrimEnd('=')),
                };
                root = newRoot;
            }

            return root;
        }

        private Node makeASTNode(Stack<Token> polish)
        {
            if (polish.Count == 0)
                return null;
            var tok = polish.Pop();

            Node result = new Node(tok);
            if (tok.IsOp())
            {
                if (tok.Operation.IsUnary)
                {
                    result.Left = makeASTNode(polish);
                    if (result.Left == null)
                        InfoProvider.AddError($"Argument of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                }
                else if (!tok.Operation.IsUnary && tok.Operation.Type != OperationType.FunctionCall)
                {
                    result.Right = makeASTNode(polish);
                    result.Left = makeASTNode(polish);
                    if (result.Left == null && result.Right == null)
                        InfoProvider.AddError($"Both arguments of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                    else if (result.Left == null || result.Right == null)
                        InfoProvider.AddError($"Argument of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                    else if (tok.Operation.Type == OperationType.Assign && result.Left.Token.IsConstant())
                        InfoProvider.AddError("rvalue can't be on left side of assignment operator", ExceptionType.lValueExpected, tok.Position);
                }
                else // if(tok.Operation.Type == OperationType.FunctionCall)
                {
                    int argCount = tok.Operation.ArgumentCount + 1;
                    while (argCount-- != 0)
                        result.AddRight(makeASTNode(polish));
                    if (result.Left == null)
                        InfoProvider.AddError("No function name defined", ExceptionType.FunctionName, tok.Position);
                    else if (!result.Left.Token.IsIdentifier() && !result.Left.Token.IsOp(OperationType.MemberAccess))
                        InfoProvider.AddError("Identifier expected as function name", ExceptionType.FunctionName, tok.Position);
                }
            }

            return result;
        }

        private void assignTypes(Node node, CodeBlock parent, List<IType> paramList = null)
        {
            // TODO: Andrew Senko: Add declaration checks
            List<IType> methodArgs = new List<IType>();
            if (node.Token.IsOp(OperationType.FunctionCall))
            {
                for (int i = 0; i < node.Children.Count - 1; ++i)
                {
                    var child = node.Children[i];
                    assignTypes(child, parent);
                    methodArgs.Add(child.Type);
                    //node.Type = CombineType(node.Type, child.Type);
                }
                var invokable = node.Left;
                assignTypes(invokable, parent, methodArgs);

                var method = invokable.RelatedNamedData as Method;
                int argc = node.Children.Count - 1;
                for (int i = 0; i < argc; ++i)
                {
                    var child = node.Children[i];
                    var parameter = method.Parameters[argc - i - 1];

                    // Casting if not equal types
                    if (!parameter.Type.Equals(child.Type))
                    {
                        Node converter = new Node(new Token { Type = TokenType.Operator, Operation = Operation.Cast, Position = node.Children[i].Token.Position });
                        converter.Type = parameter.Type;
                        converter.Left = child;
                        node.Children[i] = converter;
                    }
                }
                node.Type = invokable.Type;
            }
            else if(node.Token.IsOp(OperationType.NewObj))
            {
                if (node.Children.Count != 1 || !node.Left.Token.IsOp(OperationType.FunctionCall))
                    InfoProvider.AddFatal("Constructor call expected in `new` statement", ExceptionType.ConsructorExpected, node.Token.Position);

                var constructorNode = node.Left;

                for (int i = 0; i < constructorNode.Children.Count - 1; ++i)
                {
                    var child = constructorNode.Children[i];
                    assignTypes(child, parent);
                    methodArgs.Add(child.Type);
                }

                var type = constructorNode.Left;
                var clazz = classList.Find(type.Token);
                var constructor = clazz.ResolveConstructor(type.Token, methodArgs) as Method;
                constructorNode.RelatedNamedData = constructor;

                int argc = constructorNode.Children.Count - 1;
                for (int i = 0; i < argc; ++i)
                {
                    var child = constructorNode.Children[i];
                    var parameter = constructor.Parameters[argc - i - 1];

                    // Casting if not equal types
                    if (!parameter.Type.Equals(child.Type))
                    {
                        Node converter = new Node(new Token 
                        { 
                            Type = TokenType.Operator, 
                            Operation = Operation.Cast, 
                            Position = constructorNode.Children[i].Token.Position 
                        });
                        converter.Type = parameter.Type;
                        converter.Left = child;
                        constructorNode.Children[i] = converter;
                    }
                }

                // Now newobj node has type (class type), all subnodes from class constructor call, and assigned constructor instance
                node.Type = clazz;
                node.Children = constructorNode.Children.GetRange(0, argc);
                node.RelatedNamedData = constructor;
            }
            else if (node.Token.IsOp(OperationType.NewArr))
            {
                if (node.Children.Count != 1 || !node.Left.Token.IsOp(OperationType.ArrayGet))
                    InfoProvider.AddFatal("Array size specification in `new` statement expected", ExceptionType.ArraySpec, node.Token.Position);

                node.Children = node.Left.Children;
                if (!(node.Left.Token is TypedToken))
                    InfoProvider.AddFatal("Typed token expected", ExceptionType.InternalError, node.Token.Position);

                var type = (node.Left.Token as TypedToken).BoundType;

                IType typeToCheck;
                if (type is ArrayType arrayType)
                {
                    typeToCheck = arrayType.Inner;
                    node.Type = new ArrayType(typeToCheck, arrayType.Dimensions + 1);
                }
                else
                {
                    typeToCheck = type;
                    node.Type = new ArrayType(typeToCheck, 1);
                }

                if (typeToCheck is ClassType clazz && !classList.Exists(c => c.Name == clazz.Name))
                        InfoProvider.AddFatal("Unknown type in `new array` operator", ExceptionType.IllegalType, node.Token.Position);
            }
            else if (node.Token.IsOp(OperationType.MemberAccess))
            {
                // x.f() - x y . ()
                var owner = node.Left;
                var member = node.Right;

                assignTypes(owner, parent);

                var ownerClass = owner.Type as ClassType; // TODO: Check

                if (!member.Token.IsIdentifier())
                    InfoProvider.AddError("Identifier expected", ExceptionType.InvalidMemberAccess, member.Token.Position);

                // Field, property
                if (paramList == null)
                {
                    // TODO: Encapsulation
                    var decl = ownerClass.FindDeclaration(member.Token);
                    member.Type = node.Type = decl.Type;
                    member.RelatedNamedData = node.RelatedNamedData = decl;
                    assignOperationType(node, node.Left, node.Right);
                }
                else // method
                {
                    // TODO: Encapsulation
                    var decl = ownerClass.ResolveMethod(member.Token, paramList);
                    member.Type = node.Type = decl.Type;
                    member.RelatedNamedData = node.RelatedNamedData = decl;
                    assignOperationType(node, node.Left, node.Right);
                }
            }
            else if (node.Token.IsOp())
            {
                if (node.Children.Count == 1)
                {
                    assignTypes(node.Left, parent);
                    node.Type = node.Left.Type;
                    assignOperationType(node, node.Left);

                    if (node.Left.Token.Type == TokenType.Identifier && node.Left.RelatedNamedData is Variable lValueLocal)
                        lValueLocal.IsUsed = true;
                }
                else if (node.Children.Count == 2)
                {
                    assignTypes(node.Right, parent);
                    assignTypes(node.Left, parent);

                    if(node.Token.IsOp(OperationType.Assign) && node.Left.Type is ImplicitType)
                    {
                        node.Left.Type = node.Left.RelatedNamedData.Type = node.Right.Type;
                    }

                    assignOperationType(node, node.Left, node.Right);

                    if (node.Left.Token.Type == TokenType.Identifier && node.Token.IsOp(OperationType.Assign) && node.Left.RelatedNamedData is Variable lValueLocal)
                        lValueLocal.IsAssigned = true;
                    if (node.Right.Token.Type == TokenType.Identifier && node.Right.RelatedNamedData is Variable rValueLocal)
                        rValueLocal.IsUsed = true;
                        
                }
                else // WTF. Can't be
                    InfoProvider.AddError("Too many childrens in operator", ExceptionType.InternalError, node.Token.Position);
            }
            else if (node.Token.IsConstant())
            {
                node.Type = new PlainType((DataTypes)node.Token.ConstType);
            }
            else if (node.Token.IsIdentifier())
            {
                var local = parent.FindDeclaration(node.Token);
                if (local != null)
                {
                    node.RelatedNamedData = local;
                    node.Type = local.Type;
                }
                else if (paramList != null)
                {
                    var method = parent.ClassContext.ResolveMethod(node.Token, paramList);
                    node.Type = method.Type;
                    node.RelatedNamedData = method;
                }
                else
                    InfoProvider.AddFatal($"Can't resolve identifier {node.Token}", ExceptionType.UndeclaredIdentifier, node.Token.Position);
            }
        }

        private void assignOperationType(Node operationNode, Node operand)
        {
            var operation = operationNode.Token.Operation;
            int typeIndex = (int)operand.Type.Type;

            IType result = null;
            DataTypes resultType = DataTypes.Null;

            if(TypeMatrices.OperationVectorLUT.ContainsKey(operation.Type))
            {
                var typeVector = TypeMatrices.OperationVectorLUT[operation.Type];
                resultType = typeVector[typeIndex];
                result = operand.Type;
            }
            else
            {
                InfoProvider.AddFatal($"Operator {operation.View} can't be resolved", ExceptionType.InternalError, operationNode.Token.Position);
            }
            operationNode.Type = result;
        }

        private void assignOperationType(Node operationNode, Node left, Node right)
        {
            var operation = operationNode.Token.Operation;

            int leftTypeIndex = (int)left.Type.Type;
            int rightTypeIndex = (int)right.Type.Type;

            IType result = null;
            Node castingNode;
            DataTypes resultType = DataTypes.Null;

            if (TypeMatrices.OperationMatrixLUT.ContainsKey(operation.Type))
            {
                var typeMatrix = TypeMatrices.OperationMatrixLUT[operation.Type];
                resultType = typeMatrix[leftTypeIndex, rightTypeIndex];

                                                       // TODO: Specify types directly
                if (resultType != DataTypes.Null && !operation.Is(OperationType.ArrayGet, OperationType.MemberAccess))
                {
                    var hightestType = TypeMatrices.ImplicitCastMatrix[leftTypeIndex, rightTypeIndex];
                    if (left.Type.Type == hightestType)
                    {
                        // Insert type casting node to right argument
                        castingNode = createCastingNode(right, left); //createCasingNode(from, to)
                        castingNode.Left = right;
                        operationNode.Right = castingNode;
                        result = left.Type;
                    }
                    else
                    {
                        // Insert type casting node to left argument
                        castingNode = createCastingNode(right, left); //createCasingNode(from, to)
                        castingNode.Left = left;
                        operationNode.Left = castingNode;
                        result = left.Type;
                    }
                    result = new PlainType(resultType);
                }
                else if(operation.Is(OperationType.MemberAccess))
                {
                    result = right.Type;
                }
            }
            else if (operation.Type == OperationType.ArrayGet)
            {
                if (left.Type is ArrayType array)
                {
                    if(!isIntegerType(right.Type.Type))
                        InfoProvider.AddFatal($"Array index type must be integer. Now `{right.Type.ToString()}`", ExceptionType.IllegalType, operationNode.Token.Position);
                    result = array.ElementType;
                    resultType = result.Type;
                }
                InfoProvider.AddFatal($"Indexable type must be an array. Now `{left.Type.ToString()}`", ExceptionType.IllegalType, operationNode.Token.Position);
            }
            else
            {
                InfoProvider.AddFatal($"Operator {operation.View} can't be resolved", ExceptionType.InternalError, operationNode.Token.Position);
            }

            if (resultType == DataTypes.Null)
                InfoProvider.AddFatal($"Unacceptable operands type combination in operator `{operation.View}` - ({left.Type.ToString()}, {right.Type.ToString()})", 
                                     ExceptionType.IllegalType, operationNode.Token.Position);

            operationNode.Type = result;
        }

        private Node createCastingNode(Node from, Node to)
        {
            return createCastingNode(from, to.Type);
        }

        private Node createCastingNode(Node from, IType to)
        {
            var node = new Node(new Token { Type = TokenType.Operator, Operation = Operation.Cast, Position = from.Token.Position });
            node.Type = to;
            return node;
        }

        private Node tryCreateImplicitCast(Node node, IType toType)
        {
            var resultingType = TypeMatrices.ImplicitCastMatrix[(int)toType.Type, (int)node.Type.Type];
            if (resultingType == DataTypes.Null)
                return null;
            return createCastingNode(node, toType);
        }

        private void compileFile(string src)
        {
            foreach (var clazz in classList)
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
                                      method.Modifiers, method.Scope, method.Name, method.Type, method.Parameters.ToString());
            }
            InfoProvider.Print();
        }

        public static bool IsPlainType(string type, bool includeVoid = false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }

        private bool isRegisteredType(string type)
        {
            if (IsPlainType(type, true))
                return true;
            if (type == VAR)
                return true;
            return classList.Exists(c => c.Name == type);
        }

        /// <summary>
        /// Determines if one type can be implicitly casted to another
        /// It can be in two situations:
        ///     1. Source type is derived from target type
        ///     2. Source type is integer and target type is also integer but with highter range
        /// </summary>
        /// <returns><c>true</c>, if implicit cast is acceptable, <c>false</c> otherwise.</returns>
        /// <param name="fromType">Source type (from what we cast)</param>
        /// <param name="toType">Target type (to what we cast)</param>
        public static bool CanCast(IType fromType, IType toType)
        {
            if (fromType is ClassType fromClass && toType is ClassType toClass)
                return fromClass.DerivatesFrom(toClass);
            if (fromType is PlainType fromPlain && toType is PlainType toPlain)
                return fromPlain.CanCastTo(toPlain);
            return false;
        }

        private bool isIntegerType(DataTypes type)
        {
            return type < DataTypes.I64;
        }

        private bool isNumericType(DataTypes type)
        {
            return type < DataTypes.Double;
        }
    }
}


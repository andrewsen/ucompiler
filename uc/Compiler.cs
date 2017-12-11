﻿using System;
using System.Linq;
using System.IO;

using static Lab4.Alphabet;
using System.Collections.Generic;
using uc;

namespace Lab4
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

            foreach(var clazz in classList)
            {
                compileClass(clazz);
            }

            foreach (var src in compilerConfig.Sources)
            {
                // Compiling 
                // TODO: Rework it
                compileFile(src);
            }
        }

        private void compileClass(ClassType clazz)
        {
            foreach(var method in clazz.SymbolTable.Methods)
            {
                evalMethod(method);
            }
        }

        private void parseGlobalScope(string src)
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

        private void parseAttribute(TokenStream gStream)
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
                            CompilerLog.AddError("@AddMeta: Incorrect data", ExceptionType.AttributeException, gStream.SourcePosition);
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
                            CompilerLog.AddError("Meta key " + md.Key + " exists", ExceptionType.MetaKeyExists, gStream.SourcePosition);
                            //throw new CompilerException(ExceptionType.MetaKeyExists, "Meta key " + md.key + " in module " + CommandArgs.source + " exists", tokens);

                        metadataList.Add(md);
                    }
                    break;
                case "Module":
                    if (attr.Data.Count != 1 && !(attr.Data[0].Value is string))
                        CompilerLog.AddError("@Module: Incorrect name", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("Module", "Incorrect module name");
                    if (compilerConfig.OutBinaryFile == null)
                        compilerConfig.OutBinaryFile = (attr.Data[0].Value as string) + ".vas"; // TODO: FIXME
                    break;
                case "RuntimeInternal":
                    if (attr.Data.Count != 0)
                        CompilerLog.AddError("@RuntimeInternal: Too many arguments", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("RuntimeInternal", "Too many arguments");
                    if (!attr.Binded)
                        CompilerLog.AddError("@RuntimeInternal must be binded to method (check `;`)", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("RuntimeInternal", "`@RuntimeInternal` must be binded to function (check `;`)");
                    bindedAttributeList.Add(attr);
                    break;
                case "Entry":
                    if (attr.Data.Count != 0)
                        CompilerLog.AddError("@Entry: Too many arguments", ExceptionType.AttributeException, gStream.SourcePosition);
                        //throw new AttributeException("Entry", "Too many arguments");
                    if (!attr.Binded)
                        CompilerLog.AddError("@Entry must be binded to method (check `;`)", ExceptionType.AttributeException, gStream.SourcePosition);
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
                            CompilerLog.AddError("@Debug:Set: Incorrect data", ExceptionType.AttributeException, gStream.SourcePosition);
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
                    CompilerLog.AddWarning("Unknown attribute `@" + attr.Name + "`", ExceptionType.AttributeException, gStream.SourcePosition);
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

            gStream.CheckNext(BLOCK_O, ExceptionType.Brace);

            while (!gStream.Eof)
            {
                var entry = readCommonClassEntry(gStream);
                entry.Class = clazz;

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
                        CompilerLog.AddError("Void type not allowed for properties", ExceptionType.IllegalType, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native))
                        CompilerLog.AddError("Properties can't be native", ExceptionType.IllegalModifier, gStream.SourcePosition);
                    
                    Property property = parseProperty(entry, gStream);

                    clazz.SymbolTable.Add(property);
                }
                else if (gStream.Is(PAR_O))
                {
                    // Method
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                        CompilerLog.AddError("Methods can't be const", ExceptionType.IllegalModifier, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native) && !entry.Modifiers.HasFlag(ClassEntryModifiers.Static))
                        CompilerLog.AddError("Native methods must be declared as `static`", ExceptionType.IllegalModifier, gStream.SourcePosition);

                    Method method = parseMethod(entry, gStream);

                    clazz.SymbolTable.Add(method);
                }
                else
                {
                    // Field
                    if (entry.HasVoidType)
                        CompilerLog.AddError("Void type not allowed for fields", ExceptionType.IllegalType, gStream.SourcePosition);
                    if (entry.Modifiers.HasFlag(ClassEntryModifiers.Native))
                        CompilerLog.AddError("Fields can't be native", ExceptionType.IllegalModifier, gStream.SourcePosition);

                    Field field = parseField(entry, gStream);

                    clazz.SymbolTable.Add(field);
                }

                if(gStream.IsNext(BLOCK_C))
                    break;
                gStream.PushBack();
            }

            classList.Add(clazz);
        }

        private Field parseField(CommonClassEntry entry, TokenStream gStream)
        {
            Field field = new Field();
            if (gStream.Is(STAT_SEP) && entry.Modifiers.HasFlag(ClassEntryModifiers.Const))
                CompilerLog.AddError("Const field must be initialized immediately", ExceptionType.UninitedConstant, gStream.SourcePosition);
            field.FromClassEntry(entry);
            if (gStream.Is(ASSIGN))
            {
                gStream.Next();
                field.InitialExpressionPosition = gStream.TokenPosition;
                gStream.SkipTo(STAT_SEP, false);
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
                            CompilerLog.AddError("Getter for this property already exists", ExceptionType.MultipleGetters, gStream.SourcePosition);
                        property.Getter = parsePropertyFunction(entry, gStream);
                    }
                    else if (gStream.Is(SETTER_DECL))
                    {
                        if (property.Setter != null)
                            CompilerLog.AddError("Setter for this property already exists", ExceptionType.MultipleSetters, gStream.SourcePosition);
                        property.Setter = parsePropertyFunction(entry, gStream);
                    }
                    else if (gStream.Is(BLOCK_C))
                        break;
                    else
                        CompilerLog.AddError("Unexpected symbol", ExceptionType.UnexpectedToken, gStream.SourcePosition);
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
                gStream.SkipTo(STAT_SEP, true);
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
                CompilerLog.AddError("Unexpected symbol", ExceptionType.UnexpectedToken, gStream.SourcePosition);
            }

            return method;
        }

        private ParameterList readParams(TokenStream gStream)
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
                    CompilerLog.AddError("`,` or `)` expected in parameter declaration", ExceptionType.IllegalToken, gStream.SourcePosition);
            }

            return plist;
        }

        private CommonClassEntry readCommonClassEntry(TokenStream gStream)
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
            else if(token == CONST) 
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
                method.Body = evalBlock(new CodeBlock(method.Class, method.Class), method.BodyStream);
                method.Body.Parent = method.Class;
                method.Body.ClassContext = method.Class;
            }
            else
                CompilerLog.AddError("Short form is unsupported now", ExceptionType.NotImplemented, method.DeclarationPosition);
        }

        private CodeBlock evalBlock(CodeBlock parent, TokenStream lStream)
        {
            CodeBlock block = new CodeBlock(parent);

            var entryToken = lStream.Next();

            if (!(entryToken.IsIdentifier() || entryToken.IsOp() || (entryToken.IsDelim() && entryToken.Representation == BLOCK_O)))
                CompilerLog.AddError("Identifier, control keyword, type or block expected", ExceptionType.IllegalToken, lStream.SourcePosition);

            switch(entryToken.Representation)
            {
                case IF:
                    break;
                case FOR:
                    break;
                case WHILE:
                    break;
                case DO:
                    break;
                case SWITCH:
                    break;
                case FOREACH:
                    break;
                case RETURN:
                    break;
                case BLOCK_O:
                    break;
                default:
                    evalStatement(parent, lStream);
                    break;
            }

            return block;
        }

        private void evalStatement(CodeBlock parent, TokenStream lStream)
        {
            if (isRegisteredType(lStream.Current))
                evalLocalVarDeclaration(parent, lStream);

            lStream.PushBack();
            var expr = evalExpression(lStream);
            var rootNode = expressionToAST(expr);
            //assignTypes(rootNode, parent);
        }

        private void evalLocalVarDeclaration(CodeBlock parent, TokenStream lStream)
        {

        }

        public Expression evalExpression(TokenStream stream, ParsingPolicy parsingPolicy = ParsingPolicy.Brackets)
        {
            // TODO: Andrew Senko: No type control. No variable declaration control. Implement all of this

            List<Token> resultingExpression = new List<Token>();
            Stack<int> functionArgCounter = new Stack<int>();
            Stack<Token> opStack = new Stack<Token>();
            Token lastToken = Token.EOF;

            int bracketsCount = 0;

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
                    case ParsingPolicy.Semicolon:
                        return !token.IsSemicolon();
                    case ParsingPolicy.Both:
                        return bracketsCount >= 0 && !token.IsSemicolon();
                }
                throw new ArgumentOutOfRangeException(nameof(parsingPolicy), "Invalid parsing policy");
            }

            while (satisfiesPolicy(stream.Next()))
            {
                var token = stream.Current;
                if (token.IsConstant() || token.IsIdentifier())
                {
                    if (lastToken.IsOneOf(PAR_C, INDEXER_C))
                        CompilerLog.AddError("Unexpected identifier after brace", ExceptionType.Brace, stream.SourcePosition);
                    if (lastToken.IsIdentifier())
                        CompilerLog.AddError("Unexpected identifier", ExceptionType.BadExpression, stream.SourcePosition);
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
                    if (lastToken.IsOp())
                        CompilerLog.AddError("[ unexpected", ExceptionType.Brace, stream.SourcePosition);
                    opStack.Push(token);
                    lastToken = token;
                }
                else if (token == INDEXER_C)
                {
                    // a = b[c + 4];
                    // a b[c + 4] - 6 =
                    // a b c 4 + [] 6 - =
                    if (lastToken.IsOp())
                        CompilerLog.AddError("] unexpected", ExceptionType.Brace, stream.SourcePosition);

                    while (opStack.Count > 0 && opStack.Peek() != INDEXER_O)
                    {
                        if (opStack.Peek() == PAR_O)
                            CompilerLog.AddError(") expected", ExceptionType.MissingParenthesis, stream.SourcePosition);
                        resultingExpression.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0)
                        CompilerLog.AddError("[ expected", ExceptionType.Brace, stream.SourcePosition);
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
                            while(!lookupStream.Eof && callBracketsCount > 0)
                            {
                                if (lookupStream.IsNext(PAR_C, TokenType.Delimiter))
                                    callBracketsCount--;
                                else if (lookupStream.Is(PAR_O))
                                    callBracketsCount++;
                                else if(callBracketsCount == 1 && lookupStream.Is(SEP, TokenType.Delimiter))
                                    callToken.Operation.ArgumentCount++;
                            }
                        }
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

                        opStack.Push(callToken);
                        functionArgCounter.Push(0);
                    }
                    else if (!lastToken.IsOp())
                        CompilerLog.AddError("Illegal (", ExceptionType.Brace, stream.SourcePosition);
                    opStack.Push(token);
                    lastToken = token;
                }
                else if (token == PAR_C)
                {
                    bool isFuncCall = false;
                    bracketsCount--;
                    if (lastToken.IsOp() && lastToken.Operation.Type != OperationType.FunctionCall)
                        CompilerLog.AddError("Illegal )", ExceptionType.Brace, stream.SourcePosition);
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
                        CompilerLog.AddError("Expected (", ExceptionType.MissingParenthesis, stream.SourcePosition);
                    else
                        opStack.Pop();
                }
                else if (token == SEP)
                {
                    if (!(lastToken.IsConstant() || lastToken.IsIdentifier() || lastToken == PAR_C || lastToken == INDEXER_C))
                        CompilerLog.AddError("Unexpected comma", ExceptionType.UnexpectedComma, stream.SourcePosition);
                    functionArgCounter.Push(functionArgCounter.Pop() + 1);
                    while (opStack.Count != 0 && opStack.Peek() != PAR_O)
                        resultingExpression.Add(opStack.Pop());
                    lastToken = token;
                    //isUnary = true;
                }
                else if(token.Type == TokenType.Semicolon && !parsingPolicy.HasFlag(ParsingPolicy.Semicolon))
                    CompilerLog.AddError("Unexpected token", ExceptionType.UnexpectedToken, stream.SourcePosition);
                else
                    CompilerLog.AddError("Unexpected token", ExceptionType.UnexpectedToken, stream.SourcePosition);
            }
            while (opStack.Count > 0)
            {
                var op = opStack.Pop();
                if (op == PAR_O)
                    CompilerLog.AddError(") expected", ExceptionType.MissingParenthesis, stream.SourcePosition);
                if (op == INDEXER_O)
                    CompilerLog.AddError(") expected", ExceptionType.Brace, stream.SourcePosition);
                resultingExpression.Add(op);
            }
            return new Expression(resultingExpression, stream.SourcePosition);
        }

        public Node expressionToAST(Expression expr)
        {
            return makeASTNode(new Stack<Token>(expr.Tokens));
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
                        CompilerLog.AddError($"Argument of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                }
                else if (!tok.Operation.IsUnary && tok.Operation.Type != OperationType.FunctionCall)
                {
                    result.Right = makeASTNode(polish);
                    result.Left = makeASTNode(polish);
                    if (result.Left == null && result.Right == null)
                        CompilerLog.AddError($"Both arguments of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                    else if (result.Left == null || result.Right == null)
                        CompilerLog.AddError($"Argument of {tok} is missing", ExceptionType.BadExpression, tok.Position);
                    else if (tok.Operation.Type == OperationType.Assign && result.Left.Token.IsConstant())
                        CompilerLog.AddError("rvalue can't be on left side of assignment operator", ExceptionType.lValueExpected, tok.Position);
                }
                else // if(tok.Operation.Type == OperationType.FunctionCall)
                {
                    int argCount = tok.Operation.ArgumentCount + 1;
                    while (argCount-- != 0)
                        result.AddRight(makeASTNode(polish));
                    if (result.Left == null)
                        CompilerLog.AddError("No function name defined", ExceptionType.FunctionName, tok.Position);
                    else if (!result.Left.Token.IsIdentifier())
                        ;//CompilerLog.AddError("Identifier expected as function name", ExceptionType.FunctionName, tok.Position);
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
                    if(!parameter.Type.Equals(child.Type))
                    {
                        Node converter = new Node(new Token { Type = TokenType.Operator, Operation = Operation.Cast, Position = node.Children[i].Token.Position});
                        converter.Type = parameter.Type;
                        converter.Left = child;
                        node.Children[i] = converter;
                    }
                }
            }
            if (node.Token.IsOp(OperationType.MemberAccess))
            {
                // x.f() - x y . ()
                var owner = node.Left;
                var member = node.Right;

                assignTypes(owner, parent);

                var ownerClass = owner.Type as ClassType; // TODO: Check

                if (!member.Token.IsIdentifier())
                    CompilerLog.AddError("Identifier expected", ExceptionType.InvalidMemberAccess, member.Token.Position);

                // Field, property
                if (paramList == null)
                {
                    // TODO: Encapsulation
                    var decl = ownerClass.FindDeclaration(member.Token);
                    member.Type = node.Type = decl.Type;
                    member.RelatedNamedData = node.RelatedNamedData = decl;
                }
                else // method
                {
                    // TODO: Encapsulation
                    var decl = ownerClass.FindMethod(member.Token, paramList);
                    member.Type = node.Type = decl.Type;
                    member.RelatedNamedData = node.RelatedNamedData = decl;
                }
            }
            if (node.Token.IsOp())
            {
                if (node.Children.Count == 1)
                {
                    assignTypes(node.Left, parent);
                    node.Type = node.Left.Type;
                }
                else if (node.Children.Count == 2)
                {
                    assignTypes(node.Right, parent);
                    assignTypes(node.Left, parent);
                    //node.Type = getHightestType(node.Left, node.Right);
                }
                else // WTF. Can't be
                    CompilerLog.AddError("Too many childrens in operator", ExceptionType.InternalError, node.Token.Position);
            }
            else if (node.Token.IsConstant())
            {
                node.Type = new PlainType((DataTypes)node.Token.ConstType);
            }
            else if(node.Token.IsIdentifier())
            {
                var local = parent.FindDeclarationRecursively(node.Token);
                if (local != null)
                {
                    node.RelatedNamedData = local;
                    node.Type = local.Type;
                }
                else if(paramList != null)
                {
                    var method = parent.ClassContext.FindMethod(node.Token, paramList);
                    node.Type = method.Type;
                    node.RelatedNamedData = method;
                }
            }
        }

        private void compileFile(string src)
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
            CompilerLog.Print();
        }

        public static bool IsPlainType(string type, bool includeVoid=false)
        {
            var dt = PlainType.FromString(type);
            return !(dt == DataTypes.Null || (dt == DataTypes.Void && includeVoid));
        }

        private bool isRegisteredType(string type)
        {
            if (IsPlainType(type, true))
                return true;
            return classList.Exists(c => c.Name == type);
        }

        //
        // Parse
        //

        [Obsolete]
        public Expression parseExpression(TokenStream stream)
        {
			// TODO: Andrew Senko: No type control. No variable declaration control. Implement all of this

			List<Token> resultingExpression = new List<Token>();
			Stack<int> functionArgCounter = new Stack<int>();
			Stack<Token> opStack = new Stack<Token>();
            Token lastToken = Token.EOF;

            while(stream.Next().Type != TokenType.Semicolon)
			{
                //Console.WriteLine($"Current {gStream.Current}");
                //if(gStream.Current.Type == TokenType.EOF)
                //{
                //    InfoProvider.AddError("Semicilion `;` is missed", ExceptionType.SemicolonExpected, gStream.SourcePosition);
                //    break;
                //}
				var token = stream.Current;
                if(token.IsConstant() || token.IsIdentifier())
				{
                    if (lastToken.IsOneOf(PAR_C, INDEXER_C))
						CompilerLog.AddError("Unexpected identifier after brace", ExceptionType.Brace, stream.SourcePosition);
                    if (lastToken.IsIdentifier())
                        CompilerLog.AddError("Unexpected identifier", ExceptionType.BadExpression, stream.SourcePosition);
                    resultingExpression.Add(token);
                    lastToken = token;
                }
                if (token.IsOp())
                {
                    /*
                     * Пока присутствует на вершине стека токен оператор op2, и
					 * Либо оператор op1 лево-ассоциативен и его приоритет меньше, чем у оператора op2 либо равен,
					 * или оператор op1 право-ассоциативен и его приоритет меньше, чем у op2,
					 * переложить op2 из стека в выходную очередь;
					*/
                    while (opStack.Count > 0 && opStack.Peek().IsOp())
                    {
                        if ((token.Operation.Association == Association.Left && token.Operation.Priority < opStack.Peek().Operation.Priority)
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
                    if (lastToken.IsOp())
                        CompilerLog.AddError("[ unexpected", ExceptionType.Brace, stream.SourcePosition);
					opStack.Push(token);
                    lastToken = token;
                }
                else if (token == INDEXER_C)
                {
                    // a = b[c + 4];
                    // a b[c + 4] - 6 =
                    // a b c 4 + [] 6 - =
                    if (lastToken.IsOp())
                        CompilerLog.AddError("] unexpected", ExceptionType.Brace, stream.SourcePosition);

                    while (opStack.Count > 0 && opStack.Peek() != INDEXER_O)
                    {
						if ((opStack.Peek() == PAR_O))
                            CompilerLog.AddError(") expected", ExceptionType.MissingParenthesis, stream.SourcePosition);
                        resultingExpression.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0)
                        CompilerLog.AddError("[ expected", ExceptionType.Brace, stream.SourcePosition);
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
                    // Process as function
                    // x = fun(1, g(5, y*(3-i), 8));
                    if (lastToken.IsIdentifier())
                    {
                        // Function call operator
                        token.Representation = "()";
                        token.Type = TokenType.Operator;
                        token.Operation = Operation.From("()");
                        functionArgCounter.Push(0);
                    }
                    else if (!lastToken.IsOp())
                        CompilerLog.AddError("Illegal (", ExceptionType.Brace, stream.SourcePosition);
					opStack.Push(token);
					lastToken = token;
                }
                else if (token == PAR_C)
				{
					if (lastToken.IsOp())
                        CompilerLog.AddError("Illegal )", ExceptionType.Brace, stream.SourcePosition);
                    while (opStack.Count > 0 && opStack.Peek() != PAR_O)
                    {
                        if (opStack.Peek().Operation.Type == OperationType.FunctionCall)
                        {
                            var callOp = opStack.Peek();
                            callOp.Operation.ArgumentCount = functionArgCounter.Pop();
                            resultingExpression.Add(callOp);
                        }
                        else
                            resultingExpression.Add(opStack.Pop());
                    }
					if (opStack.Count == 0)
                        CompilerLog.AddError("Expected (", ExceptionType.MissingParenthesis, stream.SourcePosition);
                    else
						opStack.Pop();
                }
                else if(token == SEP)
                {
					if (!(lastToken.IsConstant() || lastToken.IsIdentifier() || lastToken == PAR_C || lastToken == INDEXER_C))
                        CompilerLog.AddError("Unexpected comma", ExceptionType.UnexpectedComma, stream.SourcePosition);
                    functionArgCounter.Push(functionArgCounter.Pop()+1);
                }
				//Console.Write("\tOp stack:\n\t");
				//foreach (var op in opStack)
				//	Console.Write($"{op} ");
				//Console.Write("\n\tToken array:\n\t");
				//foreach (var op in resultingExpression)
				//	Console.Write($"{op} ");
				//Console.WriteLine();
            }
            while(opStack.Count > 0)
            {
				var op = opStack.Pop();
				if (op == PAR_O)
                    CompilerLog.AddError(") expected", ExceptionType.MissingParenthesis, stream.SourcePosition);
                if (op == INDEXER_O)
                    CompilerLog.AddError("] expected", ExceptionType.Brace, stream.SourcePosition);
                resultingExpression.Add(op);
			}
            return new Expression(resultingExpression, stream.SourcePosition);
        }

        public Node buildAST(Expression expr)
        {
            // a b c * 4 e * + =
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
                    CompilerLog.AddError($"Both arguments of {tok} is missed", ExceptionType.BadExpression, position);
                else if (result.Left == null || result.Right == null)
					CompilerLog.AddError($"Argument of {tok} is missed", ExceptionType.BadExpression, position);
                else if (tok.Operation.Type == OperationType.Assign && result.Left.Token.IsConstant())
					CompilerLog.AddError("rvalue can't be on left side of assignment operator", ExceptionType.lValueExpected, position);
				// Pascal specific
				else if (result.Left.Token.Operation?.Type == OperationType.Assign || result.Right.Token.Operation?.Type == OperationType.Assign)
					CompilerLog.AddError($"Assignment operator can be only as a root node", ExceptionType.BadExpression, position);
            }

            return result;
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Translator;

namespace uc
{
    public class CodeGenSourceBackend
    {
        private DirectiveList directiveList;
        private MetadataList metadataList;
        private ClassList classList;
        //private FileInfo output;
        private CompilerConfig config;
        //private FileStream outputWriter;

        private ClassType currentClass;
        private Method currentMethod;

        private List<CodeEntry> code;

        private Stack<Label> continues = new Stack<Label>();
        private Stack<Label> breaks = new Stack<Label>();

        private StringBuilder result = new StringBuilder();

        public List<CodeEntry> Code => code;

        public CodeGenSourceBackend(CompilerConfig conf, DirectiveList directives, MetadataList metadata, ClassList classes) {
            config = conf;
            directiveList = directives;
            metadataList = metadata;
            classList = classes;
        }

        public void Generate() {
            //output = new FileInfo(config.OutInfoFile);
            //outputWriter = output.OpenWrite();
            foreach (var clazz in classList)
                compileClass(clazz);

            File.WriteAllText(config.OutInfoFile, result.ToString());
        }

        private void write(string line, int offset = 0) {
            result.Append(new string(' ', offset) + line);
        }

        private void writeln(string line, int offset = 0) {
            result.Append(new string(' ', offset) + line + "\n");
        }

        private string opToString(CodeEntry entry, int offset) {
            if (entry.Operation == OpCodes.LABEL)
                return (entry.Operands[0] as Label).Name + ":";
            return new string(' ', offset) + entry.ToString();
        }

        private void compileClass(ClassType clazz) {
            var scope = clazz.Scope.ToString().ToLower();
            string classDecl = $".{scope} {clazz.Name} {{\n";
            write(classDecl);

            currentClass = clazz;

            compileFields(clazz);
            compileMethods(clazz);

            write("}\n\n");

        }

        private void compileFields(ClassType clazz) {
            write(".fields\n", 4);

            var fields = clazz.SymbolTable.Fields;
            for (int i = 0; i < fields.Count; ++i) {
                var fld = fields[i];
                var scope = fld.Scope.ToString().ToLower();
                var modifiers = fld.Modifiers.ToString().ToLower();

                string line = $"{i} : {fld.Type.ToString()} -> {fld.Name} {modifiers} .{scope}";
                write($"{line}\n", 8);
            }

            write(".end-fields\n\n", 4);
        }

        private void compileMethods(ClassType clazz) {
            var methods = clazz.SymbolTable.Methods;
            for (int i = 0; i < methods.Count; ++i) {
                var method = methods[i];
                var scope = method.Scope.ToString().ToLower();
                var modifiers = method.Modifiers.ToString().ToLower();

                string line = $".{scope} {modifiers} {method.Type} {method.Name} ({method.Parameters.ToString()})";
                write($"{line} {{\n", 4);

                //currentMethod = method;
                compileMethodBody(method);

                /*code.ForEach(entry => {
                    if(entry.View.EndsWith(":"))
                        write(entry.View + "\n", 0);
                    else
                        write(entry.View + "\n", 8); 
                });*/

                write("}\n\n", 4);
            }

        }

        private void compileMethodBody(Method method) {
            write(".locals\n", 8);

            var locals = method.DeclaredLocals;
            foreach (var pair in locals) {
                var loc = pair.Key;
                var idx = pair.Value;

                string line = $"{idx} : {loc.Type.ToString()} -> {loc.Name}";
                write($"{line}\n", 12);
            }

            write(".end-locals\n", 8);

            foreach (var entry in method.IntermediateCode) {
                writeln(opToString(entry, 8));
            }
        }

        private void compileIExpr(IExpression iexpr) {
            if (iexpr is CodeBlock blk)
                compileBlock(blk);
            else if (iexpr is Expression expr)
                compileExpr(expr);
        }

        private void compileBlock(CodeBlock block) {
            foreach (var iexpr in block.Expressions) {
                switch (iexpr) {
                    case Return ret:
                        compileReturn(ret);
                        break;
                    case Break b:
                        compileBreak();
                        break;
                    case Continue c:
                        compileContinue();
                        break;
                    case If ifExpr:
                        compileIf(ifExpr);
                        break;
                    case While whileExpr:
                        compileWhile(whileExpr);
                        break;
                    case DoWhile doWhileExpr:
                        compileDoWhile(doWhileExpr);
                        break;
                    case For forExpr:
                        compileFor(forExpr);
                        break;
                    case Expression expr:
                        compileExpr(expr);
                        break;
                    case CodeBlock childBlock:
                        compileBlock(childBlock);
                        break;
                }
            }
        }

        private void compileReturn(Return ret) {
            compileExpr(ret.Expression);
            code.Add(new CodeEntry(OpCodes.RET));
        }

        private void compileBreak() {
            code.Add(new CodeEntry(OpCodes.JMP, breaks.Peek()));
        }

        private void compileContinue() {
            code.Add(new CodeEntry(OpCodes.JMP, continues.Peek()));
        }

        private void compileIf(If ifExpr) {
            var ifs = ifExpr.Conditions;

            var outLabel = new Label();
            foreach (var cond in ifs) {
                compileExpr(ifExpr.MasterIf.Condition);

                var passLabel = new Label();
                code.Add(new CodeEntry(OpCodes.JFALSE, passLabel));

                compileIExpr(cond.Body);

                code.Add(new CodeEntry(OpCodes.JFALSE, outLabel));
                code.Add(new CodeEntry(OpCodes.LABEL, passLabel));
            }
            compileIExpr(ifExpr.ElsePart);
            code.Add(new CodeEntry(OpCodes.LABEL, outLabel));
        }

        private void compileWhile(While whileExpr) {
            var inLabel = new Label();
            var elseLabel = new Label();
            var outLabel = new Label();

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            code.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileExpr(whileExpr.Condition);

            code.Add(new CodeEntry(OpCodes.JFALSE, outLabel));
            compileIExpr(whileExpr.Body);
            code.Add(new CodeEntry(OpCodes.JMP, inLabel));
            code.Add(new CodeEntry(OpCodes.LABEL, elseLabel));
            compileIExpr(whileExpr.ElsePart);
            code.Add(new CodeEntry(OpCodes.LABEL, outLabel));
            continues.Pop();
            breaks.Pop();
        }

        private void compileDoWhile(DoWhile doWhileExpr) {
            var inLabel = new Label();
            var outLabel = new Label();

            continues.Push(inLabel);
            breaks.Push(outLabel);

            code.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileIExpr(doWhileExpr.Body);

            compileExpr(doWhileExpr.Condition);
            code.Add(new CodeEntry(OpCodes.JTRUE, inLabel));
            code.Add(new CodeEntry(OpCodes.JMP, inLabel));
            code.Add(new CodeEntry(OpCodes.LABEL, outLabel));
            continues.Pop();
            breaks.Pop();
        }

        private void compileFor(For forExpr) {
            // Condition
            compileIExpr(forExpr.Scope.Expressions[0]);

            var inLabel = new Label();
            var elseLabel = new Label();
            var outLabel = new Label();

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            code.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileExpr(forExpr.Condition);
            compileIExpr(forExpr.Body);
            compileExpr(forExpr.Iteration);
            code.Add(new CodeEntry(OpCodes.JMP, inLabel));
            code.Add(new CodeEntry(OpCodes.LABEL, elseLabel));
            compileIExpr(forExpr.ElsePart);
            code.Add(new CodeEntry(OpCodes.LABEL, outLabel));
            continues.Pop();
            breaks.Pop();
        }

        private void compileExpr(Expression expr) {
            compileNode(expr.ExpressionRoot);
        }

        private void compileNode(Node node) {
            if (node.Token == null) {
                code.Add(new CodeEntry(OpCodes.NOP));
            }
            else if (node.Token.IsOp()) {
                compileOp(node);
            }
            else if (!(node.Token is TypedToken)) {
                if (node.RelatedNamedData != null)
                    loadVar(node);
                else
                    loadConst(node);
            }
        }

        private void compileOp(Node node) {
            // TODO: Rewrite
            if (node.Token.IsOp(OperationType.Assign)) {
                compileNode(node.Right);
            }
            else if (node.Token.Operation.Association == Association.Right || node.Token.IsOp(OperationType.FunctionCall)) {
                foreach (var child in node.Children)
                    compileNode(child);
            }
            else {
                for (int i = node.Children.Count - 1; i >= 0; --i)
                    compileNode(node.Children[i]);
            }

            switch (node.Token.Operation.Type) {
                case OperationType.PreInc:
                    code.Add(new CodeEntry(OpCodes.INC));
                    break;
                case OperationType.PreDec:
                    code.Add(new CodeEntry(OpCodes.DEC));
                    break;
                case OperationType.PostInc: // TODO: Shit 1
                    code.Add(new CodeEntry(OpCodes.INC));
                    break;
                case OperationType.PostDec: // TODO: Shit 2
                    code.Add(new CodeEntry(OpCodes.DEC));
                    break;
                case OperationType.UnaryPlus:
                    break;
                case OperationType.UnaryMinus:
                    code.Add(new CodeEntry(OpCodes.NEG));
                    break;
                case OperationType.Add:
                    code.Add(new CodeEntry(OpCodes.ADD));
                    break;
                case OperationType.Sub:
                    code.Add(new CodeEntry(OpCodes.SUB));
                    break;
                case OperationType.Mul:
                    code.Add(new CodeEntry(OpCodes.MUL));
                    break;
                case OperationType.Div:
                    code.Add(new CodeEntry(OpCodes.DIV));
                    break;
                case OperationType.Mod:
                    code.Add(new CodeEntry(OpCodes.REM));
                    break;
                case OperationType.BinOr:
                    code.Add(new CodeEntry(OpCodes.OR));
                    break;
                case OperationType.BinAnd:
                    code.Add(new CodeEntry(OpCodes.AND));
                    break;
                case OperationType.Xor:
                    code.Add(new CodeEntry(OpCodes.XOR));
                    break;
                case OperationType.Inv:
                    code.Add(new CodeEntry(OpCodes.INV));
                    break;
                case OperationType.ShiftLeft:
                    code.Add(new CodeEntry(OpCodes.SHL));
                    break;
                case OperationType.ShiftRight:
                    code.Add(new CodeEntry(OpCodes.SHR));
                    break;
                case OperationType.Not:
                    code.Add(new CodeEntry(OpCodes.NOT));
                    break;
                case OperationType.And:
                    code.Add(new CodeEntry(OpCodes.AND));
                    break;
                case OperationType.Or:
                    code.Add(new CodeEntry(OpCodes.OR));
                    break;
                case OperationType.Equals:
                    code.Add(new CodeEntry(OpCodes.EQ));
                    break;
                case OperationType.NotEquals:
                    code.Add(new CodeEntry(OpCodes.NEQ));
                    break;
                case OperationType.GreaterEquals:
                    code.Add(new CodeEntry(OpCodes.GTE));
                    break;
                case OperationType.LowerEquals:
                    code.Add(new CodeEntry(OpCodes.LTE));
                    break;
                case OperationType.Greater:
                    code.Add(new CodeEntry(OpCodes.GT));
                    break;
                case OperationType.Lower:
                    code.Add(new CodeEntry(OpCodes.LT));
                    break;
                case OperationType.Cast:
                    code.Add(new CodeEntry(OpCodes.CONV, new TypeOperand(node.Type)));
                    //code.Add(castToOp(node.Type));
                    break;
                case OperationType.ArrayGet:
                    code.Add(new CodeEntry(OpCodes.LDELEM));
                    break;
                case OperationType.ArrayMutate:
                    code.Add(new CodeEntry(OpCodes.STELEM));
                    break;
                case OperationType.NewObj:
                    code.Add(new CodeEntry(OpCodes.NEWOBJ, new TypeOperand(node.Type)));
                    break;
                case OperationType.NewArr:
                    code.Add(new CodeEntry(OpCodes.NEWARR, new TypeOperand((node.Left.Token as TypedToken).BoundType)));
                    break;
                case OperationType.Assign:
                    storeVar(node.Left);
                    break;
                case OperationType.FunctionCall:
                    break;
                case OperationType.MemberAccess:
                    break;
            }
        }

        private CodeEntry castToOp(IType type) {
            OpCodes castOp;
            switch (type.Type) {
                case DataTypes.Char:
                    castOp = OpCodes.CONV_CHR;
                    break;
                case DataTypes.UI8:
                    castOp = OpCodes.CONV_UI8;
                    break;
                case DataTypes.I8:
                    castOp = OpCodes.CONV_I8;
                    break;
                case DataTypes.UI16:
                    castOp = OpCodes.CONV_UI16;
                    break;
                case DataTypes.I16:
                    castOp = OpCodes.CONV_I16;
                    break;
                case DataTypes.UI32:
                    castOp = OpCodes.CONV_UI32;
                    break;
                case DataTypes.I32:
                    castOp = OpCodes.CONV_I32;
                    break;
                case DataTypes.UI64:
                    castOp = OpCodes.CONV_UI64;
                    break;
                case DataTypes.I64:
                    castOp = OpCodes.CONV_I64;
                    break;
                case DataTypes.Double:
                    castOp = OpCodes.CONV_F;
                    break;
                case DataTypes.Bool:
                    castOp = OpCodes.CONV_BOOL;
                    break;
                default:
                    return null;
            }
            return new CodeEntry(castOp);
        }

        private void loadConst(Node node) {
            switch (node.Type.Type) {
                case DataTypes.Char:
                    code.Add(new CodeEntry(OpCodes.LDC_CHR, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI8:
                    code.Add(new CodeEntry(OpCodes.LDC_UI8, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I8:
                    code.Add(new CodeEntry(OpCodes.LDC_I8, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI16:
                    code.Add(new CodeEntry(OpCodes.LDC_UI16, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I16:
                    code.Add(new CodeEntry(OpCodes.LDC_I16, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI32:
                    code.Add(new CodeEntry(OpCodes.LDC_UI32, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I32:
                    code.Add(new CodeEntry(OpCodes.LDC_I32, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI64:
                    code.Add(new CodeEntry(OpCodes.LDC_UI64, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I64:
                    code.Add(new CodeEntry(OpCodes.LDC_I64, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Double:
                    code.Add(new CodeEntry(OpCodes.LDC_F, new TokenOperand(node.Token)));
                    break;
                case DataTypes.String:
                    code.Add(new CodeEntry(OpCodes.LDC_STR, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Null:
                    code.Add(new CodeEntry(OpCodes.LDC_NULL, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Bool:
                    code.Add(new CodeEntry(OpCodes.LDC_BOOL, new TokenOperand(node.Token)));
                    break;
            }
        }

        private void loadVar(Node node) {
            switch (node.RelatedNamedData) {
                case Parameter param:
                    //var paramIdx = currentMethod.Parameters.IndexOf(param);
                    code.Add(new CodeEntry(OpCodes.LDARG, new NamedOperand(param)));
                    break;
                case Variable variable:
                    //var locIdx = currentMethod.DeclaredLocals[variable];
                    code.Add(new CodeEntry(OpCodes.LDLOC, new NamedOperand(variable)));
                    break;
                case Method meth:
                    //var sign = meth.Signature;
                    code.Add(new CodeEntry(OpCodes.CALL, new NamedOperand(meth)));
                    break;
                case Field field:
                    //var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    code.Add(new CodeEntry(OpCodes.LDFLD, new NamedOperand(field)));
                    break;
            }
        }

        private void storeVar(Node node) {
            switch (node.RelatedNamedData) {
                case Parameter param:
                    //var paramIdx = currentMethod.Parameters.IndexOf(param);
                    code.Add(new CodeEntry(OpCodes.STARG, new NamedOperand(param)));
                    break;
                case Variable variable:
                    //var locIdx = currentMethod.DeclaredLocals[variable];
                    code.Add(new CodeEntry(OpCodes.STLOC, new NamedOperand(variable)));
                    break;
                case Field field:
                    //var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    code.Add(new CodeEntry(OpCodes.STFLD, new NamedOperand(field)));
                    break;
            }
        }
    }
}

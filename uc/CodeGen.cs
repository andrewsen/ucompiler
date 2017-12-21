using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Translator;

namespace uc
{
    class CodeEntry
    {
        public string View;

        public CodeEntry(string val)
        {
            View = val;
        }
    }

    public class CodeGen
    {
        private DirectiveList directiveList;
        private MetadataList metadataList;
        private ClassList classList;
        //private FileInfo output;
        private CompilerConfig config;
        //private FileStream outputWriter;

        private ClassType currentClass;
        private Method currentMethod;

        private List<CodeEntry> code = new List<CodeEntry>();

        private Stack<string> continues = new Stack<string>();
        private Stack<string> breaks = new Stack<string>();

        private StringBuilder result = new StringBuilder();

        public CodeGen(CompilerConfig conf, DirectiveList directives, MetadataList metadata, ClassList classes)
        {
            config = conf;
            directiveList = directives;
            metadataList = metadata;
            classList = classes;
        }

        public void Generate()
        {
            //output = new FileInfo(config.OutInfoFile);
            //outputWriter = output.OpenWrite();
            foreach (var clazz in classList)
                compileClass(clazz);

            File.WriteAllText(config.OutInfoFile, result.ToString());
        }

        private void write(string line, int offset=0)
        {
            result.Append(new string(' ', offset) + line);
        }

        private void compileClass(ClassType clazz)
        {
            var scope = clazz.Scope.ToString().ToLower();
            string classDecl = $".{scope} {clazz.Name} {{\n";
            write(classDecl);

            currentClass = clazz;

            compileFields(clazz);
            compileMethods(clazz);

            write("}\n\n");

        }

        private void compileFields(ClassType clazz)
        {
            write(".fields\n", 4);

            var fields = clazz.SymbolTable.Fields;
            for (int i = 0; i < fields.Count; ++i)
            {
                var fld = fields[i];
                var scope = fld.Scope.ToString().ToLower();
                var modifiers = fld.Modifiers.ToString().ToLower();

                string line = $"{i} : {fld.Type.ToString()} -> {fld.Name} {modifiers} .{scope}";
                write($"{line}\n", 8);
            }

            write(".end-fields\n\n", 4);
        }

        private void compileMethods(ClassType clazz)
        {
            var methods = clazz.SymbolTable.Methods;
            for (int i = 0; i < methods.Count; ++i)
            {
                var method = methods[i];
                var scope = method.Scope.ToString().ToLower();
                var modifiers = method.Modifiers.ToString().ToLower();

                string line = $".{scope} {modifiers} {method.Type} {method.Name} ({method.Parameters.ToString()})";
                write($"{line} {{\n", 4);

                currentMethod = method;
                code.Clear();
                compileMethodBody(method);
                code.ForEach(entry => write(entry.View + "\n", 8));

				write("}\n\n", 4);
            }

        }

        private void compileMethodBody(Method method)
        {
            write(".locals\n", 8);

            var locals = method.DeclaredLocals;
            foreach(var pair in locals)
            {
                var loc = pair.Key;
                var idx = pair.Value;

                string line = $"{idx} : {loc.Type.ToString()} -> {loc.Name}";
                write($"{line}\n", 12);
            }

            write(".end-locals\n", 8);

            compileBlock(method.Body);
        }

        private void compileIExpr(IExpression iexpr)
        {
            if (iexpr is CodeBlock blk)
                compileBlock(blk);
            else if (iexpr is Expression expr)
                compileExpr(expr);
        }

        private void compileBlock(CodeBlock block)
        {
            foreach (var iexpr in block.Expressions)
            {
                switch (iexpr)
                {
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

        private void compileBreak()
        {
            code.Add(new CodeEntry($"jmp {breaks.Peek()}:"));
        }

        private void compileContinue()
        {
            code.Add(new CodeEntry($"jmp {continues.Peek()}:"));
        }

        private void compileIf(If ifExpr)
        {
            var ifs = ifExpr.Conditions;

            var outLabel = $"_if_out_{code.Count}";
            foreach (var cond in ifs)
            {
                compileExpr(ifExpr.MasterIf.Condition);

                var passLabel = $"_if_next_{code.Count}";
                code.Add(new CodeEntry($"jfalse {passLabel}"));

                compileIExpr(cond.Body);

                code.Add(new CodeEntry($"jmp {outLabel}"));
                code.Add(new CodeEntry($"{passLabel}:"));
            }
            compileIExpr(ifExpr.ElsePart);
            code.Add(new CodeEntry($"{outLabel}:"));
        }

        private void compileWhile(While whileExpr)
        {
            var inLabel = $"_while_in_{code.Count}";
            var elseLabel = $"_while_else_{code.Count}";
            var outLabel = $"_while_out_{code.Count}";

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            code.Add(new CodeEntry($"{inLabel}:"));
            compileExpr(whileExpr.Condition);

            code.Add(new CodeEntry($"jfalse {outLabel}"));
            compileIExpr(whileExpr.Body);
            code.Add(new CodeEntry($"jmp {inLabel}"));
            code.Add(new CodeEntry($"{elseLabel}:"));
            compileIExpr(whileExpr.ElsePart);
            code.Add(new CodeEntry($"{outLabel}:"));
            continues.Pop();
            breaks.Pop();
        }

        private void compileDoWhile(DoWhile doWhileExpr)
        {
            var inLabel = $"_do_while_in_{code.Count}";
            var outLabel = $"_do_while_out_{code.Count}";

            continues.Push(inLabel);
            breaks.Push(outLabel);

            code.Add(new CodeEntry($"{inLabel}:"));
			compileIExpr(doWhileExpr.Body);

            compileExpr(doWhileExpr.Condition);
            code.Add(new CodeEntry($"jtrue {inLabel}"));
            code.Add(new CodeEntry($"jmp {inLabel}"));
            code.Add(new CodeEntry($"{outLabel}:"));
            continues.Pop();
            breaks.Pop();
        }

        private void compileFor(For forExpr)
        {
            // Condition
            compileIExpr(forExpr.Scope.Expressions[0]);

            var inLabel = $"_for_in_{code.Count}";
			var elseLabel = $"_for_else_{code.Count}";
            var outLabel = $"_for_out_{code.Count}";

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            code.Add(new CodeEntry(inLabel));
            compileExpr(forExpr.Condition);
            compileIExpr(forExpr.Body);
            compileExpr(forExpr.Iteration);
            code.Add(new CodeEntry($"jmp {inLabel}"));
            code.Add(new CodeEntry($"{elseLabel}:"));
            compileIExpr(forExpr.ElsePart);
            code.Add(new CodeEntry($"{outLabel}:"));
            continues.Pop();
            breaks.Pop();
        }

        private void compileExpr(Expression expr)
        {
            compileNode(expr.ExpressionRoot);
        }

        private void compileNode(Node node)
        {
            if (node.Token == null)
            {
                code.Add(new CodeEntry("nop"));
            }
            else if (node.Token.IsOp())
            {
                compileOp(node);
            }
            else if(node.Left != null && !(node.Left.Token is TypedToken))
            {
                if (node.RelatedNamedData != null)
                    loadVar(node);
                else
                    loadConst(node);
            }
        }

        private void compileOp(Node node)
        {
            // TODO: Rewrite
            if (node.Token.IsOp(OperationType.Assign))
            {
                compileNode(node.Right);
            }
            else if(node.Token.Operation.Association == Association.Right || node.Token.IsOp(OperationType.FunctionCall))
            {
                foreach (var child in node.Children)
                    compileNode(child);
            }
            else
            {
                for (int i = node.Children.Count-1; i >= 0; --i)
                    compileNode(node.Children[i]);
            }

            switch (node.Token.Operation.Type)
            {
                case OperationType.PreInc:
                    code.Add(new CodeEntry("inc"));
                    break;
                case OperationType.PreDec:
                    code.Add(new CodeEntry("dec"));
                    break;
                case OperationType.PostInc: // TODO: Shit 1
                    code.Add(new CodeEntry("inc"));
                    break;
                case OperationType.PostDec: // TODO: Shit 2
                    code.Add(new CodeEntry("dec"));
                    break;
                case OperationType.UnaryPlus:
                    break;
                case OperationType.UnaryMinus:
                    code.Add(new CodeEntry("neg"));
                    break;
                case OperationType.Add:
                    code.Add(new CodeEntry("add"));
                    break;
                case OperationType.Sub:
                    code.Add(new CodeEntry("sub"));
                    break;
                case OperationType.Mul:
                    code.Add(new CodeEntry("mul"));
                    break;
                case OperationType.Div:
                    code.Add(new CodeEntry("div"));
                    break;
                case OperationType.Mod:
                    code.Add(new CodeEntry("mod"));
                    break;
                case OperationType.BinOr:
                    code.Add(new CodeEntry("or"));
                    break;
                case OperationType.BinAnd:
                    code.Add(new CodeEntry("and"));
                    break;
                case OperationType.Xor:
                    code.Add(new CodeEntry("xor"));
                    break;
                case OperationType.Inv:
                    code.Add(new CodeEntry("inv"));
                    break;
                case OperationType.ShiftLeft:
                    code.Add(new CodeEntry("shl"));
                    break;
                case OperationType.ShiftRight:
                    code.Add(new CodeEntry("shr"));
                    break;
                case OperationType.Not:
                    code.Add(new CodeEntry("not"));
                    break;
                case OperationType.And:
                    code.Add(new CodeEntry("and"));
                    break;
                case OperationType.Or:
                    code.Add(new CodeEntry("or"));
                    break;
                case OperationType.Equals:
                    code.Add(new CodeEntry("eq"));
                    break;
                case OperationType.NotEquals:
                    code.Add(new CodeEntry("neq"));
                    break;
                case OperationType.GreaterEquals:
                    code.Add(new CodeEntry("gre"));
                    break;
                case OperationType.LowerEquals:
                    code.Add(new CodeEntry("lre"));
                    break;
                case OperationType.Greater:
                    code.Add(new CodeEntry("gr"));
                    break;
                case OperationType.Lower:
                    code.Add(new CodeEntry("lr"));
                    break;
                case OperationType.Cast:
                    code.Add(new CodeEntry("conv_" + node.Type.ToString()));
                    break;
                case OperationType.ArrayGet:
                    code.Add(new CodeEntry("ldelem"));
                    break;
                case OperationType.ArrayMutate:
                    code.Add(new CodeEntry("stelem"));
                    break;
                case OperationType.NewObj:
                    code.Add(new CodeEntry("newobj " + node.Type.ToString()));
                    break;
                case OperationType.NewArr:
                    code.Add(new CodeEntry("newarr " + (node.Left.Token as TypedToken).BoundType.ToString()));
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

        private void loadConst(Node node)
        {
            switch (node.Type.Type)
            {
                case DataTypes.Char:
                    code.Add(new CodeEntry($"ldc_char {node.Token.Representation}"));
                    break;
                case DataTypes.UI8:
                    code.Add(new CodeEntry($"ldc_ui8 {node.Token.Representation}"));
                    break;
                case DataTypes.I8:
                    code.Add(new CodeEntry($"ldc_i8 {node.Token.Representation}"));
                    break;
                case DataTypes.UI16:
                    code.Add(new CodeEntry($"ldc_ui16 {node.Token.Representation}"));
                    break;
                case DataTypes.I16:
                    code.Add(new CodeEntry($"ldc_i16 {node.Token.Representation}"));
                    break;
                case DataTypes.UI32:
                    code.Add(new CodeEntry($"ldc_ui32 {node.Token.Representation}"));
                    break;
                case DataTypes.I32:
                    code.Add(new CodeEntry($"ldc_i32 {node.Token.Representation}"));
                    break;
                case DataTypes.UI64:
                    code.Add(new CodeEntry($"ldc_ui64 {node.Token.Representation}"));
                    break;
                case DataTypes.I64:
                    code.Add(new CodeEntry($"ldc_i64 {node.Token.Representation}"));
                    break;
                case DataTypes.Double:
                    code.Add(new CodeEntry($"ldc_f {node.Token.Representation}"));
                    break;
                case DataTypes.String:
                    code.Add(new CodeEntry($"ldc_str {node.Token.Representation}"));
                    break;
                case DataTypes.Null:
                    code.Add(new CodeEntry($"ldnull {node.Token.Representation}"));
                    break;
                case DataTypes.Bool:
                    code.Add(new CodeEntry($"ldc_bool {node.Token.Representation}"));
                    break;
            }
        }

        private void loadVar(Node node)
        {
            switch (node.RelatedNamedData)
            {
                case Parameter param:
                    var paramIdx = currentMethod.Parameters.IndexOf(param) + 1;
                    code.Add(new CodeEntry($"ldarg {paramIdx}"));
                    break;
                case Variable variable:
                    var locIdx = currentMethod.DeclaredLocals[variable];
                    code.Add(new CodeEntry($"ldloc {locIdx}"));
                    break;
                case Method meth:
                    var sign = meth.Signature;
                    code.Add(new CodeEntry($"call {sign}"));
                    break;
                case Field field:
                    var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    code.Add(new CodeEntry($"ldfld {field.Class.Name}::{fldIdx}"));
                    break;
            }
        }

        private void storeVar(Node node)
        {
            switch (node.RelatedNamedData)
            {
                case Parameter param:
                    var paramIdx = currentMethod.Parameters.IndexOf(param) + 1;
                    code.Add(new CodeEntry($"starg {paramIdx}"));
                    break;
                case Variable variable:
                    var locIdx = currentMethod.DeclaredLocals[variable];
                    code.Add(new CodeEntry($"stloc {locIdx}"));
                    break;
                case Field field:
                    var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    code.Add(new CodeEntry($"stfld {field.Class.Name}::{fldIdx}"));
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Translator;

namespace uc
{
    public interface IOperand
    {

    }

    public class NamedOperand : IOperand
    {
        public readonly INamedDataElement element;

        public NamedOperand(INamedDataElement val)
        {
            element = val;
        }

        public override string ToString() => element.Name;
    }

    public class TokenOperand : IOperand
    {
        public readonly Token InnerToken;

        public TokenOperand(Token inner)
        {
            InnerToken = inner;
        }

        public override string ToString() => InnerToken.ToString();
    }

    public class TypeOperand : IOperand
    {
        public readonly IType Type;

        public TypeOperand(IType inner)
        {
            Type = inner;
        }

        public override string ToString() => Type.ToString();
    }

    public class Label : IOperand
    {
        private static int _uniqueIndex = 0;

        public readonly string Name = $"L_{_uniqueIndex++}";

        public override string ToString() => Name;
    }

    public class CodeEntry
    {
        public readonly OpCodes Operation;
        public readonly IOperand[] Operands;

        public CodeEntry(OpCodes op, params IOperand[] operands)
        {
            Operation = op;
            Operands = operands;
        }

        public override string ToString()
        {
            return $"{Operation.ToString().ToLower()} {string.Join(" ", (object[])Operands)}";
        }
    }

    public class IRCodeGenerator
    {
        private Method _method;

        private Stack<Label> continues = new Stack<Label>();
        private Stack<Label> breaks = new Stack<Label>();

        public IRCodeGenerator(Method method)
        {
            _method = method;
        }

        public void Generate()
        {
            compileMethodBody(_method);
        }

        private void compileMethodBody(Method method)
        {
            method.IntermediateCode = new List<CodeEntry>();
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

        private void compileReturn(Return ret)
        {
            compileExpr(ret.Expression);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.RET));
        }

        private void compileBreak()
        {
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JMP, breaks.Peek()));
        }

        private void compileContinue()
        {
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JMP, continues.Peek()));
        }

        private void compileIf(If ifExpr)
        {
            var ifs = ifExpr.Conditions;

            var outLabel = new Label();
            foreach (var cond in ifs)
            {
                compileExpr(ifExpr.MasterIf.Condition);

                var passLabel = new Label();
                _method.IntermediateCode.Add(new CodeEntry(OpCodes.JFALSE, passLabel));

                compileIExpr(cond.Body);

                _method.IntermediateCode.Add(new CodeEntry(OpCodes.JFALSE, outLabel));
                _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, passLabel));
            }
            compileIExpr(ifExpr.ElsePart);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, outLabel));
        }

        private void compileWhile(While whileExpr)
        {
            var inLabel = new Label();
            var elseLabel = new Label();
            var outLabel = new Label();

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileExpr(whileExpr.Condition);

            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JFALSE, outLabel));
            compileIExpr(whileExpr.Body);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JMP, inLabel));
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, elseLabel));
            compileIExpr(whileExpr.ElsePart);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, outLabel));
            continues.Pop();
            breaks.Pop();
        }

        private void compileDoWhile(DoWhile doWhileExpr)
        {
            var inLabel =  new Label();
            var outLabel = new Label();

            continues.Push(inLabel);
            breaks.Push(outLabel);

            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileIExpr(doWhileExpr.Body);

            compileExpr(doWhileExpr.Condition);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JTRUE, inLabel));
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JMP, inLabel));
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, outLabel));
            continues.Pop();
            breaks.Pop();
        }

        private void compileFor(For forExpr)
        {
            // Condition
            compileIExpr(forExpr.Scope.Expressions[0]);

            var inLabel =   new Label();
            var elseLabel = new Label();
            var outLabel =  new Label();

            continues.Push(inLabel);
            breaks.Push(elseLabel);

            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, inLabel));
            compileExpr(forExpr.Condition);
            compileIExpr(forExpr.Body);
            compileExpr(forExpr.Iteration);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.JMP, inLabel));
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, elseLabel));
            compileIExpr(forExpr.ElsePart);
            _method.IntermediateCode.Add(new CodeEntry(OpCodes.LABEL, outLabel));
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
                _method.IntermediateCode.Add(new CodeEntry(OpCodes.NOP));
            }
            else if (node.Token.IsOp())
            {
                compileOp(node);
            }
            else if(!(node.Token is TypedToken))
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
                compileNode(node.Left);
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
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.INC));
                    break;
                case OperationType.PreDec:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.DEC));
                    break;
                case OperationType.PostInc: // TODO: Shit 1
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.INC));
                    break;
                case OperationType.PostDec: // TODO: Shit 2
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.DEC));
                    break;
                case OperationType.UnaryPlus:
                    break;
                case OperationType.UnaryMinus:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.NEG));
                    break;
                case OperationType.Add:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.ADD));
                    break;
                case OperationType.Sub:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.SUB));
                    break;
                case OperationType.Mul:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.MUL));
                    break;
                case OperationType.Div:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.DIV));
                    break;
                case OperationType.Mod:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.REM));
                    break;
                case OperationType.BinOr:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.OR));
                    break;
                case OperationType.BinAnd:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.AND));
                    break;
                case OperationType.Xor:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.XOR));
                    break;
                case OperationType.Inv:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.INV));
                    break;
                case OperationType.ShiftLeft:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.SHL));
                    break;
                case OperationType.ShiftRight:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.SHR));
                    break;
                case OperationType.Not:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.NOT));
                    break;
                case OperationType.And:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.AND));
                    break;
                case OperationType.Or:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.OR));
                    break;
                case OperationType.Equals:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.EQ));
                    break;
                case OperationType.NotEquals:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.NEQ));
                    break;
                case OperationType.GreaterEquals:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.GTE));
                    break;
                case OperationType.LowerEquals:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LTE));
                    break;
                case OperationType.Greater:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.GT));
                    break;
                case OperationType.Lower:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LT));
                    break;
                case OperationType.Cast:
                    //_method.IntermediateCode.Add(new CodeEntry(OpCodes.CONV, new TypeOperand(node.Type)));
                    _method.IntermediateCode.Add(castToOp(node.Type));
                    break;
                case OperationType.ArrayGet:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDELEM));
                    break;
                case OperationType.ArrayMutate:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.STELEM));
                    break;
                case OperationType.NewObj:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.NEWOBJ, new TypeOperand(node.Type), new NamedOperand(node.RelatedNamedData)));
                    break;
                case OperationType.NewArr:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.NEWARR, new TypeOperand((node.Left.Token as TypedToken).BoundType)));
                    break;
                case OperationType.Assign:
                    storeVar(node.Left);
                    if (node.ValueAction.HasFlag(ValueAction.Load))
                        loadVar(node.Left);
                    break;
                case OperationType.FunctionCall:
                    break;
                case OperationType.MemberAccess:
                    break;
            }
        }

        private CodeEntry castToOp(IType type)
        {
            OpCodes castOp;
            switch (type.Type)
            {
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

        private void loadConst(Node node)
        {
            switch (node.Type.Type)
            {
                case DataTypes.Char:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_CHR, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI8:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_UI8, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I8:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_I8, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI16:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_UI16, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I16:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_I16, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI32:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_UI32, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I32:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_I32, new TokenOperand(node.Token)));
                    break;
                case DataTypes.UI64:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_UI64, new TokenOperand(node.Token)));
                    break;
                case DataTypes.I64:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_I64, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Double:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_F, new TokenOperand(node.Token)));
                    break;
                case DataTypes.String:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_STR, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Null:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_NULL, new TokenOperand(node.Token)));
                    break;
                case DataTypes.Bool:
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDC_BOOL, new TokenOperand(node.Token)));
                    break;
            }
        }

        private void loadVar(Node node)
        {
            switch (node.RelatedNamedData)
            {
                case Parameter param:
                    //var paramIdx = currentMethod.Parameters.IndexOf(param);
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDARG, new NamedOperand(param)));
                    break;
                case Variable variable:
                    //var locIdx = currentMethod.DeclaredLocals[variable];
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDLOC, new NamedOperand(variable)));
                    break;
                case Method meth:
                    //var sign = meth.Signature;
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.CALL, new NamedOperand(meth)));
                    break;
                case Field field:
                    //var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.LDFLD, new NamedOperand(field)));
                    break;
            }
        }

        private void storeVar(Node node)
        {
            switch (node.RelatedNamedData)
            {
                case Parameter param:
                    //var paramIdx = currentMethod.Parameters.IndexOf(param);
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.STARG, new NamedOperand(param)));
                    break;
                case Variable variable:
                    //var locIdx = currentMethod.DeclaredLocals[variable];
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.STLOC, new NamedOperand(variable)));
                    break;
                case Field field:
                    //var fldIdx = field.Class.SymbolTable.FieldIndex(field);
                    _method.IntermediateCode.Add(new CodeEntry(OpCodes.STFLD, new NamedOperand(field)));
                    break;
            }
        }
    }
}

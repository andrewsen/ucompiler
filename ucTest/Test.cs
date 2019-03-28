using NUnit.Framework;
using System;
using Compiler;
using System.Collections.Generic;
using Compiler;

namespace CompilerTest
{
    [TestFixture()]
    public class Test
    {
        private Compiler compiler;
        private const string testSource = @"/home/senko/qt/ualang/framework_v2/bin/test.mc2";
        private const string testOut = @"/home/senko/qt/ualang/framework_v2/bin/test.a2";
        private const string testExpr2 =
@"
class Test
{
    public int number;

    public Test();

    private double fun(int x)
    {
        int[] arr = new int[10];
        arr[3] = 42;
    }
}
";
        private const string testExpr3 = @"x = 3 - i++;";
        private const string testExpr4 = @"y = x.fun();"; // x fld . fun . () next .
        private const string testExpr5 = @"gen(1, a+(b*c), 3+2, true);";
        private const string testExpr6 = @"f(g(a, b), h(c + d));";

        [SetUp]
        public void InitCompiler() {
            compiler = new Compiler(new CompilerConfig() {
                Sources = { "<test>" }
            });
        }

        [Test]
        public void TestExpression1() {
            //PrintExpression(compiler.evalExpression(new TokenStream(testExpr1, "<test>"), ParsingPolicy.Semicolon));
            CodeBlock root = new CodeBlock();
            var stream = new TokenStream(testExpr1, "<test>");
            stream.Next();
            compiler.evalStatement(root, stream);
            Assert.NotNull(root);
        }

        [Test]
        public void TestExpression2() {
            compiler = new Compiler(new CompilerConfig() {
                OutInfoFile = "/home/senko/qt/ualang/framework_v2/bin/unit-tests.a2",
                Sources = { "/home/senko/qt/ualang/framework_v2/bin/test.mc2" }
            });
            compiler.Compile();
            Assert.NotNull(compiler);
        }

        //[Test]
        public void TestExpression3() {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr3, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression4() {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr4, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression5() {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr5, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression6() {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr6, "<test>"), ParsingPolicy.SyncTokens));
        }

        [Test]
        public void TestCodegen() {
            var generator = new Compiler(new CompilerConfig {
                OutInfoFile = testOut,
                Sources = new List<string> { testSource }
            });

            var classes = generator.ClassList;

            foreach (var clazz in classes) {
                foreach (var method in clazz.SymbolTable.Methods) {
                    Assert.True(validateCode(method.IntermediateCode));
                }
            }
        }

        private bool validateCode(List<CodeEntry> intermediateCode) {
            int stackSize = 0;
            foreach (var op in intermediateCode) {
                switch (op.Operation) {
                    case OpCodes.NOP:
                        // stack 0
                        break;
                    case OpCodes.INV:
                        // stack 0
                        break;
                    case OpCodes.DUP:
                        stackSize++;
                        break;
                    case OpCodes.ADD:
                        stackSize--;
                        break;
                    case OpCodes.SUB:
                        stackSize--;
                        break;
                    case OpCodes.MUL:
                        stackSize--;
                        break;
                    case OpCodes.DIV:
                        stackSize--;
                        break;
                    case OpCodes.REM:
                        stackSize--;
                        break;
                    case OpCodes.NEG:
                        break;
                    case OpCodes.CONV_UI8:
                        break;
                    case OpCodes.CONV_I8:
                        break;
                    case OpCodes.CONV_UI16:
                        break;
                    case OpCodes.CONV_I16:
                        break;
                    case OpCodes.CONV_CHR:
                        break;
                    case OpCodes.CONV_I32:
                        break;
                    case OpCodes.CONV_UI32:
                        break;
                    case OpCodes.CONV_I64:
                        break;
                    case OpCodes.CONV_UI64:
                        break;
                    case OpCodes.CONV_F:
                        break;
                    case OpCodes.CONV_BOOL:
                        break;
                    case OpCodes.JMP:
                    case OpCodes.JZ:
                    case OpCodes.JFALSE:
                    case OpCodes.JNZ:
                    case OpCodes.JTRUE:
                    case OpCodes.JNULL:
                    case OpCodes.JNNULL:
                        if (stackSize != 0)
                            return false;
                        break;
                    case OpCodes.CALL:
                    case OpCodes.CALLS:
                        var meth = (op.Operands[0] as NamedOperand).element as Method;
                        stackSize -= meth.Parameters.Count;
                        if (!(meth.Type is PlainType p && p.Type == DataTypes.Void))
                            stackSize++;
                        break;
                    case OpCodes.NEWARR:
                        break;
                    case OpCodes.NEWOBJ:
                        var ctor = (op.Operands[1] as NamedOperand).element as Method;
                        stackSize -= ctor.VisibleParameters.Count - 1; // Return value
                        break;
                    case OpCodes.LDLOC:
                        stackSize++;
                        break;
                    case OpCodes.LDLOC_0:
                        stackSize++;
                        break;
                    case OpCodes.LDLOC_1:
                        stackSize++;
                        break;
                    case OpCodes.LDLOC_2:
                        stackSize++;
                        break;
                    case OpCodes.STLOC:
                        stackSize--;
                        break;
                    case OpCodes.STLOC_0:
                        stackSize--;
                        break;
                    case OpCodes.STLOC_1:
                        stackSize--;
                        break;
                    case OpCodes.STLOC_2:
                        stackSize--;
                        break;
                    case OpCodes.LDELEM:
                        stackSize--; // arr, index -> elem
                        break;
                    case OpCodes.LDELEM_0:
                        // arr -> elem
                        break;
                    case OpCodes.LDELEM_1:
                        break;
                    case OpCodes.LDELEM_2:
                        break;
                    case OpCodes.LD_AREF:
                        break;
                    case OpCodes.LD_BYREF:
                        break;
                    case OpCodes.STELEM:
                        break;
                    case OpCodes.STELEM_0:
                        break;
                    case OpCodes.STELEM_1:
                        break;
                    case OpCodes.STELEM_2:
                        break;
                    case OpCodes.ST_BYREF:
                        break;
                    case OpCodes.LDARG:
                        break;
                    case OpCodes.LDARG_0:
                        break;
                    case OpCodes.LDARG_1:
                        break;
                    case OpCodes.LDARG_2:
                        break;
                    case OpCodes.STARG:
                        break;
                    case OpCodes.STARG_0:
                        break;
                    case OpCodes.STARG_1:
                        break;
                    case OpCodes.STARG_2:
                        break;
                    case OpCodes.LDFLD:
                        break;
                    case OpCodes.LDFLD_0:
                        break;
                    case OpCodes.LDFLD_1:
                        break;
                    case OpCodes.LDFLD_2:
                        break;
                    case OpCodes.STFLD:
                        break;
                    case OpCodes.STFLD_0:
                        break;
                    case OpCodes.STFLD_1:
                        break;
                    case OpCodes.STFLD_2:
                        break;
                    case OpCodes.LDSFLD:
                        break;
                    case OpCodes.LDSFLD_0:
                        break;
                    case OpCodes.LDSFLD_1:
                        break;
                    case OpCodes.LDSFLD_2:
                        break;
                    case OpCodes.STSFLD:
                        break;
                    case OpCodes.STSFLD_0:
                        break;
                    case OpCodes.STSFLD_1:
                        break;
                    case OpCodes.STSFLD_2:
                        break;
                    case OpCodes.LDC_STR:
                        break;
                    case OpCodes.LDC_UI8:
                        break;
                    case OpCodes.LDC_I8:
                        break;
                    case OpCodes.LDC_UI16:
                        break;
                    case OpCodes.LDC_I16:
                        break;
                    case OpCodes.LDC_CHR:
                        break;
                    case OpCodes.LDC_I32:
                        break;
                    case OpCodes.LDC_UI32:
                        break;
                    case OpCodes.LDC_I64:
                        break;
                    case OpCodes.LDC_UI64:
                        break;
                    case OpCodes.LDC_F:
                        break;
                    case OpCodes.LDC_TRUE:
                        break;
                    case OpCodes.LDC_FALSE:
                        break;
                    case OpCodes.LDC_NULL:
                        break;
                    case OpCodes.AND:
                        break;
                    case OpCodes.OR:
                        break;
                    case OpCodes.EQ:
                        break;
                    case OpCodes.NEQ:
                        break;
                    case OpCodes.NOT:
                        break;
                    case OpCodes.XOR:
                        break;
                    case OpCodes.INC:
                        break;
                    case OpCodes.DEC:
                        break;
                    case OpCodes.SHL:
                        break;
                    case OpCodes.SHR:
                        break;
                    case OpCodes.POP:
                        break;
                    case OpCodes.GT:
                        break;
                    case OpCodes.GTE:
                        break;
                    case OpCodes.LT:
                        break;
                    case OpCodes.LTE:
                        break;
                    case OpCodes.SIZEOF:
                        break;
                    case OpCodes.TYPEOF:
                        break;
                    case OpCodes.RET:
                        break;
                    case OpCodes.LABEL:
                        break;
                    case OpCodes.CONV:
                        break;
                    case OpCodes.LDC_BOOL:
                        break;
                    default:
                        break;
                }
                if (stackSize < 0)
                    return false;
            }
        }

        private void PrintExpression(List<Token> expression) {
            var result = "";
            foreach (var t in expression) {
                result += t + " ";
            }
            Console.WriteLine(result);

            var root = compiler.expressionToAST(expression);
            Assert.NotNull(root, "Root is null");

            InfoProvider.Print();
        }
    }
}

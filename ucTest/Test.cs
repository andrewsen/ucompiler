using NUnit.Framework;
using System;
using Translator;
using System.Collections.Generic;

namespace ucTest
{
    [TestFixture()]
    public class Test
    {
        private Compiler compiler;
        private const string testExpr1 = @"double x = 2 + 3.0 * 4;";
        private const string testExpr2 = 
@"
class Test
{
    public int number;

    public Test();

    private double fun(int x)
    {
        int val = x + 4;
        double res = fun(val) + x;
    }

    private void testf()
    {
        double x = number + fun(4) * 4.3;
    }
}
";
        private const string testExpr3 = @"x = 3 - i++;";
        private const string testExpr4 = @"y = x.fun();"; // x fld . fun . () next .
        private const string testExpr5 = @"gen(1, a+(b*c), 3+2, true);";
        private const string testExpr6 = @"f(g(a, b), h(c + d));";

        [SetUp]
        public void InitCompiler()
        {
            compiler = new Compiler(new CompilerConfig()
            {
                Sources = { "<test>" }
            });
        }

        [Test]
        public void TestExpression1()
        {
            //PrintExpression(compiler.evalExpression(new TokenStream(testExpr1, "<test>"), ParsingPolicy.Semicolon));
            CodeBlock root = new CodeBlock();
            var stream = new TokenStream(testExpr1, "<test>");
            stream.Next();
            compiler.evalStatement(root, stream);
            Assert.NotNull(root);
        }

        [Test]
        public void TestExpression2()
        {
            compiler = new Compiler(new CompilerConfig()
            {
                Sources = { testExpr2 }
            });
            compiler.Compile();
            Assert.NotNull(compiler);
        }

        //[Test]
        public void TestExpression3()
        {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr3, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression4()
        {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr4, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression5()
        {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr5, "<test>"), ParsingPolicy.SyncTokens));
        }

        //[Test]
        public void TestExpression6()
        {
            PrintExpression(compiler.buildPostfixForm(new TokenStream(testExpr6, "<test>"), ParsingPolicy.SyncTokens));
        }

        private void PrintExpression(List<Token> expression)
        {
            var result = "";
            foreach(var t in expression)
            {
                result += t + " ";
            }
            Console.WriteLine(result);

            var root = compiler.expressionToAST(expression);
            Assert.NotNull(root, "Root is null");

            InfoProvider.Print();
        }
    }
}

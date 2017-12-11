using NUnit.Framework;
using System;
using Lab4;

namespace ucTest
{
    [TestFixture()]
    public class Test
    {
        private Compiler compiler;
        private const string testExpr1 = @"a + b * c + d";
        private const string testExpr2 = @"x = 3 - ++i;";
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
            PrintExpression(compiler.evalExpression(new Reader(testExpr1, "<test>"), ParsingPolicy.Semicolon));
        }

        [Test]
        public void TestExpression2()
        {
            PrintExpression(compiler.evalExpression(new Reader(testExpr2, "<test>"), ParsingPolicy.Semicolon));
        }

        [Test]
        public void TestExpression3()
        {
            PrintExpression(compiler.evalExpression(new Reader(testExpr3, "<test>"), ParsingPolicy.Semicolon));
        }

        [Test]
        public void TestExpression4()
        {
            PrintExpression(compiler.evalExpression(new Reader(testExpr4, "<test>"), ParsingPolicy.Semicolon));
        }

        [Test]
        public void TestExpression5()
        {
            PrintExpression(compiler.evalExpression(new Reader(testExpr5, "<test>"), ParsingPolicy.Semicolon));
        }

        [Test]
        public void TestExpression6()
        {
            PrintExpression(compiler.evalExpression(new Reader(testExpr6, "<test>"), ParsingPolicy.Semicolon));
        }

        private void PrintExpression(Expression expression)
        {
            var result = "";
            foreach(var t in expression.Tokens)
            {
                result += t + " ";
            }
            Console.WriteLine(result);

            var root = compiler.expressionToAST(expression);
            Assert.NotNull(root, "Root is null");

            CompilerLog.Print();
        }
    }
}

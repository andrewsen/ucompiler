using System;
using System.Collections.Generic;
using System.IO;

namespace Translator
{
    class MainClass
    {
        private const string testExpr =
@"
class Test
{
    public int number;

    public Test()
    {
        ;
        var test = new Test[10];
        //int[] arr = new int[10];
    }

    private double fun(int x)
    {
        var val = x + 4;
        double res;
        if(x > 0) {
            res = fun(val) + x;
        }
        else if(x < 0)
            res = 0;
        else
        {
            res = 1;
        }
    }

    private void testf()
    {
        double x = number + fun(4) * 4.3;
    }
}

class Another
{
    public void print(string str)
    {
    }

    public void Func()
    {
        var test = new Test();
        test.fun(3);

        //for(int i = 0; i < 10; ++i)

        while(true)
            print(""hello"");
        else
            print(""bye"");
    }
}
";
        public static void Main(string[] args)
        {
            // a = arr[4] + (c - d * 4);
            //TokenStream stream = new TokenStream(File.ReadAllText(args[0]), args[0]);
            Compiler compiler = new Compiler(new CompilerConfig() {
                Sources = new List<string> { testExpr }
            });

            InfoProvider.ErrorLimitReached += () => {
                InfoProvider.Print();
                Environment.Exit(1);
            };

            //var expr = compiler.parseExpression(new TokenStream(args[0], "<stdin>")) as Expression;
            ////var root = compiler.buildAST(expr);
            //foreach(var tok in expr.Tokens)
            //{
            //    Console.Write(tok + " ");
            //}
            //Console.WriteLine();
            //InfoProvider.Print();
            //try
            //{
                compiler.Compile();
            //}
            //catch(Exception)
            //{
            //    InfoProvider.Print();
            //}
            return;
            //Token tok = stream.Next();
            //while (tok.Type != TokenType.EOF)
            //{
            //    Console.WriteLine("{" + tok + "}: " + tok.Type + 
            //        (tok.Type == TokenType.Constant ? (", " + tok.ConstType) : "" ) + "\n\t" + stream.SourcePosition);
            //    tok = stream.Next();
            //}
        }
    }
}

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
        
    }

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
        public static void Main(string[] args)
        {
            // a = arr[4] + (c - d * 4);
            //TokenStream stream = new TokenStream(File.ReadAllText(args[0]), args[0]);
            Compiler compiler = new Compiler(new CompilerConfig() {
                Sources = new List<string> { testExpr }
            });
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

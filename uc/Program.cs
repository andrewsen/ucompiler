using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    class MainClass
    {
        public static void Main(string[] args) {
            // a = arr[4] + (c - d * 4);
            //TokenStream stream = new TokenStream(File.ReadAllText(args[0]), args[0]);
            Compiler compiler = new Compiler(new CompilerConfig() {
                OutInfoFile = args[1],
                Sources = new List<string> { args[0] }

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

using System;
using System.Collections.Generic;
using System.IO;

namespace Translator
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //'if (a>b) then begin end;'
            // a = arr[4] + (c - d * 4);
            //TokenStream stream = new TokenStream(File.ReadAllText(args[0]), args[0]);
            InfoProvider.ErrorLimitReached += () =>
            {
                InfoProvider.Print();
                Environment.Exit(1);
            };
			Compiler compiler = new Compiler(new CompilerConfig() {
                Sources = new System.Collections.Generic.List<string>() { args[0] }
            });
            var gStream = new TokenStream(File.ReadAllText(args[0]), args[0]);

            CodeBlock rootBlock = new CodeBlock();
            gStream.Next();
            compiler.evalIf(rootBlock, gStream);
            PrintBlock(rootBlock, 0);

            //var expr = compiler.parseExpression(new TokenStream(args[0], "<stdin>")) as Expression;
            //var root = compiler.buildAST(expr);
            //foreach(var tok in expr.Tokens)
            //{
                //Console.Write(tok + " ");
			//}
            //Console.WriteLine();
            InfoProvider.Print();
            //compiler.Compile();
            //Token tok = stream.Next();
            //while (tok.Type != TokenType.EOF)
            //{
            //    Console.WriteLine("{" + tok + "}: " + tok.Type + 
            //        (tok.Type == TokenType.Constant ? (", " + tok.ConstType) : "" ) + "\n\t" + stream.SourcePosition);
            //    tok = stream.Next();
            //}
        }

        public static void PrintBlock(CodeBlock block, int level)
        {
            foreach(var stmt in block.Children)
            {
                switch(stmt)
                {
					case Statement s:
						Console.WriteLine(new string(' ', level) + "Statement:");
                        PrintNode(s.Root, level + 2);
                        break;
					case CodeBlock blk:
                        Console.WriteLine(new string(' ', level) + "Code block begin");
                        PrintBlock(blk, level + 2);
						Console.WriteLine(new string(' ', level) + "Code block end");
                        break;
					case If ifExpr:
						Console.WriteLine(new string(' ', level) + "If:");
						Console.WriteLine(new string(' ', level + 2) + "Condition:");
						PrintNode(ifExpr.Condition, level + 4);
						Console.WriteLine(new string(' ', level + 2) + "Body:");
						Console.WriteLine(new string(' ', level + 2) + "Code block begin");
						PrintBlock(ifExpr.Block, level + 4);
						Console.WriteLine(new string(' ', level + 2) + "Code block end");
                        break;
                }
            }
        }

        public static void PrintNode(Node node, int level)
        {
            Console.Write(new string(' ', level) + node.Token.ToString());
            if(node.Left != null)
            {
                Console.WriteLine(":");
				PrintNode(node.Left, level + 2);
                if (node.Right != null)
				{
                    PrintNode(node.Right, level + 2);
				}
			}
			Console.WriteLine();
        }
    }
}

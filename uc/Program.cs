using System;
using System.IO;

namespace Translator
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //TokenStream stream = new TokenStream(File.ReadAllText(args[0]), args[0]);
            Compiler compiler = new Compiler(new CompilerConfig() {
                Sources = new System.Collections.Generic.List<string>() {args[0]}
            });
            compiler.Compile();
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

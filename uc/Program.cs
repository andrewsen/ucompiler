using System;
using System.Collections.Generic;
using System.IO;

namespace Lab4
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Compiler compiler = new Compiler(new CompilerConfig {
                Sources = new List<string> { "" }
            });

            var expr = compiler.evalExpression(new Reader("b=(2*a + c/d)*2*a", ""), ParsingPolicy.Both);
            var ast = compiler.expressionToAST(expr);
            CompilerLog.Print();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gamma_compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            GammaLexer lexer = new GammaLexer();

            string fileContents;
            using (StreamReader streamReader = new StreamReader(args[0], Encoding.UTF8))
            {
                fileContents = streamReader.ReadToEnd();
            }

            List<CharDFA.Token> tokens = lexer.LexString(args[0], fileContents);

            foreach (CharDFA.Token token in tokens)
            {
                Console.WriteLine(token.name + ", " + token.value);
            }

            GammaParser parser = new GammaParser();
            TokenSA.Node ast = parser.TokenStreamToAST(tokens);



            Console.WriteLine("Press any key to end.");
            Console.ReadKey();

        }
    }
}

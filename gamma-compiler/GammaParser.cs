using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gamma_compiler
{
    class GammaParser
    {

        public enum NodeType {
            ExternFunction,
            Constant,
            FunctionDef,
            VariableDef,
            ModuleDef,
            ModuleBody,
            Statement,
            Expression,
            Literal,
            Variable
        }

        public enum OpOrder
        {



            // Binary Operators
            OpNot,
            OpBinAnd,
            OpBinOr,
            OpBinXor,
        }

        private TokenSA tsa;

        public GammaParser()
        {
            tsa = new TokenSA();

            tsa.RegisterHandler("SYMBOL", HandleSYMBOLToken);
        }
        
        public TokenSA.Node TokenStreamToAST(List<CharDFA.Token> stream)
        {
            TokenSA.Node ast = tsa.ProcessTokens(stream);
            return ast;
        }

        protected bool HandleSYMBOLToken(TokenSA.Node parent, TokenSA.TokenStreamer stream)
        {
            TokenSA.Node node;

            CharDFA.Token? token;

            switch (parent.token.Value.value)
            {
                case "module":


                    while (true)
                    {
                        token = stream.Peek();
                        if (!token.HasValue) break;
                        // Raise an error?

                        
                        if (token.Value.name == "SYMBOL" || token.Value.name == "ACCESSOR")
                        {
                            node = tsa.ProcessNext();
                            if (node == null) break;

                            parent.children.Add(node);
                        }

                        if (token.Value.name == "SEMICOLON")
                        {
                            node = tsa.ProcessNext();
                            break;
                        }

                    }
                    

                    // all of the following until the semi-colon should be ident and accessor


                    break;

                case "":

                    break;

                default:

                    break;
            }

            


            return false;
        }

        private TokenSA.Node symbolModule(TokenSA.Node parent, TokenSA.TokenStreamer stream)
        {
            //parent.
            return parent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gamma_compiler
{
    /* Token Stack Automaton */
    class TokenSA
    {

        public delegate bool CallbackTokenHandler(Node node, TokenStreamer stream);

        public class Node
        {
            public string node_type;
            public List<Node> children;
            public CharDFA.Token? token;

            public Node()
            {
                children = new List<Node>();
                node_type = null;
                token = null;
            }
        }

        public class Comparator
        {
            public enum Direction
            {
                IsIn,
                IsNotIn
            }

            public Direction direction { private set; get; }
            public string[] options { private set; get; }

            public Comparator(Direction dir, string[] options)
            {
                direction = dir;
                this.options = options;
            }

            public bool IsMatch(string val)
            {
                if (direction == Direction.IsIn)
                {
                    return options.Contains(val);
                }
                else
                {
                    return !options.Contains(val);
                }

            }
        }



        public class TokenStreamer
        {
            public List<CharDFA.Token> _tokens { private set; get; }
            public int _pos { private set; get; }

            public TokenStreamer(List<CharDFA.Token> tokens)
            {
                _tokens = tokens;
                _pos = 0;
            }

            public CharDFA.Token? Next()
            {
                if (_pos < _tokens.Count)
                    _pos++;
                else
                    return null;

                return _tokens[_pos - 1];
            }

            public CharDFA.Token? Previous()
            {
                if (_pos > 0)
                    _pos--;
                else
                    return null;

                return _tokens[_pos + 1];
            }

            public CharDFA.Token? Peek()
            {
                if (_pos < _tokens.Count)
                    return _tokens[_pos];

                return null;
            }

            public CharDFA.Token? Recall()
            {
                if (_pos > 0)
                    return _tokens[_pos - 1];

                return null;
            }

        }


        private List<Node> _stack;
        private Node _root = null;
        private TokenStreamer _stream;

        private void PushNode(Node node)
        {
            _stack.Add(node);
        }

        private void PopNode()
        {
            _stack.RemoveAt(_stack.Count - 1);
        }

        public TokenSA()
        {
            _stack = new List<Node>();
            _tokenProductions = new Dictionary<string, CallbackTokenHandler>();
        }

        public Node ProcessTokens(List<CharDFA.Token> tokens)
        {
            _stream = new TokenStreamer(tokens);

            // Create the root node
            _root = new Node();

            PushNode(_root);
            Process(_root);
            PopNode();

            return _root;
        }

        private Dictionary<string, CallbackTokenHandler> _tokenProductions;
        public void RegisterHandler(string token_name, CallbackTokenHandler handler)
        {
            _tokenProductions.Add(token_name, handler);
        }

        public Node ProcessNext()
        {
            CharDFA.Token? token = _stream.Next();

            if (!token.HasValue)
                return null;

            Node node = new Node();
            node.token = token.Value;

            if (_tokenProductions.ContainsKey(token.Value.name))
            {
                PushNode(node);
                _tokenProductions[token.Value.name](node, _stream);
                PopNode();
            }

            return node;
        }

        private void Process(Node parent)
        {

            while (_stream.Peek().HasValue)
            {
                CharDFA.Token? token = _stream.Next();


                // return if stream has ended
                if (token == null) break;


                Node node = new Node();
                node.token = token.Value;

                if (_tokenProductions.ContainsKey(token.Value.name))
                {
                    PushNode(node);
                    _tokenProductions[token.Value.name](node, _stream);
                    PopNode();
                }

                parent.children.Add(node);
            }


        }
    }
}

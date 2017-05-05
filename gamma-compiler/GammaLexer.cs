using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gamma_compiler
{
    class GammaLexer
    {
        private CharDFA cdfa;

        public GammaLexer()
        {
            this.cdfa = new CharDFA();

            CharDFA.State sta_start = cdfa.NewState("start");

            // Ignore whitespace
            sta_start.NewDelta("white_space", "[\\r\\n\\t\\ \x03]", sta_start, false, true);

            // Parse Comment
            CharDFA.State sta_comment_start = cdfa.NewState("comment_start");
            CharDFA.State sta_comment_text = cdfa.NewState("comment_text");
            CharDFA.State sta_comment_end = cdfa.NewState("comment_end", "COMMENT");
            sta_start.NewDelta("start_to_comment_start", "[\\#]", sta_comment_start);
            sta_comment_start.NewDelta("comment_start_to_comment_text", "[^\\r\\n]", sta_comment_text);
            sta_comment_start.NewDelta("comment_start_to_comment_end", "[\\r\\n]", sta_comment_end);
            sta_comment_text.NewDelta("comment_text_to_comment_text", "[^\\r\\n]", sta_comment_text);
            sta_comment_text.NewDelta("comment_text_to_comment_end", "[\\r\\n]", sta_comment_end);

            // Parse CPointer
            CharDFA.State sta_cpointer = cdfa.NewState("cpointer", "CPOINTER");
            sta_start.NewDelta("start_to_cpointer", "[\\&]", sta_cpointer);

            // Parse Reference
            CharDFA.State sta_reference = cdfa.NewState("reference", "REFERENCE");
            sta_start.NewDelta("start_to_reference", "[\\$]", sta_reference);

            // Parse FunPointer
            CharDFA.State sta_funpointer = cdfa.NewState("funpointer", "FUNPOINTER");
            sta_start.NewDelta("start_to_funpointer", "[\\$]", sta_funpointer);

            // Parse ArgSeparator 
            CharDFA.State sta_argseparator = cdfa.NewState("argseparator", "ARGSEPARATOR");
            sta_start.NewDelta("start_to_argseparator", "[\\,]", sta_argseparator);

            // Parse Add
            CharDFA.State sta_add = cdfa.NewState("add", "ADD");
            sta_start.NewDelta("start_to_add", "[\\+]", sta_add);

            // Parse Subtract, can turn into a negative number if followed by any digit
            CharDFA.State sta_subtract = cdfa.NewState("subtract", "SUBTRACT");
            CharDFA.State sta_subtract_end = cdfa.NewState("subtract_end", "SUBTRACT");
            sta_start.NewDelta("start_to_subtract", "[\\-]", sta_subtract);
            sta_subtract.NewDelta("subtract_to_end", "[^\\-0-9]", sta_subtract_end);

            // Parse Divide
            CharDFA.State sta_divide = cdfa.NewState("divide", "DIVIDE");
            sta_start.NewDelta("start_to_divide", "[\\/]", sta_divide);

            // Parse Multiply
            CharDFA.State sta_multiply = cdfa.NewState("multiply", "MULTIPLY");
            sta_start.NewDelta("start_to_multiply", "[\\*]", sta_multiply);

            // Parse Int, can start from a dash (SUBTRACT Token)
            //  Int can transition into Float
            CharDFA.State sta_int = cdfa.NewState("int");
            CharDFA.State sta_int_end = cdfa.NewState("end_int", "INT_B10");

            CharDFA.State sta_int_zero = cdfa.NewState("int_zero");
            CharDFA.State sta_int_bin = cdfa.NewState("int_bin");
            CharDFA.State sta_int_bin_end = cdfa.NewState("int_bin_end", "INT_B2");
            CharDFA.State sta_int_oct = cdfa.NewState("int_oct");
            CharDFA.State sta_int_oct_end = cdfa.NewState("int_oct_end", "INT_B8");
            CharDFA.State sta_int_hex = cdfa.NewState("int_hex");
            CharDFA.State sta_int_hex_end = cdfa.NewState("int_hex_end", "INT_B16");

            sta_start.NewDelta("start_to_int", "[1-9]", sta_int);
            sta_start.NewDelta("start_to_int_zero", "[0]", sta_int_zero);
            sta_subtract.NewDelta("subtract_to_int", "[0-9]", sta_int);

            sta_int.NewDelta("int_to_int", "[0-9]", sta_int);
            sta_int.NewDelta("int_to_end", "[^0-9\\.]", sta_int_end, false, false);

            sta_int_zero.NewDelta("int_zero_to_int_bin", "[b]", sta_int_bin);
            sta_int_zero.NewDelta("int_zero_to_int_oct", "[o]", sta_int_oct);
            sta_int_zero.NewDelta("int_zero_to_int_hex", "[x]", sta_int_hex);
            sta_int_zero.NewDelta("int_zero_to_int_end", "[^box]", sta_int_end, false, false);

            sta_int_bin.NewDelta("int_bin_to_int_bin", "[01]", sta_int_bin);
            sta_int_bin.NewDelta("int_bin_to_int_bin_end", "[^01]", sta_int_bin_end);

            sta_int_oct.NewDelta("int_oct_to_int_oct", "[0-7]", sta_int_oct);
            sta_int_oct.NewDelta("int_oct_to_int_oct_end", "[^0-7]", sta_int_oct_end);

            sta_int_hex.NewDelta("int_hex_to_int_hex", "[0-9A-F]", sta_int_hex);
            sta_int_hex.NewDelta("int_hex_to_int_hex_end", "[^0-9A-F]", sta_int_hex_end);


            // Parse Float
            // Floats are only started from an int, once a decimal point is found
            CharDFA.State sta_float = cdfa.NewState("float");
            CharDFA.State sta_float_end = cdfa.NewState("end_float", "FLOAT");
            sta_int.NewDelta("int_to_float", "[\\.]", sta_float);
            sta_float.NewDelta("float_to_float", "[0-9]", sta_float);
            sta_float.NewDelta("float_to_end", "[^0-9]", sta_float_end, false, false);


            // Parse String
            CharDFA.State sta_string_start = cdfa.NewState("string_start");
            CharDFA.State sta_string_chars = cdfa.NewState("string_chars");
            CharDFA.State sta_string_escape = cdfa.NewState("string_escape");
            CharDFA.State sta_string_end = cdfa.NewState("string_end", "STRING");
            sta_start.NewDelta("start_to_string", "[\\\"]", sta_string_start);
            sta_string_start.NewDelta("string_start_to_string", "[^\\\"\\\\]", sta_string_chars);
            sta_string_start.NewDelta("string_start_to_string_escape", "[\\\\]", sta_string_escape);
            sta_string_start.NewDelta("string_start_to_string_end", "[\\\"]", sta_string_end);

            sta_string_chars.NewDelta("string_to_string", "[^\\\"\\\\]", sta_string_chars);
            sta_string_chars.NewDelta("string_to_string_escape", "[\\\\]", sta_string_escape);
            sta_string_escape.NewDelta("string_escape_to_string", "[.]", sta_string_chars);

            sta_string_chars.NewDelta("string_to_string_end", "[\\\"]", sta_string_end);

            // Parse symbol
            CharDFA.State sta_symbol_start = cdfa.NewState("symbol_start");
            CharDFA.State sta_symbol_chars = cdfa.NewState("symbol_chars");
            CharDFA.State sta_symbol_end = cdfa.NewState("symbol_end", "SYMBOL");
            sta_start.NewDelta("start_to_symbol", "[a-zA-Z_]", sta_symbol_start);
            sta_symbol_start.NewDelta("symbol_start_to_symbol_chars", "[a-zA-Z_0-9]", sta_symbol_chars);
            sta_symbol_chars.NewDelta("symbol_chars_to_symbol_chars", "[a-zA-Z_0-9]", sta_symbol_chars);
            sta_symbol_chars.NewDelta("symbol_chars_to_symbol_end", "[^a-zA-Z_0-9]", sta_symbol_end, false, false);
            sta_symbol_start.NewDelta("symbol_chars_to_symbol_end", "[^a-zA-Z_0-9]", sta_symbol_end, false, false);

            // Parse Accessor
            CharDFA.State sta_accessor = cdfa.NewState("accessor", "ACCESSOR");
            sta_start.NewDelta("start_to_accessor", "[\\.]", sta_accessor);

            // Parse Left Brace
            CharDFA.State sta_lbrace = cdfa.NewState("lbrace", "LBRACE");
            sta_start.NewDelta("start_to_lbrace", "[\\{]", sta_lbrace);

            // Parse Right Brace
            CharDFA.State sta_rbrace = cdfa.NewState("rbrace", "RBRACE");
            sta_start.NewDelta("start_to_rbrace", "[\\}]", sta_rbrace);

            // Parse Left Bracket
            CharDFA.State sta_lbracket = cdfa.NewState("lbracket", "LBRACKET");
            sta_start.NewDelta("start_to_lbracket", "[\\[]", sta_lbracket);

            // Parse Right Bracket
            CharDFA.State sta_rbracket = cdfa.NewState("rbracket", "RBRACKET");
            sta_start.NewDelta("start_to_rbracket", "[\\]]", sta_rbracket);

            // Parse Left Paren
            CharDFA.State sta_lparen = cdfa.NewState("lparen", "LPAREN");
            sta_start.NewDelta("start_to_lparen", "[\\(]", sta_lparen);

            // Parse Right Paren
            CharDFA.State sta_rparen = cdfa.NewState("rparen", "RPAREN");
            sta_start.NewDelta("start_to_rparen", "[\\)]", sta_rparen);

            // Parse end expr
            CharDFA.State sta_semicolon = cdfa.NewState("semicolon", "SEMICOLON");
            sta_start.NewDelta("start_to_semicolon", "[;]", sta_semicolon);

            // Colon
            CharDFA.State sta_colon = cdfa.NewState("colon", "COLON");
            CharDFA.State sta_colon_end = cdfa.NewState("colon_end", "COLON");
            sta_start.NewDelta("start_to_colon", "[:]", sta_colon);

            CharDFA.State sta_namespace = cdfa.NewState("namespace", "NAMESPACE");
            sta_colon.NewDelta("colon_to_namespace", "[:]", sta_namespace);

            sta_colon.NewDelta("colon_to_colon_end", "[^:]", sta_colon_end, false, false);

            // Parse Assign & equality
            CharDFA.State sta_assign = cdfa.NewState("assign", "ASSIGN");
            CharDFA.State sta_assign_end = cdfa.NewState("assign_end", "ASSIGN");
            sta_start.NewDelta("start_to_assign", "[=]", sta_assign);
            sta_assign.NewDelta("start_to_assign", "[^=]", sta_assign_end, false, false);
            CharDFA.State sta_equals = cdfa.NewState("equals", "EQ");
            sta_assign.NewDelta("start_to_assign", "[=]", sta_equals);

            // Parse Equality
            CharDFA.State sta_lt = cdfa.NewState("lt", "LT");
            CharDFA.State sta_lt_end = cdfa.NewState("lt_end", "LT");
            sta_start.NewDelta("start_to_lt", "[<]", sta_lt);
            CharDFA.State sta_lteq = cdfa.NewState("lteq", "LTEQ");
            sta_lt.NewDelta("lt_to_lteq", "[=]", sta_lteq);
            sta_lt.NewDelta("start_to_lt", "[^=]", sta_lt_end, false, false);

            CharDFA.State sta_gt = cdfa.NewState("gt", "GT");
            CharDFA.State sta_gt_end = cdfa.NewState("gt_end", "GT");
            sta_start.NewDelta("start_to_gt", "[>]", sta_gt);
            CharDFA.State sta_gteq = cdfa.NewState("gteq", "GTEQ");
            sta_gt.NewDelta("gt_to_gteq", "[=]", sta_gteq);
            sta_gt.NewDelta("start_to_gt", "[^=]", sta_gt_end, false, false);
        }

        public List<CharDFA.Token> LexString(string filename, string source)
        {
            List<CharDFA.Token> tokens = cdfa.ProcessFile(filename, source);
            return tokens;
        }
    }
}

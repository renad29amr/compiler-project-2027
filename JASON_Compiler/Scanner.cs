using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    
    Keyword, Type, Init, Length,
    Identifier, Int, Float,
    String, Char,
    Operator, Undefined,
    Plus, Minus, Multiply, Divide, Modulus,
    Assign,
    Equal, NotEqual, LessThan, GreaterThan, LessEqual, GreaterEqual,
    Range,
    LParen, RParen, LBrace, RBrace, LBracket, RBracket, Comma, Colon, Dot
}
namespace JASON_Compiler
{
    

    public class Token
    {
       public string lex;
       public Token_Class token_type;
    }

    public class Scanner
    {

        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();
        Regex Keyword = new Regex(
    @"^(var|val|fun|class|if|else|while|for|in|return|true|false|null|new|print|input|break|continue)$");

         Regex Type = new Regex(@"^(Int|Float|String|Bool|Char|Array|Void)$");
        Regex init = new Regex(@"^(init|length)$");
        Regex Identifier = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
        Regex Int = new Regex(@"^[+-]?[0-9]+$");

         Regex Float = new Regex(@"^[+-]?[0-9]+\.[0-9]+$");

        Regex String = new Regex(@"^""[^""\n]*""$");

        Regex Char = new Regex(@"^'[^'\n]'$");

        public Scanner()
        {

            Operators.Add("+", Token_Class.Plus);
            Operators.Add("-", Token_Class.Minus);
            Operators.Add("*", Token_Class.Multiply);
            Operators.Add("/", Token_Class.Divide);
            Operators.Add("%", Token_Class.Modulus);
            Operators.Add("=", Token_Class.Assign);
            Operators.Add("==", Token_Class.Equal);
            Operators.Add("!=", Token_Class.NotEqual);
            Operators.Add("<", Token_Class.LessThan);
            Operators.Add(">", Token_Class.GreaterThan);
            Operators.Add("<=", Token_Class.LessEqual);
            Operators.Add(">=", Token_Class.GreaterEqual);
            Operators.Add("..", Token_Class.Range);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add("(", Token_Class.LParen);
            Operators.Add(")", Token_Class.RParen);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
            Operators.Add("[", Token_Class.LBracket);
            Operators.Add("]", Token_Class.RBracket);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add(":", Token_Class.Colon);



        }


        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                // Read words: keywords, types, identifiers, init, length
                if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    while (i + 1 < SourceCode.Length &&
                           (char.IsLetterOrDigit(SourceCode[i + 1]) ||
                            SourceCode[i + 1] == '_'))
                    {
                        i++;
                    }

                    CurrentLexeme = SourceCode.Substring(j, i - j + 1);
                    FindTokenClass(CurrentLexeme);
                }

                // Read integers and floating-point numbers
                else if (char.IsDigit(CurrentChar))
                {
                    while (i + 1 < SourceCode.Length &&
                           char.IsDigit(SourceCode[i + 1]))
                    {
                        i++;
                    }

                    if (i + 2 < SourceCode.Length &&
                        SourceCode[i + 1] == '.' &&
                        char.IsDigit(SourceCode[i + 2]))
                    {
                        i++;

                        while (i + 1 < SourceCode.Length &&
                               char.IsDigit(SourceCode[i + 1]))
                        {
                            i++;
                        }
                    }
                    while (i + 1 < SourceCode.Length &&
          (char.IsLetterOrDigit(SourceCode[i + 1]) || SourceCode[i + 1] == '_'))
                    {
                        i++;
                    }

                    CurrentLexeme = SourceCode.Substring(j, i - j + 1);
                    FindTokenClass(CurrentLexeme);
                }

                // Read strings and character 
                else if (CurrentChar == '"' || CurrentChar == '\'')
                {
                    char quote = CurrentChar;

                    while (i + 1 < SourceCode.Length &&
                           SourceCode[i + 1] != quote &&
                           SourceCode[i + 1] != '\n' &&
                           SourceCode[i + 1] != '\r')
                    {
                        i++;
                    }

                    if (i + 1 < SourceCode.Length &&
                        SourceCode[i + 1] == quote)
                    {
                        i++;
                    }

                    CurrentLexeme = SourceCode.Substring(j, i - j + 1);
                    FindTokenClass(CurrentLexeme);
                }
                else
                {
                    if (i + 1 < SourceCode.Length &&
                        Operators.ContainsKey(SourceCode.Substring(i, 2)))
                    {
                        FindTokenClass(SourceCode.Substring(i, 2));
                        i++;
                    }
                    else if (!Operators.ContainsKey(CurrentLexeme) &&
             i + 1 < SourceCode.Length &&
             (char.IsLetterOrDigit(SourceCode[i + 1]) || SourceCode[i + 1] == '_'))
                    {
                        i++;
                        while (i + 1 < SourceCode.Length &&
                               (char.IsLetterOrDigit(SourceCode[i + 1]) || SourceCode[i + 1] == '_'))
                        {
                            i++;
                        }
                        FindTokenClass(SourceCode.Substring(j, i - j + 1));
                    }
                    else if ("+-*/%!".IndexOf(CurrentChar) >= 0 &&
                             i + 1 < SourceCode.Length &&
                             (char.IsLetter(SourceCode[i + 1]) || SourceCode[i + 1] == '_') &&
                             ValueExpected())
                    {
                        i++;
                        while (i + 1 < SourceCode.Length &&
                               (char.IsLetterOrDigit(SourceCode[i + 1]) || SourceCode[i + 1] == '_'))
                        {
                            i++;
                        }
                        FindTokenClass(SourceCode.Substring(j, i - j + 1));
                    }
                    else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '/')
                {
                    while (i < SourceCode.Length && SourceCode[i] != '\n')
                        i++;
                }

                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    int end = SourceCode.IndexOf("*/", i + 2);
                    if (end == -1)
                    {
                        FindTokenClass("/*");
                        i = SourceCode.Length;
                    }
                    else
                        i = end + 1;
                }


                else
                {
                    if (i + 1 < SourceCode.Length)
                    {
                        string two = CurrentLexeme + SourceCode[i + 1];
                        if (Operators.ContainsKey(two))
                        {
                            FindTokenClass(two);
                            i++;
                            continue;
                        }
                    }
                    FindTokenClass(CurrentLexeme);
                }
                    
                }

            }

            JASON_Compiler.TokenStream = Tokens;
        }

        bool ValueExpected()
        {
            if (Tokens.Count == 0)
                return true;                                  

            Token prev = Tokens[Tokens.Count - 1];

            if (Operators.ContainsKey(prev.lex))
                return prev.lex != ")" && prev.lex != "]";     

            if (prev.token_type == Token_Class.Keyword)       
                return prev.lex != "true" && prev.lex != "false" && prev.lex != "null";

            return false;                                      
        }

        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (Keyword.IsMatch(Lex))
                Tok.token_type = Token_Class.Keyword;
            else if (Type.IsMatch(Lex))
                Tok.token_type = Token_Class.Type;
            else if (init.IsMatch(Lex))
                Tok.token_type = (Lex == "init") ? Token_Class.Init : Token_Class.Length;
            //Is it an identifier?
            else if (isIdentifier(Lex))
                Tok.token_type = Token_Class.Identifier;
            else if (Float.IsMatch(Lex))
                Tok.token_type = Token_Class.Float;
            //Is it a Constant?
            else if (isConstant(Lex))
                Tok.token_type = Token_Class.Int;
            else if (String.IsMatch(Lex))
                Tok.token_type = Token_Class.String;
            else if (Char.IsMatch(Lex))
                Tok.token_type = Token_Class.Char;
            //Is it an operator?
            else if (Operators.ContainsKey(Lex))
                Tok.token_type = Operators[Lex];

            //Is it an undefined?
            else
            {
                Tok.token_type = Token_Class.Undefined;
                Errors.Error_List.Add("Lexical error: unrecognized token " + Lex);
            }

            Tokens.Add(Tok);
        }



        // Check if the lex is an identifier or not.
        bool isIdentifier(string lex)
        {
            return Identifier.IsMatch(lex);
        }

        // Check if the lex is a constant (Number) or not.
        bool isConstant(string lex)
        {
            return Int.IsMatch(lex);
        }
    }
}

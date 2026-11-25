using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;

        public Node(string name)
        {
            Name = name;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;

        public Node root;

        public Node StartParsing(List<Token> tokenStream)
        {
            InputPointer = 0;
            TokenStream = tokenStream;

            Errors.Error_List.Clear();

            root = Program();

            if (InputPointer < TokenStream.Count)
            {
                Errors.Error_List.Add(
                    "Parsing Error: Unexpected token " +
                    TokenStream[InputPointer].lex +
                    " after Program\r\n"
                );
            }

            MessageBox.Show(
                Errors.Error_List.Count == 0
                    ? "Parsing Successful"
                    : "Parsing Failed"
            );

            return root;
        }



        bool Check(Token_Class expected)
        {
            return InputPointer < TokenStream.Count &&
                   TokenStream[InputPointer].token_type == expected;
        }

        bool CheckKeyword(string keyword)
        {
            return InputPointer < TokenStream.Count &&
                   TokenStream[InputPointer].token_type == Token_Class.Keyword &&
                   TokenStream[InputPointer].lex == keyword;
        }

        string CurrentLexeme()
        {
            return InputPointer < TokenStream.Count
                ? TokenStream[InputPointer].lex
                : "";
        }

        Node MatchKeyword(string keyword)
        {
            if (CheckKeyword(keyword))
            {
                Node node = new Node(keyword);
                InputPointer++;
                return node;
            }

            Errors.Error_List.Add(
                "Parsing Error: Expected keyword " + keyword + "\r\n"
            );

            return null;
        }

        public Node match(Token_Class expectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if (expectedToken == TokenStream[InputPointer].token_type)
                {
                    Node node = new Node(TokenStream[InputPointer].lex);
                    InputPointer++;
                    return node;
                }

                Errors.Error_List.Add(
                    "Parsing Error: Expected " +
                    expectedToken +
                    " but found " +
                    TokenStream[InputPointer].lex +
                    "\r\n"
                );

                return null;
            }

            Errors.Error_List.Add(
                "Parsing Error: Expected " +
                expectedToken +
                " but reached end of input\r\n"
            );

            return null;
        }

        void Add(Node parent, Node child)
        {
            if (child != null)
                parent.Children.Add(child);
        }


        // Program -> Declarations

        Node Program()
        {
            Node node = new Node("Program");
            Add(node, Declarations());
            return node;
        }

        // Declarations -> Declaration Declarations | ε

        Node Declarations()
        {
            // write your code here to check atleast the declare sturcure 
            // without adding procedures
            Node node = new Node("Declarations");

            while (InputPointer < TokenStream.Count)
            {
                if (CheckKeyword("var") ||
                    CheckKeyword("val") ||
                    CheckKeyword("class") ||
                    CheckKeyword("fun"))
                {
                    int oldPointer = InputPointer;

                    Add(node, Declaration());

                    if (InputPointer == oldPointer)
                        break;
                }
                else
                    break;
            }

            return node;
        }

        // Declaration -> VarDecl | ConstDecl | ClassDecl | FunctionDecl

        Node Declaration()
        {
            Node node = new Node("Declaration");

            if (CheckKeyword("var"))
                Add(node, VarDecl());

            else if (CheckKeyword("val"))
                Add(node, ConstDecl());

            else if (CheckKeyword("class"))
                Add(node, ClassDecl());

            else if (CheckKeyword("fun"))
                Add(node, FunctionDecl());

            else
            {
                Errors.Error_List.Add(
                    "Parsing Error: Invalid Declaration\r\n"
                );

                if (InputPointer < TokenStream.Count)
                    InputPointer++;
            }

            return node;
        }


        // VarDecl -> var VarDecls

        Node VarDecl()
        {
            Node node = new Node("VarDecl");

            Add(node, MatchKeyword("var"));
            Add(node, VarDecls());

            return node;
        }

        // VarDecls -> identifier OptionalType VarInit VarDeclsTail

        Node VarDecls()
        {
            Node node = new Node("VarDecls");

            Add(node, match(Token_Class.Identifier));
            Add(node, OptionalType());
            Add(node, VarInit());
            Add(node, VarDeclsTail());

            return node;
        }

        // OptionalType -> : Type | ε

        Node OptionalType()
        {
            Node node = new Node("OptionalType");

            if (Check(Token_Class.Colon))
            {
                Add(node, match(Token_Class.Colon));
                Add(node, Type());
            }

            return node;
        }

        // VarInit -> = VariableInitializer | ε

        Node VarInit()
        {
            Node node = new Node("VarInit");

            if (Check(Token_Class.Assign))
            {
                Add(node, match(Token_Class.Assign));
                Add(node, VariableInitializer());
            }

            return node;
        }

        // VarDeclsTail -> , identifier OptionalType VarInit VarDeclsTail | ε

        Node VarDeclsTail()
        {
            Node node = new Node("VarDeclsTail");

            while (Check(Token_Class.Comma))
            {
                Add(node, match(Token_Class.Comma));
                Add(node, match(Token_Class.Identifier));
                Add(node, OptionalType());
                Add(node, VarInit());
            }

            return node;
        }

        // ConstDecl -> val identifier : Type = VariableInitializer

        Node ConstDecl()
        {
            Node node = new Node("ConstDecl");

            Add(node, MatchKeyword("val"));
            Add(node, match(Token_Class.Identifier));
            Add(node, match(Token_Class.Colon));
            Add(node, Type());
            Add(node, match(Token_Class.Assign));
            Add(node, VariableInitializer());

            return node;
        }

        // Type -> BaseType | ArrayType

        Node Type()
        {
            Node node = new Node("Type");

            if (Check(Token_Class.Type) && CurrentLexeme() == "Array")
                Add(node, ArrayType());
            else
                Add(node, BaseType());

            return node;
        }

        // BaseType -> int | float | bool | string | char | Identifier

        Node BaseType()
        {
            Node node = new Node("BaseType");

            if (Check(Token_Class.Type) &&
                CurrentLexeme() != "Array" &&
                CurrentLexeme() != "Void")
            {
                Add(node, match(Token_Class.Type));
            }
            else if (Check(Token_Class.Identifier))
            {
                Add(node, match(Token_Class.Identifier));
            }
            else
            {
                Errors.Error_List.Add(
                    "Parsing Error: Expected BaseType\r\n"
                );
            }

            return node;
        }

        // ArrayType -> Array < Type >

        Node ArrayType()
        {
            Node node = new Node("ArrayType");

            Add(node, match(Token_Class.Type));
            Add(node, match(Token_Class.LessThan));
            Add(node, Type());
            Add(node, match(Token_Class.GreaterThan));

            return node;
        }

        // VariableInitializer -> Expression | [ InitList ]

        Node VariableInitializer()
        {
            Node node = new Node("VariableInitializer");

            if (Check(Token_Class.LBracket))
            {
                Add(node, match(Token_Class.LBracket));
                Add(node, InitList());
                Add(node, match(Token_Class.RBracket));
            }
            else
                Add(node, Expression());

            return node;
        }

        // InitList -> VariableInitializer InitListTail | ε

        Node InitList()
        {
            Node node = new Node("InitList");

            if (!Check(Token_Class.RBracket))
            {
                Add(node, VariableInitializer());
                Add(node, InitListTail());
            }

            return node;
        }

        // InitListTail -> , VariableInitializer InitListTail | ε

        Node InitListTail()
        {
            Node node = new Node("InitListTail");

            while (Check(Token_Class.Comma))
            {
                Add(node, match(Token_Class.Comma));
                Add(node, VariableInitializer());
            }

            return node;
        }

        // ClassDecl -> class identifier ClassBody

        Node ClassDecl()
        {
            Node node = new Node("ClassDecl");

            Add(node, MatchKeyword("class"));
            Add(node, match(Token_Class.Identifier));
            Add(node, ClassBody());

            return node;
        }

        // ClassBody -> { ClassMembers }

        Node ClassBody()
        {
            Node node = new Node("ClassBody");

            Add(node, match(Token_Class.LBrace));
            Add(node, ClassMembers());
            Add(node, match(Token_Class.RBrace));

            return node;
        }

        // ClassMembers -> ClassMember ClassMembers | ε

        Node ClassMembers()
        {
            Node node = new Node("ClassMembers");

            while (CheckKeyword("var") ||
                   CheckKeyword("val") ||
                   CheckKeyword("fun") ||
                   Check(Token_Class.Init))
            {
                int oldPointer = InputPointer;

                Add(node, ClassMember());

                if (InputPointer == oldPointer)
                    break;
            }

            return node;
        }

        // ClassMember -> VarDecl | ConstDecl | FunctionDecl | ConstructorDecl

        Node ClassMember()
        {
            Node node = new Node("ClassMember");

            if (CheckKeyword("var"))
                Add(node, VarDecl());

            else if (CheckKeyword("val"))
                Add(node, ConstDecl());

            else if (CheckKeyword("fun"))
                Add(node, FunctionDecl());

            else if (Check(Token_Class.Init))
                Add(node, ConstructorDecl());

            else
                Errors.Error_List.Add(
                    "Parsing Error: Invalid ClassMember\r\n"
                );

            return node;
        }


        // FunctionDecl -> FunctionHeader Block

        Node FunctionDecl()
        {
            Node node = new Node("FunctionDecl");

            Add(node, FunctionHeader());
            Add(node, Block());

            return node;
        }

        // FunctionHeader -> fun identifier ( Parameters ) : ReturnType

        Node FunctionHeader()
        {
            Node node = new Node("FunctionHeader");

            Add(node, MatchKeyword("fun"));
            Add(node, match(Token_Class.Identifier));
            Add(node, match(Token_Class.LParen));
            Add(node, Parameters());
            Add(node, match(Token_Class.RParen));
            Add(node, match(Token_Class.Colon));
            Add(node, ReturnType());

            return node;
        }

        // Parameters -> Parameter ParametersTail | ε

        Node Parameters()
        {
            Node node = new Node("Parameters");

            if (Check(Token_Class.Identifier))
            {
                Add(node, Parameter());
                Add(node, ParametersTail());
            }

            return node;
        }

        // ParametersTail -> , Parameter ParametersTail | ε

        Node ParametersTail()
        {
            Node node = new Node("ParametersTail");

            while (Check(Token_Class.Comma))
            {
                Add(node, match(Token_Class.Comma));
                Add(node, Parameter());
            }

            return node;
        }

        // Parameter -> Identifier : Type

        Node Parameter()
        {
            Node node = new Node("Parameter");

            Add(node, match(Token_Class.Identifier));
            Add(node, match(Token_Class.Colon));
            Add(node, Type());

            return node;
        }

        // ReturnType -> Type | Void

        Node ReturnType()
        {
            Node node = new Node("ReturnType");

            if (Check(Token_Class.Type) && CurrentLexeme() == "Void")
                Add(node, match(Token_Class.Type));
            else
                Add(node, Type());

            return node;
        }

        // ConstructorDecl -> ConstructorHeader Block

        Node ConstructorDecl()
        {
            Node node = new Node("ConstructorDecl");

            Add(node, ConstructorHeader());
            Add(node, Block());

            return node;
        }

        // ConstructorHeader -> init ( Parameters )

        Node ConstructorHeader()
        {
            Node node = new Node("ConstructorHeader");

            Add(node, match(Token_Class.Init));
            Add(node, match(Token_Class.LParen));
            Add(node, Parameters());
            Add(node, match(Token_Class.RParen));

            return node;
        }


        // Block -> { Statements }

        Node Block()
        {
            // write your code here to match statements
            Node node = new Node("Block");

            Add(node, match(Token_Class.LBrace));
            Add(node, Statements());
            Add(node, match(Token_Class.RBrace));

            return node;
        }

        // Statements -> Statement Statements | ε

        Node Statements()
        {
            Node node = new Node("Statements");

            while (InputPointer < TokenStream.Count &&
                   !Check(Token_Class.RBrace))
            {
                int oldPointer = InputPointer;

                Add(node, Statement());

                if (InputPointer == oldPointer)
                {
                    Errors.Error_List.Add(
                        "Parsing Error: Unable to parse statement\r\n"
                    );

                    InputPointer++;
                }
            }

            return node;
        }

        // Statement -> VarDecl | ConstDecl | Assignment | IfStatement | WhileStatement | ForStatement |ReturnStatement | BreakStatement | ContinueStatement | FunctionCall | PrintStatement | Block | IdentifierStatement

        Node Statement()
        {
            Node node = new Node("Statement");

            if (CheckKeyword("if"))
                Add(node, IfStatement());

            else if (CheckKeyword("var"))
                Add(node, VarDecl());

            else if (CheckKeyword("val"))
                Add(node, ConstDecl());

            else if (CheckKeyword("while"))
                Add(node, WhileStatement());

            else if (CheckKeyword("for"))
                Add(node, ForStatement());

            else if (CheckKeyword("return"))
                Add(node, ReturnStatement());

            else if (CheckKeyword("break"))
                Add(node, BreakStatement());

            else if (CheckKeyword("continue"))
                Add(node, ContinueStatement());

            else if (CheckKeyword("print"))
                Add(node, PrintStatement());

            else if (Check(Token_Class.LBrace))
                Add(node, Block());

            else if (Check(Token_Class.Identifier))
                Add(node, IdentifierStatement());

            else
            {
                Errors.Error_List.Add(
                    "Parsing Error: Invalid Statement\r\n"
                );

                if (InputPointer < TokenStream.Count)
                    InputPointer++;
            }

            return node;
        }


        Node IdentifierStatement()
        {
            Node node = new Node("IdentifierStatement");

            Add(node, match(Token_Class.Identifier));

            while (true)
            {
                if (Check(Token_Class.Dot))
                {
                    Add(node, match(Token_Class.Dot));
                    Add(node, match(Token_Class.Identifier));

                    if (Check(Token_Class.LParen))
                    {
                        Add(node, match(Token_Class.LParen));
                        Add(node, ArgumentList());
                        Add(node, match(Token_Class.RParen));
                    }
                }
                else if (Check(Token_Class.LBracket))
                {
                    Add(node, ArrayAccessTail());
                }
                else if (Check(Token_Class.LParen))
                {
                    Add(node, match(Token_Class.LParen));
                    Add(node, ArgumentList());
                    Add(node, match(Token_Class.RParen));
                }
                else
                    break;
            }

            if (Check(Token_Class.Assign))
            {
                Add(node, match(Token_Class.Assign));
                Add(node, Expression());
            }

            return node;
        }



        // IfStatement -> if Condition Block ElseClause

        Node IfStatement()
        {
            Node node = new Node("IfStatement");

            Add(node, MatchKeyword("if"));
            Add(node, Condition());
            Add(node, Block());
            Add(node, ElseClause());

            return node;
        }

        // ElseClause -> else Block | ε

        Node ElseClause()
        {
            Node node = new Node("ElseClause");

            if (CheckKeyword("else"))
            {
                Add(node, MatchKeyword("else"));
                Add(node, Block());
            }

            return node;
        }

        // WhileStatement -> while Condition Block

        Node WhileStatement()
        {
            Node node = new Node("WhileStatement");

            Add(node, MatchKeyword("while"));
            Add(node, Condition());
            Add(node, Block());

            return node;
        }

        // ForStatement -> for identifier in ForIterable Block

        Node ForStatement()
        {
            Node node = new Node("ForStatement");

            Add(node, MatchKeyword("for"));
            Add(node, match(Token_Class.Identifier));
            Add(node, MatchKeyword("in"));
            Add(node, ForIterable());
            Add(node, Block());

            return node;
        }

        // ForIterable -> RangeExpression | Expression

        Node ForIterable()
        {
            Node node = new Node("ForIterable");

            Add(node, RangeExpression());

            return node;
        }

        // RangeExpression -> Expression .. Expression

        Node RangeExpression()
        {
            Node node = new Node("RangeExpression");

            Add(node, Expression());

            if (Check(Token_Class.Range))
            {
                Add(node, match(Token_Class.Range));
                Add(node, Expression());
            }

            return node;
        }


        // ReturnStatement -> return ReturnValue

        Node ReturnStatement()
        {
            Node node = new Node("ReturnStatement");

            Add(node, MatchKeyword("return"));
            Add(node, ReturnValue());

            return node;
        }

        // ReturnValue -> Expression | ε

        Node ReturnValue()
        {
            Node node = new Node("ReturnValue");

            if (InputPointer < TokenStream.Count &&
                !Check(Token_Class.RBrace))
            {
                Add(node, Expression());
            }

            return node;
        }

        Node BreakStatement()
        {
            Node node = new Node("BreakStatement");
            Add(node, MatchKeyword("break"));
            return node;
        }

        Node ContinueStatement()
        {
            Node node = new Node("ContinueStatement");
            Add(node, MatchKeyword("continue"));
            return node;
        }

        // Expression -> Term ExpressionTail

        Node Expression()
        {
            Node node = new Node("Expression");

            Add(node, Term());
            Add(node, ExpressionTail());

            return node;
        }

        // ExpressionTail -> AddOp Term ExpressionTail | ε

        Node ExpressionTail()
        {
            Node node = new Node("ExpressionTail");

            while (Check(Token_Class.Plus) ||
                   Check(Token_Class.Minus))
            {
                Add(node, AddOp());
                Add(node, Term());
            }

            return node;
        }

        // Term -> Factor TermTail

        Node Term()
        {
            Node node = new Node("Term");

            Add(node, Factor());
            Add(node, TermTail());

            return node;
        }

        // TermTail -> MultOp Factor TermTail | ε

        Node TermTail()
        {
            Node node = new Node("TermTail");

            while (Check(Token_Class.Multiply) ||
                   Check(Token_Class.Divide) ||
                   Check(Token_Class.Modulus))
            {
                Add(node, MultOp());
                Add(node, Factor());
            }

            return node;
        }

        // Factor -> UnaryOp Factor | PrimaryExpression

        Node Factor()
        {
            Node node = new Node("Factor");

            if (Check(Token_Class.Plus) ||
                Check(Token_Class.Minus))
            {
                Add(node, UnaryOp());
                Add(node, Factor());
            }
            else
                Add(node, PrimaryExpression());

            return node;
        }



        // Condition -> Expression ConditionTail

        Node Condition()
        {
            Node node = new Node("Condition");

            Add(node, Expression());
            Add(node, ConditionTail());

            return node;
        }

        // ConditionTail -> RelOp Expression | ε

        Node ConditionTail()
        {
            Node node = new Node("ConditionTail");

            if (Check(Token_Class.LessThan) ||
                Check(Token_Class.GreaterThan) ||
                Check(Token_Class.LessEqual) ||
                Check(Token_Class.GreaterEqual) ||
                Check(Token_Class.Equal) ||
                Check(Token_Class.NotEqual))
            {
                Add(node, RelOp());
                Add(node, Expression());
            }

            return node;
        }

        Node RelOp()
        {
            Node node = new Node("RelOp");

            if (Check(Token_Class.LessThan))
                Add(node, match(Token_Class.LessThan));

            else if (Check(Token_Class.GreaterThan))
                Add(node, match(Token_Class.GreaterThan));

            else if (Check(Token_Class.LessEqual))
                Add(node, match(Token_Class.LessEqual));

            else if (Check(Token_Class.GreaterEqual))
                Add(node, match(Token_Class.GreaterEqual));

            else if (Check(Token_Class.Equal))
                Add(node, match(Token_Class.Equal));

            else if (Check(Token_Class.NotEqual))
                Add(node, match(Token_Class.NotEqual));

            return node;
        }


        Node AddOp()
        {
            Node node = new Node("AddOp");

            if (Check(Token_Class.Plus))
                Add(node, match(Token_Class.Plus));

            else if (Check(Token_Class.Minus))
                Add(node, match(Token_Class.Minus));

            return node;
        }

        Node MultOp()
        {
            Node node = new Node("MultOp");

            if (Check(Token_Class.Multiply))
                Add(node, match(Token_Class.Multiply));

            else if (Check(Token_Class.Divide))
                Add(node, match(Token_Class.Divide));

            else if (Check(Token_Class.Modulus))
                Add(node, match(Token_Class.Modulus));

            return node;
        }

        Node UnaryOp()
        {
            Node node = new Node("UnaryOp");

            if (Check(Token_Class.Minus))
                Add(node, match(Token_Class.Minus));

            else if (Check(Token_Class.Plus))
                Add(node, match(Token_Class.Plus));

            return node;
        }


        // FunctionCall -> identifier ( ArgumentList )

        Node FunctionCall()
        {
            Node node = new Node("FunctionCall");

            Add(node, match(Token_Class.Identifier));
            Add(node, match(Token_Class.LParen));
            Add(node, ArgumentList());
            Add(node, match(Token_Class.RParen));

            return node;
        }

        // ArgumentList -> Arguments | ε

        Node ArgumentList()
        {
            Node node = new Node("ArgumentList");

            if (!Check(Token_Class.RParen))
                Add(node, Arguments());

            return node;
        }

        // Arguments -> Argument ArgumentTail

        Node Arguments()
        {
            Node node = new Node("Arguments");

            Add(node, Argument());
            Add(node, ArgumentTail());

            return node;
        }

        // ArgumentTail -> , Argument ArgumentTail | ε

        Node ArgumentTail()
        {
            Node node = new Node("ArgumentTail");

            while (Check(Token_Class.Comma))
            {
                Add(node, match(Token_Class.Comma));
                Add(node, Argument());
            }

            return node;
        }

        // Argument -> Expression

        Node Argument()
        {
            Node node = new Node("Argument");

            Add(node, Expression());

            return node;
        }


        // ObjectCreation -> new identifier ( ArgumentList )

        Node ObjectCreation()
        {
            Node node = new Node("ObjectCreation");

            Add(node, MatchKeyword("new"));
            Add(node, match(Token_Class.Identifier));
            Add(node, match(Token_Class.LParen));
            Add(node, ArgumentList());
            Add(node, match(Token_Class.RParen));

            return node;
        }


        Node Literal()
        {
            Node node = new Node("Literal");

            if (Check(Token_Class.Int))
                Add(node, match(Token_Class.Int));

            else if (Check(Token_Class.Float))
                Add(node, match(Token_Class.Float));

            else if (Check(Token_Class.String))
                Add(node, match(Token_Class.String));

            else if (Check(Token_Class.Char))
                Add(node, match(Token_Class.Char));

            else if (CheckKeyword("true"))
                Add(node, MatchKeyword("true"));

            else if (CheckKeyword("false"))
                Add(node, MatchKeyword("false"));

            else if (CheckKeyword("null"))
                Add(node, MatchKeyword("null"));

            else
                Errors.Error_List.Add(
                    "Parsing Error: Expected Literal\r\n"
                );

            return node;
        }



        // ArrayAccess -> identifier [ Expression ]

        Node ArrayAccess()
        {
            Node node = new Node("ArrayAccess");

            Add(node, match(Token_Class.Identifier));
            Add(node, ArrayAccessTail());

            return node;
        }

        Node ArrayAccessTail()
        {
            Node node = new Node("ArrayAccessTail");

            Add(node, match(Token_Class.LBracket));
            Add(node, Expression());
            Add(node, match(Token_Class.RBracket));

            return node;
        }


        // ObjectAccess -> identifier . identifier ObjectAccessTail

        Node ObjectAccess()
        {
            Node node = new Node("ObjectAccess");

            Add(node, match(Token_Class.Identifier));
            Add(node, ObjectAccessTailFull());

            return node;
        }

        Node ObjectAccessTailFull()
        {
            Node node = new Node("ObjectAccessTail");

            Add(node, match(Token_Class.Dot));
            Add(node, match(Token_Class.Identifier));

            while (true)
            {
                if (Check(Token_Class.LParen))
                {
                    Add(node, match(Token_Class.LParen));
                    Add(node, ArgumentList());
                    Add(node, match(Token_Class.RParen));
                }
                else if (Check(Token_Class.LBracket))
                    Add(node, ArrayAccessTail());
                else
                    break;
            }

            return node;
        }



        Node PrintStatement()
        {
            Node node = new Node("PrintStatement");

            Add(node, MatchKeyword("print"));
            Add(node, match(Token_Class.LParen));
            Add(node, Expression());
            Add(node, match(Token_Class.RParen));

            return node;
        }

        Node InputExpression()
        {
            Node node = new Node("InputExpression");

            Add(node, MatchKeyword("input"));
            Add(node, match(Token_Class.LParen));
            Add(node, match(Token_Class.RParen));

            return node;
        }


        Node ParenthesizedExpression()
        {
            Node node = new Node("ParenthesizedExpression");

            Add(node, match(Token_Class.LParen));
            Add(node, Expression());
            Add(node, match(Token_Class.RParen));

            return node;
        }

        Node PrimaryExpression()
        {
            Node node = new Node("PrimaryExpression");

            if (Check(Token_Class.Int) ||
                Check(Token_Class.Float) ||
                Check(Token_Class.String) ||
                Check(Token_Class.Char) ||
                CheckKeyword("true") ||
                CheckKeyword("false") ||
                CheckKeyword("null"))
            {
                Add(node, Literal());
            }
            else if (CheckKeyword("new"))
            {
                Add(node, ObjectCreation());
            }
            else if (CheckKeyword("input"))
            {
                Add(node, InputExpression());
            }
            else if (Check(Token_Class.LParen))
            {
                Add(node, ParenthesizedExpression());
            }
            else if (Check(Token_Class.Identifier))
            {
                Add(node, IdentifierExpression());
            }
            else
            {
                Errors.Error_List.Add(
                    "Parsing Error: Invalid PrimaryExpression\r\n"
                );

                if (InputPointer < TokenStream.Count)
                    InputPointer++;
            }

            return node;
        }


        Node IdentifierExpression()
        {
            Node node = new Node("IdentifierExpression");

            Add(node, match(Token_Class.Identifier));

            while (true)
            {
                if (Check(Token_Class.Dot))
                {
                    Add(node, match(Token_Class.Dot));
                    Add(node, match(Token_Class.Identifier));

                    if (Check(Token_Class.LParen))
                    {
                        Add(node, match(Token_Class.LParen));
                        Add(node, ArgumentList());
                        Add(node, match(Token_Class.RParen));
                    }
                }
                else if (Check(Token_Class.LBracket))
                {
                    Add(node, ArrayAccessTail());
                }
                else if (Check(Token_Class.LParen))
                {
                    Add(node, match(Token_Class.LParen));
                    Add(node, ArgumentList());
                    Add(node, match(Token_Class.RParen));
                }
                else
                    break;
            }

            return node;
        }


        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");

            TreeNode treeRoot = PrintTree(root);

            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);

            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
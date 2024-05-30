// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            var grammar = @"
genericLexer WhileLexer;

[String] STRING;
[Int] INT;
[AlphaId] ID; 


[ KeyWord] IF:""if"";
[ KeyWord] THEN:""then"";
[ KeyWord] ELSE:""else"";
[ KeyWord] WHILE:""while"";
[ KeyWord] DO:""do"";
[ KeyWord] SKIP:""skip"";
[ KeyWord] TRUE:""true"";
[ KeyWord] FALSE:""false"";
[ KeyWord] NOT:""not"";
[ KeyWord] AND:""and"";
[ KeyWord] OR:""or"";
[ KeyWord] PRINT:""print"";

 [Sugar] GREATER : "">"";

[Sugar] LESSER : ""<"";

[Sugar] EQUALS : ""=="";

[Sugar] DIFFERENT : ""!="";

[Sugar] CONCAT : ""."";

[Sugar] ASSIGN : "":="";

[Sugar] PLUS : ""+"";
[Sugar] MINUS : ""-"";
[Sugar] TIMES : ""*"";
[Sugar] DIVIDE : ""/"";

[Sugar] LPAREN : ""("";
[Sugar] RPAREN : "")"";
[Sugar] SEMICOLON : "";"";

parser WhileParser;


[Right 50] LESSER;
[Right 50] GREATER;
[Right 50] EQUALS;
[Right 50]DIFFERENT;

[Right 10] CONCAT;
       
[Right 10] PLUS;
[Left 10] MINUS;
[Right 50] TIMES;
[Left 50]DIVIDE;

[Prefix 100] MINUS;

[Right 10] OR;
[Right 50] AND;
[Prefix 100] NOT;

@name(program);
-> statement :  LPAREN statement RPAREN ;

@name(sequence);
statement : sequence;


[Operand] operand : [INT | TRUE | FALSE | STRING | ID];
[Operand] operand : LPAREN WhileParser_expressions RPAREN;

@name(sequenceStatement);
sequence : statementPrim additionalStatements*;

@name(additionalStatement);
additionalStatements : SEMICOLON statementPrim;

@name(conditionalStatement);
statementPrim: IF WhileParser_expressions THEN statement ELSE statement;

@name(whileStatement);
statementPrim: WHILE WhileParser_expressions DO statement;

@name(assignmentStatement);
statementPrim: ID ASSIGN WhileParser_expressions;

@name(skipStatement);
statementPrim: SKIP;

@name(printStatement);
statementPrim: PRINT WhileParser_expressions;

";

            string source = @"
1 / 2 / 3 + 4
";

            var r = CslyProcessor.GenerateParser(grammar,"ns","int");

            if (r.IsOK)
            {
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.ParserName}.cs", r.Result.Parser);
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.LexerName}.cs", r.Result.Lexer);
                Console.WriteLine(r.Result.Parser);
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
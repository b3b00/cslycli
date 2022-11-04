# csly-cli

[![Coverage Status](https://coveralls.io/repos/github/b3b00/cslycli/badge.svg?branch=main)](https://coveralls.io/github/b3b00/cslycli?branch=main)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/cslycli/blob/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/csly-cli.svg?kill_cache=1)](https://www.nuget.org/packages/csly-cli)

## Presentation

CSLY CLI is a CLI companion tool for [CSLY](https://github.com/b3b00/csly). It helps quickly test a CSLY lexer/parser without the need to code it.
It describes the lexer/parser as a text file (ala yacc) with dedicated notations for CSLY specificities.

C# sources can also be generated to bootstrap real development using the prototyped parser. 

## installation

```
dotnet too install csly-cli
```

## usage

There is 2 usages of csly-cli :
   - 


### testing the parser

the test command tries to parse a source file according to a grammar specification file and produces :
  - error messages if 
    - grammar specification is erroneous.
    - source file does not comply to grammar.
  - the syntax tree produced by the parser in 2 possible format
    - json (maybe for direct use)
    - graphviz dot for visualization (i personnaly use [Graphviz online](https://dreampuf.github.io/GraphvizOnline/)

```csly-cli test``` :
  - -g --grammar * : path to grammar specification file
  - -s --source * : path to source file to parse
  - -o --output : output folder for syntax tree files
  - -f --format : [JSON|DOT] syntax tree file formats (many format may be specified separated by space)


  if no ouput is provided then CLSY-CLI display if parse succeeded and errors in case of failure.
  
  **examples**
  ```
  csly-cli test -g myGrammar.txt  --source mySource.txt -o ./outputDir -f JSON DOT 
  ```
  try to parse mySource.txt according to grammar myGrammar.txt and output the syntax tree to outputDir/mySource.json (JSon) and outputDir/mySource.dot (Graphviz dot)

  ```
  csly-cli test -g myGrammar.txt  --source mySource.txt  
  ```
  try to parse mySource.txt according to grammar myGrammar.txt and output success or errors.

  ### generating C# sources

  ```csly-cli generate``` : 

   - -g --grammar * : path to grammar specification file
   - -n --namespace * : parser namespace   
   - -o --output * : parser output type (see [parser typing](https://github.com/b3b00/csly/wiki/defining-your-parser#parser-types) for CSLY parser typing)

## parser specification file format

### Grammar main structure 


a grammar specification consists of two parts : 
  - the lexer specification starting with ```genericLexer <NAME_OF_THE_LEXER>;```
  - the parser grammar specification starting with ```pasrer <NAME_OF_THE_PARSER>```

comments are :
   - sinle line : starting with # (ala shell script)
   - multi line : starting with /* and ending with */ (ala C) 

### lexer 

CSLY-CLI only implements the CSLY [generic lexer](https://github.com/b3b00/csly/wiki/GenericLexer). Each token is defined as a pair of token predefined type and token identifier. Some token may need additional parameters.
Each token starts with a token type and ends with a `;` :

 - identifiers
    - ```[AlphaId] <ID_TOKEN_NAME>;``` : Only alpha characters 
    - ```[AlphaNumId] <ID_TOKEN_NAME>;``` : Starting with an alpha char and then alpha or numeric char. 
    - ```[AlphaNumDashId] <ID_TOKEN_NAME>;``` : Starting with an alpha or ` _` (underscore) char and then alphanumeric or `-`(minus) or `_` (underscore) char.
  - Integer : ```[Int] <INT_TOKEN_NAME>;```
  - Double / Decimal : ```[Double] <DOUBLE_TOKEN_NAME>;```
  - Keywords : ```[KeyWord] <KEYWORD_TOKEN_NAME> : '<KEYWORD_VALUE>';```
  - syntaxic sugar : ```[SUGAR] <SUGAR_TOKEN_NAME> : '<SUGAR_VALUE>';```
  - String : ```[String] <STRING_TOKEN_NAME> : '<string_delim_char>' '<string_escape_char>;```
  - Single line comments : ```[SingleLineComment] LINECOMMENT : '#'```;
  - multi line comments : ```[MultiLineComment] BLOCKCOMMENT : '/*' '*/'```;
  
  **simple lexer examples**

  ```
  genericLexer sample;

# only use alpha chars for identifier
  [AlphaId] ID;
# integer token
  [Int] INT;
# keywords for if ... then ... else
  [KeyWord] IF : 'if';
  [KeyWord] THEN : 'then';
  [KeyWord] ELSE : 'else';
# sugar for opening and closing braces
  [Sugar] OPEN_BRACE : '{';
  [Sugar] CLOSE_BRACE : '}';
# string with " as delimiter and \ as escaper
  [String] STRING : '"' '\';
```

### parser


#### ***basic rules***

Grammar rules follow the classic EBNF syntax:

```terminal_name : clauses ; #ended by a semicolone ';'```

each clause is separated by spaces.

clauses can be (all examples will use the simple lexer defined above)

 - simple terminal or non terminal references : the name of the terminal or non terminal (case sensitive) ```nonTerm : OPEN_BRACE otherNonTerm CLOSE_BRACE```
 - a group of clauses surrounded by parantheses : ```group : ( first ID third );```
 - an alternate of terminal or non terminal surounded by square brackets : ```alt : [IF|THEN|ELSE];```
 - a repetition of clauses (simple, groups or alternate)
   - one or more with '+' : ``` oneOrMore : (ID STRING)+;```
   - zero or more with '*' : ``` zeroOrMore : ID*;```
 - a optional clause with '?' : ```option : [IF|THEN|ELSE]?``` 
 - an explicit token as a string surrounded by ' : ```comma : ',';```

For full documentation refer to [CSLY EBNF parser](https://github.com/b3b00/csly/wiki/EBNF-parser)

#### ***expression parsing***

CSLY offers extension to ease the parse of expression (see [expression parsing](https://github.com/b3b00/csly/wiki/expression-parsing)).

the generated expression "sub-parser" root rule is named <PARSER_NAME>_expressions. <PARSER_NAME> is the parser name defined at the begining of the parser definitions (see https://github.com/b3b00/cslycli#grammar-main-structure)

*** operations ***

Infix operations are specified by either :
   - [Right] <PRECEDENCE(integer)> <TOKEN_NAME> for right associative operation
   - [Left] <PRECEDENCE(integer)> <TOKEN_NAME> for left associative operation

where <PRECEDENCE> is the priority level of the operation and <TOKEN_NAME> is the name of the sugar token for the operator. an explicit token may be used instead of the token name:
```
# left associative addition using token PLUS
[Left] 10 ADD;

# right associative exponentiation using explicit token
[Right] 100 '^';
```

Prefix operations are defined quite the same way :
 - [Prefix] <PRECEDENCE> <TOKEN_NAME>

```
[Prefix] 150 '-';
```

*** operands ***

Operands are rules tagged with the special ```[Operand]``` attribute at the begining of the rule :

```
[Operand] intOperand : INT;
[Operand] stringOperand : STRING;
[Operand] groupOperand : '(' nonTerminal ')';

```

*** simple arithmetic parser ***

```
genericLexer arithLexer;

parser arithParser;


```

#### ***root rule***

the root rule of the grammar is defined by '->' at the begining of the rule :
```-> root : other clauses;```


### specification formal grammar using csly-cli specification file (going meta :) )

```

genericLexer GrammarLexer;

[KeyWord] LEXER : 'genericLexer' ;
[KeyWord] PARSER : 'parser' ;
[String] STRING : '''' ''''; 
[Int] INT;
[KeyWord] DOUBLE : 'Double';
[KeyWord] ALPHAID : 'AlphaId';
[KeyWord] ALPHANUMID : 'AlphaNumId';
[KeyWord] ALPHANUMDASHID : 'AlphaNumDashId';  
[KeyWord] KEYWORD : 'KeyWord';
[KeyWord] SUGAR : 'Sugar';
[KeyWord] RIGHT : 'Right';
[KeyWord] LEFT : 'Left';
[KeyWord] PREFIX : 'Prefix';
[KeyWord] OPERAND : 'Operand';
[KeyWord] STRINGTOKEN : 'String';
[KeyWord] INTTOKEN : 'Int';
[KeyWord] SINGLELINECOMMENT : 'SingleLineComment';
[KeyWord] MULTILINECOMMENT : 'MultiLineComment';
[AlphaNumDashId] ID;
[SingleLineComment] LINECOMMENT : '#';
[MultiLineComment] BLOCKCOMMENT : '/*' '*/';

[Sugar] OR : '|';
[Sugar] START : '->';

parser GrammarParser;

-> root: genericRoot parserRoot ;



# Lexer


genericRoot : LEXER ID ';'  token*;

token :'[' [KEYWORD|SUGAR|SINGLELINECOMMENT] ']' ID ':' STRING ';';

token : '[' [STRINGTOKEN|INTTOKEN|ALPHAID|ALPHANUMID|ALPHANUMDASHID|DOUBLE] ']' ID ';';

token : '[' [STRINGTOKEN|MULTILINECOMMENT] ']' ID ':' STRING STRING ';';



# parser

parserRoot : PARSER ID ';' rule*;

rule  : START? ('[' OPERAND ']')? ID ':' clause+ ';';

# expressions

rule : '[' PREFIX INT ']' ID ';';

rule : '[' [RIGHT|LEFT] INT ']' ID ';';



# clauses

item : [ ID | STRING ];

clause : item '*';

clause : item '+';

clause : item '?';

clause : item ;

clause : choiceclause;

clause : group;


# choices

choiceclause : '['  item ( OR item)* ']';

clause : choiceclause '+';

clause : choiceclause '*';

clause : choiceclause '?';

# groups

group : '('  item* ')';

clause : group '+';

clause : group '*';

clause : group '?';

```




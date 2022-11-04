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
   - -o --output * : parser output type 




## parser specification file format

### specification formal grammar using csly-cli specification file (going meta :) )

```
genericLexer; 
  
 [String] STRING; 
 [Int] INT; 
 [AlphaId] ID; # to be derived for every identifier types 
  
  
 [KeyWord] IF:'if'; 
 [KeyWord] THEN:'then'; 
 [KeyWord] ELSE:'else'; 
 [KeyWord] WHILE:'while'; 
 [KeyWord] DO:'do'; 
 [KeyWord] SKIP:'skip'; 
 [KeyWord] TRUE:'true'; 
 [KeyWord] FALSE:'false'; 
 [KeyWord] NOT:'not'; 
 [KeyWord] AND:'and'; 
 [KeyWord] OR:'or'; 
 [KeyWord] PRINT:'print'; 
  
 [Sugar] GREATER : '>'; 
  
 [Sugar] LESSER : '<'; 
  
 [Sugar] EQUALS : '=='; 
  
 [Sugar] DIFFERENT : '!='; 
  
 [Sugar] CONCAT : '.'; 
  
 [Sugar] ASSIGN : ':='; 
  
 [Sugar] PLUS : '+'; 
 [Sugar] MINUS : '-'; 
 [Sugar] TIMES : '*'; 
 [Sugar] DIVIDE : '/'; 
  
 [Sugar] LPAREN : '('; 
 [Sugar] RPAREN : ')'; 
 [Sugar] SEMICOLON : ';'; 
  
 parser; 
  
 # operations  
  
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
  
 # operands 
  
 [Operand] INT; 
 [Operand] TRUE; 
 [Operand] FALSE; 
 [Operand] STRING; 
 [Operand] ID; 
  
 # statements 
  
 @ statement :  LPAREN statement RPAREN ; 
  
 statement : sequence; 
  
 sequence : statementPrim additionalStatements*; 
  
 additionalStatements : SEMICOLON statementPrim; 
  
 statementPrim: IF dynamicParser_expressions THEN statement ELSE statement; 
  
 statementPrim: WHILE dynamicParser_expressions DO statement; 
  
 statementPrim: ID ASSIGN dynamicParser_expressions; 
  
 statementPrim: SKIP; 
  
 statementPrim: PRINT dynamicParser_expressions;
```




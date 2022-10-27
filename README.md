# csly-cli

[![Coverage Status](https://coveralls.io/repos/github/b3b00/cslycli/badge.svg?branch=main)](https://coveralls.io/github/b3b00/cslycli?branch=main)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/main/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/csly-cli.svg?kill_cache=1)](https://www.nuget.org/packages/csly-cli)

CSLY CLI is a tool to quickly test a CSLY lexer/parser without the need to code it.
It describes the lexer/parser as a text file (ala yacc) with dedicated notations for CSLY specificities.

C# sources can also be generated to bootstrap real development using the prototyped parser. 

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

# CSLY CLI arguments

## testing the parser

parse :
  - -g --grammar * : chemin vers le fichier grammaire
  - -s --source * : chemin vers le fichier source
  - -o --output : chemin vers le fichier sortie
  - -f --format : [json|dot] format du fichier de sortie

  if no ouput is provided then CLSY-CLI display if parse succeeded and errors in case of failure.

  ## generating C# sources

  generate : 

   - -g --grammar * : chemin vers le fichier grammaire
   - -n --namespace * : parser namespace   
   - -t --type * : parser output type


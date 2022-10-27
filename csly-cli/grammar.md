```
root: genericRoot parserRoot 


#
# Lexer
#

genericRoot : 'genericLexer' ID ';'  token*

token :'[' ['KeyWord'|'Sugar'] ']' ID COLON STRING ';'

token : '[' ['String'|'Int'|'AlphaId'|'Double'] ']' ID ';'



#
# parser
#


parserRoot : 'parser' ID ';' rule*


rule  : '@'? '[Operand]'? ID ':' clause+ ';'


# expressions

rule : '[' 'Operand' ']' ID ';'

rule : '[' 'Prefix' INT ']' ID ';'

rule : '[' ['Right'|'Left'] INT ']' ID ';'



# clauses

item : [ ID | STRING ]

clause : item '*'

clause : item '+'

clause : item '?'

clause : item 

clause : choiceclause

clause : group


# choices

choiceclause : '['  item ( '|' item)* ']'

clause : choiceclause '+'

clause : choiceclause '*'

clause : choiceclause '?'

# groups

group : '('  item* ')'

clause : group '+'

clause : group '*'

clause : group '?'


``` 
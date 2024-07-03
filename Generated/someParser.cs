using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace hexa
{
    [ParserRoot("root")]
    public class HexaParser
    {
        [Production("root : hexa + int")]
        [NodeName("racine")]
        public Object root_hexa_int(List<Object> p0, Object p1)
        {
            return default(Object);
        }

        [Production("hexa : HEXA")]
        [NodeName("hexadecimal")]
        public Object hexa_HEXA(Token<HexaLexer> p0)
        {
            return default(Object);
        }

        [Production("int : INT")]
        [NodeName("entier")]
        public Object int_INT(Token<HexaLexer> p0)
        {
            return default(Object);
        }
    }
}
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace hexa
{
    [ParserRoot("root")]
    public class HexaParser
    {
        [Production("root : HEXA + INT")]
        public Object root_HEXA_INT(List<Token<HexaLexer>> p0, Token<HexaLexer> p1)
        {
            return default(Object);
        }
    }
}
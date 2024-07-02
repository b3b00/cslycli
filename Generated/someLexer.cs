using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace hexa
{
    public enum HexaLexer
    {
        [Int]
        INT,
        [Hexa("6x")]
        HEXA,
        [AlphaId]
        ID,
    }
}
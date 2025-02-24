using System.Diagnostics;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.parser
{
    [DebuggerDisplay("Terminal {(ExpectedToken.ToString())}{Discarded ? \"[d]\" : \"\"}")]
    public class TerminalClause : IClause
    {

        public bool IsImplicit { get; set; }
        
        public string TokenName  { get; set; }
        
        public string ImplicitToken { get; set; }
        
        public bool IsDiscarded { get; set; }
        
        public TerminalClause(bool isImplicit, string value)
        {
            IsImplicit = isImplicit;
            if (IsImplicit)
            {
                ImplicitToken = value;
            }
            else
            {
                TokenName = value;
            }
        }

        

        public override string ToString()
        {
            return IsImplicit ? $"'{ImplicitToken}'" : TokenName;
        }

        public LexerPosition Position { get; set; }
    }

}
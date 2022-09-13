using System.Diagnostics;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.parser
{
    [DebuggerDisplay("Terminal {(ExpectedToken.ToString())}{Discarded ? \"[d]\" : \"\"}")]
    public class TerminalClause : IClause
    {
        public TerminalClause(string name)
        {
            Name = name;
        }

        public string Name  { get; set; }

        
    }

}
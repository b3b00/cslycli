using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace csly.cli.model.parser
{
    public class ChoiceClause : IClause
    {
        public  List<IClause> Choices { get; set; }
        public ChoiceClause(IClause clause)
        {
            Choices = new List<IClause> {clause};
        }
        
        public ChoiceClause(List<IClause> choices)
        {
            Choices = choices;
        }
        
        public ChoiceClause(IClause choice, List<IClause> choices) : this(choice)
        {
            Choices.AddRange(choices);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[ ");
            builder.Append(string.Join(" | ", Choices.Select(x => x.ToString())));
            builder.Append(" ]");
            return builder.ToString();
        }

        public bool IsTerminalChoice => Choices.Any(x => x is TerminalClause);
        public bool IsNonTerminalChoice => Choices.Any(x => x is NonTerminalClause);
        


    }
}
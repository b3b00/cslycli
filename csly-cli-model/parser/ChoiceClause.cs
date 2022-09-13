using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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


    }
}
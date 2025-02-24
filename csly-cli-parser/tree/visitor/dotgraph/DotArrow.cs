using System.Text;
using clsy.cli.model.tree.visitor.dotgraph;

namespace cli.sly.model.tree.visitor.dotgraph
{
    public class DotArrow : IDot
    {
        public DotNode Source { get; private set; }
        public DotNode Destination { get; private set; }
        
        public DotArrow(DotNode src, DotNode dest)
        {
            Source = src;
            Destination = dest;
        }
        
        public string Attribute(string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return $" {name}={value}";
            }

            return "";
        }

        public string ArrowHeadShape { get; set; }
        public string ToGraph()
        {
            var builder = new StringBuilder();
            builder.Append($"{Source.Name}->{Destination?.Name} [ ");

            builder.Append(Attribute("arrowshape", ArrowHeadShape));
            
            builder.Append("];");
            return builder.ToString();
        }

    }
}
using sly.lexer;
using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.tree
{
    public class SyntaxEpsilon : ISyntaxNode 
    {
        public bool IsEpsilon => true;
        public bool Discarded { get; } = false;
        public string Name => "Epsilon";

        public bool HasByPassNodes { get; set; } = false;

        [ExcludeFromCodeCoverage]
        public string Dump(string currentTab, string singleTab)
        {
            return $"Epsilon";
        }

        [ExcludeFromCodeCoverage]
        public string ToJson(int index = 0)
        {
            return $@"""{index}.Epsilon"":""e""";
        }
    }
}

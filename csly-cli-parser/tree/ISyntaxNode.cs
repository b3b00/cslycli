namespace csly.cli.model.tree
{
    public interface ISyntaxNode 
    {

        public bool IsEpsilon { get;}

        bool Discarded { get;  }
        string Name { get; }

        bool HasByPassNodes { get; set; }

        string Dump(string currentTab, string singleTab );

        string ToJson(int index = 0);

    }
}

namespace csly.cli.model.parser
{
    public abstract class ManyClause: IClause
    {
        public IClause Clause { get; set; }

    }
}
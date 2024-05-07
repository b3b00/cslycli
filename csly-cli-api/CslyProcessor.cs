using clsy.cli.builder.parser;

namespace csly_cli_api;



public class CliResult
{
    private bool _isOk;

    public bool IsOK => _isOk;
    
    private string _result;

    public string Result => _result;

    private List<string> _errors;

    public List<string> Errors => _errors;

    public CliResult(List<string> errors)
    {
        _isOk = false;
        _errors = errors;
    }

    public CliResult(string result)
    {
        _isOk = true;
        _result = result;
    }
}

public class CslyProcessor
{
    public static CliResult GetDot(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, "TestParser",
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("DOT", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToDotGraph) });
            if (r.IsError)
            {
                return new CliResult(r.Error.Select(x => $"parse error : {x}").ToList());
            }
            else
            {
                return new CliResult(r.Value[0].content);
            }
        }
        else
        {
            return new CliResult(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }

    public static CliResult GetJson(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, "TestParser",
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("JSON", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToDotGraph) });
            if (r.IsError)
            {
                return new CliResult(r.Error.Select(x => $"parse error : {x}").ToList());
            }
            else
            {
                return new CliResult(r.Value[0].content);
            }
        }
        else
        {
            return new CliResult(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }
}
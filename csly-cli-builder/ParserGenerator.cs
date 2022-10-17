using csly.cli.model.parser;

namespace clsy.cli.builder;

public class ParserGenerator
{
    public static string GenerateParser(ParserModel model, string name, string nameSpace)
    {
        var head = GetHeader(name, nameSpace);
        var body = GetBody(model);
        var foot = getFooter();
        return head+"\n"+body+"\n"+foot;
    }

    private static string GetBody(ParserModel model)
    {
        return "";
    }
    
    private static string GetHeader(string name, string nameSpace)
    {
        return $@"
using sly.lexer;

namespace {nameSpace} {{

    public class {name} {{
";
    }

    private static string getFooter()
    {
        return @"
    }
}";
    }
}
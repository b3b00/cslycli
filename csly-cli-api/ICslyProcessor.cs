using csly.cli.model;

namespace csly_cli_api;

public interface ICslyProcessor
{
    /// <summary>
    /// Compiles and check a grammar specification 
    /// </summary>
    /// <param name="grammar">the grammar specification</param>
    /// <returns></returns>
    CliResult<Model> Compile(string grammar);

    /// <summary>
    /// Parses a source according to a grammar specification
    /// </summary>
    /// <param name="grammar">The grammar specification</param>
    /// <param name="source">The source</param>
    /// <returns></returns>
    CliResult<string> Parse(string grammar, string source);

    /// <summary>
    /// Returns a graphviz dot representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
    CliResult<string> GetDot(string grammar, string source);
    
    /// <summary>
    /// Returns a mermaid js flow chart representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
    CliResult<string> GetMermaid(string grammar, string source);

    /// <summary>
    /// Returns a json representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
    CliResult<string> GetJson(string grammar, string source);

    /// <summary>
    /// generates C# source for lexer and parse according to a grammar spec.
    /// </summary>
    /// <param name="grammar">grammar spec</param>
    /// <param name="nameSpace">namespace of the generated source</param>
    /// <param name="outputType">expected output type for the parser</param>
    /// <returns></returns>
    CliResult<GeneratedSource> GenerateParser(string grammar, string nameSpace, string outputType);

    /// <summary>
    /// Extracts a grammar spec from C# lexer and parser source file 
    /// </summary>
    /// <param name="parser">parser source file content</param>
    /// <param name="lexer">lexer source file content</param>
    /// <returns></returns>
    CliResult<string> ExtractGrammar(string parser, string lexer);
    
    
}
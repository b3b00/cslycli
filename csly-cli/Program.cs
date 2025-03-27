using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using clsy.cli.builder.parser;
using CommandLine;
using csly.cli.model.tree;
using csly_cli_api;
using decompiler;
using sly.cli.options;
using specificationExtractor;

namespace cslycli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CslyProcessor processor = new();
            _ = Parser
                .Default.ParseArguments<
                    TestOptions,
                    GenerateOptions,
                    GenerateProjectOptions,
                    ExtractOptions,
                    DecompileOptions,
                    CompileOptions
                >(args)
                .MapResult(
                    (TestOptions test) => Test(test, processor),
                    (GenerateOptions generate) => Generate(generate, processor),
                    (GenerateProjectOptions generateProject) =>
                        Generate(generateProject.ToGenerateOptions(), processor, true),
                    (ExtractOptions extract) => Extract(extract, processor),
                    (DecompileOptions decompile) => Decompile(decompile, processor),
                    (CompileOptions compile) => Compile(compile, processor),
                    errors =>
                    {
                        foreach (Error error in errors)
                        {
                            Console.WriteLine(error.ToString());
                        }

                        return 1;
                    }
                );
        }

        private static int Compile(CompileOptions compile, CslyProcessor processor)
        {
            string grammarSource = File.ReadAllText(compile.Grammar);
            var result = processor.Compile(grammarSource);
            if (result.IsError)
            {
                Console.WriteLine("Errors in grammar specification file:");
                foreach (string error in result.Errors)
                {
                    Console.WriteLine(error);
                }

                return 1;
            }
            else
            {
                Console.WriteLine("grammar is OK!");
                return 0;
            }
        }

        private static int Decompile(DecompileOptions decompile, CslyProcessor processor)
        {
            Decompiler decompiler = new();
            string specification = decompiler.Decompile(
                decompile.LexerFqn,
                decompile.ParserFqn,
                decompile.AssemblyPath
            );

            if (File.Exists(decompile.SpecificationOutputFile))
            {
                File.Delete(decompile.SpecificationOutputFile);
            }

            File.WriteAllText(decompile.SpecificationOutputFile, specification);

            return 0;
        }

        private static int Extract(ExtractOptions extract, CslyProcessor processor)
        {
            SpecificationExtractor extractor = new();
            string specification = extractor.ExtractFromFiles(
                extract.LexerPath,
                extract.ParserPath
            );

            if (File.Exists(extract.SpecificationOutputFile))
            {
                File.Delete(extract.SpecificationOutputFile);
            }

            File.WriteAllText(extract.SpecificationOutputFile, specification);

            return 0;
        }

        private static int Generate(
            GenerateOptions generate,
            CslyProcessor processor,
            bool generateProject = false
        )
        {
            FileInfo fi = new(generate.Grammar);
            _ = fi.Name.Replace(fi.Extension, "");
            _ = new ParserBuilder();

            string grammar = File.ReadAllText(generate.Grammar);

            CliResult<string> compilationResult = processor.Compile(grammar);
            if (compilationResult.IsError)
            {
                Console.WriteLine("Errors in grammar specification file:");
                foreach (string error in compilationResult.Errors)
                {
                    Console.WriteLine(error);
                }

                return 1;
            }

            CliResult<GeneratedSource> generationResult = processor.GenerateParser(
                grammar,
                generate.NameSpace,
                generate.ParserOutput
            );

            if (generationResult.IsOK)
            {
                string path = Path.Combine(
                    generate.OutputDir,
                    generationResult.Result.LexerName + ".cs"
                );
                File.WriteAllText(path, generationResult.Result.Lexer);

                path = Path.Combine(generate.OutputDir, generationResult.Result.ParserName + ".cs");
                File.WriteAllText(path, generationResult.Result.Parser);

                if (generateProject)
                {
                    File.WriteAllText(
                        Path.Combine(generate.OutputDir, generationResult.Result.ParserName)
                            + ".csproj",
                        generationResult.Result.Project
                    );
                    File.WriteAllText(
                        Path.Combine(generate.OutputDir, "Program.cs"),
                        generationResult.Result.Program
                    );
                }
                return 0;
            }
            Console.WriteLine("Errors while generating source code:");
            foreach (string error in generationResult.Errors)
            {
                Console.WriteLine(error);
            }

            return 2;
        }

        private static int Test(TestOptions test, CslyProcessor processor)
        {
            FileInfo fi = new(test.Grammar);
            string parserName = fi.Name.Replace(fi.Extension, "");
            ParserBuilder builder = new();

            static string emptyProcessor(Type type, Type lexerType, ISyntaxNode tree)
            {
                return "";
            }

            string grammarSource = File.ReadAllText(test.Grammar);

            clsy.cli.builder.Result<csly.cli.model.Model> mod = builder.CompileModel(grammarSource);
            if (mod.IsError)
            {
                Console.WriteLine("Errors in grammar specification file:");
                foreach (string error in mod.Error)
                {
                    Console.WriteLine(error);
                }

                return 1;
            }

            Console.WriteLine("Model compilation succeeded.");

            List<(OutputFormat format, SyntaxTreeProcessor processor)> formatters = new();

            if (test.OutputTypes.Any())
            {
                Dictionary<OutputFormat, SyntaxTreeProcessor> formats =
                    new()
                    {
                        { OutputFormat.DOT, ParserBuilder.SyntaxTreeToDotGraph },
                        { OutputFormat.JSON, ParserBuilder.SyntaxTreeToJson },
                        { OutputFormat.MERMAID, ParserBuilder.SyntaxTreeToMermaid }
                    };

                formatters = test
                    .OutputTypes.Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Select(x => (x, formats[x]))
                    .ToList();
            }
            else
            {
                formatters = new List<(OutputFormat, SyntaxTreeProcessor)>()
                {
                    (OutputFormat.NO, emptyProcessor)
                };
            }

            string grammar = File.ReadAllText(test.Grammar);
            string source = File.ReadAllText(test.Source);

            clsy.cli.builder.Result<List<(string format, string content)>, List<string>> result =
                builder.Getz(
                    grammar,
                    source,
                    parserName,
                    formatters.ConvertAll(x => (x.format.ToString(), x.processor))
                );

            if (result.IsError)
            {
                Console.WriteLine("Errors in source file:");
                foreach (string error in result.Error)
                {
                    Console.WriteLine(error);
                }

                return 2;
            }

            Console.WriteLine("Parse succeeded.");

            if (test.OutputTypes?.Any() == true)
            {
                foreach ((string format, string content) in result.Value)
                {
                    Dictionary<string, string> extensions =
                        new()
                        {
                            { nameof(OutputFormat.DOT), ".dot" },
                            { nameof(OutputFormat.MERMAID), ".mermaid" },
                            { nameof(OutputFormat.JSON), ".json" }
                        };

                    string outputFileExtension = extensions[format];

                    string outputFileName = Path.Combine(
                        test.Output,
                        parserName + outputFileExtension
                    );
                    if (File.Exists(outputFileName))
                    {
                        File.Delete(outputFileName);
                    }

                    File.AppendAllText(outputFileName, content);

                    Console.WriteLine($"file {outputFileName} generated.");
                }
            }

            return 0;
        }
    }
}

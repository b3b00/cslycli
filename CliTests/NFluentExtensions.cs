using clsy.cli.builder;
using csly_cli_api;
using csly.cli.model;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using NFluent;
using NFluent.Extensibility;
using NFluent.Kernel;
using sly.buildresult;
using sly.lexer;
using sly.parser;

namespace CliTests;

public static class NFluentParseExtensions
    {
        public static List<string> GetLines(this string content)
        {
            List<string> lines = new List<string>();
            using (StringReader reader = new StringReader(content))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }

            return lines;
        }
        
        
        
        public static ICheckLink<ICheck<Result<Model,List<string>>>> IsOkModel(this ICheck<Result<Model,List<string>>> context) => IsOkResult<Model>(context);
        
        public static ICheckLink<ICheck<Result<Model,List<string>>>> IsNotOkModel(this ICheck<Result<Model,List<string>>> context) => IsNotOkResult<Model>(context);
        
        
        public static ICheckLink<ICheck<Result<T,List<string>>>> IsNotOkResult<T>(this ICheck<Result<T,List<string>>> context) 
        {
            var checker = ExtensibilityHelper.ExtractChecker(context);
            var c = checker.ExecuteCheck(() =>
                {
                    var sut = checker.Value;
                    if (!sut.IsError)
                    {
                        var errorMessage = @$"result should be KO";
                        throw new FluentCheckException(errorMessage);
                    }
                },
                "result was expected to be ok, but is ko."
            );
            
            
            return c;
        }
        
        
        public static ICheckLink<ICheck<Result<T,List<string>>>> IsOkResult<T>(this ICheck<Result<T,List<string>>> context) 
        {
            var checker = ExtensibilityHelper.ExtractChecker(context);
            var c= checker.ExecuteCheck(() =>
                {
                    var sut = checker.Value;
                    if (sut.IsError)
                    {
                        var errorMessage = @$"result contains errors : {string.Join("\n", sut.Error)}";
                        throw new FluentCheckException(errorMessage);
                    }
                },

                "result was expected to be wrong, but is ok");
            
            
            return c;
        }
        
        public static ICheckLink<ICheck<CliResult<T>>> IsOkCliResult<T>(this ICheck<CliResult<T>> context) 
        {
            var checker = ExtensibilityHelper.ExtractChecker(context);
            var c= checker.ExecuteCheck(() =>
            {
                var sut = checker.Value;
                if (sut.IsError)
                {
                    var errorMessage = @$"result contains errors : {string.Join("\n", sut.Errors)}";
                    throw new FluentCheckException(errorMessage);
                }
            },

            "result was expected to be wrong, but is ok");
            
            
            return c;
        }
        
        public static ICheckLink<ICheck<CliResult<T>>> IsNotOkCliResult<T>(this ICheck<CliResult<T>> context) 
        {
            var checker = ExtensibilityHelper.ExtractChecker(context);
            var c= checker.ExecuteCheck(() =>
                {
                    var sut = checker.Value;
                    if (!sut.IsError)
                    {
                        var errorMessage = @$"result is ok but should be wrong";
                        throw new FluentCheckException(errorMessage);
                    }
                },

                "result was expected to be ok, but is wronh");
            
            
            return c;
        }

        
        public static ICheckLink<ICheck<ParseResult<IN,OUT>>> IsOkParsing<IN,OUT>(this ICheck<ParseResult<IN,OUT>> context) where IN : struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, $"parse failed")
                .FailWhen(sut => sut.Result == null, "parse result is null")
                .OnNegate("parse expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<LexerResult<IN>>> IsOkLexing<IN>(this ICheck<LexerResult<IN>> context) where IN : struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "lexing failed")
                .FailWhen(sut => sut.Tokens == null, "lexing result is null")
                .OnNegate("parse expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<BuildResult<T>>> IsOk<T>(this ICheck<BuildResult<T>> context) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "parse failed")
                .FailWhen(sut => sut.Result == null, "parser result is null")
                .OnNegate("parser expected to fail.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<BuildResult<T>>> HasError<T>(this ICheck<BuildResult<T>> context, ErrorCodes expectedErrorCode, string expectedMessageNeedle) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.IsError, "parser has not failed.")
                .FailWhen(sut => !sut.Errors.Any() , "parser has no error")
                .FailWhen(sut => !sut.Errors.Exists(x => x.Code == expectedErrorCode && x.Message.Contains(expectedMessageNeedle)),"error {expected} not found in {checked}")
                .DefineExpectedValue($"{expectedErrorCode} : >{expectedMessageNeedle}<")
                .OnNegate("error {expected} found.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<Token<T>>> IsEqualTo<T>(this ICheck<Token<T>> context, T expectedTokenId, string expectedValue) where T: struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.TokenID.Equals(expectedTokenId), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Value.Equals(expectedValue), "expecting {expected} found {checked}.")
                .OnNegate("token is exact")
                .DefineExpectedValue($"{expectedTokenId} : >{expectedValue}<")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<Token<T>>> IsEqualTo<T>(this ICheck<Token<T>> context, T expectedTokenId, string expectedValue, int expectedLine, int expectedColumn) where T: struct
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.TokenID.Equals(expectedTokenId), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Value.Equals(expectedValue), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Position.Line.Equals(expectedLine), "expecting {expected} found {checked}.")
                .FailWhen(sut => !sut.Position.Column.Equals(expectedColumn), "expecting {expected} found {checked}.")
                .OnNegate("token is exact")
                .DefineExpectedValue($"{expectedTokenId} : >{expectedValue}<")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<IEnumerable<T>>> IsSingle<T>(this ICheck<IEnumerable<T>> context)
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.Any() && sut.Count() != 1,
                    "{expected} is expected to have 1 and only 1 element.");
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<Result<T>>> IsOk<T>(this ICheck<Result<T>> context) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.IsError, "parser has failed. found {checked}.")
                .FailWhen(sut => sut.Error != null && sut.Error.Any() , "parser has errors. found {checked}")
                .DefineExpectedValue("no error expected")
                .OnNegate("no error found.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
    }
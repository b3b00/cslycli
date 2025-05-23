﻿using clsy.cli.builder.parser.cli.model;
using clsy.cli.model.lexer;
using clsy.cli.model.parser;
using csly.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;
using Range = csly.cli.model.lexer.Range;

namespace csly.cli.parser;

[ParserRoot("root")]
public class CLIParser
{
    #region roots

    [Production("root: genericRoot parserRoot ")]
    public ICLIModel Root(ICLIModel genericLex, ICLIModel parser, ParserContext context)
    {
        return new Model(genericLex as LexerModel, parser as ParserModel);
    }

    [Production("parserRoot : PARSER[d] ID SEMICOLON[d] parser_optimization* rule*")]
    public ICLIModel Parser(Token<CLIToken> name, List<ICLIModel> optimizations, List<ICLIModel> rules,
        ParserContext context)
    {
        var optims = optimizations.Cast<ParserOptimization>().ToList();
        var model = new ParserModel()
        {
            UseMemoization = optims.Exists(x => x.UseMemoization),
            BroadenTokenWindow = optims.Exists(x => x.BroadenTokenWindow),
            AutoCloseIndentations = optims.Exists(x => x.AutoCloseIndentation),
            Name = name.Value,
            Rules = rules.Cast<Rule>().ToList()
        };
        var root = model.Rules.FirstOrDefault(x => x.IsRoot);

        model.Position = name.Position;

        return model;
    }

    [Production(
        "parser_optimization : LEFTBRACKET[d] [USEMEMOIZATION|BROADENTOKENWINDOW|AUTOCLOSEINDENTATION] RIGHTBRACKET[d]")]
    public ICLIModel Optimization(Token<CLIToken> optimizationToken, ParserContext context)
    {
        return new ParserOptimization()
        {
            UseMemoization = optimizationToken.TokenID == CLIToken.USEMEMOIZATION,
            BroadenTokenWindow = optimizationToken.TokenID == CLIToken.BROADENTOKENWINDOW,
            AutoCloseIndentation = optimizationToken.TokenID == CLIToken.AUTOCLOSEINDENTATION,
            Position = optimizationToken.Position
        };
    }

    #endregion


    #region generic lexer

    [Production("genericRoot : GENERICLEXER[d] ID SEMICOLON[d] lexer_option* modedToken*")]
    public ICLIModel Lexer(Token<CLIToken> name, List<ICLIModel> optionList, List<ICLIModel> tokens,
        ParserContext context)
    {
        var opts = optionList.Cast<LexerOptions>();
        var options = new LexerOptions()
        {
            IgnoreWS = opts.Select(x => x.IgnoreWS).FirstOrDefault(x => x.HasValue),
            IndentationAware = opts.Select(x => x.IndentationAware).FirstOrDefault(x => x.HasValue),
            IgnoreEOL = opts.Select(x => x.IgnoreEOL).FirstOrDefault(x => x.HasValue),
            IgnoreKeyWordCase = opts.Select(x => x.IgnoreKeyWordCase).FirstOrDefault(x => x.HasValue)
        };
        return new LexerModel(tokens.Cast<TokenModel>().ToList(), options, name.Value) { Position = name.Position };
    }

    [Production("modedToken : mode* token")]
    public ICLIModel ModedToken(List<ICLIModel> modes, ICLIModel token, ParserContext context)
    {
        TokenModel model = token as TokenModel;
        foreach (var mode in modes)
        {
            switch (mode)
            {
                case PopModel pop:
                {
                    model.IsPop = true;
                    break;
                }
                case PushModel push:
                {
                    model.PushMode = push.Target;
                    break;
                }
                case ModeModel mod:
                {
                    model.AddModes(mod.Modes);
                    break;
                }
            }
        }

        return model;
    }

    [Production("mode : LEFTBRACKET[d] PUSH[d] LEFTPAREN[d] STRING RIGHTPAREN[d] RIGHTBRACKET[d]")]
    public ICLIModel Push(Token<CLIToken> push, ParserContext context)
    {
        return new PushModel(push.StringWithoutQuotes) { Position = push.Position };
    }

    [Production("mode : LEFTBRACKET[d] POP RIGHTBRACKET[d]")]
    public ICLIModel Pop(Token<CLIToken> pop, ParserContext context)
    {
        return new PopModel() { Position = pop.Position };
    }

    [Production("mode : LEFTBRACKET[d] MODE[d] LEFTPAREN[d] STRING (COMMA[d] STRING )* RIGHTPAREN[d] RIGHTBRACKET[d]")]
    public ICLIModel Modes(Token<CLIToken> mode, List<Group<CLIToken, ICLIModel>> modes, ParserContext context)
    {
        var mods = modes.Select(x => x.Token(0).StringWithoutQuotes).ToList();

        return new ModeModel(mode.StringWithoutQuotes, mods) { Position = mode.Position };
    }

    [Production("mode : LEFTBRACKET[d] MODE[d] RIGHTBRACKET[d]")]
    public ICLIModel ModeDefault(ParserContext context)
    {
        return new ModeModel(new List<string>());
    }


    [Production(
        "token : attribute* LEFTBRACKET[d] [SUGARTOKEN|SINGLELINECOMMENT|HEXATOKEN] RIGHTBRACKET[d] IdentifierOrString COLON[d] STRING SEMICOLON[d]")]
    public ICLIModel OneArgToken(List<ICLIModel> attributes, Token<CLIToken> type, ICLIModel id,
        Token<CLIToken> value, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.KEYWORDTOKEN => GenericToken.KeyWord,
            CLIToken.SUGARTOKEN => GenericToken.SugarToken,
            CLIToken.SINGLELINECOMMENT => GenericToken.Comment,
            CLIToken.HEXATOKEN => GenericToken.Hexa,
            _ => GenericToken.SugarToken
        };
        var idValue = (id as IdentifierOrString).Value;
        context.AddEnumName(idValue);
        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue,
            value.StringWithoutQuotes) { Position = type.Position };
    }

    [Production(
        "token : attribute* LEFTBRACKET[d] [STRINGTOKEN|CHARTOKEN|MULTILINECOMMENT|CUSTOMIDTOKEN] RIGHTBRACKET[d] IdentifierOrString COLON[d] STRING STRING SEMICOLON[d]")]
    public ICLIModel TwoArgToken(List<ICLIModel> attributes, Token<CLIToken> type, ICLIModel id,
        Token<CLIToken> arg1, Token<CLIToken> arg2, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.CHARTOKEN => GenericToken.Char,
            CLIToken.MULTILINECOMMENT => GenericToken.Comment,
            CLIToken.CUSTOMIDTOKEN => GenericToken.Identifier,
            _ => GenericToken.SugarToken
        };
        var idValue = (id as IdentifierOrString).Value; 
        context.AddEnumName(idValue);
        
        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue,IdentifierType.Custom,
                arg1.StringWithoutQuotes.Replace("\\\\", "\\"), arg2.StringWithoutQuotes.Replace("\\\\", "\\"))
            { Position = type.Position };
    }

    [Production(
        "token : attribute* LEFTBRACKET[d] [KEYWORDTOKEN] RIGHTBRACKET[d] IdentifierOrString COLON[d]  STRING* SEMICOLON[d]")]
    public ICLIModel ManyArgToken(List<ICLIModel> attributes, Token<CLIToken> type, ICLIModel id,
        List<Token<CLIToken>> args, ParserContext context)
    {
        var tokenType = GenericToken.KeyWord;
        var idValue = (id as IdentifierOrString).Value;
        context.AddEnumName(idValue);
        var aargs = args.Select(arg => arg.StringWithoutQuotes.Replace("\\\\", "\\")).ToArray();
        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue, aargs)
            { Position = type.Position };
    }

    [Production(
        "token :attribute* LEFTBRACKET[d] DATETOKEN[d] RIGHTBRACKET[d] IdentifierOrString COLON[d] [DDMMYYYY|YYYYMMDD] CHAR SEMICOLON[d]")]
    public ICLIModel DateToken(List<ICLIModel> attributes, ICLIModel id, Token<CLIToken> dateType,
        Token<CLIToken> separator, ParserContext context)
    {
        var tokenType = GenericToken.Date;
        string format = dateType.TokenID.ToString();
        var idValue = (id as IdentifierOrString).Value;
        context.AddEnumName(idValue);
        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue, format,
            separator.CharValue.ToString()) { Position = id.Position };
    }

    [Production(
        "token :attribute* LEFTBRACKET[d] UPTOTOKEN[d] RIGHTBRACKET[d] IdentifierOrString COLON[d] STRING* SEMICOLON[d]")]
    public ICLIModel UpToToken(List<ICLIModel> attributes, ICLIModel id, List<Token<CLIToken>> delimiters,
        ParserContext context)
    {
        var tokenType = GenericToken.UpTo;
        var idValue = (id as IdentifierOrString).Value;
        context.AddEnumName(idValue);
        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue,
            delimiters.Select(x => x.StringWithoutQuotes).ToArray()) { Position = id.Position };
    }

    [Production(
        "token : attribute* LEFTBRACKET[d] [STRINGTOKEN|INTTOKEN|ALPHAIDTOKEN|ALPHANUMIDTOKEN|ALPHANUMDASHIDTOKEN|DOUBLETOKEN|HEXATOKEN] RIGHTBRACKET[d] IdentifierOrString SEMICOLON[d]")]
    public ICLIModel NoArgToken(List<ICLIModel> attributes, Token<CLIToken> type, ICLIModel id,
        ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.INTTOKEN => GenericToken.Int,
            CLIToken.DOUBLETOKEN => GenericToken.Double,
            CLIToken.ALPHAIDTOKEN => GenericToken.Identifier,
            CLIToken.ALPHANUMIDTOKEN => GenericToken.Identifier,
            CLIToken.ALPHANUMDASHIDTOKEN => GenericToken.Identifier,
            CLIToken.HEXATOKEN => GenericToken.Hexa,
            _ => GenericToken.SugarToken
        };
        var idType = type.TokenID switch
        {
            CLIToken.ALPHAIDTOKEN => IdentifierType.Alpha,
            CLIToken.ALPHANUMIDTOKEN => IdentifierType.AlphaNumeric,
            CLIToken.ALPHANUMDASHIDTOKEN => IdentifierType.AlphaNumericDash,
            _ => IdentifierType.Alpha
        };
        var idValue = (id as IdentifierOrString).Value;
        context.AddEnumName(idValue);
        if (type.TokenID == CLIToken.STRINGTOKEN)
        {
            return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue, idType, "\"", "\\")
                { Position = id.Position };
        }

        return new TokenModel(attributes.Cast<AttributeModel>().ToList(), tokenType, idValue, idType)
            { Position = id.Position };
    }

    [Production("token : attribute* LEFTBRACKET[d] EXTENSIONTOKEN[d] RIGHTBRACKET[d] IdentifierOrString extension ")]
    public ICLIModel ExtensionToken(List<ICLIModel> attributes, ICLIModel id, ICLIModel extension,
        ParserContext context)
    {
        var idValue = (id as IdentifierOrString).Value;
        (extension as ExtensionTokenModel).Name = idValue;
        (extension as ExtensionTokenModel).SetAttributes(attributes.Cast<AttributeModel>());
        context.AddEnumName(idValue);
        return (extension as ExtensionTokenModel);
    }

    #endregion

    #region lexer extensions

    [Production("extension : OPEN_EXT[d] transition_chain+ CLOSE_EXT[d]")]
    public ICLIModel Extension(List<ICLIModel> chains, ParserContext context)
    {
        var ext = new ExtensionTokenModel(null, chains.Cast<TransitionChain>().ToList());
        // TODO check if end exists !
        return ext;
    }

    [Production(
        "transition_chain : (LEFTPAREN[d] ID RIGHTPAREN[d])? transition+  (ARROW ENDTOKEN)?")] // TODO SEMICOLON really useful ?
    public ICLIModel Chain(ValueOption<Group<CLIToken, ICLIModel>> startNode, List<ICLIModel> transitions,
        ValueOption<Group<CLIToken, ICLIModel>> end, ParserContext context)
    {
        var trans = transitions.Cast<ITransition>().ToList();
        var chain = new TransitionChain(trans, end.IsSome);
        var start = startNode.Match(
            (x) =>
            {
                chain.StartingNodeName = x.Token(0).Value;
                return x;
            },
            () =>
            {
                chain.StartingNodeName = GenericLexer<CLIToken>.start;
                return null;
            });
        return chain;
    }

    [Production("transition : ARROW[d] (LEFTPAREN[d] ID RIGHTPAREN[d])? pattern repeater? (AT[d] ID)?")]
    public ICLIModel Transition(ValueOption<Group<CLIToken, ICLIModel>> mark, ICLIModel pattern,
        ValueOption<ICLIModel> repeater, ValueOption<Group<CLIToken, ICLIModel>> target, ParserContext context)
    {
        var transition = pattern as ITransition;
        var t = repeater.Match((x) =>
            {
                transition.Repeater = x as TransitionRepeater;
                return transition;
            },
            () => transition) as ITransition;
        mark.Match(
            (x) =>
            {
                t.Mark = x.Token(0).Value;
                return x;
            },
            () => null);
        target.Match(
            (x) =>
            {
                t.Target = x.Token(0).Value;
                return x;
            },
            () => null);

        return t;
    }

    [Production("repeater : ZEROORMORE[d]")]
    public ICLIModel RepeatZeroOrMore(ParserContext context)
    {
        return new TransitionRepeater(RepeaterType.ZeroOrMore);
    }

    [Production("repeater : ONEORMORE[d]")]
    public ICLIModel RepeatOneOrMore(ParserContext context)
    {
        return new TransitionRepeater(RepeaterType.OneOrMore);
    }

    [Production("repeater : LEFTCURL[d] INT RIGHTCURL[d]")]
    public ICLIModel RepeatMany(Token<CLIToken> many, ParserContext context)
    {
        return new TransitionRepeater(RepeaterType.Count, many.IntValue);
    }


    [Production("pattern : CHAR")]
    public ICLIModel SinglePattern(Token<CLIToken> single, ParserContext context)
    {
        return new CharacterTransition(single.CharValue);
    }


    [Production("pattern : LEFTBRACKET[d] range (COMMA[d] range)* RIGHTBRACKET[d]")]
    public ICLIModel RangePattern(ICLIModel headRange, List<Group<CLIToken, ICLIModel>> tailsRanges,
        ParserContext context)
    {
        List<Range> ranges = new List<Range>() { headRange as Range };
        var tail = tailsRanges.Select(x =>
        {
            var xx = x.Value(0) as Range;
            return xx;
        }).ToList();
        ranges.AddRange(tail);

        return new RangeTransition(ranges);
    }

    [Production("range : CHAR DASH[d] CHAR")]
    public ICLIModel RangeDefinition(Token<CLIToken> start, Token<CLIToken> end, ParserContext context)
    {
        return new Range(start.CharValue, end.CharValue);
    }

    #endregion

    #region lexer options

    // [Keyword("IndentationAware")] INDENTATIONAWARE,
    // [Keyword("IgnoreWhiteSpaces")] IGNOREWHITESPACES,
    // [Keyword("IgnoreEndOfLines")] IGNOREEOL,
    // [Keyword("IgnoreKeyWordCase")] IGNOREKEYWORDCASING,

    [Production(
        "lexer_option : LEFTBRACKET[d] [IGNOREKEYWORDCASING|INDENTATIONAWARE|IGNOREWHITESPACES|IGNOREEOL] LEFTPAREN[d][TRUE|FALSE]RIGHTPAREN[d] RIGHTBRACKET[d]")]
    public ICLIModel lexerOption(Token<CLIToken> option, Token<CLIToken> enabledFlag, ParserContext context)
    {
        bool enabled = enabledFlag.Value == "true";
        return new LexerOptions()
        {
            IgnoreWS = option.TokenID == CLIToken.IGNOREWHITESPACES ? enabled : null,
            IgnoreEOL = option.TokenID == CLIToken.IGNOREEOL ? enabled : null,
            IgnoreKeyWordCase = option.TokenID == CLIToken.IGNOREKEYWORDCASING ? enabled : null,
            IndentationAware = option.TokenID == CLIToken.INDENTATIONAWARE ? enabled : null
        };
    }

    #endregion

    #region parser
    
    [Production(@"IdentifierOrString : [ID | GENERICLEXER | STRINGTOKEN |  PARSER | CHARTOKEN | INTTOKEN | DATETOKEN | DOUBLETOKEN | HEXATOKEN | 
                ALPHAIDTOKEN | ALPHANUMIDTOKEN | ALPHANUMDASHIDTOKEN | CUSTOMIDTOKEN | KEYWORDTOKEN | SUGARTOKEN | SINGLELINECOMMENT | UPTOTOKEN | 
                MULTILINECOMMENT | EXTENSIONTOKEN | PUSH | MODE | POP | TRUE | FALSE | INDENT | UINDENT | YYYYMMDD | DDMMYYYY | STRING ]")]
    public ICLIModel IdOrString(Token<CLIToken> token, ParserContext context)
    {
        return new IdentifierOrString(token);
    }
    
    
    [Production("operand :  LEFTBRACKET[d] OPERAND[d] RIGHTBRACKET[d]")]
    public GrammarNode Operand(ParserContext context)
    {
        return null;
    }

    [Production("rule  :  attribute* ARROW? operand? IdentifierOrString COLON[d] clause+ SEMICOLON[d]")]
    //[Production("rule  :  ARROW ? operand? Identifier COLON[d] clause+ SEMICOLON[d]")]
    public GrammarNode Rule(List<ICLIModel> attributes, Token<CLIToken> root, ValueOption<ICLIModel> operand,
        ICLIModel name, List<ICLIModel> clauses, ParserContext context)
    {
        var rule = new Rule(operand.IsSome);
        rule.SetAttributes(attributes.Cast<AttributeModel>().ToList());
        var ntName = name as IdentifierOrString;
        rule.NonTerminalName = ntName.Value;
        rule.Clauses = clauses.Cast<IClause>().ToList();
        rule.IsRoot = !root.IsEmpty;
        rule.Position = root.Position;
        return rule;
    }
    
    [Production("rule  :  attribute* operand? IdentifierOrString+ SEMICOLON[d]")]
    public GrammarNode ShortOPerand(List<ICLIModel> attributes, ValueOption<ICLIModel> operand, List<ICLIModel> ids,  ParserContext context)
    {
        var rule = new Rule(operand.IsSome);
        rule.SetAttributes(attributes.Cast<AttributeModel>().ToList());
        var identifiers = ids.Cast<IdentifierOrString>().ToList();
        rule.NonTerminalName = "operand";
        if (ids.Count == 1)
        {
            rule.Clauses = new List<IClause>() { BuildTerminalOrNonTerminal(identifiers[0], context) };
        }
        else
        {
            var choice = new ChoiceClause(identifiers.Select(x => BuildTerminalOrNonTerminal(x, context)).ToList());
            rule.Clauses = new List<IClause>() { choice };
        }

        rule.IsRoot = false;
        rule.Position = identifiers[0].Position;
        return rule;
    }


    [Production("rule : attribute* LEFTBRACKET[d] PREFIX[d] INT RIGHTBRACKET[d] IdentifierOrString* SEMICOLON[d]")]
    public ICLIModel PrefixRule(List<ICLIModel> attributes, Token<CLIToken> precedence, List<ICLIModel> ids,
        ParserContext context)
    {
        var identifiers = ids.Cast<IdentifierOrString>().ToList();
        if (ids.Count == 1)
        {
            
            var rule = new PrefixRule(identifiers[0].Value, identifiers[0].TokenId == CLIToken.STRING,
                precedence.IntValue);
            rule.SetAttributes(attributes.Cast<AttributeModel>());
            rule.Position = precedence.Position;
            return rule;
        }

        var r = new ManyPrefixRule(identifiers.Select(x =>
        {
            var rule = new PrefixRule(x.Value, x.TokenId == CLIToken.STRING,
                precedence.IntValue);
            rule.Position = precedence.Position;
            return rule;
        }).ToList());
        r.SetAttributes(attributes.Cast<AttributeModel>());
        return r;
    }

    [Production("rule : attribute* LEFTBRACKET[d] POSTFIX[d] INT RIGHTBRACKET[d] IdentifierOrString* SEMICOLON[d]")]
    public ICLIModel PostfixRule(List<ICLIModel> attributes, Token<CLIToken> precedence, List<ICLIModel> ids,
        ParserContext context)
    {
        var identifiers = ids.Cast<IdentifierOrString>().ToList();
        if (ids.Count == 1)
        {
            var rule = new PostfixRule(identifiers[0].Value, identifiers[0].TokenId == CLIToken.STRING,
                precedence.IntValue);
            rule.SetAttributes(attributes.Cast<AttributeModel>());
            rule.Position = precedence.Position;
            return rule;
        }

        var r = new ManyPostfixRule(identifiers.Select(x =>
        {
            var value = x.IsExplicit ? x.Value.Substring(1, x.Value.Length - 1) : x.Value;
            var rule = new PostfixRule(value, x.TokenId == CLIToken.STRING,
                precedence.IntValue);
            rule.Position = precedence.Position;
            return rule;
        }).ToList());
        r.SetAttributes(attributes.Cast<AttributeModel>());
        return r;
    }

    [Production("rule : attribute* LEFTBRACKET[d] [RIGHT|LEFT] INT RIGHTBRACKET[d] IdentifierOrString+ SEMICOLON[d]")]
    public ICLIModel InfixRule(List<ICLIModel> attributes, Token<CLIToken> rightOrLeft, Token<CLIToken> precedence,
        List<ICLIModel> ids, ParserContext context)
    {
        var identifiers = ids.Cast<IdentifierOrString>().ToList();
        if (identifiers.Count == 1)
        {
            var rule = new InfixRule(identifiers[0].Value, identifiers[0].TokenId == CLIToken.STRING,
                rightOrLeft.TokenID == CLIToken.LEFT ? Associativity.Left : Associativity.Right,
                precedence.IntValue);
            rule.SetAttributes(attributes.Cast<AttributeModel>());
            rule.Position = precedence.Position;
            return rule;
        }

        var r = new ManyInfixRule(identifiers.Select(x =>
        {
            var rule = new InfixRule(x.Value, x.TokenId == CLIToken.STRING,
                rightOrLeft.TokenID == CLIToken.LEFT ? Associativity.Left : Associativity.Right,
                precedence.IntValue);
            rule.Position = precedence.Position;
            return rule;
        }).ToList());
        r.SetAttributes(attributes.Cast<AttributeModel>());
        return r;
    }


    [Production("item : IdentifierOrString")]
    public IClause Clause(IdentifierOrString item, ParserContext context)
    {
        return BuildTerminalOrNonTerminal(item, context);
    }

    [Production("item : choiceclause")]
    public IClause ClauseChoice(IClause choice, ParserContext context) => choice; 


    #region clauses

    [Production("clause : item ZEROORMORE[d]")]
    public IClause ZeroMoreClause(IClause item, ParserContext context)
    {
        return new ZeroOrMoreClause(item) { Position = item.Position };
    }

    [Production("clause : item ONEORMORE[d]")]
    public IClause OneMoreClause(IClause item, ParserContext context)
    {
        return new OneOrMoreClause(item) { Position = item.Position };
    }
    
    [Production("clause : item LEFTCURL[d] INT RIGHTCURL[d]")]
    public IClause Repeat(IClause item, Token<CLIToken> count, ParserContext context)
    {
        return new RepeatClause(item, count.IntValue) { Position = item.Position };
    }
    
    [Production("clause : item LEFTCURL[d] INT DASH[d] INT RIGHTCURL[d]")]
    public IClause RepeatRange(IClause item, Token<CLIToken> min, Token<CLIToken> max,ParserContext context)
    {
        return new RepeatClause(item, min.IntValue, max.IntValue) { Position = item.Position };
    }

    [Production("clause : item OPTION[d]")]
    public IClause OptionClause(IClause item, ParserContext context)
    {
        return new OptionClause(item) { Position = item.Position };
    }

    [Production("clause :discardeditem")]
    public IClause DiscardedClause(IClause item, ParserContext context)
    {
        return item;
    }


    [Production("clause : item ")]
    public IClause SimpleClause(IClause item, ParserContext context)
    {
        return item;
    }

    #endregion

    #region choices

    [Production("clause : choiceclause")]
    public IClause AlternateClause(ChoiceClause choices, ParserContext context)
    {
        return choices;
    }

    [Production("choiceclause : LEFTBRACKET[d]  item ( OR[d] item)* RIGHTBRACKET[d]  ")]
    public IClause AlternateChoices(ICLIModel head, List<Group<CLIToken, ICLIModel>> tail, ParserContext context)
    {
        var choice = new ChoiceClause(head as IClause, tail.Select(x => x.Value(0)).Cast<IClause>().ToList());
        return choice;
    }

    [Production("clause : choiceclause ONEORMORE[d] ")]
    public IClause ChoiceOneOrMore(ChoiceClause choices, ParserContext context)
    {
        return new OneOrMoreClause(choices);
    }

    [Production("clause : choiceclause ZEROORMORE[d] ")]
    public IClause ChoiceZeroOrMore(ChoiceClause choices, ParserContext context)
    {
        return new ZeroOrMoreClause(choices);
    }

    [Production("clause : choiceclause LEFTCURL[d] INT RIGHTCURL[d]")]
    public IClause RepeatChoice(ChoiceClause item, Token<CLIToken> count, ParserContext context)
    {
        return new RepeatClause(item, count.IntValue) { Position = item.Position };
    }
    
    [Production("clause : choiceclause LEFTCURL[d] INT DASH[d] INT RIGHTCURL[d]")]
    public IClause RepeatChoiceRange(ChoiceClause item, Token<CLIToken> min, Token<CLIToken> max,ParserContext context)
    {
        return new RepeatClause(item, min.IntValue, max.IntValue) { Position = item.Position };
    }

    [Production("clause : choiceclause OPTION[d] ")]
    public IClause ChoiceOptional(ChoiceClause choices, ParserContext context)
    {
        return new OptionClause(choices);
    }

    #endregion


    #region groups

    [Production("clause : group")]
    public IClause GroupClause(IClause group, ParserContext context) => group;

    [Production("group : LEFTPAREN[d] discardeditem* RIGHTPAREN[d] ")]
    public GroupClause Group(List<ICLIModel> clauses, ParserContext context)
    {
        var group = new GroupClause(clauses.Cast<IClause>().ToList());
        return group;
        ;
    }

    [Production("discardeditem : item DISCARD?")]
    public ICLIModel discardedItem(ICLIModel item, Token<CLIToken> discard, ParserContext context)
    {
        if (item is TerminalClause term)
        {
            term.IsDiscarded = true;
        }
        else if (item is NonTerminalClause nonterm)
        {
            if (!discard.IsEmpty)
            {
                context.AddError(
                    $" non terminal clause {nonterm.NonTerminalName} can not be discarded ! {discard.Position.Line}");
            }
        }

        return item;
    }


    [Production("clause : group ONEORMORE[d] ")]
    public IClause GroupOneOrMore(GroupClause group, ParserContext context)
    {
        return new OneOrMoreClause(group);
    }

    [Production("clause : group ZEROORMORE [d]")]
    public IClause GroupZeroOrMore(GroupClause group, ParserContext context)
    {
        return new ZeroOrMoreClause(group);
    }
    [Production("clause : group LEFTCURL[d] INT RIGHTCURL[d]")]
    public IClause RepeatGroup(GroupClause item, Token<CLIToken> count, ParserContext context)
    {
        return new RepeatClause(item, count.IntValue) { Position = item.Position };
    }
    
    [Production("clause : group LEFTCURL[d] INT DASH[d] INT RIGHTCURL[d]")]
    public IClause RepeatGroupRange(GroupClause item, Token<CLIToken> min, Token<CLIToken> max,ParserContext context)
    {
        return new RepeatClause(item, min.IntValue, max.IntValue) { Position = item.Position };
    }

    [Production("clause : group OPTION[d] ")]
    public IClause GroupOptional(GroupClause group, ParserContext context)
    {
        return new OptionClause(group);
    }

    #endregion


    #region attributes

    [Production(
        "attribute :  AT[d] ID LEFTPAREN[d] [ID|STRING] (COMMA[d] [ID|STRING])*  RIGHTPAREN[d] SEMICOLON[d]")]
    public ICLIModel Attribute(Token<CLIToken> attributeName, Token<CLIToken> firstAttributeValue,
        List<Group<CLIToken, ICLIModel>> attributeValues, ParserContext context)
    {
        IEnumerable<string> values = new List<string>() { firstAttributeValue.StringWithoutQuotes };
        values = values.Concat(attributeValues.Select(x => x.Token(0).StringWithoutQuotes));
        return new AttributeModel(attributeName.Value, values) { Position = attributeName.Position };
    }

    #endregion


    private IClause BuildTerminalOrNonTerminal(IdentifierOrString identifier, ParserContext context)
    {
        bool isTerminal = context.IsTerminal(identifier.Value) || identifier.TokenId == CLIToken.STRING ||
                          identifier.TokenId == CLIToken.INDENT || identifier.TokenId == CLIToken.UINDENT;

        IClause clause = null;

        if (isTerminal)
        {
            if (identifier.TokenId == CLIToken.STRING)
            {
                clause = new TerminalClause(true, identifier.StringWithoutQuotes);
                clause.Position = identifier.Position;
            }
            else
            {
                clause = new TerminalClause(false, identifier.Value);
                clause.Position = identifier.Position;
            }
        }
        else
        {
            clause = new NonTerminalClause(identifier.Value);
            clause.Position = identifier.Position;
        }


        return clause;
    }

    #endregion
}
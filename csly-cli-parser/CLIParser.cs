using clsy.cli.builder.parser.cli.model;
using clsy.cli.model.lexer;
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
        
        return new Model(genericLex as LexerModel, parser as ParserModel) ;
    }

    [Production("parserRoot : PARSER[d] ID SEMICOLON[d] parser_optimization* rule*")]
    public ICLIModel Parser(Token<CLIToken> name, List<ICLIModel> optimizations, List<ICLIModel> rules, ParserContext context)
    {
        var optims = optimizations.Cast<ParserOptimization>().ToList();
        var model = new ParserModel()
        {
            UseMemoization = optims.Exists(x => x.UseMemoization),
            BroadenTokenWindow = optims.Exists(x => x.BroadenTokenWindow),
            Name = name.Value,
            Rules = rules.Cast<Rule>().ToList()
        };
        var root = model.Rules.FirstOrDefault(x => x.IsRoot);

        return model;

    }

    [Production("parser_optimization : LEFTBRACKET[d] [USEMEMOIZATION|BROADENTOKENWINDOW] RIGHTBRACKET[d]")]
    public ICLIModel Optimization(Token<CLIToken> optimizationToken, ParserContext context)
    {
        return new ParserOptimization()
        {
            UseMemoization = optimizationToken.TokenID == CLIToken.USEMEMOIZATION,
            BroadenTokenWindow = optimizationToken.TokenID == CLIToken.BROADENTOKENWINDOW
        };
    }
    
    #endregion
    
    

   #region generic lexer
    
    [Production("genericRoot : GENERICLEXER[d] ID SEMICOLON[d] lexer_option* modedToken*")]
    public ICLIModel Lexer(Token<CLIToken> name, List<ICLIModel> optionList, List<ICLIModel> tokens, ParserContext context)
    {
        var opts = optionList.Cast<LexerOptions>();
        var options = new LexerOptions()
        {
            IgnoreWS = opts.Any(x => x.IgnoreWS),
            IndentationAware = opts.Any(x => x.IndentationAware),
            IgnoreEOL = opts.Any(x => x.IgnoreEOL),
            IgnoreKeyWordCase = opts.Any(x => x.IgnoreKeyWordCase)
        };
        return new LexerModel(tokens.Cast<TokenModel>().ToList(),options, name.Value);
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
                case ModeModel mod :
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
        return new PushModel(push.StringWithoutQuotes);
    }
    
    [Production("mode : LEFTBRACKET[d] POP[d] RIGHTBRACKET[d]")]
    public ICLIModel Pop(ParserContext context)
    {
        return new PopModel();
    }
    
    [Production("mode : LEFTBRACKET[d] MODE[d] LEFTPAREN[d] STRING (COMMA[d] STRING )* RIGHTPAREN[d] RIGHTBRACKET[d]")]
    public ICLIModel Modes(Token<CLIToken> mode, List<Group<CLIToken,ICLIModel>> modes, ParserContext context)
    {
        var mods = modes.Select(x => x.Token(0).StringWithoutQuotes).ToList();
        
        return new ModeModel(mode.StringWithoutQuotes, mods);
    }
    
    [Production("mode : LEFTBRACKET[d] MODE[d] RIGHTBRACKET[d]")]
    public ICLIModel ModeDefault(ParserContext context)
    {
        return new ModeModel(new List<string>());
    }
    

    [Production(
        "token :LEFTBRACKET[d] [KEYWORDTOKEN|SUGARTOKEN|SINGLELINECOMMENT] RIGHTBRACKET[d] ID COLON[d] STRING SEMICOLON[d]")]
    public ICLIModel OneArgToken(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> value, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.KEYWORDTOKEN => GenericToken.KeyWord,
            CLIToken.SUGARTOKEN => GenericToken.SugarToken,
            CLIToken.SINGLELINECOMMENT => GenericToken.Comment,
            _ => GenericToken.SugarToken
        };
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value,value.StringWithoutQuotes);
    }
    
    [Production(
        "token :LEFTBRACKET[d] [STRINGTOKEN|CHARTOKEN|MULTILINECOMMENT] RIGHTBRACKET[d] ID COLON[d] STRING STRING SEMICOLON[d]")]
    public ICLIModel TwoArgToken(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> arg1, Token<CLIToken> arg2, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.CHARTOKEN => GenericToken.Char,
            CLIToken.MULTILINECOMMENT => GenericToken.Comment,
            _ => GenericToken.SugarToken
        };
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value,arg1.StringWithoutQuotes.Replace("\\\\","\\"), arg2.StringWithoutQuotes.Replace("\\\\","\\"));
    }
    
    [Production(
        "token :LEFTBRACKET[d] DATETOKEN[d] RIGHTBRACKET[d] ID COLON[d] [DDMMYYYY|YYYYMMDD] CHAR SEMICOLON[d]")]
    public ICLIModel DateToken(Token<CLIToken> id, Token<CLIToken> dateType, Token<CLIToken> separator, ParserContext context)
    {
        var tokenType = GenericToken.Date;
        string format = dateType.TokenID.ToString();
            
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value,format, separator.CharValue.ToString());
    }

    [Production("token : LEFTBRACKET[d] [STRINGTOKEN|INTTOKEN|ALPHAIDTOKEN|ALPHANUMIDTOKEN|ALPHANUMDASHIDTOKEN|DOUBLETOKEN] RIGHTBRACKET[d] ID SEMICOLON[d]")]
    public ICLIModel NoArgToken(Token<CLIToken> type, Token<CLIToken> id, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.INTTOKEN => GenericToken.Int,
            CLIToken.DOUBLETOKEN => GenericToken.Double,
            CLIToken.ALPHAIDTOKEN => GenericToken.Identifier,
            CLIToken.ALPHANUMIDTOKEN => GenericToken.Identifier,
            CLIToken.ALPHANUMDASHIDTOKEN => GenericToken.Identifier,
            _ => GenericToken.SugarToken
        };
        var idType = type.TokenID switch
        {
            CLIToken.ALPHAIDTOKEN => IdentifierType.Alpha,
            CLIToken.ALPHANUMIDTOKEN => IdentifierType.AlphaNumeric,
            CLIToken.ALPHANUMDASHIDTOKEN => IdentifierType.AlphaNumericDash,
            _ => IdentifierType.Alpha
        };
        context.AddEnumName(id.Value);
        if (type.TokenID == CLIToken.STRINGTOKEN)
        {
            return new TokenModel(tokenType, id.Value,idType, "\"", "\\");
        }
        
        return new TokenModel(tokenType, id.Value, idType);
    }

    [Production("token : LEFTBRACKET[d] EXTENSIONTOKEN[d] RIGHTBRACKET[d] ID extension ")]
    public ICLIModel ExtensionToken(Token<CLIToken> id, ICLIModel extension, ParserContext context)
    {
        (extension as ExtensionTokenModel).Name = id.Value;
        context.AddEnumName(id.Value);
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

    [Production("transition_chain : (LEFTPAREN[d] ID RIGHTPAREN[d])? transition+  (ARROW ENDTOKEN)?")] // TODO SEMICOLON really useful ?
    public ICLIModel Chain(ValueOption<Group<CLIToken, ICLIModel>> startNode, List<ICLIModel> transitions, ValueOption<Group<CLIToken, ICLIModel>> end, ParserContext context)
    {
        var trans = transitions.Cast<ITransition>().ToList();
        var chain =  new TransitionChain(trans, end.IsSome);
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
    public ICLIModel Transition(ValueOption<Group<CLIToken, ICLIModel>> mark, ICLIModel pattern,  ValueOption<ICLIModel> repeater, ValueOption<Group<CLIToken, ICLIModel>> target, ParserContext context)
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
    public ICLIModel RangePattern(ICLIModel headRange, List<Group<CLIToken, ICLIModel>> tailsRanges, ParserContext context)
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
          IgnoreWS = option.TokenID == CLIToken.IGNOREWHITESPACES && enabled,
          IgnoreEOL = option.TokenID == CLIToken.IGNOREEOL && enabled,
          IgnoreKeyWordCase = option.TokenID == CLIToken.IGNOREKEYWORDCASING && enabled,
          IndentationAware = option.TokenID == CLIToken.INDENTATIONAWARE && enabled
      };
  }

  
  #endregion

  #region  parser


  [Production("operand :  LEFTBRACKET[d] OPERAND[d] RIGHTBRACKET[d]")]
  public GrammarNode Operand(ParserContext context)
  {
      return null;
  }
  

        [Production("rule  : ARROW ? operand? ID COLON[d] clause+ SEMICOLON[d]")]
        public GrammarNode Root(Token<CLIToken> root, ValueOption<ICLIModel> operand,Token<CLIToken> name, List<ICLIModel> clauses, ParserContext context)
        {
            var rule = new Rule(operand.IsSome);
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Cast<IClause>().ToList();
            rule.IsRoot = !root.IsEmpty;
            return rule;
        }
        
        // [Production("rule : LEFTBRACKET[d] OPERAND[d] RIGHTBRACKET[d] ID SEMICOLON[d]")]
        // public ICLIModel OperandRule(Token<CLIToken> id, ParserContext context)
        // {
        //     return new OperandRule(id.Value, context.IsTerminal(id.Value));
        // }
        
        [Production("rule : LEFTBRACKET[d] PREFIX[d] INT RIGHTBRACKET[d] [ID|STRING] SEMICOLON[d]")]
        public ICLIModel PrefixRule(Token<CLIToken> precedence, Token<CLIToken> id, ParserContext context)
        {
            return new PrefixRule(id.Value, id.TokenID == CLIToken.STRING, precedence.IntValue);
        }
        
        [Production("rule : LEFTBRACKET[d] POSTFIX[d] INT RIGHTBRACKET[d] [ID|STRING] SEMICOLON[d]")]
        public ICLIModel PostfixRule(Token<CLIToken> precedence, Token<CLIToken> id, ParserContext context)
        {
            return new PostfixRule(id.Value, id.TokenID == CLIToken.STRING, precedence.IntValue);
        }
        
        [Production("rule : LEFTBRACKET[d] [RIGHT|LEFT] INT RIGHTBRACKET[d] [ID|STRING] SEMICOLON[d]")]
        public ICLIModel InfixRule(Token<CLIToken> rightOrLeft, Token<CLIToken> precedence, Token<CLIToken> id, ParserContext context)
        {
            return new InfixRule(id.Value, id.TokenID == CLIToken.STRING, rightOrLeft.TokenID == CLIToken.LEFT ? Associativity.Left : Associativity.Right, precedence.IntValue);
        }


        [Production("item : [ ID | STRING ] ")]
        public IClause Clause(Token<CLIToken> item, ParserContext context)
        {
            return BuildTerminalOrNonTerminal(item, context);
        }
        
        
        #region clauses
       
        [Production("clause : item ZEROORMORE[d]")]
        public IClause ZeroMoreClause(IClause item, ParserContext context)
        {
            return new ZeroOrMoreClause(item);
        }

        [Production("clause : item ONEORMORE[d]")]
        public IClause OneMoreClause(IClause item, ParserContext context)
        {
            return new OneOrMoreClause(item);
        }

        [Production("clause : item OPTION")]
        public IClause OptionClause(IClause  item, Token<CLIToken> discarded, ParserContext context)
        {
            return new OptionClause(item);
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
        public IClause AlternateChoices(ICLIModel head, List<Group<CLIToken,ICLIModel>> tail, ParserContext context)
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
        

        [Production("clause : choiceclause OPTION[d] ")]
        public IClause ChoiceOptional(ChoiceClause choices, ParserContext context)
        {
            return new OptionClause(choices);
        }

        #endregion
        
        

        #region  groups

        [Production("clause : group")]
        public IClause GroupClause(IClause group) => group;
        
        [Production("group : LEFTPAREN[d] discardeditem* RIGHTPAREN[d] ")]
        public GroupClause Group( List<ICLIModel> clauses, ParserContext context)
        {
            var group = new GroupClause(clauses.Cast<IClause>().ToList());
            return group;;
        }

        [Production("discardeditem : item DISCARD?")]
        public ICLIModel discardedItem(ICLIModel item, Token<CLIToken> discard, ParserContext context)
        {
            if (item is TerminalClause term)
            {
                term.IsDiscarded = true;
            }
            else if( item is NonTerminalClause nonterm)
            {
                if (!discard.IsEmpty)
                {
                    throw new ArgumentException(
                        $" non terminal clause {nonterm.NonTerminalName} can not be discarded ! {discard.Position.Line}");
                }
            }

            return item;
        }
        
        
        [Production("clause : group ONEORMORE[d] ")]
        public IClause GroupOneOrMore( GroupClause group, ParserContext context)
        {
            return new OneOrMoreClause(group);
        }

        [Production("clause : group ZEROORMORE [d]")]
        public IClause GroupZeroOrMore(GroupClause group, ParserContext context)
        {
            return new ZeroOrMoreClause(group);
        }

        [Production("clause : group OPTION[d] ")]
        public IClause GroupOptional(GroupClause group, ParserContext context)
        {
            return new OptionClause(group);
        }


        

        #endregion


        


        private IClause BuildTerminalOrNonTerminal(Token<CLIToken> token, ParserContext context)
        {
            bool isTerminal = context.IsTerminal(token.Value) || token.TokenID == CLIToken.STRING;

            IClause clause = null;

            if (isTerminal)
            {

                if (token.TokenID == CLIToken.ID)
                {
                    clause =  new TerminalClause(false, token.Value);
                }
                else if (token.TokenID == CLIToken.STRING)
                {
                    clause = new TerminalClause(true,token.StringWithoutQuotes);
                }
            }
            else
            {

                clause = new NonTerminalClause(token.Value);

            }

            return clause;
        }

        #endregion
        
}
using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace csly.cli.parser;



public class CLIParser
{
    [Production("root: genericRoot parserRoot ")] 
    public ICLIModel Root(ICLIModel genericLex, ICLIModel parser, ParserContext context)
    {
        return new Model(genericLex as LexerModel, parser as ParserModel) ;
    }

    [Production("parserRoot : PARSER[d] ID SEMICOLON[d] rule*")]
    public ICLIModel Parser(Token<CLIToken> name, List<ICLIModel> rules, ParserContext context)
    {
        return new ParserModel()
        {
            Name = name.Value,
            Rules = rules.Cast<Rule>().ToList()
        };
    }
    
    #region generic lexer

  
    
    [Production("genericRoot : GENERICLEXER[d] ID SEMICOLON[d]  token*")]
    public ICLIModel Lexer(Token<CLIToken> name, List<ICLIModel> tokens, ParserContext context)
    {
        return new LexerModel(tokens.Cast<TokenModel>().ToList(), name.Value);
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
        "token :LEFTBRACKET[d] [STRINGTOKEN|MULTILINECOMMENT] RIGHTBRACKET[d] ID COLON[d] STRING STRING SEMICOLON[d]")]
    public ICLIModel TwoArgToken(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> arg1, Token<CLIToken> arg2, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.MULTILINECOMMENT => GenericToken.Comment,
            _ => GenericToken.SugarToken
        };
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value,arg1.StringWithoutQuotes, arg2.StringWithoutQuotes);
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
        return null;
    }


    [Production("extension : OPEN_EXT[d] transition* CLOSE_EXT[d]")]
    public ICLIModel Extension(List<ICLIModel> transitions, ParserContext context)
    {
        return null;
    }
    
    [Production("transition : ARROW[d] pattern repeater? (AT[d] ID)?")]
    public ICLIModel Transition(ICLIModel pattern, ValueOption<ICLIModel> repeater, ValueOption<Group<CLIToken, ICLIModel>> id, ParserContext context)
    {
        return null;
    }
    
    

    [Production("repeater : ZEROORMORE[d]")]
    public ICLIModel RepeatZeroOrMore(ParserContext context)
    {
        return null;
    }
    
    [Production("repeater : ZEROORMORE[d]")]
    public ICLIModel RepeatOneOrMore(ParserContext context)
    {
        return null;
    }
    
    [Production("repeater : LEFTCURL[d] INT RIGHTCURL[d]")]
    public ICLIModel RepeatMany(Token<CLIToken> many, ParserContext context)
    {
        return null;
    }
    

    [Production("pattern : CHAR")]
    public ICLIModel SinglePattern(Token<CLIToken> single, ParserContext context)
    {
        return null;
    }
    
    
    [Production("pattern : LEFTBRACKET[d] range (COMMA[d] range)* RIGHTBRACKET[d]")]
    public ICLIModel RangePattern(ICLIModel headRange, List<Group<CLIToken, ICLIModel>> tailsRanges, ParserContext context)
    {
        return null;
    }
    
    [Production("range : CHAR DASH[d] CHAR")]
    public ICLIModel Range(Token<CLIToken> start, Token<CLIToken> end, ParserContext context)
    {
        return null;
    }
    
    
  #endregion

  #region  parser


  [Production("operand :  LEFTBRACKET[d] OPERAND[d] RIGHTBRACKET[d]")]
  public GrammarNode Operand(ParserContext context)
  {
      return null;
  }
  

        [Production("rule  : ROOT ? operand? ID COLON[d] clause+ SEMICOLON[d]")]
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
        
        [Production("group : LEFTPAREN[d]  item* RIGHTPAREN[d] ")]
        public GroupClause Group( List<ICLIModel> clauses, ParserContext context)
        {
            var group = new GroupClause(clauses[0] as IClause);
            group.Clauses.AddRange(clauses.Skip(1).Cast<IClause>());
            return group;;
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
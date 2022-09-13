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

    [Production("parserRoot : PARSER[d] SEMICOLON[d] rule*")]
    public ICLIModel Parser(List<ICLIModel> rules, ParserContext context)
    {
        return new ParserModel()
        {
            Rules = rules.Cast<Rule>().ToList()
        };
    }
    
    #region generic lexer

  
    
    [Production("genericRoot : GENERICLEXER[d] SEMICOLON[d]  token*")]
    public ICLIModel lexer(List<ICLIModel> tokens, ParserContext context)
    {
        return new LexerModel(tokens.Cast<TokenModel>().ToList());
    }

    [Production(
        "token :LEFTBRACKET[d] [KEYWORDTOKEN|SUGARTOKEN] RIGHTBRACKET[d] ID COLON[d] STRING SEMICOLON[d]")]
    public ICLIModel Token(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> value, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.KEYWORDTOKEN => GenericToken.KeyWord,
            CLIToken.SUGARTOKEN => GenericToken.SugarToken,
            _ => GenericToken.SugarToken
        };
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value,value.StringWithoutQuotes);
    }

    [Production("token : LEFTBRACKET[d] [STRINGTOKEN|INTTOKEN|ALPHAIDTOKEN|DOUBLETOKEN] RIGHTBRACKET[d] ID SEMICOLON[d]")]
    public ICLIModel StringToken(Token<CLIToken> type, Token<CLIToken> id, ParserContext context)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.INTTOKEN => GenericToken.Int,
            CLIToken.DOUBLETOKEN => GenericToken.Double,
            CLIToken.ALPHAIDTOKEN => GenericToken.Identifier,
            _ => GenericToken.SugarToken
        };
        if (type.TokenID == CLIToken.STRINGTOKEN)
        {
            return new TokenModel(tokenType, id.Value,"\"", "\\");
        }
        context.AddEnumName(id.Value);
        return new TokenModel(tokenType,id.Value, "");
    } 
    
  #endregion

  #region  parser

 

        [Production("rule : ID COLON[d] clause+ SEMICOLON[d]")]
        public GrammarNode Root(Token<CLIToken> name, List<ICLIModel> clauses, ParserContext context)
        {
            var rule = new Rule();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Cast<IClause>().ToList();
            return rule;
        }
        
        [Production("rule : LEFTBRACKET[d] OPERAND[d] RIGHTBRACKET[d] ID SEMICOLON[d]")]
        public ICLIModel OperandRule(Token<CLIToken> id, ParserContext context)
        {
            return new OperandRule(id.Value, context.IsTerminal(id.Value));
        }
        
        [Production("rule : LEFTBRACKET[d] PREFIX[d] INT RIGHTBRACKET[d] ID SEMICOLON[d]")]
        public ICLIModel PrefixRule(Token<CLIToken> precedence, Token<CLIToken> id, ParserContext context)
        {
            return new PrefixRule(id.Value,precedence.IntValue);
        }
        
        [Production("rule : LEFTBRACKET[d] [RIGHT|LEFT] INT RIGHTBRACKET[d] ID SEMICOLON[d]")]
        public ICLIModel InfixRule(Token<CLIToken> rightOrLeft, Token<CLIToken> precedence, Token<CLIToken> id, ParserContext context)
        {
            return new InfixRule(id.Value,rightOrLeft.TokenID == CLIToken.LEFT ? Associativity.Left : Associativity.Right, precedence.IntValue);
        }
        
        
       
        [Production("clause : ID ZEROORMORE[d]")]
        public IClause ZeroMoreClause(Token<CLIToken> id, ParserContext context)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value, context);
            return new ZeroOrMoreClause(innerClause);
        }

        [Production("clause : ID ONEORMORE[d]")]
        public IClause OneMoreClause(Token<CLIToken> id, ParserContext context)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value,context);
            return new OneOrMoreClause(innerClause);
        }

        [Production("clause : ID OPTION")]
        public IClause OptionClause(Token<CLIToken> id, Token<CLIToken> discarded, ParserContext context)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value,context);
            return new OptionClause(innerClause);
        }

        [Production("clause : ID")]
        public IClause SimpleDiscardedClause(Token<CLIToken> id, ParserContext context)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, context);
            return clause;
        }

        
        [Production("clause : choiceclause")]
        public IClause AlternateClause(ChoiceClause choices)
        {
            return choices;
        }

        [Production("choiceclause : LEFTBRACKET[d]  choices RIGHTBRACKET[d]  ")]
        public IClause AlternateChoices(IClause choices)
        {            
            return choices;
        }
       
        
        [Production("choices : ID ( OR[d] ID)*")]
        public IClause ChoicesString(Token<CLIToken> head, List<Group<CLIToken,ICLIModel>> tail, ParserContext context)
        {
            var first = BuildTerminalOrNonTerimal(head.Value, context);
            var choice = new ChoiceClause(first, tail.Select(x => BuildTerminalOrNonTerimal(x.Token(0).Value,context)).ToList());
            return choice;
        }
        
        [Production("choices : ID OR choices ")]
        public IClause ChoicesMany(Token<CLIToken> head, Token<CLIToken> discardOr, ChoiceClause tail, ParserContext context)
        {
            var headClause = BuildTerminalOrNonTerimal(head.Value,context); 
            return new ChoiceClause(headClause,tail.Choices);
        }

       

        [Production("clause : ID ")]
        public IClause SimpleClause(Token<CLIToken> id, ParserContext context)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value,context);
            return clause;
        }


        #region  groups

        [Production("clause : LEFTPAREN[d]  groupclauses RIGHTPAREN[d] ")]
        public GroupClause Group( GroupClause clauses, ParserContext context)
        {
            return clauses;
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

        [Production("clause : LEFTPAREN  groupclauses RIGHTPAREN ONEORMORE ")]
        public IClause GroupOneOrMore(Token<CLIToken> discardLeft, GroupClause clauses,
            Token<CLIToken> discardRight, Token<CLIToken> oneZeroOrMore, ParserContext context)
        {
            return new OneOrMoreClause(clauses);
        }

        [Production("clause : LEFTPAREN  groupclauses RIGHTPAREN ZEROORMORE ")]
        public IClause GroupZeroOrMore(Token<CLIToken> discardLeft, GroupClause clauses,
            Token<CLIToken> discardRight, Token<CLIToken> discardZeroOrMore, ParserContext context)
        {
            return new ZeroOrMoreClause(clauses);
        }

        [Production("clause : LEFTPAREN  groupclauses RIGHTPAREN OPTION ")]
        public IClause GroupOptional(Token<CLIToken> discardLeft, GroupClause group,
            Token<CLIToken> discardRight, Token<CLIToken> option, ParserContext context)
        {
            return new OptionClause(group);
        }


        [Production("groupclauses : ID*")]
        public GroupClause GroupClauses(List<Token<CLIToken>> groups, ParserContext context)
        {
            var group = new GroupClause(BuildTerminalOrNonTerimal(groups[0].Value,context));
            group.Clauses.AddRange(groups.Skip(1).Select(x => BuildTerminalOrNonTerimal(x.Value,context)).ToList());
            return group;
        }

        #endregion


        private IClause BuildTerminalOrNonTerimal(string name, ParserContext context)
        {

            bool isTerminal = context.IsTerminal(name);

            IClause clause = null;

            if (isTerminal)
                clause = new TerminalClause(name);
            else
            {

                clause = new NonTerminalClause(name);

            }

            return clause;
        }

        #endregion
        
}
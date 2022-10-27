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
    public ICLIModel lexer(Token<CLIToken> name, List<ICLIModel> tokens, ParserContext context)
    {
        return new LexerModel(tokens.Cast<TokenModel>().ToList(), name.Value);
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
        context.AddEnumName(id.Value);
        if (type.TokenID == CLIToken.STRINGTOKEN)
        {
            return new TokenModel(tokenType, id.Value,"\"", "\\");
        }
        return new TokenModel(tokenType,id.Value, "");
    } 
    
  #endregion

  #region  parser

 

        [Production("rule  : ROOT? ID COLON[d] clause+ SEMICOLON[d]")]
        public GrammarNode Root(Token<CLIToken> root, Token<CLIToken> name, List<ICLIModel> clauses, ParserContext context)
        {
            var rule = new Rule();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Cast<IClause>().ToList();
            rule.IsRoot = !root.IsEmpty;
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


        [Production("item : [ ID | STRING ] ")]
        public IClause Clause(Token<CLIToken> item, ParserContext context)
        {
            return BuildTerminalOrNonTerminal(item, context);
        }
        
       
        [Production("clause : item ZEROORMORE[d]")]
        public IClause ZeroMoreClause(IClause item, ParserContext context)
        {
            //var innerClause = BuildTerminalOrNonTerimal(id.Value, context);
            return new ZeroOrMoreClause(item);
        }

        [Production("clause : item ONEORMORE[d]")]
        public IClause OneMoreClause(IClause item, ParserContext context)
        {
            // var innerClause = BuildTerminalOrNonTerimal(id.Value,context);
            return new OneOrMoreClause(item);
        }

        [Production("clause : item OPTION")]
        public IClause OptionClause(IClause  item, Token<CLIToken> discarded, ParserContext context)
        {
            return new OptionClause(item);
        }

        // [Production("clause : item")]
        // public IClause SimpleDiscardedClause(IClause  item, ParserContext context)
        // {
        //     return item;
        // }

        
        [Production("clause : choiceclause")]
        public IClause AlternateClause(ChoiceClause choices, ParserContext context)
        {
            return choices;
        }

        [Production("choiceclause : LEFTBRACKET[d]  choices RIGHTBRACKET[d]  ")]
        public IClause AlternateChoices(IClause choices, ParserContext context)
        {            
            return choices;
        }
       
        
        [Production("choices : item ( OR[d] item)*")]
        public IClause ChoicesString(ICLIModel head, List<Group<CLIToken,ICLIModel>> tail, ParserContext context)
        {
            // var first = BuildTerminalOrNonTerimal(head.Value, context);
            var choice = new ChoiceClause(head as IClause, tail.Select(x => x.Value(0)).Cast<IClause>().ToList());
            return choice;
        }
        
        [Production("choices : item OR[d] choices ")]
        public IClause ChoicesMany(IClause head,  ChoiceClause tail, ParserContext context)
        {
            // var headClause = BuildTerminalOrNonTerimal(head.Value,context); 
            return new ChoiceClause(head,tail.Choices);
        }

       

        [Production("clause : item ")]
        public IClause SimpleClause(IClause item, ParserContext context)
        {
            // var clause = BuildTerminalOrNonTerimal(id,context);
            return item as IClause;
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


        [Production("groupclauses : item*")]
        public GroupClause GroupClauses(List<ICLIModel> groups, ParserContext context)
        {
            var group = new GroupClause(groups[0] as IClause);
            group.Clauses.AddRange(groups.Skip(1).Cast<IClause>());
            return group;
        }

        #endregion


        // private IClause BuildTerminalOrNonTerimal(string name, ParserContext context)
        // {
        //
        //     bool isTerminal = context.IsTerminal(name);
        //
        //     IClause clause = null;
        //
        //     if (isTerminal)
        //         clause = new TerminalClause(name);
        //     else
        //     {
        //
        //         clause = new NonTerminalClause(name);
        //
        //     }
        //
        //     return clause;
        // }


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
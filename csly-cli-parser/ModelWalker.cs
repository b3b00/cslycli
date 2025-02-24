using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;
using sly.lexer;

namespace csly.cli.model;

public class ModelWalker<T>
{
    private IModelVisitor<T> _visitor;

    public ModelWalker(IModelVisitor<T> visitor)
    {
        _visitor = visitor;
    }
    public T Walk(Model model, T result)
    {
        result = Walk(model.LexerModel, result);
        result = Walk(model.ParserModel, result);
        return result;
    }

    #region lexer
    public T Walk(LexerModel lexerModel, T result)
    {
        result = _visitor.Visit(lexerModel, result);
        foreach (var token in lexerModel.Tokens)
        {
            result = Walk(token, result);
        }
        return result;
    }

    private T Walk(TokenModel token, T result)
    {
        result = _visitor.Visit(token, result);
        if (token is ExtensionTokenModel extensionTokenModel)
        {
            result = Walk(extensionTokenModel, result);
        }

        return result;
    }

   

    private T Walk(ExtensionTokenModel extension, T result)
    {
        result = _visitor.VisitExtension(extension,result);
        if (extension.Chains.Any())
        {
            foreach (var chain in extension.Chains)
            {
                result = Walk(chain, result);
            }

        }

        return result;
    }
    
    private T Walk(TransitionChain chain, T result)
    {
        result = _visitor.VisitChain(chain, result);
        if (chain.Transitions.Any())
        {
            foreach (var transition in chain.Transitions)
            {
                result = Walk(transition, result);
            }
        }
        return result;
    }
    
    private T Walk(ITransition transition, T result)
    {
        //result = _visitor.VisitTransition(transition, result);

        switch (transition)
        {
            case CharacterTransition characterTransition:
            {
                result = _visitor.VisitCharacterTransition(characterTransition, result);
                break;
            }
            case ExceptTransition exceptTransition:
            {
                result = _visitor.VisitExceptTransition(exceptTransition, result);
                break;
            }
            case RangeTransition rangeTransition:
            {
                result = _visitor.VisitRangeTransition(rangeTransition, result);
                break;
            }
        }
        
        return result;
    }
    
    

    #endregion
    
    #region parser
    public T Walk(ParserModel parserModel, T result)
    {
        result = _visitor.Visit(parserModel, result);
        foreach (var rule in parserModel.Rules)
        {
            result = Walk(rule, result);
        }

        return result;
    }

    private T Walk(Rule rule, T result)
    {
        result = _visitor.Visit(rule, result);
        foreach (var clause in rule.Clauses)
        {
            result = Walk(clause, result);
        }
        return result;
    }

    private T Walk(IClause clause, T result)
    {

        switch (clause)
        {
            case NonTerminalClause nonTerminalClause:
            {
                result = WalkNonTerminalClause(nonTerminalClause, result);
                break;
            }
            case ManyClause many:
            {
                result = WalkManyClause(many, result);
                break;
            }
            case TerminalClause terminalClause:
            {
                result = WalkTerminalClause(terminalClause, result);
                break;
            }
            case OptionClause option:
            {
                result = WalkOptionClause(option, result);
                break;
            }
            case GroupClause group:
            {
                result = WalkGroupClause(group, result);
                break;
            }
            case ChoiceClause choice:
            {
                result = WalkChoiceClause(choice, result);
                break;
            }
                
        }


        return result;
    }

    private T WalkGroupClause(GroupClause group, T result)
    {
        result = _visitor.VisitorGroupClause(group, result);
        foreach (var clause in group.Clauses)
        {
            result = Walk(clause, result);
        }
        return result;
    }

    private T WalkOptionClause(OptionClause option, T result)
    {
        result = _visitor.VisitorOptionClause(option, result);
        result = Walk(option.Clause,result);
        return result;
    }


    private T WalkTerminalClause(TerminalClause terminalClause, T result)
    {
        result = _visitor.VisitTerminalClause(terminalClause, result);
        return result;
    }

    private T WalkNonTerminalClause(NonTerminalClause nonTerminalClause, T result)
    {
        result = _visitor.VisitNonTerminalClause(nonTerminalClause, result);
        return result;
    }



    private T WalkManyClause(ManyClause many, T result)
    {
        result = _visitor.VisitManyClause(many, result);

        result = Walk(many.Clause as IClause, result);
        
        return result;
    }

    private T WalkChoiceClause(ChoiceClause choice, T result)
    {
        result = _visitor.VisitChoiceClause(choice, result);
        foreach (var clause in choice.Choices)
        {
            result = Walk(clause, result);
        }
        return result;
    }
    

    

    #endregion
}
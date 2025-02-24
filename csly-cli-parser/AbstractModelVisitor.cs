using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;

namespace csly.cli.model;

public class AbstractModelVisitor<T> : IModelVisitor<T>
{
    public virtual T Visit(LexerModel lexer, T result)
    {
        return result;
    }

    public virtual T Visit(TokenModel token, T result)
    {
        return result;
    }

    public virtual T VisitExtension(ExtensionTokenModel extension, T result)
    {
        return result;
    }

    public virtual T VisitChain(TransitionChain chain, T result)
    {
        return result;
    }

    public virtual T VisitTransition(ITransition transition, T result)
    {
        return result;
    }

    public virtual T VisitCharacterTransition(CharacterTransition characterTransition, T result)
    {
        return result;
    }

    public virtual T VisitExceptTransition(ExceptTransition exceptTransition, T result)
    {
        return result;
    }

    public virtual T VisitRangeTransition(RangeTransition rangeTransition, T result)
    {
        return result;
    }


    public virtual T Visit(ParserModel parser, T result)
    {
        return result;
    }

    public virtual T Visit(Rule rule, T result)
    {
        return result;
    }

    public virtual T Visit(IClause clause, T result)
    {
        return result;
    }

    public virtual T VisitManyClause(ManyClause many, T result)
    {
        return result;
    }

    public virtual T VisitNonTerminalClause(NonTerminalClause nonTerminalClause, T result)
    {
        return result;
    }

    public virtual T VisitTerminalClause(TerminalClause terminalClause, T result)
    {
        return result;
    }

    public virtual T VisitorOptionClause(OptionClause option, T result)
    {
        return result;
    }

    public virtual T VisitorGroupClause(GroupClause group, T result)
    {
        return result;
    }

    public virtual T VisitChoiceClause(ChoiceClause choice, T result)
    {
        return result;
    }

}
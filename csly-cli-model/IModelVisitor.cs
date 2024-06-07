using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;

namespace csly.cli.model;

public interface IModelVisitor<T>
{
    
    #region lexer

    T Visit(LexerModel lexer, T result);

    T Visit(TokenModel token, T result);

    T VisitExtension(ExtensionTokenModel extension, T result);

    T VisitChain(TransitionChain chain, T result);

    T VisitTransition(ITransition transition, T result);
    
    T VisitCharacterTransition(CharacterTransition characterTransition, T result);
    
    T VisitExceptTransition(ExceptTransition exceptTransition, T result);
    
    T VisitRangeTransition(RangeTransition rangeTransition, T result);
    
    #endregion

    #region parser

        
    T Visit(ParserModel parser, T result);

    T Visit(Rule rule, T result);

    T Visit(IClause clause, T result);

    T VisitManyClause(ManyClause many, T result);
    
    T VisitNonTerminalClause(NonTerminalClause nonTerminalClause, T result);
    
    T VisitTerminalClause(TerminalClause terminalClause, T result);
    
   T VisitorOptionClause(OptionClause option, T result);
   
    T VisitorGroupClause(GroupClause group, T result);

    T VisitChoiceClause(ChoiceClause choice, T result);

    #endregion



}
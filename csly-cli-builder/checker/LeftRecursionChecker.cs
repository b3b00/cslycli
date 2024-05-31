using csly.cli.model;
using csly.cli.model.parser;

namespace clsy.cli.builder.checker;

public class LeftRecursionChecker
{

    private readonly ParserModel _model;
        public LeftRecursionChecker(ParserModel model)
        {
            _model = model;
        }

        private static List<string> BuildPath(List<string> current, string step)
        {
            var newPath = new List<string>();
            newPath.AddRange(current);
            newPath.Add(step);
            return newPath;
        }

        public static List<string> Lst(params string[] args)
        {
            return args.ToList<string>();
        }

        public static List<string> GetLeftClausesName(IClause clause)
        {
            switch (clause)
            {
                case NonTerminalClause nonTerminal:
                    return Lst(nonTerminal.NonTerminalName);
                case ManyClause many:
                    return GetLeftClausesName(many.Clause);
                case OptionClause option:
                    return GetLeftClausesName(option.Clause);
                case ChoiceClause choice when choice.IsNonTerminalChoice:
                    return choice.Choices.SelectMany<IClause, string>(x => GetLeftClausesName(x)).ToList<string>();
                case GroupClause group:
                    return GetLeftClausesName(group.Clauses.First<IClause>());
                default:
                    return new List<string>();
            }
        }

        private List<string> GetLeftClausesName(Rule rule)
        {
            List<string> lefts = new List<string>();


            if (rule.Clauses.Any())
            {
                int i = 0;
                IClause current = rule.Clauses[0] as IClause;
                var currentLefts = GetLeftClausesName(current);
                bool stopped = false;
                while (i < rule.Clauses.Count && !stopped && currentLefts != null && currentLefts.Any<string>())
                {
                    stopped = !current.MayBeEmpty();
                    lefts.AddRange(currentLefts);
                    stopped = !current.MayBeEmpty();
                    i++;
                    if (i < rule.Clauses.Count<IClause>())
                    {
                        current = rule.Clauses[i];
                        currentLefts = GetLeftClausesName(current);
                    }
                    else
                    {
                        current = null;
                        currentLefts = null;
                    }
                }
            }
            return lefts;
        }


        public (bool foundRecursion, List<List<string>> recursions) CheckLeftRecursion()
        {
            List<List<string>> recursions = new List<List<string>>();
            bool foundRecursion = false;
            foreach (var nonTerminal in _model.GetNonTerminals())
            {
                var (found,recursion) = CheckLeftRecursion(nonTerminal, new List<string> {nonTerminal});
                if (found)
                {
                    foundRecursion = true;
                    recursions.AddRange(recursion);
                }
            }

           

            return (foundRecursion, recursions);
        }
        
        public (bool recursionFound,List<List<string>> recursion) CheckLeftRecursion(string nonTerminal, List<string> currentPath)
        {
            var foundRecursion = false;
            List<List<string>> recursions = new List<List<string>>();
            
            var (found,path) = FindRecursion(currentPath);
            if (found)
            {
                return (true,new List<List<string>> {currentPath});
            }
            
            var leftClauses = _model.GetRulesForNonTerminal(nonTerminal).SelectMany<Rule, string>(x => GetLeftClausesName(x)).ToList<string>();
            
            foreach (var leftClause in leftClauses)
            {
                // if (configuration.NonTerminals.TryGetValue(leftClause, out var newNonTerminal) && newNonTerminal != null)
                // {
                    var nPath = BuildPath(currentPath, leftClause);
                    var (foundRRuleRecursion, recursion) = CheckLeftRecursion(leftClause, nPath);
                    if (!foundRRuleRecursion)
                    {
                        continue;
                    }
                    foundRecursion = true;
                    recursions.AddRange(recursion);

                // }
            }

            return (foundRecursion, recursions);


        }

        private static (bool, string) FindRecursion(List<string> path)
        {
            for (int i = 0; i < path.Count - 1;i++)
            {
                string step = path[i];
                int next = path.LastIndexOf(step);
                if (next > i)
                {
                    string failure = string.Join(" > ",path.GetRange(i, next - i + 1));
                    return (true, failure);
                }
            }
            
            return (false, null);
        }
        
        
    }
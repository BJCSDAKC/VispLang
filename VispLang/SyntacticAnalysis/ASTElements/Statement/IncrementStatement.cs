using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class IncrementStatemnet : IStatement
    {
        public readonly string Name;
        public readonly List<IIndexator> Indexators;

        public IncrementStatemnet(string name, List<IIndexator> indexators)
        {
            Name = name;
            Indexators = indexators;
        }

    }
}

using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class DecrementStatemnet : IStatement
    {
        public readonly string Name;
        public readonly List<IIndexator> Indexators;

        public DecrementStatemnet(string name, List<IIndexator> indexators)
        {
            Name = name;
            Indexators = indexators;
        }

    }
}

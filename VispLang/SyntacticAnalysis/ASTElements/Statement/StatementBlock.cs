namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class StatementBlock : IStatement
    {
        public readonly List<IStatement> Statements;

        public StatementBlock(List<IStatement> statements)
        {
            Statements = statements;
        }
    }
}

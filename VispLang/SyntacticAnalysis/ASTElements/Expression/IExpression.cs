namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public interface IExpression
    {
        int GetLine();
        int GetColumn();
    }
}

using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class ArrayIndexator : IIndexator
    {
        public readonly IExpression Index;

        public ArrayIndexator(IExpression index)
        {
            Index = index;
        }
    }
}

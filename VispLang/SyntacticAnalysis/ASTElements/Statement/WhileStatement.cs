using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class WhileStatement : IStatement
    {
        public readonly IExpression Condition;
        public readonly IStatement Body;

        public WhileStatement(IExpression condition, IStatement body)
        {
            Condition = condition;
            Body = body;
        }
    }
}

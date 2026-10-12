using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class IfElseStatement : IStatement
    {
        public readonly IExpression Condition;
        public readonly IStatement IfBody;
        public readonly IStatement? ElseBody;

        public IfElseStatement(IExpression condition, IStatement ifBody, IStatement? elseBody = null)
        {
            Condition = condition;
            IfBody = ifBody;
            ElseBody = elseBody;
        }

    }
}

using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class ForStatement : IStatement
    {
        public readonly IStatement Initializer, Iterator, Body;
        public readonly IExpression Condition;

        public ForStatement(IStatement initializer, IStatement iterator, IStatement body, IExpression condition)
        {
            Initializer = initializer;
            Iterator = iterator;
            Body = body;
            Condition = condition;
        }
    }
}

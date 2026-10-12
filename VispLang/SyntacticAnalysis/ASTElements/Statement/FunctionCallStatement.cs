using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class FunctionCallStatement : IStatement   
    {
        public readonly string Function;
        public readonly List<IExpression> Arguments;
        public readonly List<IIndexator> Indexators;

        public FunctionCallStatement(string function, List<IExpression> arguments, List<IIndexator> indexators)
        {
            Function = function;
            Arguments = arguments;
            Indexators = indexators;
        }
    }
}

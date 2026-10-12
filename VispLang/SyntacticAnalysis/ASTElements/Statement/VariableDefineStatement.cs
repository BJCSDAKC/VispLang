using VispLang.SyntacticAnalysis.ASTElements.Expression;
using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class VariableDefineStatement : IStatement
    {
        public readonly string Name;
        public readonly IType Type;
        public readonly IExpression? Value;

        public VariableDefineStatement(IType type, string name, IExpression? value)
        {
            Type = type;
            Name = name;
            Value = value;
        }
    }
}

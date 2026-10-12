using System.Data.Common;
using VispLang.SyntacticAnalysis.ASTElements.Expression;
using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Definions
{
    public class VariableDefinion : IDefinion
    {
        public readonly string Name;
        public readonly IType Type;
        public readonly IExpression? Value;
        public readonly int Line, Column;

        public VariableDefinion(string name, IType type, IExpression? value, int line, int column)
        {
            Name = name;
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}

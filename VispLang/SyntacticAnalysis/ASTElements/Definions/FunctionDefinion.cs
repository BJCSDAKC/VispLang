using VispLang.SyntacticAnalysis.ASTElements.Statement;
using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Definions
{
    public class FunctionDefinion : IDefinion
    {
        public readonly string Name;
        public readonly IType ReturnType;
        public readonly List<(IType type, string name)> Agruments;
        public readonly IStatement Body;
        public readonly int Line, Column;

        public FunctionDefinion(string name, IType returnType, List<(IType type, string name)> arguments, IStatement body, int line, int column)
        {
            Name = name;
            ReturnType = returnType;
            Agruments = arguments;
            Body = body;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}

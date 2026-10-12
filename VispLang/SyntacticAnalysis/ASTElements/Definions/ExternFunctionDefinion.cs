using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Definions
{
    public class ExternFunctionDefinion : IDefinion
    {
        public readonly string Name;
        public readonly IType ReturnType;
        public readonly List<IType> Arguments;
        public readonly int Line, Column;

        public ExternFunctionDefinion(string name, IType returnType, List<IType> arguments, int line, int column)
        {
            Name = name;
            ReturnType = returnType;
            Arguments = arguments;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}

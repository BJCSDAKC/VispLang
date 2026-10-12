using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.CodeGeneration.NASM_x86_64
{
    public class Variable
    {
        public readonly string Name;
        public readonly IType Type;
        public readonly int Position;

        public Variable(string name, IType type, int position)
        {
            Name = name;
            Type = type;
            Position = position;
        }
    }
}

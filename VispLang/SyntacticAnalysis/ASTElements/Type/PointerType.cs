namespace VispLang.SyntacticAnalysis.ASTElements.Type
{
    public class PointerType : IType
    {
        public readonly IType Type;

        public PointerType(IType type)
        {
            Type = type;
        }

        public bool IsInteger() => false;

        public bool IsPointer() => true;

        public override string ToString() => Type + "*";
    }
}

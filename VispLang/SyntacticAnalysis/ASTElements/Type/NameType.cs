namespace VispLang.SyntacticAnalysis.ASTElements.Type
{
    public class NameType : IType
    {
        public readonly string Name;

        public NameType(string name)
        {
            Name = name;    
        }

        public bool IsInteger()
        {
            return "int" == Name;
        }

        public bool IsPointer() => false;

        public override string ToString() => Name;
    }
}

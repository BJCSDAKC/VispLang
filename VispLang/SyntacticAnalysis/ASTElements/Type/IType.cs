namespace VispLang.SyntacticAnalysis.ASTElements.Type
{
    public interface IType
    {
        bool IsInteger();
        bool IsPointer();
    }
}

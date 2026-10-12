namespace VispLang.SyntacticAnalysis.ASTElements.Type
{
    public class FunctionPointerType : IType
    {
        public readonly IType ReturnType;
        public readonly List<IType> ArgumentTypes;

        public FunctionPointerType(IType ret, List<IType> args)
        {
            ReturnType = ret;
            ArgumentTypes = args;
        }

        public override string ToString()
        {
            string res = ReturnType.ToString() + "(";
            for (int i = 0; i < ArgumentTypes.Count; i++) 
            {
                res += ArgumentTypes[i].ToString();
                if (i != ArgumentTypes.Count - 1) 
                    res += ", "; 
            }
            return res + ")";
        }

        public bool IsInteger() => false;
        public bool IsPointer() => false;
    }
}

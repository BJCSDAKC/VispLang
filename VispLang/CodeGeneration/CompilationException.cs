using VispLang.SyntacticAnalysis.ASTElements.Definions;
using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.CodeGeneration
{
    public class CompilationException : Exception
    {
        public CompilationException(string exception) : base(exception) { }
        public CompilationException(string exception, int line, int column) : base(exception + $" Строка:{line} Символ:{column}") { }
        public CompilationException(string exception, IExpression position) : base(exception + $" Строка:{position.GetLine()} Символ:{position.GetColumn()}") { }
        public CompilationException(string exception, IDefinion position) : base(exception + $" Строка:{position.GetLine()} Символ:{position.GetColumn()}") { }
    }
}

namespace VispLang.CodeGeneration.NASM_x86_64
{
    public class Loop
    {
        public readonly string StartLabel, EndLabel;
        public readonly int BreakStack, ContinueStack;

        public Loop(string startLabel, string endLabel, int breakStack, int continueStack)
        {
            StartLabel = startLabel;
            EndLabel = endLabel;
            BreakStack = breakStack;
            ContinueStack = continueStack;
        }
    }
}

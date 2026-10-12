using VispLang.CodeGeneration;
using VispLang.CodeGeneration.NASM_x86_64;
using VispLang.LexicalAnalysis;
using VispLang.SyntacticAnalysis;

namespace VispLang
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var NASMProvader = new NASMx86_64CodeGenerationProvider();
            foreach (string s in args) 
            {
                string path = s.Trim('"');
                try
                {
                    var tokens = new Lexer().Tokenize(File.ReadAllText(path));
                    var exp = new Parser().ParseFile(tokens);
                   
                    foreach (var def in exp)
                        NASMProvader.CompileDefinion(def);

                }
                catch (CompilationException ce) { Console.WriteLine(ce.Message + $" Файл: {path}"); return; }
                catch (FileNotFoundException) { Console.WriteLine($"Файл {path} не найден."); }
                catch { Console.WriteLine("Неизвестная ошибка."); }

            }
            if(args.Length != 0)
            {
                if (!File.Exists("result.asm")) 
                    File.Create("result.asm");
                File.WriteAllText("result.asm", NASMProvader.GetCompiledCode());
            }
        }
    }
}

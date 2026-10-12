using System.Text;
using VispLang.SyntacticAnalysis.ASTElements.Definions;
using VispLang.SyntacticAnalysis.ASTElements.Expression;
using VispLang.SyntacticAnalysis.ASTElements.Statement;
using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.CodeGeneration.NASM_x86_64
{
    public class NASMx86_64CodeGenerationProvider : ICodeGenerationProvider
    {
        private StringBuilder NasmTextSection, NasmDataSection, NasmEntry, NasmExtern;
        private int StackPointer = 0;
        private int LabelCounter = 0;
        private int StringCounter = 0;
        private List<Variable> Variables = [];
        private Stack<Loop> Loops = [];

        private List<FunctionDefinion> FunctionDefinions = [];
        private List<VariableDefinion> VariableDefinions = [];

        private IType CurrentFunctionReturnType = null;
        private int CurrentReturnAdress = 0;

        public NASMx86_64CodeGenerationProvider() 
        {
            NasmTextSection = new StringBuilder();
            NasmTextSection.Append("section .text\n\tglobal entry\n\tbits 64\n\n");
            NasmDataSection = new StringBuilder();
            NasmDataSection.Append("section .data\n");
            NasmEntry = new StringBuilder();
            NasmEntry.AppendLine("entry:");
            NasmExtern = new StringBuilder();
            NasmExtern.AppendLine("\nextern ExitProcess");
        }

        private int GetSize(IType type)
        {
            if (type is PointerType)
                return 8;
            if (type is FunctionPointerType)
                return 8;
            if (type is NameType nt)
            {
                switch (nt.Name) 
                {
                    case "void": return 0;
                    case "int": return 8;
                }
            }
            throw new Exception();
        }

        private bool IsEquals(IType left, IType right)
        {
            switch (left)
            {
                case NameType nt:
                    if(right is NameType rnt)
                        return nt.Name == rnt.Name;
                    return false;
                case PointerType pt:
                    if (right is PointerType rpt)
                        return IsEquals(pt.Type, rpt.Type);
                    return false;
            }
            return false;
        }

        private string GetLabel() 
        {
            return ".l" + (LabelCounter++);
        }

        private string GetSringLabel()
        {
            return "_str" + StringCounter++;
        }

        public void CompileDefinion(IDefinion definion)
        {
            if(definion is ExternFunctionDefinion efd)
            {
                FunctionDefinions.Add(new FunctionDefinion(efd.Name, efd.ReturnType, efd.Arguments.Select(x => (x, "")).ToList(), null!, efd.Line, efd.Column));
                NasmExtern.AppendLine("extern _" + efd.Name);
            }
            if(definion is FunctionDefinion fd)
            {
                CurrentFunctionReturnType = fd.ReturnType;
                FunctionDefinions.Add(fd);
                StackPointer += (fd.Agruments.Count + 1) * 8;
                for (int i = 0; i < fd.Agruments.Count; i++)
                    Variables.Add(new Variable(fd.Agruments[i].name, fd.Agruments[i].type, (i + 1) * 8));
                int stack = StackPointer;
                CurrentReturnAdress = StackPointer;
                NasmTextSection.AppendLine($"_{fd.Name}:");
                CompileStatement(fd.Body);
                if (StackPointer > stack)
                {
                    NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                    StackPointer = stack;
                }
                NasmTextSection.AppendLine($"\tret {fd.Agruments.Count * 8}");
                Variables.Clear();
                Loops.Clear();
            }
            if(definion is VariableDefinion vd)
            {
                if (VariableDefinions.Any(x => x.Name == vd.Name)) throw new CompilationException("Перемнная с таким названием уже существует", vd);
                VariableDefinions.Add(vd);
                NasmDataSection.AppendLine($"\t_{vd.Name}: dq 0");
                if (vd.Value != null)
                {
                    CompileExpression(vd.Value, NasmEntry);
                    NasmEntry.AppendLine("\tpop rax");
                    StackPointer -= 8;
                    NasmEntry.AppendLine($"\tmov qword [_{vd.Name}], rax");
                }
            }
        }

        public IType CompileExpression(IExpression expression, StringBuilder sb)
        {
            if(expression is NumberExpression ne)
            {
                sb.AppendLine($"\tpush qword {ne.Value}");
                StackPointer += 8;
                return new NameType("int");
            }
            if(expression is UnaryExpression ue) 
            {
                IType t = CompileExpression(ue.Expression, sb);
                sb.AppendLine("\tpop rax");
                StackPointer -= 8;
                switch (ue.Operation)
                {
                    case UnaryOperation.Neg:
                        if(!t.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражению с типом {t}", ue);
                        sb.AppendLine("\tneg rax");
                        break;
                    case UnaryOperation.BitNot:
                        if (!t.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражению с типом {t}", ue);
                        sb.AppendLine("\tnot rax");
                        break;
                    case UnaryOperation.LogicNot:
                        if (!t.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражению с типом {t}", ue);
                        sb.AppendLine("\ttest rax, rax");
                        sb.AppendLine("\tsetz al");
                        sb.AppendLine("\tmovzx rax, al");
                        break;
                    case UnaryOperation.PrefixIncrement:
                        if (t.IsInteger())
                        {
                            sb.AppendLine("\tinc rax");
                            break;
                        }
                        if (t is PointerType pti)
                        {
                            sb.AppendLine($"\tadd rax, {GetSize(pti.Type)}");
                            break;
                        }
                        throw new CompilationException($"Нельзя применить этот оператор к выражению с типом {t}", ue);
                    case UnaryOperation.PrefixDecrement:
                        if (t.IsInteger())
                        {
                            sb.AppendLine("\tdec rax");
                            break;
                        }
                        if (t is PointerType ptd)
                        {
                            sb.AppendLine($"\tsub rax, {GetSize(ptd.Type)}");
                            break;
                        }
                        throw new CompilationException($"Нельзя применить этот оператор к выражению с типом {t}", ue);
                }
                sb.AppendLine("\tpush rax");
                StackPointer += 8;
                return t;
            }
            if(expression is BinaryExpression be)
            {
                IType ltype = CompileExpression(be.Left, sb);
                IType rtype = CompileExpression(be.Right, sb);
                sb.AppendLine("\tpop rbx");
                StackPointer -= 8;
                sb.AppendLine("\tpop rax");
                StackPointer -= 8;
                IType? restype = null;
                switch (be.Operation)
                {
                    case BinaryOperation.Add:
                        if (ltype.IsInteger() && rtype.IsInteger())
                        {
                            sb.AppendLine("\tadd rax, rbx");
                            restype = new NameType("int");
                            break;
                        }
                        if (ltype is PointerType pta && rtype.IsInteger())
                        {
                            sb.AppendLine("\tmov rcx, rax");
                            sb.AppendLine("\tmov rax, rbx");
                            sb.AppendLine($"\timul rax, {GetSize(pta.Type)}");
                            sb.AppendLine("\tadd rax, rcx");
                            restype = pta;
                            break;
                        }
                        throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                    case BinaryOperation.Sub:
                        if (ltype.IsInteger() && rtype.IsInteger())
                        {
                            sb.AppendLine("\tsub rax, rbx");
                            restype = new NameType("int");
                            break;
                        }
                        if (ltype is PointerType pts && rtype.IsInteger())
                        {
                            sb.AppendLine("\tmov rcx, rax");
                            sb.AppendLine("\tmov rax, rbx");
                            sb.AppendLine($"\timul rax, {GetSize(pts.Type)}");
                            sb.AppendLine("\tsub rcx, rax");
                            sb.AppendLine("\tmov rax, rcx");
                            restype = pts;
                            break;
                        }
                        throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                    case BinaryOperation.Mul:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\timul rbx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.Div:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\txor rdx, rdx");
                        sb.AppendLine("\tdiv rbx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.Rem:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\txor rdx, rdx");
                        sb.AppendLine("\tdiv rbx");
                        sb.AppendLine("\tmov rax, rdx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.Equals:
                        if (!(ltype.IsInteger() && rtype.IsInteger()) && !(ltype.IsPointer() && rtype.IsPointer())) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsete al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = ltype;
                        break;
                    case BinaryOperation.NotEquals:
                        if (!(ltype.IsInteger() && rtype.IsInteger()) && !(ltype.IsPointer() && rtype.IsPointer())) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsetne al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = ltype;
                        break;
                    case BinaryOperation.GreatThan:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsetg al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.GreatThanOrEqual:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new Exception();
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsetge al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.LessThan:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsetl al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.LessThanOrEqual:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tcmp rax, rbx");
                        sb.AppendLine("\tsetle al");
                        sb.AppendLine("\tmovzx rax, al");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.BitAnd:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tand rax, rbx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.BitOr:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tor rax, rbx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.BitXor:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\txor rax, rbx");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.LogicalOr:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\tor rax, rbx");
                        sb.AppendLine("\tsetnz al");
                        sb.AppendLine("\tmoxzx rax, al");
                        restype = new NameType("int");
                        break;
                    case BinaryOperation.LogicalAnd:
                        if (!ltype.IsInteger() || !rtype.IsInteger()) throw new CompilationException($"Нельзя применить этот оператор к выражениям с типами {ltype} и {rtype}", be);
                        sb.AppendLine("\ttest rax, rax");
                        sb.AppendLine("\tsetnz al");
                        sb.AppendLine("\ttest rbx, rbx");
                        sb.AppendLine("\tsetnz bl");
                        sb.AppendLine("\tand al, bl");
                        sb.AppendLine("\tmoxzx rax, al");
                        restype = new NameType("int");
                        break;
                }
                sb.AppendLine("\tpush rax");
                StackPointer += 8;
                return restype!;
            }
            if(expression is SizeOfExpression soe)
            {
                StackPointer += 8;
                sb.AppendLine($"\tpush qword {GetSize(soe.Type)}");
                return new NameType("int");
            }
            if(expression is VariableExpression ve)
            {
                if (Variables.Any(x => x.Name == ve.Value))
                {
                    Variable variable = Variables.FindLast(x => x.Name == ve.Value)!;
                    int variablepos = variable.Position;
                    if (variablepos == StackPointer)
                        sb.AppendLine("\tmov rax, qword [rsp]");
                    else
                        sb.AppendLine($"\tmov rax, qword [rsp + {StackPointer - variablepos}]");
                    sb.AppendLine("\tpush rax");
                    StackPointer += 8;
                    return variable.Type;
                }
                if(VariableDefinions.Any(x => x.Name == ve.Value))
                {
                    VariableDefinion vd = VariableDefinions.FindLast(x => x.Name == ve.Value)!;
                    sb.AppendLine($"\tmov rax, qword [_{ve.Value}]");
                    sb.AppendLine("\tpush rax");
                    StackPointer += 8;
                    return vd.Type;
                }
                throw new CompilationException($"Не существует переменной с именем {ve.Value}.", ve);
            }
            if(expression is AdressOfExpression aoe)
            {
                if(aoe.Expression is not VariableExpression vare) throw new CompilationException($"Этот оператор применяется только к перемнным.", aoe);
                if (Variables.Any(x => x.Name == vare.Value))
                {
                    Variable vari = Variables.FindLast(x => x.Name == vare.Value)!;
                    if (StackPointer == vari.Position)
                        sb.AppendLine("\tlea rax, [rsp]");
                    else
                        sb.AppendLine($"\tlea rax, [rsp + {StackPointer - vari.Position}]");
                    sb.AppendLine("\tpush rax");
                    StackPointer += 8;
                    return new PointerType(vari.Type);
                }
                if(VariableDefinions.Any(x => x.Name == vare.Value))
                {
                    VariableDefinion vard = VariableDefinions.FindLast(x => x.Name == vare.Value)!;
                    sb.AppendLine($"\tmov rax, _{vard.Name}");
                    sb.AppendLine($"\tpush rax");
                    StackPointer += 8;
                    return new PointerType(vard.Type);
                }
                throw new CompilationException($"Не существует переменной с именем {vare.Value}.", vare);
            }
            if(expression is DereferenceExpression de)
            {
                IType type = CompileExpression(de.Expression, sb);
                if(!type.IsPointer()) throw new Exception();
                sb.AppendLine("\tpop rax");
                sb.AppendLine("\tmov rax, [rax]");
                sb.AppendLine("\tpush rax");
                return (type as PointerType)!.Type;
            }
            if(expression is FunctionCallExpression fce) 
            {
                int stack = StackPointer;
                int align = StackPointer % 16;
                if (align != 0)
                    sb.AppendLine($"\tsub rsp, {align}");
                if(fce.Function is VariableExpression fve)
                {
                    if(FunctionDefinions.Any(x => x.Name == fve.Value))
                    {
                        FunctionDefinion func = FunctionDefinions.FindLast(x => x.Name == fve.Value)!;
                        if (fce.Arguments.Count != func.Agruments.Count) throw new CompilationException("Несоответсвие между количеством аргументов.", fve);
                        for (int i = 0; i < fce.Arguments.Count; i++) 
                        {
                            IType t = CompileExpression(fce.Arguments[i], sb);
                            if (!IsEquals(t, func.Agruments[i].type)) throw new CompilationException($"Аргумент под номером {i+1} не соответствует ожидаемому типу.", fve);
                        }
                        sb.AppendLine($"\tcall _{func.Name}");
                        if (align != 0)
                            sb.AppendLine($"\tadd rsp, {align}");
                        sb.AppendLine("\tpush rax");
                        StackPointer = stack + 8;
                        return func.ReturnType;
                    }
                }
                List<IType> argTypes = [];
                for (int i = 0; i < fce.Arguments.Count; i++)
                    argTypes.Add(CompileExpression(fce.Arguments[i], sb));
                IType funcType = CompileExpression(fce.Function, sb);
                if (funcType is not FunctionPointerType fpt) throw new CompilationException("Выражение должно быть указателем на функцию.", fce);
                if (fce.Arguments.Count != fpt.ArgumentTypes.Count) throw new CompilationException("Несоответсвие между количеством аргументов.", fce);
                for (int i = 0; i < fce.Arguments.Count; i++)
                    if (!IsEquals(argTypes[i], fpt.ArgumentTypes[i])) throw new CompilationException($"Аргумент под номером {i + 1} не соответствует ожидаемому типу.", fce);
                sb.AppendLine("\tpop rax");
                sb.AppendLine("\tcall rax");
                if (align != 0)
                    sb.AppendLine($"\tadd rsp, {align}");
                sb.AppendLine("\tpush rax");
                StackPointer = stack + 8;
                return fpt.ReturnType;
            }
            if(expression is CastExpression ce)
            {
                CompileExpression(ce.Expression, sb);
                return ce.Type;
            }
            if(expression is StringExpression se)
            {
                string strlabel = GetSringLabel();
                NasmDataSection.Append($"\t{strlabel}: db ");
                for (int i = 0; i < se.Value.Length; i++) 
                {
                    char current = se.Value[i];
                    if (current != '\\')
                    {
                        NasmDataSection.Append($"{current & 0xff}, {(current & 0x00ff) >> 8}, ");
                    }
                    else
                    {
                        if (i == se.Value.Length - 1) throw new CompilationException("Нераспознанная escape-последовательность в конце строки.", se);
                        switch (se.Value[i + 1])
                        {
                            case '0': NasmDataSection.Append("0, 0, "); break;
                            case 't': NasmDataSection.Append("9, 0, "); break;
                            case 'n': NasmDataSection.Append("10, 0, "); break;
                            case 'r': NasmDataSection.Append("13, 0, "); break;
                            case '\\': NasmDataSection.Append("92, 0, "); break;
                        }
                        i++;
                    }
                }
                NasmDataSection.Append("0, 0\n");
                sb.AppendLine($"\tpush qword {strlabel}");
                StackPointer += 8;
                return new PointerType(new NameType("int"));
            }
            if(expression is IndexExpression ie)
            {
                IType artype = CompileExpression(ie.Array, sb);
                IType idxtype = CompileExpression(ie.Index, sb);
                if (!idxtype.IsInteger()) throw new CompilationException("Неприемлимый тип для индека.", ie);
                if (artype is not PointerType pt) throw new CompilationException("Выражение обязано быть указателем.", ie);
                sb.AppendLine("\tpop rax");
                StackPointer -= 8;
                sb.AppendLine($"\timul rax, rax, {GetSize(pt.Type)}");
                sb.AppendLine("\tadd rax, qword [rsp]");
                sb.AppendLine("\tmov rax, qword [rax]");
                sb.AppendLine("\tmov qword [rsp], rax");
                return pt.Type;
            }
            throw new Exception();
        }

        public void CompileStatement(IStatement statement)
        {
            if(statement is StatementBlock sb) 
            {
                int stack = StackPointer;
                foreach (var st in sb.Statements)
                    CompileStatement(st);
                Variables.RemoveAll(x => x.Position > stack);
                if (StackPointer > stack) 
                {
                    NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                    StackPointer = stack;
                }
                return;
            }
            if(statement is VariableDefineStatement vds)
            {
                if (Variables.Any(x => x.Name == vds.Name)) throw new Exception();
                if (vds.Value != null)
                {
                    IType type = CompileExpression(vds.Value, NasmTextSection);
                    if(!IsEquals(type, vds.Type)) throw new Exception();
                }
                else
                {
                    NasmTextSection.AppendLine($"\tpush qword 0");
                    StackPointer += 8;
                }
                Variables.Add(new(vds.Name, vds.Type, StackPointer));
                return;
            }
            if(statement is IfElseStatement ies)
            {
                IType type = CompileExpression(ies.Condition, NasmTextSection);
                if (!IsEquals(type, new NameType("int"))) throw new Exception();
                if(ies.ElseBody == null)
                {
                    string finishlabel = GetLabel();
                    NasmTextSection.AppendLine("\tpop rax");
                    StackPointer -= 8;
                    NasmTextSection.AppendLine("\ttest rax, rax");
                    NasmTextSection.AppendLine($"\tjz {finishlabel}");
                    int stack = StackPointer;
                    CompileStatement(ies.IfBody);
                    Variables.RemoveAll(x => x.Position > stack);
                    if (StackPointer > stack)
                    {
                        NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                        StackPointer = stack;
                    }
                    NasmTextSection.AppendLine(finishlabel + ":");
                }
                else
                {
                    string elselabel = GetLabel();
                    string finishlabel = GetLabel();
                    NasmTextSection.AppendLine("\tpop rax");
                    StackPointer -= 8;
                    NasmTextSection.AppendLine("\ttest rax, rax");
                    NasmTextSection.AppendLine($"\tjz {elselabel}");
                    int stack = StackPointer;
                    CompileStatement(ies.IfBody);
                    Variables.RemoveAll(x => x.Position > stack);
                    if (StackPointer > stack)
                    {
                        NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                        StackPointer = stack;
                    }
                    NasmTextSection.AppendLine($"\tjmp {finishlabel}");
                    NasmTextSection.AppendLine(elselabel + ":");
                    stack = StackPointer;
                    Variables.RemoveAll(x => x.Position > stack);
                    if (StackPointer > stack)
                    {
                        NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                        StackPointer = stack;
                    }
                    CompileStatement(ies.ElseBody);
                    NasmTextSection.AppendLine(finishlabel + ":");
                }
                return;
            }
            if(statement is WhileStatement ws)
            {
                string startlabel = GetLabel();
                string endlabel = GetLabel();
                int stack = StackPointer;
                Loops.Push(new(startlabel, endlabel, stack, stack));
                NasmTextSection.AppendLine(startlabel + ":");
                IType type = CompileExpression(ws.Condition, NasmTextSection);
                if (!IsEquals(type, new NameType("int"))) throw new Exception();
                NasmTextSection.AppendLine("\tpop rax");
                StackPointer -= 8;
                NasmTextSection.AppendLine("\ttest rax, rax");
                NasmTextSection.AppendLine("\tjz " + endlabel);
                CompileStatement(ws.Body);
                NasmTextSection.AppendLine("\tjmp " + startlabel);
                NasmTextSection.AppendLine(endlabel + ":");
                Variables.RemoveAll(x => x.Position > stack);
                if (StackPointer > stack)
                {
                    NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - stack}");
                    StackPointer = stack;
                }
                Loops.Pop();
                return;
            }
            if(statement is ForStatement fs)
            {
                string startlabel = GetLabel();
                string endlabel = GetLabel();
                int startstack = StackPointer;
                CompileStatement(fs.Initializer);
                int loopstack = StackPointer;
                Loops.Push(new(startlabel, endlabel, startstack, loopstack));
                NasmTextSection.AppendLine(startlabel + ":");
                IType t = CompileExpression(fs.Condition, NasmTextSection);
                if(!t.IsInteger()) throw new Exception();
                NasmTextSection.AppendLine("\tpop rax");
                StackPointer -= 8;
                NasmTextSection.AppendLine("\ttest rax, rax");
                NasmTextSection.AppendLine("\tjz " + endlabel);
                CompileStatement(fs.Body);
                CompileStatement(fs.Iterator);
                NasmTextSection.AppendLine("\tjmp " + startlabel);
                NasmTextSection.AppendLine(endlabel + ":");
                Variables.RemoveAll(x => x.Position > startstack);
                if (StackPointer > startstack)
                {
                    NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - startstack}");
                    StackPointer = startstack;
                }
                Loops.Pop();
                return;
            }
            if(statement is ContinueStatement cs)
            {
                if (Loops.Count == 0) throw new Exception();
                Loop loop = Loops.Peek();
                NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - loop.ContinueStack}");
                NasmTextSection.AppendLine("\tjmp " + loop.StartLabel);
                return;
            }
            if(statement is BreakStatement bs)
            {
                if (Loops.Count == 0) throw new Exception();
                Loop loop = Loops.Peek();
                NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - loop.BreakStack}");
                NasmTextSection.AppendLine("\tjmp " + loop.EndLabel);
                return;
            }
            if(statement is AssignValueStatement avs)
            {
                IType type = CompileExpression(avs.Value, NasmTextSection);
                IType vartype;
                if (Variables.Any(x => x.Name == avs.Variable))
                {
                    Variable variable = Variables.FindLast(x => x.Name == avs.Variable)!;
                    vartype = variable.Type;
                    if (StackPointer == variable.Position)
                        NasmTextSection.AppendLine("\tlea rdi, [rsp]");
                    else
                        NasmTextSection.AppendLine($"\tlea rdi, [rsp + {StackPointer - variable.Position}]");
                }
                else if (VariableDefinions.Any(x => x.Name == avs.Variable))
                {
                    VariableDefinion vdf = VariableDefinions.FindLast(x => x.Name == avs.Variable)!;
                    NasmTextSection.AppendLine($"\tmov rdi, _{vdf.Name}");
                    vartype = vdf.Type;
                }
                else throw new CompilationException("Присвоение значения неизвестной переменной.", avs.Line, avs.Column);
                
                for (int i = 0; i < avs.Indexators.Count; i++)
                {
                    if (vartype is not PointerType ptr) throw new Exception();
                    vartype = ptr.Type;
                    if (avs.Indexators[i] is DereferenceIndexator di)
                        NasmTextSection.AppendLine($"\tmov rdi, [rdi]");
                    if (avs.Indexators[i] is ArrayIndexator ai)
                    {
                        if (!CompileExpression(ai.Index, NasmTextSection).IsInteger()) throw new Exception();
                        NasmTextSection.AppendLine("\tpop rax");
                        StackPointer -= 8;
                        NasmTextSection.AppendLine($"\timul rax, rax, {GetSize(vartype)}");
                        NasmTextSection.AppendLine("\tadd rdi, rax");
                    }
                }
                if (!IsEquals(vartype, type)) throw new CompilationException("Тип переменной не соответствкет типу выражения.", avs.Line, avs.Column);
                NasmTextSection.AppendLine("\tpop rax");
                StackPointer -= 8;
                NasmTextSection.AppendLine("\tmov qword [rdi], rax");
                return;
            }
            if(statement is IncrementStatemnet ist)
            {
                IType vartype;
                if (Variables.Any(x => x.Name == ist.Name))
                {
                    Variable variable = Variables.FindLast(x => x.Name == ist.Name)!;
                    int variablepos = variable.Position;
                    vartype = variable.Type;
                    if (variablepos == StackPointer)
                        NasmTextSection.AppendLine("\tlea rdi, [rsp]");
                    else
                        NasmTextSection.AppendLine($"\tlea rdi, [rsp + {StackPointer - variablepos}]");
                }
                else if (VariableDefinions.Any(x => x.Name == ist.Name))
                {
                    VariableDefinion vard = VariableDefinions.FindLast(x => x.Name == ist.Name)!;
                    vartype = vard.Type;
                    NasmTextSection.AppendLine($"\tmov rdi, _{vard.Name}");
                }
                else throw new Exception();
                for (int i = 0; i < ist.Indexators.Count; i++)
                {
                    if (vartype is not PointerType ptr) throw new Exception();
                    vartype = ptr.Type;
                    if (ist.Indexators[i] is DereferenceIndexator di)
                        NasmTextSection.AppendLine($"\tmov rdi, [rdi]");
                    if (ist.Indexators[i] is ArrayIndexator ai)
                    {
                        if (!CompileExpression(ai.Index, NasmTextSection).IsInteger()) throw new Exception();
                        NasmTextSection.AppendLine("\tpop rax");
                        StackPointer -= 8;
                        NasmTextSection.AppendLine($"\timul rax, rax, {GetSize(vartype)}");
                        NasmTextSection.AppendLine("\tadd rdi, rax");
                    }
                }

                if (vartype.IsInteger())
                    NasmTextSection.AppendLine("\tinc qword [rdi]");
                else if (vartype is PointerType pt)
                    NasmTextSection.AppendLine($"\tadd qword [rdi], {GetSize(vartype)}");
                else throw new Exception();
                return;
            }    
            if(statement is DecrementStatemnet dst)
            {
                IType vartype;
                if (Variables.Any(x => x.Name == dst.Name))
                {
                    Variable variable = Variables.FindLast(x => x.Name == dst.Name)!;
                    int variablepos = variable.Position;
                    vartype = variable.Type;
                    if (variablepos == StackPointer)
                        NasmTextSection.AppendLine("\tlea rdi, [rsp]");
                    else
                        NasmTextSection.AppendLine($"\tlea rdi, [rsp + {StackPointer - variablepos}]");
                }
                else if (VariableDefinions.Any(x => x.Name == dst.Name))
                {
                    VariableDefinion vard = VariableDefinions.FindLast(x => x.Name == dst.Name)!;
                    vartype = vard.Type;
                    NasmTextSection.AppendLine($"\tmov rdi, _{vard.Name}");
                }
                else throw new Exception();
                for (int i = 0; i < dst.Indexators.Count; i++)
                {
                    if (vartype is not PointerType ptr) throw new Exception();
                    vartype = ptr.Type;
                    if (dst.Indexators[i] is DereferenceIndexator di)
                        NasmTextSection.AppendLine($"\tmov rdi, [rdi]");
                    if (dst.Indexators[i] is ArrayIndexator ai)
                    {
                        if (!CompileExpression(ai.Index, NasmTextSection).IsInteger()) throw new Exception();
                        NasmTextSection.AppendLine("\tpop rax");
                        StackPointer -= 8;
                        NasmTextSection.AppendLine($"\timul rax, rax, {GetSize(vartype)}");
                        NasmTextSection.AppendLine("\tadd rdi, rax");
                    }
                }
                if (vartype.IsInteger())
                    NasmTextSection.AppendLine("\tdec qword [rdi]");
                else if (vartype is PointerType pt)
                    NasmTextSection.AppendLine($"\tsub qword [rdi], {GetSize(vartype)}");
                else throw new Exception();
                return;
            }
            if(statement is FunctionCallStatement fcs)
            {
                int stack = StackPointer;
                int align = StackPointer % 16;
                if (align != 0)
                    NasmTextSection.AppendLine($"\tsub rsp, {align}");
                StackPointer += align;
                if (Variables.Any(x => x.Name == fcs.Function))
                {
                    Variable variable = Variables.FindLast(x => x.Name == fcs.Function)!;
                    IType vartype = variable.Type;
                    int variablepos = variable.Position;
                    if (variablepos == StackPointer)
                        NasmTextSection.AppendLine("\tlea rdi, [rsp]");
                    else
                        NasmTextSection.AppendLine($"\tlea rdi, [rsp + {StackPointer - variablepos}]");
                    for (int i = 0; i < fcs.Indexators.Count; i++)
                    {
                        if (vartype is not PointerType ptr) throw new Exception();
                        vartype = ptr.Type;
                        if (fcs.Indexators[i] is DereferenceIndexator di)
                            NasmTextSection.AppendLine($"\tmov rdi, [rdi]");
                        if (fcs.Indexators[i] is ArrayIndexator ai)
                        {
                            if (!CompileExpression(ai.Index, NasmTextSection).IsInteger()) throw new Exception();
                            NasmTextSection.AppendLine("\tpop rax");
                            StackPointer -= 8;
                            NasmTextSection.AppendLine($"\timul rax, rax, {GetSize(vartype)}");
                            NasmTextSection.AppendLine("\tadd rdi, rax");
                        }
                        NasmTextSection.AppendLine($"\tmov rdi, [rdi]");
                    }
                    if (vartype is not FunctionPointerType fpt) throw new Exception();
                    if (fcs.Arguments.Count != fpt.ArgumentTypes.Count) throw new Exception();
                    for (int i = 0; i < fcs.Arguments.Count; i++)
                        if (!IsEquals(CompileExpression(fcs.Arguments[i], NasmTextSection), fpt.ArgumentTypes[i]))
                            throw new Exception();
                    NasmTextSection.AppendLine("\tcall rdi"); 
                    if (align != 0)
                        NasmTextSection.AppendLine($"\tadd rsp, {align}");
                    StackPointer = stack;
                    return;
                }
                if(FunctionDefinions.Any(x => x.Name == fcs.Function))
                {
                    FunctionDefinion func = FunctionDefinions.FindLast(x => x.Name == fcs.Function)!;
                    if (fcs.Arguments.Count != func.Agruments.Count) throw new Exception();
                    for (int i = 0; i < fcs.Arguments.Count; i++)
                    {
                        IType t = CompileExpression(fcs.Arguments[i], NasmTextSection);
                        if (!IsEquals(t, func.Agruments[i].type)) throw new Exception();
                    }
                    NasmTextSection.AppendLine($"\tcall _{func.Name}");
                    if (align != 0)
                        NasmTextSection.AppendLine($"\tadd rsp, {align}");
                    StackPointer = stack;
                    return;
                }
                throw new Exception();
            }
            if(statement is ReturnStatement rs)
            {
                if (rs.Expression != null)
                {
                    IType type = CompileExpression(rs.Expression, NasmTextSection);
                    if (!IsEquals(type, CurrentFunctionReturnType)) throw new Exception();
                    NasmTextSection.AppendLine("\tpop rax");
                    StackPointer -= 8;
                }
                else
                {
                    if (!IsEquals(new NameType("void"), CurrentFunctionReturnType)) throw new Exception();
                    if (StackPointer > CurrentReturnAdress)
                        NasmTextSection.AppendLine($"\tadd rsp, {StackPointer - CurrentReturnAdress}");
                    NasmTextSection.AppendLine($"\tret {CurrentReturnAdress - 8}");
                }
                return;
            }
        }

        public string GetCompiledCode()
        {
            if (FunctionDefinions.Any(x => x.Name == "main"))
            {
                NasmEntry.AppendLine("\tsub rsp, 8");
                NasmEntry.AppendLine("\tcall _main");
                NasmEntry.AppendLine("\tmov rcx, rax");
                NasmEntry.AppendLine("\tsub rsp, 32");
                NasmEntry.AppendLine("\tcall ExitProcess");
            }
            else
                throw new CompilationException("Не удалость найти точку входа.");
            return NasmDataSection.ToString() + '\n' + NasmTextSection.ToString() + NasmEntry.ToString() + NasmExtern.ToString();
        }
    }
}

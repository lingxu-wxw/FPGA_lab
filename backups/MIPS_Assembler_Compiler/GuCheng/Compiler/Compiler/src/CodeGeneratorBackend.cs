using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    namespace CodeGeneratorBackend
    {
        

        class Param
        {
            public Param(int bit_start, int bit_end,int value)
            {
                _bit_end = bit_end;
                _bit_start = bit_start;
                Val = value;
            }
            public int Val { get; set; }
            public Param Clone() { return new Param(_bit_start, _bit_end,Val); }
            public int Through(int source) { return Util.binary_set(source, Val, _bit_start, _bit_end); }
            private int _bit_start;
            private int _bit_end;
        }

        class MIPSInstruction
        {
            private const int maxParams = 6;
            public MIPSInstruction(string name,Param[] ParamList,Serializer serializer)
            {
                _name = name;
                _serializer = serializer;
                _paramList = new Param[maxParams];
                for (int i = 0; i < maxParams; ++i)
                    if (ParamList[i] != null)
                        _paramList[i] = ParamList[i].Clone();
                    else
                        _paramList[i] = null;
            }
            public int getInstruction()
            {
                int result = 0;
                for (int i = 0; i < maxParams; ++i)
                {
                    if (_paramList[i] != null)
                        result = _paramList[i].Through(result);
                }
                return result;
            }
            public MIPSInstruction Clone()
            {
                return new MIPSInstruction(_name, _paramList,_serializer);
            }
            public string Serialize(int pos)
            {
                return _serializer.ToString(this,pos);
            }
            private Serializer _serializer;
            public string Name { get { return _name; } }
            private string _name;
            public Param[] _paramList;
        }


        struct TranslatedInstruction
        {
            public Instruction ins;
            public List<MIPSInstruction> mips;
            public int pos;
        }

        class MIPSMachine
        {
            public MIPSMachine(int codeLength)
            {
                translatedInstruction = new TranslatedInstruction[codeLength];
                this.codeLength = codeLength;

                allocatedVaribles = 0;
                GlobalPositionDict = new Dictionary<string, int>();
                GlobalValueDict = new Dictionary<int, int>();

            }
            public int AllocateNewData(int initialValue)
            {
                GlobalValueDict[allocatedVaribles] = initialValue;
                return allocatedVaribles++;
            }
            public int FlagGlobal(string name,int initialValue)
            {
                int index;
                if (GlobalPositionDict.ContainsKey(name))
                    index = GlobalPositionDict[name];
                else
                {
                    index = AllocateNewData(initialValue);
                    GlobalPositionDict[name] = index;
                }
                GlobalValueDict[index] = initialValue;
                return 4*index;
            }
            public void InsertInstruction(int i,Instruction ins, List<MIPSInstruction> mips)
            {
                if (i == 0) translatedInstruction[i].pos = 4;
                else translatedInstruction[i].pos = translatedInstruction[i - 1].pos + translatedInstruction[i - 1].mips.Count;
                translatedInstruction[i].ins = ins;
                translatedInstruction[i].mips = mips;
            }
            public int GetInstructionPosition(int i)
            {
                return translatedInstruction[i].pos;
            }
            public void ProcessJFake()
            {
                for (int i = 0; i < codeLength; ++i)
                {
                    for (int j = 0; j < translatedInstruction[i].mips.Count; ++j)
                    {
                        if (translatedInstruction[i].mips[j].Name == "jal")
                        {
                            translatedInstruction[i].mips[j]._paramList[1].Val = (translatedInstruction[i].pos + j + 1);
                        }
                    }
                }
                for (int i = 0; i < codeLength; ++i)
                {
                    for (int j = 0; j < translatedInstruction[i].mips.Count; ++j)
                    {
                        if (translatedInstruction[i].mips[j].Name == "jfake")
                        {
                            int toJump = translatedInstruction[i + translatedInstruction[i].mips[j]._paramList[1].Val].pos;
                            translatedInstruction[i].mips[j] = MIPSInstructions.getInstruction("j");
                            translatedInstruction[i].mips[j]._paramList[1].Val = toJump;
                        }
                    }
                }
            }

            public void Output(string codeFile,string dataFile,string asmFile)
            {
                int userStackInit = allocatedVaribles * 4;
                int systemStackInit = dataMemSize * 4;
                // write data file
                StringBuilder sb = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();

                sb.Append("DEPTH=").Append(dataMemSize).Append(";\n");
                sb.Append("WIDTH = 32;\nADDRESS_RADIX = HEX;\nDATA_RADIX = HEX;\nCONTENT\nBEGIN\n");
                for (int i = 0; i < allocatedVaribles; ++i)
                {
                    sb.Append(i.ToString("X")).Append(" : ").Append(
                        GlobalValueDict[i].ToString("X8")
                        ).Append(";\n");

                }
                sb.Append("END;");
                System.IO.StreamWriter fs =
                    new System.IO.StreamWriter(dataFile);
                fs.Write(sb.ToString());
                fs.Close();

                int j = 1;
                sb = new StringBuilder();
                sb.Append("DEPTH=").Append(codeMemSize).Append(";\n");
                sb.Append("WIDTH = 32;\nADDRESS_RADIX = HEX;\nDATA_RADIX = HEX;\nCONTENT\nBEGIN\n0 : 00000000;\n");
                MIPSInstruction MIns = MIPSInstructions.getInstruction("addi");
                MIns._paramList[1].Val = 28;
                MIns._paramList[2].Val = 0;
                MIns._paramList[3].Val = 4 * allocatedVaribles;

                sb.Append(j).Append(" : ").Append(
                        MIns.getInstruction().ToString("X8")
                       ).Append(";\n");
                sb2.Append("P" + j).Append(" : ").Append(
                        MIns.Serialize(j)
                       ).Append(Environment.NewLine);
                ++j;
                MIns = MIPSInstructions.getInstruction("addi");
                MIns._paramList[1].Val = 30;
                MIns._paramList[2].Val = 0;
                MIns._paramList[3].Val = 4 * allocatedVaribles;

                sb.Append(j).Append(" : ").Append(
                        MIns.getInstruction().ToString("X8")
                       ).Append(";\n");
                sb2.Append("P" + j).Append(" : ").Append(
                        MIns.Serialize(j)
                       ).Append(Environment.NewLine);
                ++j;
                MIns._paramList[1].Val = 29;
                MIns._paramList[3].Val = dataMemSize * 4 - 4;
                sb.Append(j).Append(" : ").Append(
                        MIns.getInstruction().ToString("X8")
                       ).Append(";\n");
                sb2.Append("P" + j).Append(" : ").Append(
                        MIns.Serialize(j)
                       ).Append(Environment.NewLine);
                ++j;
                foreach (var ins in this.translatedInstruction)
                {
                    sb2.Append(Environment.NewLine);
                    foreach (var t in ins.mips)
                    {
                        sb.Append(j.ToString("X")).Append(" : ").Append(
                        t.getInstruction().ToString("X8")
                       ).Append(";\n");
                        sb2.Append("P"+j.ToString("X")).Append(" : ").Append(
                        t.Serialize(j)
                       ).Append(Environment.NewLine);
                        ++j;
                    }
                }
                sb.Append("END;");
                 fs =
                    new System.IO.StreamWriter(codeFile);
                fs.Write(sb.ToString());
                fs.Close();


                fs =
                    new System.IO.StreamWriter(asmFile);
                fs.Write(sb2.ToString());
                fs.Close();
                if (j >= codeMemSize)
                {
                    System.Console.WriteLine("Warning: Max code size exceeded");
                    System.Console.WriteLine("Code will NOT work");
                }
            }

            public int dataMemSize = 4096;
            public int codeMemSize = 1024;
            private int codeLength;
            private TranslatedInstruction[] translatedInstruction;
            private Dictionary<string, int> GlobalPositionDict;
            private Dictionary<int, int> GlobalValueDict;
            private int allocatedVaribles;
        }

        

        static class Translater
        {
            public static MIPSMachine Translate(Machine m)
            {
                MIPSMachine mm = new MIPSMachine(m.CodeLength);
                
                for (int i = 0; i < m.CodeLength; ++i)
                    mm.InsertInstruction(i,m.code[i], Translate(m.code[i], mm, m));
                foreach (var pair in m.globalMap)
                    mm.FlagGlobal(pair.Key, 4 * mm.GetInstructionPosition(pair.Value.pointer));
                mm.ProcessJFake();
                return mm;
            }
            public static List<MIPSInstruction> Translate(Instruction ins,MIPSMachine mm,Machine m)
            {
                List<MIPSInstruction> L = new List<MIPSInstruction>();
                MIPSInstruction MIns;
                switch (ins.opCode)
                {
                        // userStackBase $r28 userStackTop $r30
                        // systemStackTop $29
                    case OpCode.PUSHNUM: //ok
                        //lui $r1,imm
                        //addi $r1,$r0,imm 
                        if (ins.pointer > 0x7fff || ins.pointer < -0x8000)
                        {
                            uint higher = (uint)ins.pointer & 0xffff0000;
                            higher = higher >> 16;
                            MIns = MIPSInstructions.getInstruction("lui");
                            MIns._paramList[1].Val = 1;
                            MIns._paramList[2].Val = (int)higher;
                            L.Add(MIns);
                            MIns = MIPSInstructions.getInstruction("ori");
                            MIns._paramList[1].Val = MIns._paramList[2].Val = 1;
                            MIns._paramList[3].Val = ins.pointer & 0xffff;
                            L.Add(MIns);
                        }
                        else
                        {
                            MIns = MIPSInstructions.getInstruction("addi");
                            MIns._paramList[1].Val = 1;
                            MIns._paramList[3].Val = ins.pointer;
                            L.Add(MIns);
                        }
                        //sw $r1,0($r30) 
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        //addi $r30,$r30,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        break;
                    case OpCode.POP: 
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4 * ins.pointer;
                        L.Add(MIns);
                        // subi $r30,$r30,4*ins.pointer
                        break;
                    case OpCode.GETLOCAL: //ok
                        // addi $r1,$28,4*(ins.pointer-1)
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 28;
                        MIns._paramList[3].Val = 4 * (ins.pointer - 1);
                        L.Add(MIns);
                        // lw $r3,0($r1)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 1;
                        L.Add(MIns);
                        //sw $r3,0($r30) 
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        //addi $r30,$r30,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        break;
                    case OpCode.SETLOCAL:
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r3,0($r30) 
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        // addi $r1,$28,4*(ins.pointer-1)
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 28;
                        MIns._paramList[3].Val = 4 * (ins.pointer - 1);
                        L.Add(MIns);
                        // sw $r3,0($r1)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 1;
                        L.Add(MIns);
                        break;
                    case OpCode.GETGLOBAL: //ok
                        //lw $r1,imm（$r0) TODO:In case of large memory support
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = mm.FlagGlobal(m.GetString(ins.pointer),0);
                        L.Add(MIns);
                        //sw $r1,0($r30) 
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        //addi $r30,$r30,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        break;
                    case OpCode.SETGLOBAL: //ok
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30) 
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        //sw $r1,imm（$r0) TODO:In case of large memory support
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = mm.FlagGlobal(m.GetString(ins.pointer),0);
                        L.Add(MIns);
                        break;
                    case OpCode.CALL: //ok
                        //jal 0 ;$31 = pc+4
                        MIns = MIPSInstructions.getInstruction("jal");
                        MIns._paramList[1].Val = 0;
                        L.Add(MIns);
                        //addi $31,$31,4*8
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 31;
                        MIns._paramList[3].Val = 36;
                        L.Add(MIns);
                        //sw $31,0($29)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 31;
                        MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //sw $28,-4$(29)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 28;
                        MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //subi $29,$29,8
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = -8;
                        L.Add(MIns);
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 30; MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //subi $28,$30,4*ins.pointer
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 28; MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4 * ins.pointer;
                        L.Add(MIns);
                        //lw $r1,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //nop
                        L.Add(MIPSInstructions.getInstruction("add", 0, 0, 0));
                        //jr $r1
                        MIns = MIPSInstructions.getInstruction("jr");
                        MIns._paramList[1].Val = 1;
                        L.Add(MIns);
                        break;
                    case OpCode.RET: //ok
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //add $30,$28,0
                        MIns = MIPSInstructions.getInstruction("add");
                        MIns._paramList[1].Val = 30;
                        MIns._paramList[2].Val = 28;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //addi $29,$29,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        //lw $30,0($29)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 28;
                        MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //addi $29,$29,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        //lw $31,0($29)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 31;
                        MIns._paramList[2].Val = 29;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //sw $r1,0($r30) 
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        //addi $r30,$r30,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        //jr $31
                        MIns = MIPSInstructions.getInstruction("jr");
                        MIns._paramList[1].Val = 31;
                        L.Add(MIns);
                        break;
                    case OpCode.CMP:
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //lw $r2,-4($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //addi $r3,$r0,1
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 1;
                        L.Add(MIns);
                        //addi $r4,$r0,0
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 4;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //bne $r2,$r1,2
                        MIns = MIPSInstructions.getInstruction("bne");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 1;
                        MIns._paramList[3].Val = 2;
                        L.Add(MIns);
                        //or $r4,$r4,$r0/3 ? (ins.pointer & CompareMode.COMPARE_EQUAL) != 0
                        MIns = MIPSInstructions.getInstruction("or");
                        MIns._paramList[1].Val = 4;
                        MIns._paramList[2].Val = 4;
                        MIns._paramList[3].Val = (ins.pointer & CompareMode.COMPARE_EQUAL) != 0 ? 3 : 0;
                        L.Add(MIns);
                        //j 7
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 7;
                        L.Add(MIns);
                        //sub $r1,$r2,$r1 ;r1 = num
                        MIns = MIPSInstructions.getInstruction("sub");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 2;
                        MIns._paramList[3].Val = 1;
                        L.Add(MIns);
                        //lui $r2,16'h0x8000
                        MIns = MIPSInstructions.getInstruction("lui");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 0x8000;
                        L.Add(MIns);
                        //and $r1,$r1,$r2 ; $r1<0?
                        MIns = MIPSInstructions.getInstruction("and");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 1;
                        MIns._paramList[3].Val = 2;
                        L.Add(MIns);
                        //bne $r1,$r0,2 ;$r1 <0 num<0
                        MIns = MIPSInstructions.getInstruction("bne");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 2;
                        L.Add(MIns);
                        //or $r4,$r4,$r0/3 ? code[pc].pointer & CompareMode.COMPARE_GREATER) != 0;
                        MIns = MIPSInstructions.getInstruction("or");
                        MIns._paramList[1].Val = 4;
                        MIns._paramList[2].Val = 4;
                        MIns._paramList[3].Val = (ins.pointer & CompareMode.COMPARE_GREATER) != 0 ? 3 : 0;
                        L.Add(MIns);
                        //j 1
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 1;
                        L.Add(MIns);
                        //or $r4,$r4,$r0/3 ? (code[pc].pointer & CompareMode.COMPARE_LESS) != 0;
                        MIns = MIPSInstructions.getInstruction("or");
                        MIns._paramList[1].Val = 4;
                        MIns._paramList[2].Val = 4;
                        MIns._paramList[3].Val = (ins.pointer & CompareMode.COMPARE_LESS) != 0 ? 3 : 0;
                        L.Add(MIns);
                        //sw $4,-4($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 4;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        break;
                    case OpCode.JUMP: 
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30) 
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        L.Add(MIns);
                        L.Add(MIPSInstructions.getInstruction("add", 0, 0, 0));
                        //beq $r1,$r0,1
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 1;
                        L.Add(MIns);
                        //j ins.pointer
                        MIns = MIPSInstructions.getInstruction("jfake"); 
                        MIns._paramList[1].Val = ins.pointer;
                        L.Add(MIns);
                        break;
                    case OpCode.JUMPA:
                        //j ins.pointer
                        MIns = MIPSInstructions.getInstruction("jfake"); 
                        MIns._paramList[1].Val = ins.pointer;
                        L.Add(MIns);
                        break;
                    case OpCode.ADJLOCAL: //ok
                        //addi $r3,$r0,ins.pointer
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 3;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 4 * ins.pointer;
                        L.Add(MIns);
                        //sub $r2,$r30,$28
                        MIns = MIPSInstructions.getInstruction("sub");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 28;
                        L.Add(MIns);
                        //beq $r2,$r0,3
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 3;
                        MIns._paramList[3].Val = 3;
                        L.Add(MIns);
                        //sw $r0,0($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //addi $r30,$r30,4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 30;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 4;
                        L.Add(MIns);
                        //j -5
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = -5;
                        L.Add(MIns);
                        break;
                    case OpCode.NOT:
                        //lw $r1,-4($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //beq $r1,$r0,2
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 2;
                        L.Add(MIns);
                        //sw $r0,-4($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //j 2
                        MIns = MIPSInstructions.getInstruction("beq");
                        MIns._paramList[1].Val = 0;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 2;
                        L.Add(MIns);
                        //addi $r1,$r0,1
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 0;
                        MIns._paramList[3].Val = 1;
                        L.Add(MIns);
                        //sw $r1,-4($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        break;
                    case OpCode.ARITH: //ok
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 30;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //lw $r2,-4($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        switch (ins.pointer)
                        {
                            case OpCode.ARITH_ADD:
                                //add $r1,$r2,$r1
                                MIns = MIPSInstructions.getInstruction("add");
                                MIns._paramList[1].Val = 1;
                                MIns._paramList[2].Val = 2;
                                MIns._paramList[3].Val = 1;
                                L.Add(MIns);
                                break;
                            case OpCode.ARITH_SUB:
                                //sub $r1,$r2,$r1
                                MIns = MIPSInstructions.getInstruction("sub");
                                MIns._paramList[1].Val = 1;
                                MIns._paramList[2].Val = 2;
                                MIns._paramList[3].Val = 1;
                                L.Add(MIns);
                                break;
                            case OpCode.ARITH_MUL:
                                // $r1 = $r2 * $r1
                                // srl $3,$2,31
                                L.Add(MIPSInstructions.getInstruction("srl", 3, 2, 31));
                                // srl $4,$1,31
                                L.Add(MIPSInstructions.getInstruction("srl", 4, 1, 31));
                                // xor $3,$4,$3 $3 = signed bit
                                L.Add(MIPSInstructions.getInstruction("xor", 3, 4, 3));
                                // sll $3,$3,31
                                L.Add(MIPSInstructions.getInstruction("sll", 3, 3, 31));
                                // lui $5,0x7fff
                                L.Add(MIPSInstructions.getInstruction("lui", 5, 0x7fff, 0));
                                // ori $5,$5,0xffff 
                                L.Add(MIPSInstructions.getInstruction("ori", 5, 5, 0xffff));
                                // and $1,$1,$5
                                L.Add(MIPSInstructions.getInstruction("and", 1, 1, 5));
                                // and $2,$2,$5
                                L.Add(MIPSInstructions.getInstruction("and", 2, 2, 5));
                                // and $4,$0,$0 //$4 = result
                                L.Add(MIPSInstructions.getInstruction("and", 4, 0, 0));
                                //loop:
                                // andi $6,$2,1
                                L.Add(MIPSInstructions.getInstruction("andi", 6, 2, 1));
                                // beq $6,$0,1
                                L.Add(MIPSInstructions.getInstruction("beq", 6, 0, 1));
                                // add $4,$4,$1
                                L.Add(MIPSInstructions.getInstruction("add", 4, 4, 1));
                                // sll $1,$1,1
                                L.Add(MIPSInstructions.getInstruction("sll", 1, 1, 1));
                                // srl $2,$2,1
                                L.Add(MIPSInstructions.getInstruction("srl", 2, 2, 1));
                                // bne $2,$0,-6
                                L.Add(MIPSInstructions.getInstruction("bne", 2, 0, -6));
                                // add $1,$4,$3
                                L.Add(MIPSInstructions.getInstruction("add", 1, 4, 3));
                                break;
                            default:
                                throw new RuntimeException("Unknown arith type " + ins.pointer.ToString());
                        }
                        //sw $r1,-4($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        break;
                    case OpCode.IN:
                        //lw $r1,-4($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //$r1 = port
                        L.Add(MIPSInstructions.getInstruction("add", 0, 0, 0));
                        //lw $r2,0($r1)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 1;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        L.Add(MIPSInstructions.getInstruction("add", 0, 0, 0));
                        //sw $r2,-4($r30)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        break;
                    case OpCode.OUT:
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 30;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r1,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //$r1 = data
                        //addi $r30,$r30,-4
                        MIns = MIPSInstructions.getInstruction("addi");
                        MIns._paramList[1].Val = 30;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = -4;
                        L.Add(MIns);
                        //lw $r2,0($r30)
                        MIns = MIPSInstructions.getInstruction("lw");
                        MIns._paramList[1].Val = 2;
                        MIns._paramList[2].Val = 30;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        //$r2 = port
                        //sw $r1,0($r2)
                        MIns = MIPSInstructions.getInstruction("sw");
                        MIns._paramList[1].Val = 1;
                        MIns._paramList[2].Val = 2;
                        MIns._paramList[3].Val = 0;
                        L.Add(MIns);
                        break;
                    case OpCode.PSEUDO_BREAK:
                    case OpCode.PSEUDO_CONTINUE:
                        throw new RuntimeException("Unexpected break or continue outside while loop");
                    default:
                        throw new RuntimeException("Unknown instruction");
                }
                return L;
            }
        }


        static class MIPSInstructions
        {
            static MIPSInstructions()
            {
                protoDict = new Dictionary<string, MIPSInstruction>();
                //R
                protoDict["add"] = new MIPSInstruction("add", add_param,RThreeParamSerializer.GetInstance());
                protoDict["sub"] = new MIPSInstruction("sub", sub_param, RThreeParamSerializer.GetInstance());
                protoDict["and"] = new MIPSInstruction("and", and_param, RThreeParamSerializer.GetInstance());
                protoDict["or"] = new MIPSInstruction("or", or_param, RThreeParamSerializer.GetInstance());
                protoDict["xor"] =  new MIPSInstruction("xor", xor_param,   RThreeParamSerializer.GetInstance());
                protoDict["sll"] =  new MIPSInstruction("sll", sll_param,  RSRSerializer.GetInstance());
                protoDict["srl"] =  new MIPSInstruction("srl", srl_param,   RSRSerializer.GetInstance());
                protoDict["sra"] =  new MIPSInstruction("sra", sra_param,   RSRSerializer.GetInstance());
                protoDict["jr"] =  new MIPSInstruction("jr", jr_param,  RJRSerializer.GetInstance());
                //I
                protoDict["addi"] =  new MIPSInstruction("addi", i_param,  IThreeParamSerializer.GetInstance());
                protoDict["addi"]._paramList[0].Val = 8;

                protoDict["andi"] =  new MIPSInstruction("andi", i_param,  IThreeParamSerializer.GetInstance());
                protoDict["andi"]._paramList[0].Val = 12;

                protoDict["ori"] =  new MIPSInstruction("ori", i_param,  IThreeParamSerializer.GetInstance());
                protoDict["ori"]._paramList[0].Val = 13;

                protoDict["xori"] =  new MIPSInstruction("xori", i_param, IThreeParamSerializer.GetInstance());
                protoDict["xori"]._paramList[0].Val = 14;

                protoDict["lw"] =  new MIPSInstruction("lw", i_param, ILwSwSerializer.GetInstance());
                protoDict["lw"]._paramList[0].Val = 35;

                protoDict["sw"] =  new MIPSInstruction("sw", i_param, ILwSwSerializer.GetInstance());
                protoDict["sw"]._paramList[0].Val = 43;

                protoDict["beq"] =  new MIPSInstruction("beq", ib_param,  IBranchParamSerializer.GetInstance());
                protoDict["beq"]._paramList[0].Val = 4;

                protoDict["bne"] =  new MIPSInstruction("bne", ib_param,  IBranchParamSerializer.GetInstance());
                protoDict["bne"]._paramList[0].Val = 5;

                protoDict["lui"] =  new MIPSInstruction("lui", lui_param, ILuiSerializer.GetInstance());
                protoDict["lui"]._paramList[0].Val = 15;

                protoDict["j"] =  new MIPSInstruction("j", j_param, JSerializer.GetInstance());
                protoDict["j"]._paramList[0].Val = 2;

                protoDict["jal"] =  new MIPSInstruction("jal", j_param, JSerializer.GetInstance());
                protoDict["jal"]._paramList[0].Val = 3;

                protoDict["jfake"] =  new MIPSInstruction("jfake", j_param, JSerializer.GetInstance());
                protoDict["jfake"]._paramList[0].Val = 2;

            }

            static public MIPSInstruction getInstruction(string name)
            {
                return protoDict[name].Clone();
            }

            static public MIPSInstruction getInstruction(string name, int param1, int param2, int param3)
            {
                var inst = protoDict[name].Clone();
                inst._paramList[1].Val = param1;
                inst._paramList[2].Val = param2;
                if (param3 != 0)
                    inst._paramList[3].Val = param3;
                return inst;
            }

            static private Dictionary<string, MIPSInstruction> protoDict;


            private static Param[] add_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,32), // funct
            };
            private static Param[] sub_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,34), // funct
            };
            private static Param[] and_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,36), // funct
            };
            private static Param[] or_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,37), // funct
            };
            private static Param[] xor_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,38), // funct
            };
            private static Param[] sll_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(21,25,0), // rs
                    new Param(0,5,0), // funct
            };
            private static Param[] srl_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(21,25,0), // rs
                    new Param(0,5,2), // funct
            };
            private static Param[] sra_param = {
                    new Param(26,31,0), //opcode
                    new Param(11,15,0), // rd
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(21,25,0), // rs
                    new Param(0,5,3), // funct
            };
            private static Param[] jr_param = {
                    new Param(26,31,0), //opcode
                    new Param(21,25,0), // rs
                    new Param(11,15,0), // rd
                    new Param(16,20,0), // rt
                    new Param(6,10,0), // sa
                    new Param(0,5,8), // funct
            };

            private static Param[] i_param = {
                    new Param(26,31,8), //opcode
                    new Param(16,20,0), // rt
                    new Param(21,25,0), // rs
                    new Param(0,15,0), // imm
                    null,
                    null,
            };
            private static Param[] lui_param = {
                    new Param(26,31,8), //opcode
                    new Param(16,20,0), // rt
                    //new Param(21,25,0), // rs
                    new Param(0,15,0), // imm
                    null,
                    null,
                    null,
            };
            private static Param[] ib_param = {
                    new Param(26,31,8), //opcode
                    new Param(21,25,0), // rs
                    new Param(16,20,0), // rt
                    new Param(0,15,0), // imm
                    null,
                    null,
            };
            private static Param[] j_param = {
                    new Param(26,31,8), //opcode
                    new Param(0,25,0), //addr
                    null,
                    null,
                    null,
                    null,
            };

        }

            interface Serializer
            {
                string ToString(MIPSInstruction inst,int pos);
            }

            class RThreeParamSerializer
                :Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + ",$" + inst._paramList[2].Val + ",$" + inst._paramList[3].Val;
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static RThreeParamSerializer _ins = new RThreeParamSerializer();
            }

            class RSRSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + ",$" + inst._paramList[2].Val + "," + inst._paramList[3].Val;
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static RSRSerializer _ins = new RSRSerializer();
            }

            class RJRSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val;
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static RJRSerializer _ins = new RJRSerializer();
            }

            class IThreeParamSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + ",$" + inst._paramList[2].Val + "," + inst._paramList[3].Val;
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static IThreeParamSerializer _ins = new IThreeParamSerializer();
            }

            class IBranchParamSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + ",$" + inst._paramList[2].Val + "," + "P" + (inst._paramList[3].Val + pos + 1).ToString("X");
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static IBranchParamSerializer _ins = new IBranchParamSerializer();
            }

            class ILwSwSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + "," + inst._paramList[3].Val + "($" + inst._paramList[2].Val + ")";
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static ILwSwSerializer _ins = new ILwSwSerializer();
            }

            class ILuiSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " " + "$" + inst._paramList[1].Val + "," + inst._paramList[2].Val;
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static ILuiSerializer _ins = new ILuiSerializer();
            }

            class JSerializer
                : Serializer
            {
                public string ToString(MIPSInstruction inst, int pos)
                {
                    return inst.Name + " P" + inst._paramList[1].Val.ToString("X");
                }
                public static Serializer GetInstance()
                {
                    return _ins;
                }
                private static JSerializer _ins = new JSerializer();
            }

        static class Util
        {
            public static int binary_set(int input, int to_set, int bit_start, int bit_end)
            {
                int result = input;
                int bitmask = 0;
                for (int i = bit_start; i <= bit_end; ++i)
                {
                    bitmask += 1 << i;
                }
                to_set = to_set << bit_start;
                to_set &= bitmask;
                result &= (~bitmask);
                result |= to_set;
                return result;
            }
        }
    }
}

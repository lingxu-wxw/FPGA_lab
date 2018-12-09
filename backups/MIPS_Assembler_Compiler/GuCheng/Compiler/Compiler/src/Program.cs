using System;
using System.Collections.Generic;
using Interpreter;
using Interpreter.CodeGeneratorBackend;

namespace MyProgram
{

    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs;
            Machine m = new Machine();
         

            try
            {
                Console.WriteLine("Usage: Compiler [script] [codeMemorySize=64] [dataMemorySize=32]");
                if (args.Length > 0)
                    fs = new FileStream(args[0]);
                else
                {
                    Console.WriteLine("Warning: Input file not specified,use script.txt as default");
                    fs = new FileStream("script.txt");
                }

                //Step 1 Compile
                m.Compile(fs);
                //Step 2 Translate
                MIPSMachine mm = Translater.Translate(m);

                if (args.Length > 1)
                {
                    mm.codeMemSize = Convert.ToInt32(args[1]);
                    Console.WriteLine("Setting codeMemSize To " + args[1]);
                }
                else
                {
                    mm.codeMemSize = 64;
                    Console.WriteLine("Warning: codeMemSize not defined, use 64 as default,double check your design file to match this size.");
                    Console.WriteLine("Or code will not work");
                }

                if (args.Length > 2)
                {
                    mm.dataMemSize = Convert.ToInt32(args[2]);
                    Console.WriteLine("Setting dataMemSize To " + args[2]);
                }
                else
                {
                    mm.dataMemSize = 32;
                    Console.WriteLine("Warning: dataMemSize not defined, use 32 as default,double check your design file to match this size.");
                    Console.WriteLine("Or code will not work");
                }
                

                mm.Output("sc_instmem.mif", "sc_datamem.mif", "asm.txt");
            }
            catch (CompileException e)
            {
                System.Console.WriteLine("At line " + e.Line+ " : "+ e.Description);
                System.Console.WriteLine(e.ToString());
            }
            catch (RuntimeException e)
            {
                System.Console.WriteLine(e.Description);
                System.Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.ToString());
            }
             
        }
    }
    
}

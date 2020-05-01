using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PatchMtouch
{
    class Program
    {
        static void Patch(string source, string destination)
        {
            var assembly = AssemblyDefinition.ReadAssembly(source);
            var linker = assembly.MainModule.Types.Where(t => t.FullName == "MonoTouch.Tuner.Linker").Single();
            var createLinkContext = linker.Methods.Where(m => m.Name == "CreateLinkContext").Single();
            var processor = createLinkContext.Body.GetILProcessor();
            var ret = createLinkContext.Body.Instructions.Where(i => i.OpCode.Code == Code.Ret).Single();

            var linkContext = assembly.MainModule.Types.Where(t => t.FullName == "MonoTouch.Tuner.MonoTouchLinkContext").Single()
                .BaseType.Resolve()
                .BaseType.Resolve();
            var setLogMessages = linkContext.Methods.Where(m => m.Name == "set_LogMessages").Single();

            processor.InsertBefore(ret, Instruction.Create(OpCodes.Dup));
            processor.InsertBefore(ret, Instruction.Create(OpCodes.Ldc_I4_1));
            processor.InsertBefore(ret, Instruction.Create(OpCodes.Callvirt, setLogMessages));

            assembly.Write (destination);
            Console.WriteLine($"patched {destination}");
        }
        static int Main()
        {
            var mtouchDir = "/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mtouch/";
            var mtouchExe = Path.Combine(mtouchDir, "mtouch.exe");
            if (!File.Exists(mtouchExe)) {
                Console.Error.WriteLine("unable to find mtouch");
                return -1;
            }

            var mtouchBackup = Path.Combine(mtouchDir, "mtouch.exe.bak");
            if (File.Exists(mtouchBackup)) {
                Console.Error.WriteLine($"backup already exists at {mtouchBackup}");
                return -1;
            }

            Console.WriteLine($"moving mtouch to {mtouchBackup}");
            File.Move(mtouchExe, mtouchBackup);

            Patch(mtouchBackup, mtouchExe);

            return 0;
        }
    }
}

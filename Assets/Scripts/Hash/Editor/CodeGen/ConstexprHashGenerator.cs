using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine;

namespace Hash.Editor.CodeGen
{
public class ConstexprHashGenerator : ILPostProcessor
{
    private readonly string logSavePath = Application.temporaryCachePath + "/HashLog.txt";

    public override ILPostProcessor GetInstance()
    {
        return this;
    }

    public override bool WillProcess(ICompiledAssembly compiledAssembly)
    {
        var referenceDlls = compiledAssembly.References
                                            .Select(Path.GetFileNameWithoutExtension);

        return referenceDlls.Any(x => x == "Hash");
    }

    public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
    {
        if (!WillProcess(compiledAssembly))
            return null;

        var assemblyDefinition = Utils.LoadAssemblyDefinition(compiledAssembly);

        var hashMap = new Dictionary<string, int>();

        var builder = new StringBuilder();
        
        void TryGenerateType(TypeDefinition typeDef)
        {
            foreach (var method in typeDef.Methods)
            {
                var processor = method.Body.GetILProcessor();

                for (var index = 0; index < processor.Body.Instructions.Count; index++)
                {
                    var bodyInstruction = processor.Body.Instructions[index];
                    if (bodyInstruction.OpCode == OpCodes.Call && bodyInstruction.Previous.OpCode == OpCodes.Ldstr &&
                        bodyInstruction.Operand.ToString() == "System.Int32 Hash.Runtime.Hash::CalcHash(System.String)")
                    {
                        var hashStr = bodyInstruction.Previous.Operand.ToString();
                        int hash;
                        if (hashMap.ContainsKey(hashStr))
                        {
                            hash = hashMap[hashStr];
                        }
                        else
                        {
                            hash = Animator.StringToHash(hashStr);
                            hashMap.Add(hashStr, hash);
                        }

                        var remove1 = processor.Body.Instructions[index - 1];
                        var remove2 = processor.Body.Instructions[index];
                        var ins     = processor.Create(OpCodes.Ldc_I4, hash);
                        foreach (var instruction in processor.Body.Instructions)
                        {
                            if (instruction.Previous == remove2)
                                instruction.Previous = ins;
                            if (instruction.Next == remove1)
                                instruction.Next = ins;
                            if (instruction.Operand is Instruction)
                            {
                                if (instruction.Operand == remove1 || instruction.Operand == remove2)
                                    instruction.Operand = ins;
                            }
                        }
                        
                        processor.Body.Instructions.RemoveAt(index - 1);
                        processor.Body.Instructions.RemoveAt(index - 1);
                        processor.Body.Instructions.Insert(index - 1, ins);

                        builder.AppendLine(method.Name);
                        foreach (var instruction in processor.Body.Instructions)
                        {
                            builder.AppendLine(instruction + "  " + instruction.Operand?.GetType());
                        }

                        builder.AppendLine();
                    }
                }
            }
        }

        foreach (var typeDef in assemblyDefinition.MainModule.Types.Where(typeDef => typeDef.FullName != "<Module>"))
        {
            TryGenerateType(typeDef);
        }

        foreach (var keyValuePair in hashMap)
        {
            builder.AppendLine(keyValuePair.Key + "  " + keyValuePair.Value);
        }
        File.WriteAllText(logSavePath, builder.ToString());
        
        var pe  = new MemoryStream();
        var pdb = new MemoryStream();

        var writeParameter = new WriterParameters
        {
            SymbolWriterProvider = new PortablePdbWriterProvider(),
            SymbolStream         = pdb,
            WriteSymbols         = true
        };

        assemblyDefinition.Write(pe, writeParameter);

        return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), null);
    }
}
}
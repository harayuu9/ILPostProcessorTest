using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Hash.Editor.CodeGen
{
internal static class Utils
{
    public static AssemblyDefinition LoadAssemblyDefinition(ICompiledAssembly compiledAssembly)
    {
        var resolver = new PostProcessorAssemblyResolver(compiledAssembly);
        var readerParameters = new ReaderParameters
        {
            SymbolStream               = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray()),
            SymbolReaderProvider       = new PortablePdbReaderProvider(),
            AssemblyResolver           = resolver,
            ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
            ReadingMode                = ReadingMode.Immediate
        };

        var peStream           = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray());
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

        resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

        return assemblyDefinition;
    }
}
}
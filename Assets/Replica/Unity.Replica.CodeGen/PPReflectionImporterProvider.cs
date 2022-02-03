using Mono.Cecil;

namespace Unity.Replica.Editor.CodeGen {
    internal class PPReflectionImporterProvider : IReflectionImporterProvider {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition moduleDefinition) {
            return new PPReflectionImporter(moduleDefinition);
        }
    }
}
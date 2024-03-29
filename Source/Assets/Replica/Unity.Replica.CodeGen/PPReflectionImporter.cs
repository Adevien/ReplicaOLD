﻿using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Unity.Replica.Editor.CodeGen {
    internal class PPReflectionImporter : DefaultReflectionImporter {
        private const string k_SystemPrivateCoreLib = "System.Private.CoreLib";
        private readonly AssemblyNameReference m_CorrectCorlib;

        public PPReflectionImporter(ModuleDefinition module) : base(module) {
            m_CorrectCorlib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib" || a.Name == "netstandard" || a.Name == k_SystemPrivateCoreLib);
        }

        public override AssemblyNameReference ImportReference(AssemblyName reference) {
            return m_CorrectCorlib != null && reference.Name == k_SystemPrivateCoreLib ? m_CorrectCorlib : base.ImportReference(reference);
        }
    }
}
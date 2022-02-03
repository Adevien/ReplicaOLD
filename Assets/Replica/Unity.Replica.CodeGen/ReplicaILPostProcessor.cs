using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using ILPPInterface = Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor;
using UnityEngine;
using Replica.Runtime;
using Mono.Collections.Generic;

namespace Unity.Replica.Editor.CodeGen {
    internal sealed class ReplicaILPostProcessor : ILPPInterface {
        public override ILPPInterface GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly CA) {
            return CA.Name == CGH.AssemblyName || CA.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == CGH.AssemblyName);
        }

        private readonly List<DiagnosticMessage> m_Diagnostics = new List<DiagnosticMessage>();

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly) {
            if (!WillProcess(compiledAssembly)) return null;

            m_Diagnostics.Clear();

            AssemblyDefinition assemblyDefinition = CGH.AssemblyDefinitionFor(compiledAssembly, out var resolver);

            if (assemblyDefinition == null) {
                m_Diagnostics.AddError($"Cannot read assembly definition: {compiledAssembly.Name}");
                return null;
            }

            ModuleDefinition module = assemblyDefinition.MainModule;

            if (module == null) {
                m_Diagnostics.AddError($"Cannot get main module from assembly definition: {compiledAssembly.Name}");
                return null;
            }

            try {
                foreach (var TypeDef in module.GetTypes().Where(t => t.Resolve().IsSubclassOf<NetworkBehaviour>())) {
                    int FlagIndex = 1;

                    foreach (PropertyDefinition property in TypeDef.Properties.Where(x => x.HasCustomAttributes).ToList()) {
                        if (property.CustomAttributes.Any(x => x.AttributeType.FullName.Contains("NetVar"))) {
                            var oldPropertyName = property.Name;

                            property.Name = $"{oldPropertyName}Network";

                            FlagIndex *= 2;

                            Console.WriteLine($"PROPERTY_GEN ({oldPropertyName} - {property.PropertyType.Name})");

                            TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Name = $"{oldPropertyName}";
                            TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Attributes = FieldAttributes.Private;

                            GenericInstanceMethod Equals = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Equals")).GetElementMethod());
                            Equals.GenericArguments.Add(TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).FieldType);

                            GenericInstanceMethod Set = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Set")).GetElementMethod());
                            Set.GenericArguments.Add(property.PropertyType);

                            Instruction endBreach = Instruction.Create(OpCodes.Nop);

                            Collection<Instruction> NewIL = new Collection<Instruction> {
                                Instruction.Create(OpCodes.Ldarg_1),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Resolve()),
                                Instruction.Create(OpCodes.Call, Equals),
                                Instruction.Create(OpCodes.Brtrue, endBreach),

                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldarg_1),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Resolve()),
                                Instruction.Create(OpCodes.Ldc_I4, FlagIndex),
                                Instruction.Create(OpCodes.Call, Set),
                                endBreach
                            };

                            if (property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.Any()) {
                                object callbackName = property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.First().Value;

                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                NewIL.Add(Instruction.Create(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(x => x.Name == "IsLocal").GetMethod)));
                                NewIL.Add(Instruction.Create(OpCodes.Brfalse, endBreach));

                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                NewIL.Add(Instruction.Create(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "GetGuard"))));
                                NewIL.Add(Instruction.Create(OpCodes.Brtrue, endBreach));

                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 1));
                                NewIL.Add(Instruction.Create(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Guard"))));

                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                NewIL.Add(Instruction.Create(OpCodes.Call, TypeDef.Methods.First(m => m.Name.Contains((string)callbackName))));

                                NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 0));
                                NewIL.Add(Instruction.Create(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Guard"))));

                                NewIL.Add(endBreach);
                            }

                            NewIL.Add(Instruction.Create(OpCodes.Ret));

                            property.SetMethod.Body.Instructions.Clear();
                            property.SetMethod.Body.Instructions.AddRangeAt(NewIL, 0);
                        }
                    }

                    const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

                    var WriteMethod = new MethodDefinition("WriteNetVars", attributes, module.ImportReference(typeof(bool)));

                    WriteMethod.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.ImportReference(typeof(Byter))));
                    WriteMethod.Parameters.Add(new ParameterDefinition("initial", ParameterAttributes.None, module.ImportReference(typeof(bool))));
                    WriteMethod.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(bool))));
                    WriteMethod.Body.InitLocals = true;

                    MethodReference baseSerialize = module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "WriteNetVars")).GetElementMethod();

                    var worker = WriteMethod.Body.GetILProcessor();

                    worker.Emit(OpCodes.Ldarg_0);
                    worker.Emit(OpCodes.Ldarg_1);
                    worker.Emit(OpCodes.Ldarg_2);
                    worker.Emit(OpCodes.Call, baseSerialize);
                    worker.Emit(OpCodes.Stloc_0);

                    Instruction initialStateLabel = worker.Create(OpCodes.Nop);

                    worker.Emit(OpCodes.Ldarg_2);
                    worker.Emit(OpCodes.Brfalse, initialStateLabel);

                    foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList()) {
                        worker.Emit(OpCodes.Ldarg_1);
                        worker.Emit(OpCodes.Ldarg_0);
                        worker.Emit(OpCodes.Ldfld, syncVar);

                        MethodReference writeFunc = module.ImportReference(typeof(Byter).GetMethod("Put", new Type[] { syncVar.FieldType.GetMonoType() }));

                        if (writeFunc != null) {
                            worker.Emit(OpCodes.Call, writeFunc);
                        }
                    }

                    worker.Emit(OpCodes.Ldc_I4_1);
                    worker.Emit(OpCodes.Ret);

                    worker.Append(initialStateLabel);

                    worker.Emit(OpCodes.Ldarg_1);
                    worker.Emit(OpCodes.Ldarg_0);
                    worker.Emit(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(m => m.Name == "Flags").GetMethod));
                    MethodReference writeIn32Func = module.ImportReference(typeof(Byter).GetMethod("Put", new Type[] { typeof(int) }));
                    worker.Emit(OpCodes.Call, writeIn32Func);

                    int dirtyBit = 1;
                    foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList()) {
                        Instruction endBreach = worker.Create(OpCodes.Nop);

                        worker.Emit(OpCodes.Ldarg_0);
                        worker.Emit(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(m => m.Name == "Flags").GetMethod));
                        worker.Emit(OpCodes.Ldc_I4, 1 << dirtyBit);
                        worker.Emit(OpCodes.And);
                        worker.Emit(OpCodes.Brfalse, endBreach);

                        worker.Emit(OpCodes.Ldarg_1);
                        worker.Emit(OpCodes.Ldarg_0);
                        worker.Emit(OpCodes.Ldfld, syncVar);

                        MethodReference writeFunc = module.ImportReference(typeof(Byter).GetMethod("Put", new Type[] { syncVar.FieldType.GetMonoType() }));
                        if (writeFunc != null) {
                            worker.Emit(OpCodes.Call, writeFunc);
                        }

                        worker.Emit(OpCodes.Ldc_I4_1);
                        worker.Emit(OpCodes.Stloc_0);

                        worker.Append(endBreach);
                        dirtyBit += 1;
                    }

                    worker.Emit(OpCodes.Ldloc_0);
                    worker.Emit(OpCodes.Ret);

                    TypeDef.Methods.Add(WriteMethod);

                    ///DESERIALIZE

                    var ReadMethod = new MethodDefinition("ReadNetVars", attributes, module.ImportReference(typeof(void)));

                    ReadMethod.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.ImportReference(typeof(Byter))));
                    ReadMethod.Parameters.Add(new ParameterDefinition("initial", ParameterAttributes.None, module.ImportReference(typeof(bool))));
                    ReadMethod.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(int))));
                    ReadMethod.Body.InitLocals = true;

                    MethodReference baseDeserialize = module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "ReadNetVars")).GetElementMethod();

                    var serWorker = ReadMethod.Body.GetILProcessor();

                    if (baseDeserialize != null) {
                        serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                        serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                        serWorker.Append(serWorker.Create(OpCodes.Ldarg_2));
                        serWorker.Append(serWorker.Create(OpCodes.Call, baseDeserialize));
                    }

                    Instruction initialStateLabel2 = serWorker.Create(OpCodes.Nop);

                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_2));
                    serWorker.Append(serWorker.Create(OpCodes.Brfalse, initialStateLabel2));

                    foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList()) {
                        MethodReference readFunc = module.ImportReference(typeof(Byter).GetMethods().First(m => m.Name.Contains("Get") && m.ReturnType == syncVar.FieldType.GetMonoType()));

                        MethodReference hookMethod = null;
                        PropertyDefinition callbackName = null;

                        callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

                        if (callbackName != null) {
                            CustomAttribute _cattribute = callbackName.CustomAttributes.First();

                            if (_cattribute.ConstructorArguments.Count > 0) {
                                hookMethod = TypeDef.Methods.First(m => m.Name.Contains((string)_cattribute.ConstructorArguments.First().Value));
                            }
                        }

                        if (hookMethod != null) {
                            VariableDefinition oldValue = new VariableDefinition(module.ImportReference(syncVar.FieldType));
                            ReadMethod.Body.Variables.Add(oldValue);

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Call, TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name)).GetMethod));
                            serWorker.Append(serWorker.Create(OpCodes.Stloc, oldValue));

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                            serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
                            serWorker.Append(serWorker.Create(OpCodes.Stfld, TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name))));

                            WriteCallback(TypeDef, serWorker, module, syncVar, oldValue);
                        } else {
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                            serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
                            serWorker.Append(serWorker.Create(OpCodes.Stfld, TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name))));
                        }
                    }

                    serWorker.Append(serWorker.Create(OpCodes.Ret));

                    serWorker.Append(initialStateLabel2);

                    MethodReference getFlags = module.ImportReference(typeof(Byter).GetMethod("GetInt32")).GetElementMethod();

                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                    serWorker.Append(serWorker.Create(OpCodes.Call, getFlags));
                    serWorker.Append(serWorker.Create(OpCodes.Stloc_0));

                    int dirtyBits = 1;
                    foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList()) {
                        Instruction varLabel = serWorker.Create(OpCodes.Nop);

                        serWorker.Append(serWorker.Create(OpCodes.Ldloc_0));
                        serWorker.Append(serWorker.Create(OpCodes.Ldc_I4, 1 << dirtyBits));
                        serWorker.Append(serWorker.Create(OpCodes.And));
                        serWorker.Append(serWorker.Create(OpCodes.Brfalse, varLabel));

                        MethodReference readFunc = module.ImportReference(typeof(Byter).GetMethods().First(m => m.Name.Contains("Get") && m.ReturnType == syncVar.FieldType.GetMonoType()));

                        MethodReference hookMethod = null;
                        PropertyDefinition callbackName = null;

                        callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

                        if (callbackName != null) {
                            CustomAttribute _cattribute = callbackName.CustomAttributes.First();

                            if (_cattribute.ConstructorArguments.Count > 0) {
                                hookMethod = TypeDef.Methods.First(m => m.Name.Contains((string)_cattribute.ConstructorArguments.First().Value));
                            }
                        }

                        if (hookMethod != null) {
                            VariableDefinition oldValue = new VariableDefinition(module.ImportReference(syncVar.FieldType));
                            ReadMethod.Body.Variables.Add(oldValue);

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Call, TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name)).GetMethod));
                            serWorker.Append(serWorker.Create(OpCodes.Stloc, oldValue));

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                            serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
                            serWorker.Append(serWorker.Create(OpCodes.Stfld, TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name))));

                            WriteCallback(TypeDef, serWorker, module, syncVar, oldValue);
                        } else {
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                            serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
                            serWorker.Append(serWorker.Create(OpCodes.Stfld, TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name))));
                        }

                        serWorker.Append(varLabel);
                        dirtyBits += 1;
                    }

                    serWorker.Append(serWorker.Create(OpCodes.Ret));

                    TypeDef.Methods.Add(ReadMethod);
                }
            } catch (Exception e) {
                m_Diagnostics.AddError((e.ToString() + e.StackTrace.ToString()).Replace("\n", "|").Replace("\r", "|"));
            }

            module.RemoveRecursiveReferences();

            MemoryStream pe = new MemoryStream();
            MemoryStream pdb = new MemoryStream();

            var writerParameters = new WriterParameters {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), m_Diagnostics);
        }

        public static void WriteCallback(TypeDefinition TypeDef, ILProcessor serWorker, ModuleDefinition module, FieldDefinition syncVar, VariableDefinition oldValue) {
            MethodReference hookMethod = null;
            PropertyDefinition callbackName = null;

            callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

            if (callbackName != null) {
                CustomAttribute _cattribute = callbackName.CustomAttributes.First();

                if (_cattribute.ConstructorArguments.Count > 0) {
                    hookMethod = TypeDef.Methods.First(m => m.Name.Contains((string)_cattribute.ConstructorArguments.First().Value));

                    Instruction syncVarEqualLabel = serWorker.Create(OpCodes.Nop);

                    GenericInstanceMethod Equals = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Equals")).GetElementMethod());
                    Equals.GenericArguments.Add(TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name)).FieldType);

                    serWorker.Append(serWorker.Create(OpCodes.Ldloc, oldValue));
                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                    serWorker.Append(serWorker.Create(OpCodes.Ldflda, syncVar));
                    serWorker.Append(serWorker.Create(OpCodes.Call, Equals));
                    serWorker.Append(serWorker.Create(OpCodes.Brtrue, syncVarEqualLabel));

                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                    serWorker.Append(serWorker.Create(OpCodes.Call, TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name)).GetMethod));
                    serWorker.Append(serWorker.Create(OpCodes.Callvirt, hookMethod));

                    serWorker.Append(syncVarEqualLabel);
                }
            }
        }
    }

    public static class TypeHelper {
        public static Type GetMonoType(this TypeReference type) {
            return Type.GetType(type.GetReflectionName(), true);
        }

        private static string GetReflectionName(this TypeReference type) {
            if (type.IsGenericInstance) {
                var genericInstance = (GenericInstanceType)type;
                return $"{genericInstance.Namespace}.{type.Name}[{String.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray())}]";
            }
            return type.FullName;
        }
    }
}
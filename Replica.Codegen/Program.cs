using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Replica.Codegen
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //TODO: ADD THE PATH TO THE DEMO EXE CURRENTLY HARDCODED
                //TODO: RE-RE-RE-RE-RE-RE-RE-RE-RE-REFACTORING
                string AssemblyPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\..\..\..\TestReplica\bin\Debug\TestReplica.exe";

                using (var module = ModuleDefinition.ReadModule(AssemblyPath, new ReaderParameters { ReadWrite = true }))
                {
                    foreach (TypeDefinition TypeDef in module.Types)
                    {
                        if (!TypeDef.IsGenericInstance && TypeDef.BaseType != null && TypeDef.BaseType.Resolve() == module.ImportReference(typeof(NetworkBehaviour)).Resolve())
                        {
                            int FlagIndex = 0;

                            foreach (PropertyDefinition property in TypeDef.Properties.Where(x => x.HasCustomAttributes).ToList())
                            {
                                if (property.CustomAttributes.Any(x => x.AttributeType.FullName.Contains("NetVar")))
                                {
                                    var oldPropertyName = property.Name;

                                    property.Name = $"{oldPropertyName}Network";

                                    FlagIndex += 1;

                                    Console.WriteLine($"PROPERTY_GEN ({oldPropertyName} - {property.PropertyType.Name})");

                                    TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Name = $"{oldPropertyName}";
                                    TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Attributes = FieldAttributes.Public;

                                    GenericInstanceMethod Equals = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Equals")).GetElementMethod());
                                    Equals.GenericArguments.Add(TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).FieldType);

                                    GenericInstanceMethod Set = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Set")).GetElementMethod());
                                    Set.GenericArguments.Add(property.PropertyType);

                                    GenericInstanceMethod getSyncVarHookGuard = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "GetGuard")).GetElementMethod());
                                    GenericInstanceMethod setSyncVarHookGuard = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "Guard")).GetElementMethod());

                                    Instruction endBreach = Instruction.Create(OpCodes.Nop);

                                    Collection<Instruction> NewIL = new Collection<Instruction>();

                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Resolve()));
                                    NewIL.Add(Instruction.Create(OpCodes.Call, Equals));
                                    NewIL.Add(Instruction.Create(OpCodes.Brtrue, endBreach));

                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(oldPropertyName)).Resolve()));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                    NewIL.Add(Instruction.Create(OpCodes.Call, Set));
                                    NewIL.Add(endBreach);

                                    if (property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.Any())
                                    {
                                        object callbackName = property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.First().Value;

                                        GenericInstanceMethod valueChanged = new GenericInstanceMethod(TypeDef.Methods.First(m => m.Name.Contains((string)callbackName)));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(x => x.Name == "IsLocal").GetMethod)));
                                        NewIL.Add(Instruction.Create(OpCodes.Brfalse, endBreach));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, getSyncVarHookGuard));
                                        NewIL.Add(Instruction.Create(OpCodes.Brtrue, endBreach));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 1));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, setSyncVarHookGuard));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                        NewIL.Add(Instruction.Create(OpCodes.Callvirt, valueChanged));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 0));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, setSyncVarHookGuard));

                                        NewIL.Add(endBreach);
                                    }

                                    NewIL.Add(Instruction.Create(OpCodes.Ret));

                                    property.SetMethod.Body.Instructions.Clear();
                                    property.SetMethod.Body.Instructions.AddRangeAt(NewIL, 0);

                                    //property.CustomAttributes.Remove(property.CustomAttributes.First(x => x.AttributeType.FullName.Contains("NetVar")));
                                }
                            }
                            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

                            var WriteMethod = new MethodDefinition("WriteNetVars", attributes, module.ImportReference(typeof(bool)));

                            WriteMethod.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.ImportReference(typeof(NetBuffer))));
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

                            foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList())
                            {
                                worker.Emit(OpCodes.Ldarg_1);
                                worker.Emit(OpCodes.Ldarg_0);
                                worker.Emit(OpCodes.Ldfld, syncVar);
                                MethodReference writeFunc = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "WriteFloat")).GetElementMethod();
                                if (writeFunc != null)
                                {
                                    worker.Emit(OpCodes.Call, writeFunc);
                                }
                            }

                            worker.Emit(OpCodes.Ldc_I4_1);
                            worker.Emit(OpCodes.Ret);

                            worker.Append(initialStateLabel);

                            worker.Emit(OpCodes.Ldarg_1);
                            worker.Emit(OpCodes.Ldarg_0);
                            worker.Emit(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(m => m.Name == "Flags").GetMethod));
                            MethodReference writeIn32Func = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "WriteInt")).GetElementMethod();
                            worker.Emit(OpCodes.Call, writeIn32Func);

                            int dirtyBit = 0;
                            foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList())
                            {
                                Instruction endBreach = worker.Create(OpCodes.Nop);

                                worker.Emit(OpCodes.Ldarg_0);
                                worker.Emit(OpCodes.Call, module.ImportReference(TypeDef.BaseType.Resolve().Properties.First(m => m.Name == "Flags").GetMethod));
                                worker.Emit(OpCodes.Ldc_I4, 1 << dirtyBit);
                                worker.Emit(OpCodes.And);
                                worker.Emit(OpCodes.Brfalse, endBreach);

                                worker.Emit(OpCodes.Ldarg_1);
                                worker.Emit(OpCodes.Ldarg_0);
                                worker.Emit(OpCodes.Ldfld, syncVar);

                                MethodReference writeFunc = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "WriteFloat")).GetElementMethod();
                                if (writeFunc != null)
                                {
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

                            ReadMethod.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.ImportReference(typeof(NetBuffer))));
                            ReadMethod.Parameters.Add(new ParameterDefinition("initial", ParameterAttributes.None, module.ImportReference(typeof(bool))));
                            ReadMethod.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(int))));
                            ReadMethod.Body.InitLocals = true;

                            MethodReference baseDeserialize = module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "ReadNetVars")).GetElementMethod();

                            var serWorker = ReadMethod.Body.GetILProcessor();

                            if (baseDeserialize != null)
                            {
                                serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                                serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                                serWorker.Append(serWorker.Create(OpCodes.Ldarg_2));
                                serWorker.Append(serWorker.Create(OpCodes.Call, baseDeserialize));
                            }

                            Instruction initialStateLabel2 = serWorker.Create(OpCodes.Nop);

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_2));
                            serWorker.Append(serWorker.Create(OpCodes.Brfalse, initialStateLabel2));

                            foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList())
                            {
                                MethodReference readFunc = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "ReadFloat")).GetElementMethod();

                                MethodReference hookMethod = null;
                                PropertyDefinition callbackName = null;

                                callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

                                if (callbackName != null)
                                {
                                    CustomAttribute _cattribute = callbackName.CustomAttributes.First();
                                    
                                    if(_cattribute.ConstructorArguments.Count > 0)
                                    {
                                        hookMethod = TypeDef.Methods.First(m => m.Name.Contains((string)_cattribute.ConstructorArguments.First().Value));
                                    }
                                }

                                if (hookMethod != null)
                                {
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
                                }
                                else
                                {
                                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                                    serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                                    serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
                                    serWorker.Append(serWorker.Create(OpCodes.Stfld, TypeDef.Fields.First(x => x.Name.Contains(syncVar.Name))));
                                }
                            }

                            serWorker.Append(serWorker.Create(OpCodes.Ret));

                            serWorker.Append(initialStateLabel2);

                            MethodReference getFlags = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "ReadInt")).GetElementMethod();

                            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
                            serWorker.Append(serWorker.Create(OpCodes.Call, getFlags));
                            serWorker.Append(serWorker.Create(OpCodes.Stloc_0));

                            int dirtyBits = 0;
                            foreach (FieldDefinition syncVar in TypeDef.Fields.Where(x => x.HasCustomAttributes).ToList())
                            {
                                Instruction varLabel = serWorker.Create(OpCodes.Nop);

                                serWorker.Append(serWorker.Create(OpCodes.Ldloc_0));
                                serWorker.Append(serWorker.Create(OpCodes.Ldc_I4, 1 << dirtyBits));
                                serWorker.Append(serWorker.Create(OpCodes.And));
                                serWorker.Append(serWorker.Create(OpCodes.Brfalse, varLabel));

                                MethodReference readFunc = module.ImportReference(module.ImportReference(typeof(NetBuffer)).Resolve().Methods.First(m => m.Name == "ReadFloat")).GetElementMethod();

                                MethodReference hookMethod = null;
                                PropertyDefinition callbackName = null;

                                callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

                                if (callbackName != null)
                                {
                                    CustomAttribute _cattribute = callbackName.CustomAttributes.First();

                                    if (_cattribute.ConstructorArguments.Count > 0)
                                    {
                                        hookMethod = TypeDef.Methods.First(m => m.Name.Contains((string)_cattribute.ConstructorArguments.First().Value));
                                    }
                                }

                                if (hookMethod != null)
                                {
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
                                }
                                else
                                {
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
                    }

                    module.Write();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();

            }
        }

        public static void WriteCallback(TypeDefinition TypeDef, ILProcessor serWorker, ModuleDefinition module, FieldDefinition syncVar, VariableDefinition oldValue)
        {
            MethodReference hookMethod = null;
            PropertyDefinition callbackName = null;

            callbackName = TypeDef.Properties.First(x => x.Name.Contains(syncVar.Name));

            if (callbackName != null)
            {
                CustomAttribute _cattribute = callbackName.CustomAttributes.First();

                if (_cattribute.ConstructorArguments.Count > 0)
                {
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

        public static void AddRangeAt<T>(this Collection<T> c, IEnumerable<T> e, int index)
        {
            foreach (var item in e)
                c.Insert(index++, item);
        }
    }
}

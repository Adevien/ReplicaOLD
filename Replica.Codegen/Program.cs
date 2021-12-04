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
                string AssemblyPath = @"D:\Replica_Solution\TestReplica\bin\Debug\TestReplica.exe";

                using (var module = ModuleDefinition.ReadModule(AssemblyPath, new ReaderParameters { ReadWrite = true }))
                {
                    foreach (TypeDefinition TypeDef in module.Types)
                    {
                        if (!TypeDef.IsGenericInstance && TypeDef.BaseType != null && TypeDef.BaseType.FullName.Contains("NetworkBehaviour"))
                        {
                            long FlagIndex = 2;

                            foreach (PropertyDefinition property in TypeDef.Properties.Where(x => x.HasCustomAttributes).ToList())
                            {
                                if (property.CustomAttributes.Any(x => x.AttributeType.FullName.Contains("NetVar")))
                                {
                                    Console.WriteLine($"PROPERTY_GEN ({property.Name} - {property.PropertyType.Name})");

                                    TypeDef.Fields.First(x => x.Name.Contains(property.Name)).Name = $"_{property.Name}_BackingField";

                                    GenericInstanceMethod netVarEqual = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "NetVarEqual")).GetElementMethod());
                                    netVarEqual.GenericArguments.Add(TypeDef.Fields.First(x => x.Name.Contains(property.Name)).FieldType);

                                    GenericInstanceMethod setNetVar = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "SetNetVar")).GetElementMethod());
                                    setNetVar.GenericArguments.Add(property.PropertyType);

                                    GenericInstanceMethod getSyncVarHookGuard = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "GetNetVarGuard")).GetElementMethod());
                                    GenericInstanceMethod setSyncVarHookGuard = new GenericInstanceMethod(module.ImportReference(TypeDef.BaseType.Resolve().Methods.First(m => m.Name == "SetNetVarGuard")).GetElementMethod());

                                    Instruction end = Instruction.Create(OpCodes.Nop);

                                    Collection<Instruction> NewIL = new Collection<Instruction>();

                                    property.SetMethod.Resolve();

                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(property.Name)).Resolve()));
                                    NewIL.Add(Instruction.Create(OpCodes.Call, netVarEqual));
                                    NewIL.Add(Instruction.Create(OpCodes.Brtrue, end));
                                  
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldflda, TypeDef.Fields.First(x => x.Name.Contains(property.Name)).Resolve()));
                                    NewIL.Add(Instruction.Create(OpCodes.Ldc_I8, FlagIndex));
                                    NewIL.Add(Instruction.Create(OpCodes.Call, setNetVar));
                                    NewIL.Add(end);


                                    if (property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.Any())
                                    {
                                        //VariableDefinition callbackValue = new VariableDefinition(property.PropertyType);
                                        //property.SetMethod.Body.Variables.Add(callbackValue);

                                        //NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        //NewIL.Add(Instruction.Create(OpCodes.Call, property.GetMethod.Resolve()));
                                        //NewIL.Add(Instruction.Create(OpCodes.Stloc, callbackValue));

                                        string callbackName = (string)property.CustomAttributes.First(x => x.AttributeType.Name == "NetVar").ConstructorArguments.First().Value;

                                        GenericInstanceMethod valueChanged = new GenericInstanceMethod(module.ImportReference(TypeDef.Methods.First(m => m.Name.Contains(callbackName)).GetElementMethod()));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I8, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, getSyncVarHookGuard));
                                        NewIL.Add(Instruction.Create(OpCodes.Brtrue, end));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I8, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 1));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, setSyncVarHookGuard));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_1));
                                        NewIL.Add(Instruction.Create(OpCodes.Callvirt, valueChanged));

                                        NewIL.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I8, FlagIndex));
                                        NewIL.Add(Instruction.Create(OpCodes.Ldc_I4, 0));
                                        NewIL.Add(Instruction.Create(OpCodes.Call, setSyncVarHookGuard));

                                        NewIL.Add(end);

                                    }

                                    NewIL.Add(Instruction.Create(OpCodes.Ret));

                                    property.SetMethod.Body.Instructions.Clear();
                                    property.SetMethod.Body.Instructions.AddRangeAt(NewIL, 0);

                                    property.CustomAttributes.Remove(property.CustomAttributes.First(x => x.AttributeType.FullName.Contains("NetVar")));

                                    FlagIndex *= 2;
                                }
                            }
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

        public static void AddRangeAt<T>(this Collection<T> c, IEnumerable<T> e, int index)
        {
            foreach (var item in e)
                c.Insert(index++, item);
        }
    }
}

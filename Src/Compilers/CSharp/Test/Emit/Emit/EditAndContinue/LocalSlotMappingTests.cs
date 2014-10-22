﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.CSharp.UnitTests;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.EditAndContinue.UnitTests
{
    public class LocalSlotMappingTests : EditAndContinueTestBase
    {
        // PDB reader can only be accessed from a single thread, so avoid concurrent compilation:
        private readonly CSharpCompilationOptions ComSafeDebugDll = TestOptions.DebugDll.WithConcurrentBuild(false);

        [Fact]
        public void OutOfOrderUserLocals()
        {
            var source = @"
using System;

public class C
{
    public static void M()
    {
        for (int i = 1; i < 1; i++) Console.WriteLine(1);
        for (int i = 1; i < 2; i++) Console.WriteLine(2);

        int j;
        for (j = 1; j < 3; j++) Console.WriteLine(3);
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: ComSafeDebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: ComSafeDebugDll);

            var v0 = CompileAndVerify(compilation0);
            v0.VerifyIL("C.M", @"
{
  // Code size       85 (0x55)
  .maxstack  2
  .locals init (int V_0, //j
                int V_1, //i
                int V_2,
                bool V_3,
                int V_4, //i
                bool V_5,
                bool V_6)
  IL_0000:  nop
  IL_0001:  ldc.i4.1
  IL_0002:  stloc.1
  IL_0003:  br.s       IL_0012
  IL_0005:  ldc.i4.1
  IL_0006:  call       ""void System.Console.WriteLine(int)""
  IL_000b:  nop
  IL_000c:  ldloc.1
  IL_000d:  stloc.2
  IL_000e:  ldloc.2
  IL_000f:  ldc.i4.1
  IL_0010:  add
  IL_0011:  stloc.1
  IL_0012:  ldloc.1
  IL_0013:  ldc.i4.1
  IL_0014:  clt
  IL_0016:  stloc.3
  IL_0017:  ldloc.3
  IL_0018:  brtrue.s   IL_0005
  IL_001a:  ldc.i4.1
  IL_001b:  stloc.s    V_4
  IL_001d:  br.s       IL_002e
  IL_001f:  ldc.i4.2
  IL_0020:  call       ""void System.Console.WriteLine(int)""
  IL_0025:  nop
  IL_0026:  ldloc.s    V_4
  IL_0028:  stloc.2
  IL_0029:  ldloc.2
  IL_002a:  ldc.i4.1
  IL_002b:  add
  IL_002c:  stloc.s    V_4
  IL_002e:  ldloc.s    V_4
  IL_0030:  ldc.i4.2
  IL_0031:  clt
  IL_0033:  stloc.s    V_5
  IL_0035:  ldloc.s    V_5
  IL_0037:  brtrue.s   IL_001f
  IL_0039:  ldc.i4.1
  IL_003a:  stloc.0
  IL_003b:  br.s       IL_004a
  IL_003d:  ldc.i4.3
  IL_003e:  call       ""void System.Console.WriteLine(int)""
  IL_0043:  nop
  IL_0044:  ldloc.0
  IL_0045:  stloc.2
  IL_0046:  ldloc.2
  IL_0047:  ldc.i4.1
  IL_0048:  add
  IL_0049:  stloc.0
  IL_004a:  ldloc.0
  IL_004b:  ldc.i4.3
  IL_004c:  clt
  IL_004e:  stloc.s    V_6
  IL_0050:  ldloc.s    V_6
  IL_0052:  brtrue.s   IL_003d
  IL_0054:  ret
}
");
            v0.VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <using version=""4"" kind=""UsingInfo"" size=""12"" namespaceCount=""1"">
          <namespace usingCount=""1"" />
        </using>
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""24"">
          <slot kind=""0"" offset=""135"" />
          <slot kind=""0"" offset=""20"" />
          <slot kind=""temp"" />
          <slot kind=""1"" offset=""11"" />
          <slot kind=""0"" offset=""79"" />
          <slot kind=""1"" offset=""70"" />
          <slot kind=""1"" offset=""147"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""20"">
        <entry il_offset=""0x0"" start_row=""7"" start_column=""5"" end_row=""7"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" start_row=""8"" start_column=""14"" end_row=""8"" end_column=""23"" file_ref=""0"" />
        <entry il_offset=""0x3"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x5"" start_row=""8"" start_column=""37"" end_row=""8"" end_column=""58"" file_ref=""0"" />
        <entry il_offset=""0xc"" start_row=""8"" start_column=""32"" end_row=""8"" end_column=""35"" file_ref=""0"" />
        <entry il_offset=""0x12"" start_row=""8"" start_column=""25"" end_row=""8"" end_column=""30"" file_ref=""0"" />
        <entry il_offset=""0x17"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x1a"" start_row=""9"" start_column=""14"" end_row=""9"" end_column=""23"" file_ref=""0"" />
        <entry il_offset=""0x1d"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x1f"" start_row=""9"" start_column=""37"" end_row=""9"" end_column=""58"" file_ref=""0"" />
        <entry il_offset=""0x26"" start_row=""9"" start_column=""32"" end_row=""9"" end_column=""35"" file_ref=""0"" />
        <entry il_offset=""0x2e"" start_row=""9"" start_column=""25"" end_row=""9"" end_column=""30"" file_ref=""0"" />
        <entry il_offset=""0x35"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x39"" start_row=""12"" start_column=""14"" end_row=""12"" end_column=""19"" file_ref=""0"" />
        <entry il_offset=""0x3b"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x3d"" start_row=""12"" start_column=""33"" end_row=""12"" end_column=""54"" file_ref=""0"" />
        <entry il_offset=""0x44"" start_row=""12"" start_column=""28"" end_row=""12"" end_column=""31"" file_ref=""0"" />
        <entry il_offset=""0x4a"" start_row=""12"" start_column=""21"" end_row=""12"" end_column=""26"" file_ref=""0"" />
        <entry il_offset=""0x50"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x54"" start_row=""13"" start_column=""5"" end_row=""13"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals>
        <local name=""j"" il_index=""0"" il_start=""0x0"" il_end=""0x55"" attributes=""0"" />
        <local name=""i"" il_index=""1"" il_start=""0x1"" il_end=""0x1a"" attributes=""0"" />
        <local name=""i"" il_index=""4"" il_start=""0x1a"" il_end=""0x39"" attributes=""0"" />
      </locals>
      <scope startOffset=""0x0"" endOffset=""0x55"">
        <namespace name=""System"" />
        <local name=""j"" il_index=""0"" il_start=""0x0"" il_end=""0x55"" attributes=""0"" />
        <scope startOffset=""0x1"" endOffset=""0x1a"">
          <local name=""i"" il_index=""1"" il_start=""0x1"" il_end=""0x1a"" attributes=""0"" />
        </scope>
        <scope startOffset=""0x1a"" endOffset=""0x39"">
          <local name=""i"" il_index=""4"" il_start=""0x1a"" il_end=""0x39"" attributes=""0"" />
        </scope>
      </scope>
    </method>
  </methods>
</symbols>
");
            var debugInfoProvider = v0.CreatePdbInfoProvider();

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(bytes0), debugInfoProvider.GetEncMethodDebugInfo);

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            // check that all user-defined and long-lived synthesized local slots are reused
            diff1.VerifyIL("C.M", @"
{
  // Code size       91 (0x5b)
  .maxstack  2
  .locals init (int V_0, //j
                int V_1, //i
                [int] V_2,
                bool V_3,
                int V_4, //i
                bool V_5,
                bool V_6,
                int V_7)
  IL_0000:  nop
  IL_0001:  ldc.i4.1
  IL_0002:  stloc.1
  IL_0003:  br.s       IL_0014
  IL_0005:  ldc.i4.1
  IL_0006:  call       ""void System.Console.WriteLine(int)""
  IL_000b:  nop
  IL_000c:  ldloc.1
  IL_000d:  stloc.s    V_7
  IL_000f:  ldloc.s    V_7
  IL_0011:  ldc.i4.1
  IL_0012:  add
  IL_0013:  stloc.1
  IL_0014:  ldloc.1
  IL_0015:  ldc.i4.1
  IL_0016:  clt
  IL_0018:  stloc.3
  IL_0019:  ldloc.3
  IL_001a:  brtrue.s   IL_0005
  IL_001c:  ldc.i4.1
  IL_001d:  stloc.s    V_4
  IL_001f:  br.s       IL_0032
  IL_0021:  ldc.i4.2
  IL_0022:  call       ""void System.Console.WriteLine(int)""
  IL_0027:  nop
  IL_0028:  ldloc.s    V_4
  IL_002a:  stloc.s    V_7
  IL_002c:  ldloc.s    V_7
  IL_002e:  ldc.i4.1
  IL_002f:  add
  IL_0030:  stloc.s    V_4
  IL_0032:  ldloc.s    V_4
  IL_0034:  ldc.i4.2
  IL_0035:  clt
  IL_0037:  stloc.s    V_5
  IL_0039:  ldloc.s    V_5
  IL_003b:  brtrue.s   IL_0021
  IL_003d:  ldc.i4.1
  IL_003e:  stloc.0
  IL_003f:  br.s       IL_0050
  IL_0041:  ldc.i4.3
  IL_0042:  call       ""void System.Console.WriteLine(int)""
  IL_0047:  nop
  IL_0048:  ldloc.0
  IL_0049:  stloc.s    V_7
  IL_004b:  ldloc.s    V_7
  IL_004d:  ldc.i4.1
  IL_004e:  add
  IL_004f:  stloc.0
  IL_0050:  ldloc.0
  IL_0051:  ldc.i4.3
  IL_0052:  clt
  IL_0054:  stloc.s    V_6
  IL_0056:  ldloc.s    V_6
  IL_0058:  brtrue.s   IL_0041
  IL_005a:  ret
}
");

            debugInfoProvider.Dispose();
        }

        /// <summary>
        /// Enc debug info is only present in debug builds.
        /// </summary>
        [Fact]
        public void DebugOnly()
        {
            var source =
@"class C
{
    static System.IDisposable F()
    {
        return null;
    }
    static void M()
    {
        lock (F()) { }
        using (F()) { }
    }
}";
            var debug = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var release = CreateCompilationWithMscorlib(source, options: TestOptions.ReleaseDll);

            CompileAndVerify(debug).VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""16"">
          <slot kind=""3"" offset=""11"" />
          <slot kind=""2"" offset=""11"" />
          <slot kind=""4"" offset=""35"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""11"">
        <entry il_offset=""0x0"" start_row=""8"" start_column=""5"" end_row=""8"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x3"" start_row=""9"" start_column=""9"" end_row=""9"" end_column=""19"" file_ref=""0"" />
        <entry il_offset=""0x12"" start_row=""9"" start_column=""20"" end_row=""9"" end_column=""21"" file_ref=""0"" />
        <entry il_offset=""0x13"" start_row=""9"" start_column=""22"" end_row=""9"" end_column=""23"" file_ref=""0"" />
        <entry il_offset=""0x16"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x21"" start_row=""10"" start_column=""9"" end_row=""10"" end_column=""20"" file_ref=""0"" />
        <entry il_offset=""0x27"" start_row=""10"" start_column=""21"" end_row=""10"" end_column=""22"" file_ref=""0"" />
        <entry il_offset=""0x28"" start_row=""10"" start_column=""23"" end_row=""10"" end_column=""24"" file_ref=""0"" />
        <entry il_offset=""0x2b"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x36"" start_row=""11"" start_column=""5"" end_row=""11"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>
");
            CompileAndVerify(release).VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""1"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
      </customDebugInfo>
      <sequencepoints total=""8"">
        <entry il_offset=""0x0"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x2"" start_row=""9"" start_column=""9"" end_row=""9"" end_column=""19"" file_ref=""0"" />
        <entry il_offset=""0x10"" start_row=""9"" start_column=""22"" end_row=""9"" end_column=""23"" file_ref=""0"" />
        <entry il_offset=""0x12"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x1c"" start_row=""10"" start_column=""9"" end_row=""10"" end_column=""20"" file_ref=""0"" />
        <entry il_offset=""0x22"" start_row=""10"" start_column=""23"" end_row=""10"" end_column=""24"" file_ref=""0"" />
        <entry il_offset=""0x24"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x2e"" start_row=""11"" start_column=""5"" end_row=""11"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>
");
        }

        [Fact]
        public void Using()
        {
            var source =
@"class C : System.IDisposable
{
    public void Dispose()
    {
    }
    static System.IDisposable F()
    {
        return new C();
    }
    static void M()
    {
        using (F())
        {
            using (var u = F())
            {
            }
            using (F())
            {
            }
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(bytes0), m => methodData0.GetEncDebugInfo());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       65 (0x41)
  .maxstack  1
  .locals init (System.IDisposable V_0,
                System.IDisposable V_1, //u
                System.IDisposable V_2)
  IL_0000:  nop
  IL_0001:  call       ""System.IDisposable C.F()""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  nop
    IL_0008:  call       ""System.IDisposable C.F()""
    IL_000d:  stloc.1
    .try
    {
      IL_000e:  nop
      IL_000f:  nop
      IL_0010:  leave.s    IL_001d
    }
    finally
    {
      IL_0012:  ldloc.1
      IL_0013:  brfalse.s  IL_001c
      IL_0015:  ldloc.1
      IL_0016:  callvirt   ""void System.IDisposable.Dispose()""
      IL_001b:  nop
      IL_001c:  endfinally
    }
    IL_001d:  call       ""System.IDisposable C.F()""
    IL_0022:  stloc.2
    .try
    {
      IL_0023:  nop
      IL_0024:  nop
      IL_0025:  leave.s    IL_0032
    }
    finally
    {
      IL_0027:  ldloc.2
      IL_0028:  brfalse.s  IL_0031
      IL_002a:  ldloc.2
      IL_002b:  callvirt   ""void System.IDisposable.Dispose()""
      IL_0030:  nop
      IL_0031:  endfinally
    }
    IL_0032:  nop
    IL_0033:  leave.s    IL_0040
  }
  finally
  {
    IL_0035:  ldloc.0
    IL_0036:  brfalse.s  IL_003f
    IL_0038:  ldloc.0
    IL_0039:  callvirt   ""void System.IDisposable.Dispose()""
    IL_003e:  nop
    IL_003f:  endfinally
  }
  IL_0040:  ret
}");
        }

        [Fact]
        public void Lock()
        {
            var source =
@"class C
{
    static object F()
    {
        return null;
    }
    static void M()
    {
        lock (F())
        {
            lock (F())
            {
            }
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(bytes0), methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       66 (0x42)
  .maxstack  2
  .locals init (object V_0,
                bool V_1,
                object V_2,
                bool V_3)
 -IL_0000:  nop
 ~IL_0001:  ldc.i4.0
  IL_0002:  stloc.1
  .try
  {
   -IL_0003:  call       ""object C.F()""
    IL_0008:  stloc.0
    IL_0009:  ldloc.0
    IL_000a:  ldloca.s   V_1
    IL_000c:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0011:  nop
   -IL_0012:  nop
   ~IL_0013:  ldc.i4.0
    IL_0014:  stloc.3
    .try
    {
     -IL_0015:  call       ""object C.F()""
      IL_001a:  stloc.2
      IL_001b:  ldloc.2
      IL_001c:  ldloca.s   V_3
      IL_001e:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
      IL_0023:  nop
     -IL_0024:  nop
     -IL_0025:  nop
      IL_0026:  leave.s    IL_0033
    }
    finally
    {
     ~IL_0028:  ldloc.3
      IL_0029:  brfalse.s  IL_0032
      IL_002b:  ldloc.2
      IL_002c:  call       ""void System.Threading.Monitor.Exit(object)""
      IL_0031:  nop
      IL_0032:  endfinally
    }
   -IL_0033:  nop
    IL_0034:  leave.s    IL_0041
  }
  finally
  {
   ~IL_0036:  ldloc.1
    IL_0037:  brfalse.s  IL_0040
    IL_0039:  ldloc.0
    IL_003a:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_003f:  nop
    IL_0040:  endfinally
  }
 -IL_0041:  ret
}
", methodToken: diff1.UpdatedMethods.Single());
        }

        /// <summary>
        /// Using Monitor.Enter(object).
        /// </summary>
        [Fact]
        public void Lock_Pre40()
        {
            var source =
@"class C
{
    static object F()
    {
        return null;
    }
    static void M()
    {
        lock (F())
        {
        }
    }
}";
            var compilation0 = CreateCompilation(source, options: TestOptions.DebugDll, references: new[] { MscorlibRef_v20 });
            var compilation1 = CreateCompilation(source, options: TestOptions.DebugDll, references: new[] { MscorlibRef_v20 });

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(bytes0), methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"{
  // Code size       27 (0x1b)
  .maxstack  1
  .locals init (object V_0)
  IL_0000:  nop
  IL_0001:  call       ""object C.F()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  call       ""void System.Threading.Monitor.Enter(object)""
  IL_000d:  nop
  .try
  {
    IL_000e:  nop
    IL_000f:  nop
    IL_0010:  leave.s    IL_001a
  }
  finally
  {
    IL_0012:  ldloc.0
    IL_0013:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0018:  nop
    IL_0019:  endfinally
  }
  IL_001a:  ret
}");
        }

        [Fact]
        public void Fixed()
        {
            var source =
@"class C
{
    unsafe static void M(string s, int[] i)
    {
        fixed (char *p = s)
        {
            fixed (int *q = i)
            {
            }
            fixed (char *r = s)
            {
            }
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.UnsafeDebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.UnsafeDebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"
{
  // Code size       80 (0x50)
  .maxstack  2
  .locals init (char* V_0, //p
                pinned string V_1,
                pinned int& V_2, //q
                [unchanged] V_3,
                char* V_4, //r
                pinned string V_5,
                int[] V_6)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  stloc.1
  IL_0003:  ldloc.1
  IL_0004:  conv.i
  IL_0005:  stloc.0
  IL_0006:  ldloc.0
  IL_0007:  brfalse.s  IL_0011
  IL_0009:  ldloc.0
  IL_000a:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_000f:  add
  IL_0010:  stloc.0
  IL_0011:  nop
  IL_0012:  ldarg.1
  IL_0013:  dup
  IL_0014:  stloc.s    V_6
  IL_0016:  brfalse.s  IL_001e
  IL_0018:  ldloc.s    V_6
  IL_001a:  ldlen
  IL_001b:  conv.i4
  IL_001c:  brtrue.s   IL_0023
  IL_001e:  ldc.i4.0
  IL_001f:  conv.u
  IL_0020:  stloc.2
  IL_0021:  br.s       IL_002c
  IL_0023:  ldloc.s    V_6
  IL_0025:  ldc.i4.0
  IL_0026:  ldelema    ""int""
  IL_002b:  stloc.2
  IL_002c:  nop
  IL_002d:  nop
  IL_002e:  ldc.i4.0
  IL_002f:  conv.u
  IL_0030:  stloc.2
  IL_0031:  ldarg.0
  IL_0032:  stloc.s    V_5
  IL_0034:  ldloc.s    V_5
  IL_0036:  conv.i
  IL_0037:  stloc.s    V_4
  IL_0039:  ldloc.s    V_4
  IL_003b:  brfalse.s  IL_0047
  IL_003d:  ldloc.s    V_4
  IL_003f:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_0044:  add
  IL_0045:  stloc.s    V_4
  IL_0047:  nop
  IL_0048:  nop
  IL_0049:  ldnull
  IL_004a:  stloc.s    V_5
  IL_004c:  nop
  IL_004d:  ldnull
  IL_004e:  stloc.1
  IL_004f:  ret
}");
        }

        [WorkItem(770053, "DevDiv")]
        [Fact]
        public void FixedMultiple()
        {
            var source =
@"class C
{
    unsafe static void M(string s1, string s2, string s3, string s4)
    {
        fixed (char* p1 = s1, p2 = s2)
        {
            *p1 = *p2;
        }
        fixed (char* p1 = s1, p3 = s3, p2 = s4)
        {
            *p1 = *p2;
            *p2 = *p3;
            fixed (char *p4 = s2)
            {
                *p3 = *p4;
            }
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.UnsafeDebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.UnsafeDebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"
{
  // Code size      166 (0xa6)
  .maxstack  2
  .locals init (char* V_0, //p1
                char* V_1, //p2
                pinned string V_2,
                pinned string V_3,
                char* V_4, //p1
                char* V_5, //p3
                char* V_6, //p2
                pinned string V_7,
                pinned string V_8,
                pinned string V_9,
                char* V_10, //p4
                pinned string V_11)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  stloc.2
  IL_0003:  ldloc.2
  IL_0004:  conv.i
  IL_0005:  stloc.0
  IL_0006:  ldloc.0
  IL_0007:  brfalse.s  IL_0011
  IL_0009:  ldloc.0
  IL_000a:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_000f:  add
  IL_0010:  stloc.0
  IL_0011:  ldarg.1
  IL_0012:  stloc.3
  IL_0013:  ldloc.3
  IL_0014:  conv.i
  IL_0015:  stloc.1
  IL_0016:  ldloc.1
  IL_0017:  brfalse.s  IL_0021
  IL_0019:  ldloc.1
  IL_001a:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_001f:  add
  IL_0020:  stloc.1
  IL_0021:  nop
  IL_0022:  ldloc.0
  IL_0023:  ldloc.1
  IL_0024:  ldind.u2
  IL_0025:  stind.i2
  IL_0026:  nop
  IL_0027:  ldnull
  IL_0028:  stloc.2
  IL_0029:  ldnull
  IL_002a:  stloc.3
  IL_002b:  ldarg.0
  IL_002c:  stloc.s    V_7
  IL_002e:  ldloc.s    V_7
  IL_0030:  conv.i
  IL_0031:  stloc.s    V_4
  IL_0033:  ldloc.s    V_4
  IL_0035:  brfalse.s  IL_0041
  IL_0037:  ldloc.s    V_4
  IL_0039:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_003e:  add
  IL_003f:  stloc.s    V_4
  IL_0041:  ldarg.2
  IL_0042:  stloc.s    V_8
  IL_0044:  ldloc.s    V_8
  IL_0046:  conv.i
  IL_0047:  stloc.s    V_5
  IL_0049:  ldloc.s    V_5
  IL_004b:  brfalse.s  IL_0057
  IL_004d:  ldloc.s    V_5
  IL_004f:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_0054:  add
  IL_0055:  stloc.s    V_5
  IL_0057:  ldarg.3
  IL_0058:  stloc.s    V_9
  IL_005a:  ldloc.s    V_9
  IL_005c:  conv.i
  IL_005d:  stloc.s    V_6
  IL_005f:  ldloc.s    V_6
  IL_0061:  brfalse.s  IL_006d
  IL_0063:  ldloc.s    V_6
  IL_0065:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_006a:  add
  IL_006b:  stloc.s    V_6
  IL_006d:  nop
  IL_006e:  ldloc.s    V_4
  IL_0070:  ldloc.s    V_6
  IL_0072:  ldind.u2
  IL_0073:  stind.i2
  IL_0074:  ldloc.s    V_6
  IL_0076:  ldloc.s    V_5
  IL_0078:  ldind.u2
  IL_0079:  stind.i2
  IL_007a:  ldarg.1
  IL_007b:  stloc.s    V_11
  IL_007d:  ldloc.s    V_11
  IL_007f:  conv.i
  IL_0080:  stloc.s    V_10
  IL_0082:  ldloc.s    V_10
  IL_0084:  brfalse.s  IL_0090
  IL_0086:  ldloc.s    V_10
  IL_0088:  call       ""int System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData.get""
  IL_008d:  add
  IL_008e:  stloc.s    V_10
  IL_0090:  nop
  IL_0091:  ldloc.s    V_5
  IL_0093:  ldloc.s    V_10
  IL_0095:  ldind.u2
  IL_0096:  stind.i2
  IL_0097:  nop
  IL_0098:  ldnull
  IL_0099:  stloc.s    V_11
  IL_009b:  nop
  IL_009c:  ldnull
  IL_009d:  stloc.s    V_7
  IL_009f:  ldnull
  IL_00a0:  stloc.s    V_8
  IL_00a2:  ldnull
  IL_00a3:  stloc.s    V_9
  IL_00a5:  ret
}
");
        }

        [Fact]
        public void ForEach()
        {
            var source =
@"using System.Collections;
using System.Collections.Generic;
class C
{
    static IEnumerable F1() { return null; }
    static List<object> F2() { return null; }
    static IEnumerable F3() { return null; }
    static List<object> F4() { return null; }
    static void M()
    {
        foreach (var @x in F1())
        {
            foreach (object y in F2()) { }
        }
        foreach (var x in F4())
        {
            foreach (var y in F3()) { }
            foreach (var z in F2()) { }
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"{
  // Code size      272 (0x110)
  .maxstack  1
  .locals init (System.Collections.IEnumerator V_0,
                object V_1, //x
                System.Collections.Generic.List<object>.Enumerator V_2,
                object V_3, //y
                [unchanged] V_4,
                System.Collections.Generic.List<object>.Enumerator V_5,
                object V_6, //x
                System.Collections.IEnumerator V_7,
                object V_8, //y
                System.Collections.Generic.List<object>.Enumerator V_9,
                object V_10, //z
                System.IDisposable V_11)
  IL_0000:  nop
  IL_0001:  nop
  IL_0002:  call       ""System.Collections.IEnumerable C.F1()""
  IL_0007:  callvirt   ""System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()""
  IL_000c:  stloc.0
  .try
  {
    IL_000d:  br.s       IL_004a
    IL_000f:  ldloc.0
    IL_0010:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0015:  stloc.1
    IL_0016:  nop
    IL_0017:  nop
    IL_0018:  call       ""System.Collections.Generic.List<object> C.F2()""
    IL_001d:  callvirt   ""System.Collections.Generic.List<object>.Enumerator System.Collections.Generic.List<object>.GetEnumerator()""
    IL_0022:  stloc.2
    .try
    {
      IL_0023:  br.s       IL_002f
      IL_0025:  ldloca.s   V_2
      IL_0027:  call       ""object System.Collections.Generic.List<object>.Enumerator.Current.get""
      IL_002c:  stloc.3
      IL_002d:  nop
      IL_002e:  nop
      IL_002f:  ldloca.s   V_2
      IL_0031:  call       ""bool System.Collections.Generic.List<object>.Enumerator.MoveNext()""
      IL_0036:  brtrue.s   IL_0025
      IL_0038:  leave.s    IL_0049
    }
    finally
    {
      IL_003a:  ldloca.s   V_2
      IL_003c:  constrained. ""System.Collections.Generic.List<object>.Enumerator""
      IL_0042:  callvirt   ""void System.IDisposable.Dispose()""
      IL_0047:  nop
      IL_0048:  endfinally
    }
    IL_0049:  nop
    IL_004a:  ldloc.0
    IL_004b:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0050:  brtrue.s   IL_000f
    IL_0052:  leave.s    IL_0069
  }
  finally
  {
    IL_0054:  ldloc.0
    IL_0055:  isinst     ""System.IDisposable""
    IL_005a:  stloc.s    V_11
    IL_005c:  ldloc.s    V_11
    IL_005e:  brfalse.s  IL_0068
    IL_0060:  ldloc.s    V_11
    IL_0062:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0067:  nop
    IL_0068:  endfinally
  }
  IL_0069:  nop
  IL_006a:  call       ""System.Collections.Generic.List<object> C.F4()""
  IL_006f:  callvirt   ""System.Collections.Generic.List<object>.Enumerator System.Collections.Generic.List<object>.GetEnumerator()""
  IL_0074:  stloc.s    V_5
  .try
  {
    IL_0076:  br.s       IL_00f2
    IL_0078:  ldloca.s   V_5
    IL_007a:  call       ""object System.Collections.Generic.List<object>.Enumerator.Current.get""
    IL_007f:  stloc.s    V_6
    IL_0081:  nop
    IL_0082:  nop
    IL_0083:  call       ""System.Collections.IEnumerable C.F3()""
    IL_0088:  callvirt   ""System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()""
    IL_008d:  stloc.s    V_7
    .try
    {
      IL_008f:  br.s       IL_009c
      IL_0091:  ldloc.s    V_7
      IL_0093:  callvirt   ""object System.Collections.IEnumerator.Current.get""
      IL_0098:  stloc.s    V_8
      IL_009a:  nop
      IL_009b:  nop
      IL_009c:  ldloc.s    V_7
      IL_009e:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
      IL_00a3:  brtrue.s   IL_0091
      IL_00a5:  leave.s    IL_00bd
    }
    finally
    {
      IL_00a7:  ldloc.s    V_7
      IL_00a9:  isinst     ""System.IDisposable""
      IL_00ae:  stloc.s    V_11
      IL_00b0:  ldloc.s    V_11
      IL_00b2:  brfalse.s  IL_00bc
      IL_00b4:  ldloc.s    V_11
      IL_00b6:  callvirt   ""void System.IDisposable.Dispose()""
      IL_00bb:  nop
      IL_00bc:  endfinally
    }
    IL_00bd:  nop
    IL_00be:  call       ""System.Collections.Generic.List<object> C.F2()""
    IL_00c3:  callvirt   ""System.Collections.Generic.List<object>.Enumerator System.Collections.Generic.List<object>.GetEnumerator()""
    IL_00c8:  stloc.s    V_9
    .try
    {
      IL_00ca:  br.s       IL_00d7
      IL_00cc:  ldloca.s   V_9
      IL_00ce:  call       ""object System.Collections.Generic.List<object>.Enumerator.Current.get""
      IL_00d3:  stloc.s    V_10
      IL_00d5:  nop
      IL_00d6:  nop
      IL_00d7:  ldloca.s   V_9
      IL_00d9:  call       ""bool System.Collections.Generic.List<object>.Enumerator.MoveNext()""
      IL_00de:  brtrue.s   IL_00cc
      IL_00e0:  leave.s    IL_00f1
    }
    finally
    {
      IL_00e2:  ldloca.s   V_9
      IL_00e4:  constrained. ""System.Collections.Generic.List<object>.Enumerator""
      IL_00ea:  callvirt   ""void System.IDisposable.Dispose()""
      IL_00ef:  nop
      IL_00f0:  endfinally
    }
    IL_00f1:  nop
    IL_00f2:  ldloca.s   V_5
    IL_00f4:  call       ""bool System.Collections.Generic.List<object>.Enumerator.MoveNext()""
    IL_00f9:  brtrue     IL_0078
    IL_00fe:  leave.s    IL_010f
  }
  finally
  {
    IL_0100:  ldloca.s   V_5
    IL_0102:  constrained. ""System.Collections.Generic.List<object>.Enumerator""
    IL_0108:  callvirt   ""void System.IDisposable.Dispose()""
    IL_010d:  nop
    IL_010e:  endfinally
  }
  IL_010f:  ret
}");
        }

        [Fact]
        public void ForEachArray1()
        {
            var source =
@"class C
{
    static void M(double[,,] c)
    {
        foreach (var x in c)
        {
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.M", @"
{
  // Code size      111 (0x6f)
  .maxstack  4
  .locals init (double[,,] V_0,
                int V_1,
                int V_2,
                int V_3,
                int V_4,
                int V_5,
                int V_6,
                double V_7) //x
 -IL_0000:  nop
 -IL_0001:  nop
 -IL_0002:  ldarg.0
  IL_0003:  stloc.0
  IL_0004:  ldloc.0
  IL_0005:  ldc.i4.0
  IL_0006:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_000b:  stloc.1
  IL_000c:  ldloc.0
  IL_000d:  ldc.i4.1
  IL_000e:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_0013:  stloc.2
  IL_0014:  ldloc.0
  IL_0015:  ldc.i4.2
  IL_0016:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_001b:  stloc.3
  IL_001c:  ldloc.0
  IL_001d:  ldc.i4.0
  IL_001e:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0023:  stloc.s    V_4
 ~IL_0025:  br.s       IL_0069
  IL_0027:  ldloc.0
  IL_0028:  ldc.i4.1
  IL_0029:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_002e:  stloc.s    V_5
 ~IL_0030:  br.s       IL_005e
  IL_0032:  ldloc.0
  IL_0033:  ldc.i4.2
  IL_0034:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0039:  stloc.s    V_6
 ~IL_003b:  br.s       IL_0053
 -IL_003d:  ldloc.0
  IL_003e:  ldloc.s    V_4
  IL_0040:  ldloc.s    V_5
  IL_0042:  ldloc.s    V_6
  IL_0044:  call       ""double[*,*,*].Get""
  IL_0049:  stloc.s    V_7
 -IL_004b:  nop
 -IL_004c:  nop
 ~IL_004d:  ldloc.s    V_6
  IL_004f:  ldc.i4.1
  IL_0050:  add
  IL_0051:  stloc.s    V_6
 -IL_0053:  ldloc.s    V_6
  IL_0055:  ldloc.3
  IL_0056:  ble.s      IL_003d
 ~IL_0058:  ldloc.s    V_5
  IL_005a:  ldc.i4.1
  IL_005b:  add
  IL_005c:  stloc.s    V_5
 -IL_005e:  ldloc.s    V_5
  IL_0060:  ldloc.2
  IL_0061:  ble.s      IL_0032
 ~IL_0063:  ldloc.s    V_4
  IL_0065:  ldc.i4.1
  IL_0066:  add
  IL_0067:  stloc.s    V_4
 -IL_0069:  ldloc.s    V_4
  IL_006b:  ldloc.1
  IL_006c:  ble.s      IL_0027
 -IL_006e:  ret
}", sequencePoints: "C.M");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size      111 (0x6f)
  .maxstack  4
  .locals init (double[,,] V_0,
                int V_1,
                int V_2,
                int V_3,
                int V_4,
                int V_5,
                int V_6,
                double V_7) //x
 -IL_0000:  nop
 -IL_0001:  nop
 -IL_0002:  ldarg.0
  IL_0003:  stloc.0
  IL_0004:  ldloc.0
  IL_0005:  ldc.i4.0
  IL_0006:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_000b:  stloc.1
  IL_000c:  ldloc.0
  IL_000d:  ldc.i4.1
  IL_000e:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_0013:  stloc.2
  IL_0014:  ldloc.0
  IL_0015:  ldc.i4.2
  IL_0016:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_001b:  stloc.3
  IL_001c:  ldloc.0
  IL_001d:  ldc.i4.0
  IL_001e:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0023:  stloc.s    V_4
 ~IL_0025:  br.s       IL_0069
  IL_0027:  ldloc.0
  IL_0028:  ldc.i4.1
  IL_0029:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_002e:  stloc.s    V_5
 ~IL_0030:  br.s       IL_005e
  IL_0032:  ldloc.0
  IL_0033:  ldc.i4.2
  IL_0034:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0039:  stloc.s    V_6
 ~IL_003b:  br.s       IL_0053
 -IL_003d:  ldloc.0
  IL_003e:  ldloc.s    V_4
  IL_0040:  ldloc.s    V_5
  IL_0042:  ldloc.s    V_6
  IL_0044:  call       ""double[*,*,*].Get""
  IL_0049:  stloc.s    V_7
 -IL_004b:  nop
 -IL_004c:  nop
 ~IL_004d:  ldloc.s    V_6
  IL_004f:  ldc.i4.1
  IL_0050:  add
  IL_0051:  stloc.s    V_6
 -IL_0053:  ldloc.s    V_6
  IL_0055:  ldloc.3
  IL_0056:  ble.s      IL_003d
 ~IL_0058:  ldloc.s    V_5
  IL_005a:  ldc.i4.1
  IL_005b:  add
  IL_005c:  stloc.s    V_5
 -IL_005e:  ldloc.s    V_5
  IL_0060:  ldloc.2
  IL_0061:  ble.s      IL_0032
 ~IL_0063:  ldloc.s    V_4
  IL_0065:  ldc.i4.1
  IL_0066:  add
  IL_0067:  stloc.s    V_4
 -IL_0069:  ldloc.s    V_4
  IL_006b:  ldloc.1
  IL_006c:  ble.s      IL_0027
 -IL_006e:  ret
}
", methodToken: diff1.UpdatedMethods.Single());
        }

        [Fact]
        public void ForEachArray2()
        {
            var source =
@"class C
{
    static void M(string a, object[] b, double[,,] c)
    {
        foreach (var x in a)
        {
            foreach (var y in b)
            {
            }
        }
        foreach (var x in c)
        {
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"
{
  // Code size      184 (0xb8)
  .maxstack  4
  .locals init (string V_0,
                int V_1,
                char V_2, //x
                object[] V_3,
                int V_4,
                object V_5, //y
                double[,,] V_6,
                int V_7,
                int V_8,
                int V_9,
                int V_10,
                int V_11,
                int V_12,
                double V_13) //x
  IL_0000:  nop
  IL_0001:  nop
  IL_0002:  ldarg.0
  IL_0003:  stloc.0
  IL_0004:  ldc.i4.0
  IL_0005:  stloc.1
  IL_0006:  br.s       IL_0033
  IL_0008:  ldloc.0
  IL_0009:  ldloc.1
  IL_000a:  callvirt   ""char string.this[int].get""
  IL_000f:  stloc.2
  IL_0010:  nop
  IL_0011:  nop
  IL_0012:  ldarg.1
  IL_0013:  stloc.3
  IL_0014:  ldc.i4.0
  IL_0015:  stloc.s    V_4
  IL_0017:  br.s       IL_0027
  IL_0019:  ldloc.3
  IL_001a:  ldloc.s    V_4
  IL_001c:  ldelem.ref
  IL_001d:  stloc.s    V_5
  IL_001f:  nop
  IL_0020:  nop
  IL_0021:  ldloc.s    V_4
  IL_0023:  ldc.i4.1
  IL_0024:  add
  IL_0025:  stloc.s    V_4
  IL_0027:  ldloc.s    V_4
  IL_0029:  ldloc.3
  IL_002a:  ldlen
  IL_002b:  conv.i4
  IL_002c:  blt.s      IL_0019
  IL_002e:  nop
  IL_002f:  ldloc.1
  IL_0030:  ldc.i4.1
  IL_0031:  add
  IL_0032:  stloc.1
  IL_0033:  ldloc.1
  IL_0034:  ldloc.0
  IL_0035:  callvirt   ""int string.Length.get""
  IL_003a:  blt.s      IL_0008
  IL_003c:  nop
  IL_003d:  ldarg.2
  IL_003e:  stloc.s    V_6
  IL_0040:  ldloc.s    V_6
  IL_0042:  ldc.i4.0
  IL_0043:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_0048:  stloc.s    V_7
  IL_004a:  ldloc.s    V_6
  IL_004c:  ldc.i4.1
  IL_004d:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_0052:  stloc.s    V_8
  IL_0054:  ldloc.s    V_6
  IL_0056:  ldc.i4.2
  IL_0057:  callvirt   ""int System.Array.GetUpperBound(int)""
  IL_005c:  stloc.s    V_9
  IL_005e:  ldloc.s    V_6
  IL_0060:  ldc.i4.0
  IL_0061:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0066:  stloc.s    V_10
  IL_0068:  br.s       IL_00b1
  IL_006a:  ldloc.s    V_6
  IL_006c:  ldc.i4.1
  IL_006d:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_0072:  stloc.s    V_11
  IL_0074:  br.s       IL_00a5
  IL_0076:  ldloc.s    V_6
  IL_0078:  ldc.i4.2
  IL_0079:  callvirt   ""int System.Array.GetLowerBound(int)""
  IL_007e:  stloc.s    V_12
  IL_0080:  br.s       IL_0099
  IL_0082:  ldloc.s    V_6
  IL_0084:  ldloc.s    V_10
  IL_0086:  ldloc.s    V_11
  IL_0088:  ldloc.s    V_12
  IL_008a:  call       ""double[*,*,*].Get""
  IL_008f:  stloc.s    V_13
  IL_0091:  nop
  IL_0092:  nop
  IL_0093:  ldloc.s    V_12
  IL_0095:  ldc.i4.1
  IL_0096:  add
  IL_0097:  stloc.s    V_12
  IL_0099:  ldloc.s    V_12
  IL_009b:  ldloc.s    V_9
  IL_009d:  ble.s      IL_0082
  IL_009f:  ldloc.s    V_11
  IL_00a1:  ldc.i4.1
  IL_00a2:  add
  IL_00a3:  stloc.s    V_11
  IL_00a5:  ldloc.s    V_11
  IL_00a7:  ldloc.s    V_8
  IL_00a9:  ble.s      IL_0076
  IL_00ab:  ldloc.s    V_10
  IL_00ad:  ldc.i4.1
  IL_00ae:  add
  IL_00af:  stloc.s    V_10
  IL_00b1:  ldloc.s    V_10
  IL_00b3:  ldloc.s    V_7
  IL_00b5:  ble.s      IL_006a
  IL_00b7:  ret
}");
        }

        /// <summary>
        /// Unlike Dev12 we can handle array with more than 256 dimensions.
        /// </summary>
        [Fact]
        public void ForEachArray_ToManyDimensions()
        {
            var source =
@"class C
{
    static void M(object o)
    {
        foreach (var x in (object[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,])o)
        {
        }
    }
}";
            // Make sure the source contains an array with too many dimensions.
            var tooManyCommas = new string(',', 256);
            Assert.True(source.IndexOf(tooManyCommas) > 0);

            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));
        }

        [Fact]
        public void AddAndDelete()
        {
            var source0 =
@"class C
{
    static object F1() { return null; }
    static string F2() { return null; }
    static System.IDisposable F3() { return null; }
    static void M()
    {
        lock (F1()) { }
        foreach (var c in F2()) { }
        using (F3()) { }
    }
}";
            // Delete one statement.
            var source1 =
@"class C
{
    static object F1() { return null; }
    static string F2() { return null; }
    static System.IDisposable F3() { return null; }
    static void M()
    {
        lock (F1()) { }
        foreach (var c in F2()) { }
    }
}";
            // Add statement with same temp kind.
            var source2 =
@"class C
{
    static object F1() { return null; }
    static string F2() { return null; }
    static System.IDisposable F3() { return null; }
    static void M()
    {
        using (F3()) { }
        lock (F1()) { }
        foreach (var c in F2()) { }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);
            v0.VerifyIL("C.M", @"
{
  // Code size       93 (0x5d)
  .maxstack  2
  .locals init (object V_0,
                bool V_1,
                string V_2,
                int V_3,
                char V_4, //c
                System.IDisposable V_5)
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stloc.1
  .try
  {
    IL_0003:  call       ""object C.F1()""
    IL_0008:  stloc.0
    IL_0009:  ldloc.0
    IL_000a:  ldloca.s   V_1
    IL_000c:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0011:  nop
    IL_0012:  nop
    IL_0013:  nop
    IL_0014:  leave.s    IL_0021
  }
  finally
  {
    IL_0016:  ldloc.1
    IL_0017:  brfalse.s  IL_0020
    IL_0019:  ldloc.0
    IL_001a:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_001f:  nop
    IL_0020:  endfinally
  }
  IL_0021:  nop
  IL_0022:  call       ""string C.F2()""
  IL_0027:  stloc.2
  IL_0028:  ldc.i4.0
  IL_0029:  stloc.3
  IL_002a:  br.s       IL_003b
  IL_002c:  ldloc.2
  IL_002d:  ldloc.3
  IL_002e:  callvirt   ""char string.this[int].get""
  IL_0033:  stloc.s    V_4
  IL_0035:  nop
  IL_0036:  nop
  IL_0037:  ldloc.3
  IL_0038:  ldc.i4.1
  IL_0039:  add
  IL_003a:  stloc.3
  IL_003b:  ldloc.3
  IL_003c:  ldloc.2
  IL_003d:  callvirt   ""int string.Length.get""
  IL_0042:  blt.s      IL_002c
  IL_0044:  call       ""System.IDisposable C.F3()""
  IL_0049:  stloc.s    V_5
  .try
  {
    IL_004b:  nop
    IL_004c:  nop
    IL_004d:  leave.s    IL_005c
  }
  finally
  {
    IL_004f:  ldloc.s    V_5
    IL_0051:  brfalse.s  IL_005b
    IL_0053:  ldloc.s    V_5
    IL_0055:  callvirt   ""void System.IDisposable.Dispose()""
    IL_005a:  nop
    IL_005b:  endfinally
  }
  IL_005c:  ret
}");

            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);
            var compilation2 = CreateCompilationWithMscorlib(source2, options: TestOptions.DebugDll);

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       69 (0x45)
  .maxstack  2
  .locals init (object V_0,
                bool V_1,
                string V_2,
                int V_3,
                char V_4, //c
                [unchanged] V_5)
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stloc.1
  .try
  {
    IL_0003:  call       ""object C.F1()""
    IL_0008:  stloc.0
    IL_0009:  ldloc.0
    IL_000a:  ldloca.s   V_1
    IL_000c:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0011:  nop
    IL_0012:  nop
    IL_0013:  nop
    IL_0014:  leave.s    IL_0021
  }
  finally
  {
    IL_0016:  ldloc.1
    IL_0017:  brfalse.s  IL_0020
    IL_0019:  ldloc.0
    IL_001a:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_001f:  nop
    IL_0020:  endfinally
  }
  IL_0021:  nop
  IL_0022:  call       ""string C.F2()""
  IL_0027:  stloc.2
  IL_0028:  ldc.i4.0
  IL_0029:  stloc.3
  IL_002a:  br.s       IL_003b
  IL_002c:  ldloc.2
  IL_002d:  ldloc.3
  IL_002e:  callvirt   ""char string.this[int].get""
  IL_0033:  stloc.s    V_4
  IL_0035:  nop
  IL_0036:  nop
  IL_0037:  ldloc.3
  IL_0038:  ldc.i4.1
  IL_0039:  add
  IL_003a:  stloc.3
  IL_003b:  ldloc.3
  IL_003c:  ldloc.2
  IL_003d:  callvirt   ""int string.Length.get""
  IL_0042:  blt.s      IL_002c
  IL_0044:  ret
}");

            var method2 = compilation2.GetMember<MethodSymbol>("C.M");
            var diff2 = compilation2.EmitDifference(
                diff1.NextGeneration,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method1, method2, GetEquivalentNodesMap(method2, method1), preserveLocalVariables: true)));

            diff2.VerifyIL("C.M",
@"{
  // Code size       93 (0x5d)
  .maxstack  2
  .locals init (object V_0,
                bool V_1,
                string V_2,
                int V_3,
                char V_4, //c
                [unchanged] V_5,
                System.IDisposable V_6)
 -IL_0000:  nop
 -IL_0001:  call       ""System.IDisposable C.F3()""
  IL_0006:  stloc.s    V_6
  .try
  {
   -IL_0008:  nop
   -IL_0009:  nop
    IL_000a:  leave.s    IL_0019
  }
  finally
  {
   ~IL_000c:  ldloc.s    V_6
    IL_000e:  brfalse.s  IL_0018
    IL_0010:  ldloc.s    V_6
    IL_0012:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0017:  nop
    IL_0018:  endfinally
  }
 ~IL_0019:  ldc.i4.0
  IL_001a:  stloc.1
  .try
  {
   -IL_001b:  call       ""object C.F1()""
    IL_0020:  stloc.0
    IL_0021:  ldloc.0
    IL_0022:  ldloca.s   V_1
    IL_0024:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0029:  nop
   -IL_002a:  nop
   -IL_002b:  nop
    IL_002c:  leave.s    IL_0039
  }
  finally
  {
   ~IL_002e:  ldloc.1
    IL_002f:  brfalse.s  IL_0038
    IL_0031:  ldloc.0
    IL_0032:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0037:  nop
    IL_0038:  endfinally
  }
 -IL_0039:  nop
 -IL_003a:  call       ""string C.F2()""
  IL_003f:  stloc.2
  IL_0040:  ldc.i4.0
  IL_0041:  stloc.3
 ~IL_0042:  br.s       IL_0053
 -IL_0044:  ldloc.2
  IL_0045:  ldloc.3
  IL_0046:  callvirt   ""char string.this[int].get""
  IL_004b:  stloc.s    V_4
 -IL_004d:  nop
 -IL_004e:  nop
 ~IL_004f:  ldloc.3
  IL_0050:  ldc.i4.1
  IL_0051:  add
  IL_0052:  stloc.3
 -IL_0053:  ldloc.3
  IL_0054:  ldloc.2
  IL_0055:  callvirt   ""int string.Length.get""
  IL_005a:  blt.s      IL_0044
 -IL_005c:  ret
}", methodToken: diff1.UpdatedMethods.Single());
        }

        [Fact]
        public void Insert()
        {
            var source0 =
@"class C
{
    static object F1() { return null; }
    static object F2() { return null; }
    static object F3() { return null; }
    static object F4() { return null; }
    static void M()
    {
        lock (F1()) { }
        lock (F2()) { }
    }
}";
            var source1 =
@"class C
{
    static object F1() { return null; }
    static object F2() { return null; }
    static object F3() { return null; }
    static object F4() { return null; }
    static void M()
    {
        lock (F3()) { } // added
        lock (F1()) { }
        lock (F4()) { } // replaced
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            // Note that the order of unique ids in temporaries follows the
            // order of declaration in the updated method. Specifically, the
            // original temporary names (and unique ids) are not preserved.
            // (Should not be an issue since the names are used by EnC only.)
            diff1.VerifyIL("C.M",
@"{
  // Code size      108 (0x6c)
  .maxstack  2
  .locals init (object V_0,
                bool V_1,
                [object] V_2,
                [bool] V_3,
                object V_4,
                bool V_5,
                object V_6,
                bool V_7)
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stloc.s    V_5
  .try
  {
    IL_0004:  call       ""object C.F3()""
    IL_0009:  stloc.s    V_4
    IL_000b:  ldloc.s    V_4
    IL_000d:  ldloca.s   V_5
    IL_000f:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0014:  nop
    IL_0015:  nop
    IL_0016:  nop
    IL_0017:  leave.s    IL_0026
  }
  finally
  {
    IL_0019:  ldloc.s    V_5
    IL_001b:  brfalse.s  IL_0025
    IL_001d:  ldloc.s    V_4
    IL_001f:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0024:  nop
    IL_0025:  endfinally
  }
  IL_0026:  ldc.i4.0
  IL_0027:  stloc.1
  .try
  {
    IL_0028:  call       ""object C.F1()""
    IL_002d:  stloc.0
    IL_002e:  ldloc.0
    IL_002f:  ldloca.s   V_1
    IL_0031:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0036:  nop
    IL_0037:  nop
    IL_0038:  nop
    IL_0039:  leave.s    IL_0046
  }
  finally
  {
    IL_003b:  ldloc.1
    IL_003c:  brfalse.s  IL_0045
    IL_003e:  ldloc.0
    IL_003f:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0044:  nop
    IL_0045:  endfinally
  }
  IL_0046:  ldc.i4.0
  IL_0047:  stloc.s    V_7
  .try
  {
    IL_0049:  call       ""object C.F4()""
    IL_004e:  stloc.s    V_6
    IL_0050:  ldloc.s    V_6
    IL_0052:  ldloca.s   V_7
    IL_0054:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0059:  nop
    IL_005a:  nop
    IL_005b:  nop
    IL_005c:  leave.s    IL_006b
  }
  finally
  {
    IL_005e:  ldloc.s    V_7
    IL_0060:  brfalse.s  IL_006a
    IL_0062:  ldloc.s    V_6
    IL_0064:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0069:  nop
    IL_006a:  endfinally
  }
  IL_006b:  ret
}");
        }

        /// <summary>
        /// Should not reuse temporary locals
        /// having different temporary kinds.
        /// </summary>
        [Fact]
        public void NoReuseDifferentTempKind()
        {
            var source =
@"class A : System.IDisposable
{
    public object Current { get { return null; } }
    public bool MoveNext() { return false; }
    public void Dispose() { }
    internal int this[A a] { get { return 0; } set { } }
}
class B
{
    public A GetEnumerator() { return null; }
}
class C
{
    static A F() { return null; }
    static B G() { return null; }
    static void M(A a)
    {
        a[F()]++;
        using (F()) { }
        lock (F()) { }
        foreach (var o in G()) { }
    }
}";

            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var testData0 = new CompilationTestData();
            var bytes0 = compilation0.EmitToArray(testData: testData0);
            var methodData0 = testData0.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(bytes0),
                methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M",
@"{
  // Code size      145 (0x91)
  .maxstack  4
  .locals init ([unchanged] V_0,
                [unchanged] V_1,
                [int] V_2,
                A V_3,
                A V_4,
                bool V_5,
                A V_6,
                object V_7, //o
                A V_8,
                A V_9,
                int V_10)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  stloc.s    V_8
  IL_0004:  call       ""A C.F()""
  IL_0009:  stloc.s    V_9
  IL_000b:  ldloc.s    V_8
  IL_000d:  ldloc.s    V_9
  IL_000f:  callvirt   ""int A.this[A].get""
  IL_0014:  stloc.s    V_10
  IL_0016:  ldloc.s    V_8
  IL_0018:  ldloc.s    V_9
  IL_001a:  ldloc.s    V_10
  IL_001c:  ldc.i4.1
  IL_001d:  add
  IL_001e:  callvirt   ""void A.this[A].set""
  IL_0023:  nop
  IL_0024:  call       ""A C.F()""
  IL_0029:  stloc.3
  .try
  {
    IL_002a:  nop
    IL_002b:  nop
    IL_002c:  leave.s    IL_0039
  }
  finally
  {
    IL_002e:  ldloc.3
    IL_002f:  brfalse.s  IL_0038
    IL_0031:  ldloc.3
    IL_0032:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0037:  nop
    IL_0038:  endfinally
  }
  IL_0039:  ldc.i4.0
  IL_003a:  stloc.s    V_5
  .try
  {
    IL_003c:  call       ""A C.F()""
    IL_0041:  stloc.s    V_4
    IL_0043:  ldloc.s    V_4
    IL_0045:  ldloca.s   V_5
    IL_0047:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_004c:  nop
    IL_004d:  nop
    IL_004e:  nop
    IL_004f:  leave.s    IL_005e
  }
  finally
  {
    IL_0051:  ldloc.s    V_5
    IL_0053:  brfalse.s  IL_005d
    IL_0055:  ldloc.s    V_4
    IL_0057:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_005c:  nop
    IL_005d:  endfinally
  }
  IL_005e:  nop
  IL_005f:  call       ""B C.G()""
  IL_0064:  callvirt   ""A B.GetEnumerator()""
  IL_0069:  stloc.s    V_6
  .try
  {
    IL_006b:  br.s       IL_0078
    IL_006d:  ldloc.s    V_6
    IL_006f:  callvirt   ""object A.Current.get""
    IL_0074:  stloc.s    V_7
    IL_0076:  nop
    IL_0077:  nop
    IL_0078:  ldloc.s    V_6
    IL_007a:  callvirt   ""bool A.MoveNext()""
    IL_007f:  brtrue.s   IL_006d
    IL_0081:  leave.s    IL_0090
  }
  finally
  {
    IL_0083:  ldloc.s    V_6
    IL_0085:  brfalse.s  IL_008f
    IL_0087:  ldloc.s    V_6
    IL_0089:  callvirt   ""void System.IDisposable.Dispose()""
    IL_008e:  nop
    IL_008f:  endfinally
  }
  IL_0090:  ret
}");
        }

        [Fact]
        public void Switch_String()
        {
            var source0 =
@"class C
{
    static string F() { return null; }
    
    static void M()
    {
        switch (F())
        {
            case ""a"": System.Console.WriteLine(1); break;
            case ""b"": System.Console.WriteLine(2); break; 
        }
    }
}";
            var source1 =
            @"class C
{
    static string F() { return null; }
    
    static void M()
    {
        switch (F())
        {
            case ""a"": System.Console.WriteLine(10); break;
            case ""b"": System.Console.WriteLine(20); break; 
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            // Validate presence of a hidden sequence point @IL_0007 that is required for proper function remapping.
            v0.VerifyIL("C.M", @"
{
  // Code size       54 (0x36)
  .maxstack  2
  .locals init (string V_0)
 -IL_0000:  nop       
 -IL_0001:  call       ""string C.F()""
  IL_0006:  stloc.0   
 ~IL_0007:  ldloc.0   
  IL_0008:  ldstr      ""a""
  IL_000d:  call       ""bool string.op_Equality(string, string)""
  IL_0012:  brtrue.s   IL_0023
  IL_0014:  ldloc.0   
  IL_0015:  ldstr      ""b""
  IL_001a:  call       ""bool string.op_Equality(string, string)""
  IL_001f:  brtrue.s   IL_002c
  IL_0021:  br.s       IL_0035
 -IL_0023:  ldc.i4.1  
  IL_0024:  call       ""void System.Console.WriteLine(int)""
  IL_0029:  nop       
 -IL_002a:  br.s       IL_0035
 -IL_002c:  ldc.i4.2  
  IL_002d:  call       ""void System.Console.WriteLine(int)""
  IL_0032:  nop       
 -IL_0033:  br.s       IL_0035
 -IL_0035:  ret       
}
", sequencePoints: "C.M");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.SwitchStatement), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       56 (0x38)
  .maxstack  2
  .locals init (string V_0)
 -IL_0000:  nop       
 -IL_0001:  call       ""string C.F()""
  IL_0006:  stloc.0   
 ~IL_0007:  ldloc.0   
  IL_0008:  ldstr      ""a""
  IL_000d:  call       ""bool string.op_Equality(string, string)""
  IL_0012:  brtrue.s   IL_0023
  IL_0014:  ldloc.0   
  IL_0015:  ldstr      ""b""
  IL_001a:  call       ""bool string.op_Equality(string, string)""
  IL_001f:  brtrue.s   IL_002d
  IL_0021:  br.s       IL_0037
 -IL_0023:  ldc.i4.s   10
  IL_0025:  call       ""void System.Console.WriteLine(int)""
  IL_002a:  nop       
 -IL_002b:  br.s       IL_0037
 -IL_002d:  ldc.i4.s   20
  IL_002f:  call       ""void System.Console.WriteLine(int)""
  IL_0034:  nop       
 -IL_0035:  br.s       IL_0037
 -IL_0037:  ret       
}", methodToken: diff1.UpdatedMethods.Single());
        }
        
        [Fact]
        public void Switch_Integer()
        {
            var source0 =
@"class C
{
    static int F() { return 1; }
    
    static void M()
    {
        switch (F())
        {
            case 1: System.Console.WriteLine(1); break;
            case 2: System.Console.WriteLine(2); break; 
        }
    }
}";
            var source1 =
            @"class C
{
    static int F() { return 1; }
    
    static void M()
    {
        switch (F())
        {
            case 1: System.Console.WriteLine(10); break;
            case 2: System.Console.WriteLine(20); break; 
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.M", @"
{
  // Code size       38 (0x26)
  .maxstack  2
  .locals init (int V_0)
  IL_0000:  nop
  IL_0001:  call       ""int C.F()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  ldc.i4.1
  IL_0009:  beq.s      IL_0013
  IL_000b:  br.s       IL_000d
  IL_000d:  ldloc.0
  IL_000e:  ldc.i4.2
  IL_000f:  beq.s      IL_001c
  IL_0011:  br.s       IL_0025
  IL_0013:  ldc.i4.1
  IL_0014:  call       ""void System.Console.WriteLine(int)""
  IL_0019:  nop
  IL_001a:  br.s       IL_0025
  IL_001c:  ldc.i4.2
  IL_001d:  call       ""void System.Console.WriteLine(int)""
  IL_0022:  nop
  IL_0023:  br.s       IL_0025
  IL_0025:  ret
}");
            // Validate that we emit a hidden sequence point @IL_0007.
            v0.VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""12"">
          <slot kind=""1"" offset=""11"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""8"">
        <entry il_offset=""0x0"" start_row=""6"" start_column=""5"" end_row=""6"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" start_row=""7"" start_column=""9"" end_row=""7"" end_column=""21"" file_ref=""0"" />
        <entry il_offset=""0x7"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x13"" start_row=""9"" start_column=""21"" end_row=""9"" end_column=""49"" file_ref=""0"" />
        <entry il_offset=""0x1a"" start_row=""9"" start_column=""50"" end_row=""9"" end_column=""56"" file_ref=""0"" />
        <entry il_offset=""0x1c"" start_row=""10"" start_column=""21"" end_row=""10"" end_column=""49"" file_ref=""0"" />
        <entry il_offset=""0x23"" start_row=""10"" start_column=""50"" end_row=""10"" end_column=""56"" file_ref=""0"" />
        <entry il_offset=""0x25"" start_row=""12"" start_column=""5"" end_row=""12"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.SwitchStatement), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       40 (0x28)
  .maxstack  2
  .locals init (int V_0)
  IL_0000:  nop
  IL_0001:  call       ""int C.F()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  ldc.i4.1
  IL_0009:  beq.s      IL_0013
  IL_000b:  br.s       IL_000d
  IL_000d:  ldloc.0
  IL_000e:  ldc.i4.2
  IL_000f:  beq.s      IL_001d
  IL_0011:  br.s       IL_0027
  IL_0013:  ldc.i4.s   10
  IL_0015:  call       ""void System.Console.WriteLine(int)""
  IL_001a:  nop
  IL_001b:  br.s       IL_0027
  IL_001d:  ldc.i4.s   20
  IL_001f:  call       ""void System.Console.WriteLine(int)""
  IL_0024:  nop
  IL_0025:  br.s       IL_0027
  IL_0027:  ret
}");
        }

        [Fact]
        public void If()
        {
            var source0 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        if (F())
        {
            System.Console.WriteLine(1);
        }
    }
}";
            var source1 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        if (F())
        {
            System.Console.WriteLine(10);
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.M", @"
{
  // Code size       20 (0x14)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  call       ""bool C.F()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0013
  IL_000a:  nop
  IL_000b:  ldc.i4.1
  IL_000c:  call       ""void System.Console.WriteLine(int)""
  IL_0011:  nop
  IL_0012:  nop
  IL_0013:  ret
}
");
            // Validate presence of a hidden sequence point @IL_0007 that is required for proper function remapping.
            v0.VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""12"">
          <slot kind=""1"" offset=""11"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""7"">
        <entry il_offset=""0x0"" start_row=""7"" start_column=""5"" end_row=""7"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" start_row=""8"" start_column=""9"" end_row=""8"" end_column=""17"" file_ref=""0"" />
        <entry il_offset=""0x7"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0xa"" start_row=""9"" start_column=""9"" end_row=""9"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0xb"" start_row=""10"" start_column=""13"" end_row=""10"" end_column=""41"" file_ref=""0"" />
        <entry il_offset=""0x12"" start_row=""11"" start_column=""9"" end_row=""11"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0x13"" start_row=""12"" start_column=""5"" end_row=""12"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.IfStatement), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       21 (0x15)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  call       ""bool C.F()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0014
  IL_000a:  nop
  IL_000b:  ldc.i4.s   10
  IL_000d:  call       ""void System.Console.WriteLine(int)""
  IL_0012:  nop
  IL_0013:  nop
  IL_0014:  ret
}");
        }

        [Fact]
        public void While()
        {
            var source0 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        while (F())
        {
            System.Console.WriteLine(1);
        }
    }
}";
            var source1 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        while (F())
        {
            System.Console.WriteLine(10);
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.M", @"
{
  // Code size       22 (0x16)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  br.s       IL_000c
  IL_0003:  nop
  IL_0004:  ldc.i4.1
  IL_0005:  call       ""void System.Console.WriteLine(int)""
  IL_000a:  nop
  IL_000b:  nop
  IL_000c:  call       ""bool C.F()""
  IL_0011:  stloc.0
  IL_0012:  ldloc.0
  IL_0013:  brtrue.s   IL_0003
  IL_0015:  ret
}
");
            // Validate presence of a hidden sequence point @IL_0012 that is required for proper function remapping.
            v0.VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""12"">
          <slot kind=""1"" offset=""11"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""8"">
        <entry il_offset=""0x0"" start_row=""7"" start_column=""5"" end_row=""7"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x3"" start_row=""9"" start_column=""9"" end_row=""9"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0x4"" start_row=""10"" start_column=""13"" end_row=""10"" end_column=""41"" file_ref=""0"" />
        <entry il_offset=""0xb"" start_row=""11"" start_column=""9"" end_row=""11"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0xc"" start_row=""8"" start_column=""9"" end_row=""8"" end_column=""20"" file_ref=""0"" />
        <entry il_offset=""0x12"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x15"" start_row=""12"" start_column=""5"" end_row=""12"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.WhileStatement), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       23 (0x17)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  br.s       IL_000d
  IL_0003:  nop
  IL_0004:  ldc.i4.s   10
  IL_0006:  call       ""void System.Console.WriteLine(int)""
  IL_000b:  nop
  IL_000c:  nop
  IL_000d:  call       ""bool C.F()""
  IL_0012:  stloc.0
  IL_0013:  ldloc.0
  IL_0014:  brtrue.s   IL_0003
  IL_0016:  ret
}");
        }

        [Fact]
        public void Do()
        {
            var source0 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        do
        {
            System.Console.WriteLine(1);
        }
        while (F());
    }
}";
            var source1 = @"
class C
{
    static bool F() { return true; }
    
    static void M()
    {
        do
        {
            System.Console.WriteLine(10);
        }
        while (F());
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.M", @"
{
  // Code size       20 (0x14)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  nop
  IL_0002:  ldc.i4.1
  IL_0003:  call       ""void System.Console.WriteLine(int)""
  IL_0008:  nop
  IL_0009:  nop
  IL_000a:  call       ""bool C.F()""
  IL_000f:  stloc.0
  IL_0010:  ldloc.0
  IL_0011:  brtrue.s   IL_0001
  IL_0013:  ret
}");
            // Validate presence of a hidden sequence point @IL_0010 that is required for proper function remapping.
            v0.VerifyPdb("C.M", @"
<symbols>
  <methods>
    <method containingType=""C"" name=""M"" parameterNames="""">
      <customDebugInfo version=""4"" count=""2"">
        <forward version=""4"" kind=""ForwardInfo"" size=""12"" declaringType=""C"" methodName=""F"" parameterNames="""" />
        <encLocalSlotMap version=""4"" kind=""EditAndContinueLocalSlotMap"" size=""12"">
          <slot kind=""1"" offset=""11"" />
        </encLocalSlotMap>
      </customDebugInfo>
      <sequencepoints total=""7"">
        <entry il_offset=""0x0"" start_row=""7"" start_column=""5"" end_row=""7"" end_column=""6"" file_ref=""0"" />
        <entry il_offset=""0x1"" start_row=""9"" start_column=""9"" end_row=""9"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0x2"" start_row=""10"" start_column=""13"" end_row=""10"" end_column=""41"" file_ref=""0"" />
        <entry il_offset=""0x9"" start_row=""11"" start_column=""9"" end_row=""11"" end_column=""10"" file_ref=""0"" />
        <entry il_offset=""0xa"" start_row=""12"" start_column=""9"" end_row=""12"" end_column=""21"" file_ref=""0"" />
        <entry il_offset=""0x10"" hidden=""true"" start_row=""16707566"" start_column=""0"" end_row=""16707566"" end_column=""0"" file_ref=""0"" />
        <entry il_offset=""0x13"" start_row=""13"" start_column=""5"" end_row=""13"" end_column=""6"" file_ref=""0"" />
      </sequencepoints>
      <locals />
    </method>
  </methods>
</symbols>");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.DoStatement), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       21 (0x15)
  .maxstack  1
  .locals init (bool V_0)
  IL_0000:  nop
  IL_0001:  nop
  IL_0002:  ldc.i4.s   10
  IL_0004:  call       ""void System.Console.WriteLine(int)""
  IL_0009:  nop
  IL_000a:  nop
  IL_000b:  call       ""bool C.F()""
  IL_0010:  stloc.0
  IL_0011:  ldloc.0
  IL_0012:  brtrue.s   IL_0001
  IL_0014:  ret
}");
        }

        [Fact]
        public void For()
        {
            var source0 = @"
class C
{
    static bool F(int i) { return true; }
    static void G(int i) { }
    
    static void M()
    {
        for (int i = 1; F(i); G(i))
        {
            System.Console.WriteLine(1);
        }
    }
}";
            var source1 = @"
class C
{
    static bool F(int i) { return true; }
    static void G(int i) { }
    
    static void M()
    {
        for (int i = 1; F(i); G(i))
        {
            System.Console.WriteLine(10);
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source0, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source1, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);

            // Validate presence of a hidden sequence point @IL_001c that is required for proper function remapping.
            v0.VerifyIL("C.M", @"
{
  // Code size       32 (0x20)
  .maxstack  1
  .locals init (int V_0, //i
                bool V_1)
 -IL_0000:  nop
 -IL_0001:  ldc.i4.1
  IL_0002:  stloc.0
 ~IL_0003:  br.s       IL_0015
 -IL_0005:  nop
 -IL_0006:  ldc.i4.1
  IL_0007:  call       ""void System.Console.WriteLine(int)""
  IL_000c:  nop
 -IL_000d:  nop
 -IL_000e:  ldloc.0
  IL_000f:  call       ""void C.G(int)""
  IL_0014:  nop
 -IL_0015:  ldloc.0
  IL_0016:  call       ""bool C.F(int)""
  IL_001b:  stloc.1
 ~IL_001c:  ldloc.1
  IL_001d:  brtrue.s   IL_0005
 -IL_001f:  ret
}", sequencePoints: "C.M");

            var methodData0 = v0.TestData.GetMethodData("C.M");
            var method0 = compilation0.GetMember<MethodSymbol>("C.M");
            var method1 = compilation1.GetMember<MethodSymbol>("C.M");
            var generation0 = EmitBaseline.CreateInitialBaseline(ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetSyntaxMapByKind(method0, SyntaxKind.ForStatement, SyntaxKind.VariableDeclarator), preserveLocalVariables: true)));

            diff1.VerifyIL("C.M", @"
{
  // Code size       33 (0x21)
  .maxstack  1
  .locals init (int V_0, //i
                bool V_1)
  IL_0000:  nop
  IL_0001:  ldc.i4.1
  IL_0002:  stloc.0
  IL_0003:  br.s       IL_0016
  IL_0005:  nop
  IL_0006:  ldc.i4.s   10
  IL_0008:  call       ""void System.Console.WriteLine(int)""
  IL_000d:  nop
  IL_000e:  nop
  IL_000f:  ldloc.0
  IL_0010:  call       ""void C.G(int)""
  IL_0015:  nop
  IL_0016:  ldloc.0
  IL_0017:  call       ""bool C.F(int)""
  IL_001c:  stloc.1
  IL_001d:  ldloc.1
  IL_001e:  brtrue.s   IL_0005
  IL_0020:  ret
}
");
        }

#if FEATURE_CSHARP6_CUT
        [Fact]
        public void SynthesizedVariablesInPrimaryConstructorBody1()
        {
            var source = @"
class C(object a)
{
    string b = a.ToString();

    { lock(a) { } }
}
";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C..ctor", @"
{
  // Code size       50 (0x32)
  .maxstack  2
  .locals init (object V_0,
                bool V_1)
  IL_0000:  ldarg.0
  IL_0001:  ldarg.1
  IL_0002:  callvirt   ""string object.ToString()""
  IL_0007:  stfld      ""string C.b""
  IL_000c:  ldarg.0
  IL_000d:  call       ""object..ctor()""
  IL_0012:  nop
  IL_0013:  nop
  IL_0014:  ldc.i4.0
  IL_0015:  stloc.1
  .try
  {
    IL_0016:  ldarg.1
    IL_0017:  stloc.0
    IL_0018:  ldloc.0
    IL_0019:  ldloca.s   V_1
    IL_001b:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0020:  nop
    IL_0021:  nop
    IL_0022:  nop
    IL_0023:  leave.s    IL_0030
  }
  finally
  {
    IL_0025:  ldloc.1
    IL_0026:  brfalse.s  IL_002f
    IL_0028:  ldloc.0
    IL_0029:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_002e:  nop
    IL_002f:  endfinally
  }
  IL_0030:  nop
  IL_0031:  ret
}");

            var methodData0 = v0.TestData.GetMethodData("C..ctor");
            var method0 = compilation0.GetMember<MethodSymbol>("C..ctor");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), methodData0.EncDebugInfoProvider());

            var method1 = compilation1.GetMember<MethodSymbol>("C..ctor");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C..ctor", @"
{
  // Code size       50 (0x32)
  .maxstack  2
  .locals init (object V_0,
                bool V_1)
 -IL_0000:  ldarg.0
  IL_0001:  ldarg.1
  IL_0002:  callvirt   ""string object.ToString()""
  IL_0007:  stfld      ""string C.b""
  IL_000c:  ldarg.0
  IL_000d:  call       ""object..ctor()""
  IL_0012:  nop
 -IL_0013:  nop
 ~IL_0014:  ldc.i4.0
  IL_0015:  stloc.1
  .try
  {
   -IL_0016:  ldarg.1
    IL_0017:  stloc.0
    IL_0018:  ldloc.0
    IL_0019:  ldloca.s   V_1
    IL_001b:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0020:  nop
   -IL_0021:  nop
   -IL_0022:  nop
    IL_0023:  leave.s    IL_0030
  }
  finally
  {
   ~IL_0025:  ldloc.1
    IL_0026:  brfalse.s  IL_002f
    IL_0028:  ldloc.0
    IL_0029:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_002e:  nop
    IL_002f:  endfinally
  }
 -IL_0030:  nop
 -IL_0031:  ret
}", methodToken: diff1.UpdatedMethods.Single());
        }

        [Fact]
        public void UserVariablesInInitializers1()
        {
            var source = @"
class B(int a) {}

class C(int x) : B(var b = 2)
{
    string b = (var a = 1).ToString();
}
";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);

            var v0 = CompileAndVerify(compilation0);
            // TODO:
        }
#endif
        [Fact]
        public void SynthesizedVariablesInLambdas1()
        {
            var source =
@"class C
{
    static object F()
    {
        return null;
    }
    static void M()
    {
        lock (F())
        {
            var f = new System.Action(() => 
            {
                lock (F())
                {
                }
            });
        }
    }
}";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll);

            var v0 = CompileAndVerify(compilation0);
            v0.VerifyIL("C.<>c__DisplayClass0.<M>b__1()", @"
{
  // Code size       36 (0x24)
  .maxstack  2
  .locals init (object V_0,
                bool V_1)
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stloc.1
  .try
  {
    IL_0003:  call       ""object C.F()""
    IL_0008:  stloc.0
    IL_0009:  ldloc.0
    IL_000a:  ldloca.s   V_1
    IL_000c:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0011:  nop
    IL_0012:  nop
    IL_0013:  nop
    IL_0014:  leave.s    IL_0021
  }
  finally
  {
    IL_0016:  ldloc.1
    IL_0017:  brfalse.s  IL_0020
    IL_0019:  ldloc.0
    IL_001a:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_001f:  nop
    IL_0020:  endfinally
  }
  IL_0021:  br.s       IL_0023
  IL_0023:  ret
}");


#if TODO // identify the lambda in a semantic edit
            var methodData0 = v0.TestData.GetMethodData("C.<M>b__0");
            var method0 = compilation0.GetMember<MethodSymbol>("C.<M>b__0");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), m => GetLocalNames(methodData0));

            var method1 = compilation1.GetMember<MethodSymbol>("C.<M>b__0");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("C.<M>b__0", @"
", methodToken: diff1.UpdatedMethods.Single());
#endif
        }

        [Fact]
        public void SyntheziedVariablesInIterator1()
        {
            var source = @"
using System.Collections.Generic;

class C
{
    public IEnumerable<int> F()
    {
        lock (F()) { }
        yield return 1;
    }
}
";
            var compilation0 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);
            var compilation1 = CreateCompilationWithMscorlib(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.<F>d__0.System.Collections.IEnumerator.MoveNext", @"
{
  // Code size      101 (0x65)
  .maxstack  2
  .locals init (int V_0,
                bool V_1,
                System.Collections.Generic.IEnumerable<int> V_2,
                bool V_3)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<F>d__0.<>1__state""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0012
  IL_000a:  br.s       IL_000c
  IL_000c:  ldloc.0
  IL_000d:  ldc.i4.1
  IL_000e:  beq.s      IL_0014
  IL_0010:  br.s       IL_0016
  IL_0012:  br.s       IL_001a
  IL_0014:  br.s       IL_005a
  IL_0016:  ldc.i4.0
  IL_0017:  stloc.1
  IL_0018:  ldloc.1
  IL_0019:  ret
  IL_001a:  ldarg.0
  IL_001b:  ldc.i4.m1
  IL_001c:  stfld      ""int C.<F>d__0.<>1__state""
  IL_0021:  nop
  IL_0022:  ldc.i4.0
  IL_0023:  stloc.3
  .try
  {
    IL_0024:  ldarg.0
    IL_0025:  ldfld      ""C C.<F>d__0.<>4__this""
    IL_002a:  callvirt   ""System.Collections.Generic.IEnumerable<int> C.F()""
    IL_002f:  stloc.2
    IL_0030:  ldloc.2
    IL_0031:  ldloca.s   V_3
    IL_0033:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
    IL_0038:  nop
    IL_0039:  nop
    IL_003a:  nop
    IL_003b:  leave.s    IL_0048
  }
  finally
  {
    IL_003d:  ldloc.3
    IL_003e:  brfalse.s  IL_0047
    IL_0040:  ldloc.2
    IL_0041:  call       ""void System.Threading.Monitor.Exit(object)""
    IL_0046:  nop
    IL_0047:  endfinally
  }
  IL_0048:  ldarg.0
  IL_0049:  ldc.i4.1
  IL_004a:  stfld      ""int C.<F>d__0.<>2__current""
  IL_004f:  ldarg.0
  IL_0050:  ldc.i4.1
  IL_0051:  stfld      ""int C.<F>d__0.<>1__state""
  IL_0056:  ldc.i4.1
  IL_0057:  stloc.1
  IL_0058:  br.s       IL_0018
  IL_005a:  ldarg.0
  IL_005b:  ldc.i4.m1
  IL_005c:  stfld      ""int C.<F>d__0.<>1__state""
  IL_0061:  ldc.i4.0
  IL_0062:  stloc.1
  IL_0063:  br.s       IL_0018
}");

#if TODO 
            var methodData0 = v0.TestData.GetMethodData("?");
            var method0 = compilation0.GetMember<MethodSymbol>("?");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), m => GetLocalNames(methodData0));

            var method1 = compilation1.GetMember<MethodSymbol>("?");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("?", @"
{", methodToken: diff1.UpdatedMethods.Single());
#endif
        }

        [Fact]
        public void SyntheziedVariablesInAsyncMethod1()
        {
            var source = @"
using System.Threading.Tasks;

class C
{
    public async Task<int> F()
    {
        lock (F()) { }
        await F();
        return 1;
    }
}
";
            var compilation0 = CreateCompilationWithMscorlib45(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);
            var compilation1 = CreateCompilationWithMscorlib45(source, options: TestOptions.DebugDll, parseOptions: TestOptions.ExperimentalParseOptions);

            var v0 = CompileAndVerify(compilation0);

            v0.VerifyIL("C.<F>d__1.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext", @"
{
  // Code size      220 (0xdc)
  .maxstack  3
  .locals init (int V_0,
                int V_1,
                System.Threading.Tasks.Task<int> V_2,
                bool V_3,
                System.Runtime.CompilerServices.TaskAwaiter<int> V_4,
                C.<F>d__1 V_5,
                System.Exception V_6)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<F>d__1.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_000c
    IL_000a:  br.s       IL_000e
    IL_000c:  br.s       IL_007a
    IL_000e:  nop
    IL_000f:  ldc.i4.0
    IL_0010:  stloc.3
    .try
    {
      IL_0011:  ldarg.0
      IL_0012:  ldfld      ""C C.<F>d__1.<>4__this""
      IL_0017:  callvirt   ""System.Threading.Tasks.Task<int> C.F()""
      IL_001c:  stloc.2
      IL_001d:  ldloc.2
      IL_001e:  ldloca.s   V_3
      IL_0020:  call       ""void System.Threading.Monitor.Enter(object, ref bool)""
      IL_0025:  nop
      IL_0026:  nop
      IL_0027:  nop
      IL_0028:  leave.s    IL_0039
    }
    finally
    {
      IL_002a:  ldloc.0
      IL_002b:  ldc.i4.0
      IL_002c:  bge.s      IL_0038
      IL_002e:  ldloc.3
      IL_002f:  brfalse.s  IL_0038
      IL_0031:  ldloc.2
      IL_0032:  call       ""void System.Threading.Monitor.Exit(object)""
      IL_0037:  nop
      IL_0038:  endfinally
    }
    IL_0039:  ldarg.0
    IL_003a:  ldfld      ""C C.<F>d__1.<>4__this""
    IL_003f:  callvirt   ""System.Threading.Tasks.Task<int> C.F()""
    IL_0044:  callvirt   ""System.Runtime.CompilerServices.TaskAwaiter<int> System.Threading.Tasks.Task<int>.GetAwaiter()""
    IL_0049:  stloc.s    V_4
    IL_004b:  ldloca.s   V_4
    IL_004d:  call       ""bool System.Runtime.CompilerServices.TaskAwaiter<int>.IsCompleted.get""
    IL_0052:  brtrue.s   IL_0097
    IL_0054:  ldarg.0
    IL_0055:  ldc.i4.0
    IL_0056:  dup
    IL_0057:  stloc.0
    IL_0058:  stfld      ""int C.<F>d__1.<>1__state""
    IL_005d:  ldarg.0
    IL_005e:  ldloc.s    V_4
    IL_0060:  stfld      ""System.Runtime.CompilerServices.TaskAwaiter<int> C.<F>d__1.<>u__$awaiter0""
    IL_0065:  ldarg.0
    IL_0066:  stloc.s    V_5
    IL_0068:  ldarg.0
    IL_0069:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int> C.<F>d__1.<>t__builder""
    IL_006e:  ldloca.s   V_4
    IL_0070:  ldloca.s   V_5
    IL_0072:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<int>, C.<F>d__1>(ref System.Runtime.CompilerServices.TaskAwaiter<int>, ref C.<F>d__1)""
    IL_0077:  nop
    IL_0078:  leave.s    IL_00db
    IL_007a:  ldarg.0
    IL_007b:  ldfld      ""System.Runtime.CompilerServices.TaskAwaiter<int> C.<F>d__1.<>u__$awaiter0""
    IL_0080:  stloc.s    V_4
    IL_0082:  ldarg.0
    IL_0083:  ldflda     ""System.Runtime.CompilerServices.TaskAwaiter<int> C.<F>d__1.<>u__$awaiter0""
    IL_0088:  initobj    ""System.Runtime.CompilerServices.TaskAwaiter<int>""
    IL_008e:  ldarg.0
    IL_008f:  ldc.i4.m1
    IL_0090:  dup
    IL_0091:  stloc.0
    IL_0092:  stfld      ""int C.<F>d__1.<>1__state""
    IL_0097:  ldloca.s   V_4
    IL_0099:  call       ""int System.Runtime.CompilerServices.TaskAwaiter<int>.GetResult()""
    IL_009e:  pop
    IL_009f:  ldloca.s   V_4
    IL_00a1:  initobj    ""System.Runtime.CompilerServices.TaskAwaiter<int>""
    IL_00a7:  ldc.i4.1
    IL_00a8:  stloc.1
    IL_00a9:  leave.s    IL_00c6
  }
  catch System.Exception
  {
    IL_00ab:  stloc.s    V_6
    IL_00ad:  nop
    IL_00ae:  ldarg.0
    IL_00af:  ldc.i4.s   -2
    IL_00b1:  stfld      ""int C.<F>d__1.<>1__state""
    IL_00b6:  ldarg.0
    IL_00b7:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int> C.<F>d__1.<>t__builder""
    IL_00bc:  ldloc.s    V_6
    IL_00be:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int>.SetException(System.Exception)""
    IL_00c3:  nop
    IL_00c4:  leave.s    IL_00db
  }
  IL_00c6:  ldarg.0
  IL_00c7:  ldc.i4.s   -2
  IL_00c9:  stfld      ""int C.<F>d__1.<>1__state""
  IL_00ce:  ldarg.0
  IL_00cf:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int> C.<F>d__1.<>t__builder""
  IL_00d4:  ldloc.1
  IL_00d5:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int>.SetResult(int)""
  IL_00da:  nop
  IL_00db:  ret
}
");

#if TODO
            var methodData0 = v0.TestData.GetMethodData("?");
            var method0 = compilation0.GetMember<MethodSymbol>("?");
            var generation0 = EmitBaseline.CreateInitialBaseline(
                ModuleMetadata.CreateFromImage(v0.EmittedAssemblyData), m => GetLocalNames(methodData0));

            var method1 = compilation1.GetMember<MethodSymbol>("?");
            var diff1 = compilation1.EmitDifference(
                generation0,
                ImmutableArray.Create(new SemanticEdit(SemanticEditKind.Update, method0, method1, GetEquivalentNodesMap(method1, method0), preserveLocalVariables: true)));

            diff1.VerifyIL("?", @"
{", methodToken: diff1.UpdatedMethods.Single());
#endif
        }
    }
}
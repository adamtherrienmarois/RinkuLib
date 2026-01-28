using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using RinkuLib.Commands;
using RinkuLib.DbParsing;
using RinkuLib.Tools;

namespace Test;

/*
Input:

Method(object)

Method(string, object)

Method(string, string, int)

Result:

Method(string, string, int) (Most params + specific types)

Method(string, object) (More params than #3)

Method(object) (Least specific)

Test 2: Incomparable Types (Stable Order)
Input:

Method(int, int)

Method(string, string)

Result:

Method(int, int)

Method(string, string) (Neither is more specific, so they stay in original order)

Test 3: The "Subset" Rule
Input:

Method(string, object)

Method(string, string)

Result:

Method(string, string) (Type quality over same length)

Method(string, object)

Test 4: Length vs. Specificity Conflict
Input:

Method(string, object, int)

Method(string, string)

Result:

Method(string, object, int)

Method(string, string) (Note: Item #1 is more specific because it has more parameters and its prefix matches #2's assignability rules)

Test 5: Complex Mix
Input:

Method(object)

Method(int, int)

Method(string, object)

Method(string)

Result:

Method(int, int) (Incomparable to others, stays at top)

Method(string, object) (More specific than #4 and #1)

Method(string) (More specific than #1)

Method(object) (Moved to end)

Test 6:

Method(object)
Method(int, int)
Method(string)
Method(string, object, int)
Method(string, string)
Method(string, object)
Method(string, string, int)*/
 
interface IBase { }
interface IDerived : IBase { }
interface IAdvanced : IDerived { }
interface IBase2 { }
interface IDerived2 : IBase2 { }
interface IAdvanced2 : IDerived2 { }
public class DMet(string name) : MethodBase {
    [ModuleInitializer]
    public static void Init() {
        var info = TypeParsingInfo.GetOrAdd(typeof(KeyValuePair<,>));
        info.AddAltName("key", "ID");
        info.SetJumpWhenNull("key", true);
    }
    private readonly string _name = name;
    public override string Name => _name;
    // Minimal overrides for abstract MethodBase
    public override MethodAttributes Attributes => throw new NotImplementedException();
    public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();
    public override Type DeclaringType => throw new NotImplementedException();
    public override Type ReflectedType => throw new NotImplementedException();
    public override MemberTypes MemberType => throw new NotImplementedException();
    public override object[] GetCustomAttributes(bool inherit) => null;
    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => null;
    public override bool IsDefined(Type attributeType, bool inherit) => false;
    public override MethodImplAttributes GetMethodImplementationFlags() => throw new NotImplementedException();
    public override ParameterInfo[] GetParameters() => throw new NotImplementedException();
    public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture) => null;
}
// A class implementing the most specific one
class MyImplementation : IAdvanced {
    public static bool Test(IBase adv, IBase2 adv2) => true;
    public static bool Test(IBase adv, IDerived2 adv2) => false;
}

// A class implementing multiple branches to test order
interface IOther { }
class MultiImplementation : IAdvanced, IOther { }
class MultiImplementation2 : IAdvanced2, IOther { }
[ShortRunJob]
public class BoolVsUlong {
    private readonly bool[] _bools = new bool[64];
    private ulong _bits;

    [Params(0, 7, 15, 31, 50)]
    public int Index;

    // --------------------
    // BOOL[]
    // --------------------

    [Benchmark]
    public bool Bool_Get() {
        return _bools[Index];
    }

    [Benchmark]
    public void Bool_SetTrue() {
        _bools[Index] = true;
    }

    [Benchmark]
    public void Bool_SetFalse() {
        _bools[Index] = false;
    }

    // --------------------
    // ULONG BITMASK
    // --------------------

    [Benchmark]
    public bool Ulong_Get() {
        return (_bits & (1UL << Index)) != 0;
    }

    [Benchmark]
    public void Ulong_SetTrue() {
        _bits |= 1UL << Index;
    }

    [Benchmark]
    public void Ulong_SetFalse() {
        _bits &= ~(1UL << Index);
    }
}

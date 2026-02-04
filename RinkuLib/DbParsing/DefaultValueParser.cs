using System.Reflection.Emit;

namespace RinkuLib.DbParsing;

public class DefaultValueParser(Type targetType, object? defaultValue) : DbItemParser {
    public override bool NeedNullSetPoint(ColumnInfo[] cols) => false;
    public override bool IsSequencial(ref int previousIndex) => true;

    public override void Emit(ColumnInfo[] cols, Generator generator, NullSetPoint nullSetPoint) {
        var under = Nullable.GetUnderlyingType(targetType);
        var t = under ?? targetType;

        if (defaultValue == null) {
            if (targetType.IsValueType && under == null)
                throw new InvalidOperationException($"Cannot assign null to non-nullable type {targetType.Name}");
            if (under is null) {
                generator.Emit(OpCodes.Ldnull);
                return;
            }
            var local = generator.GetLocal(targetType);
            generator.Emit(OpCodes.Ldloca, local);
            generator.Emit(OpCodes.Initobj, targetType);
            generator.Emit(OpCodes.Ldloc, local);
            return;
        }
        EmitConstant(generator, defaultValue);
        if (under is not null)
            generator.Emit(OpCodes.Newobj, under.GetNullableConstructor());
    }

    private static void EmitConstant(Generator generator, object value) {
        switch (value) {
            case int i:
                generator.Emit(OpCodes.Ldc_I4, i);
                break;
            case long l:
                generator.Emit(OpCodes.Ldc_I8, l);
                break;
            case float f:
                generator.Emit(OpCodes.Ldc_R4, f);
                break;
            case double d:
                generator.Emit(OpCodes.Ldc_R8, d);
                break;
            case bool b:
                generator.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;
            case string s:
                generator.Emit(OpCodes.Ldstr, s);
                break;
            case byte b:
                generator.Emit(OpCodes.Ldc_I4, (int)b);
                break;
            case short s:
                generator.Emit(OpCodes.Ldc_I4, (int)s);
                break;
            case char c:
                generator.Emit(OpCodes.Ldc_I4, (int)c);
                break;
            default:
                throw new NotSupportedException($"IL constant emission for {value.GetType().Name} is not implemented.");
        }
    }
}
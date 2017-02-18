using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace BlueMagic
{
    public static class MarshalType<T> where T : struct
    {
        public static Type Type { get; private set; }
        public static TypeCode TypeCode { get; private set; }
        public static int Size { get; private set; }
        public static bool IsIntPtr { get; private set; }
        public static bool HasUnmanagedTypes { get; private set; }
        internal unsafe delegate void* GetPointerDelegate(ref T generic);
        internal static readonly GetPointerDelegate GetPointer;

        static MarshalType()
        {
            Type = typeof(T);
            if (Type.IsEnum)
                Type = Type.GetEnumUnderlyingType();

            TypeCode = Type.GetTypeCode(Type);
            Size = Marshal.SizeOf(Type);
            IsIntPtr = Type == typeof(IntPtr);

            HasUnmanagedTypes =
                Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(
                    m => m.GetCustomAttributes(typeof(MarshalAsAttribute), true).Any());

            DynamicMethod method = new DynamicMethod(
                $"GetPinnedPointer<{Type.FullName.Replace(".", "<>")}>",
                typeof(void*),
                new[] { Type.MakeByRefType() },
                typeof(MarshalType<>).Module);

            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Conv_U);
            gen.Emit(OpCodes.Ret);
            GetPointer = (GetPointerDelegate)method.CreateDelegate(typeof(GetPointerDelegate));
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace BlueMagic
{
    public static class MarshalType<T>
    {
        public static Type Type { get; private set; }
        public static TypeCode TypeCode { get; private set; }
        public static IntPtr Size { get; private set; }
        public static bool IsIntPtr { get; private set; }
        public static bool TypeRequiresMarshal { get; private set; }
        internal unsafe delegate void* GetPointerDelegate(ref T value);
        internal static readonly GetPointerDelegate GetPointer;

        static MarshalType()
        {
            Type = typeof(T);
            if (Type.IsEnum)
                Type = Type.GetEnumUnderlyingType();

            TypeCode = Type.GetTypeCode(Type);
            Size = new IntPtr(Marshal.SizeOf(Type));
            IsIntPtr = Type == typeof(IntPtr);

            TypeRequiresMarshal =
                Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(
                    m => m.GetCustomAttributes(typeof(MarshalAsAttribute), true).Any());

            DynamicMethod method = new DynamicMethod(
                string.Format("GetPinnedPtr<{0}>", Type.FullName.Replace(".", "<>")),
                typeof(void*),
                new[]
                {
                    Type.MakeByRefType()
                },
                typeof(MarshalType<>).Module);

            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Conv_U);
            gen.Emit(OpCodes.Ret);
            GetPointer = (GetPointerDelegate)method.CreateDelegate(typeof(GetPointerDelegate));
        }

        public static unsafe T PointerToGenericType(IntPtr pointer)
        {
            switch (TypeCode)
            {
                case TypeCode.Object:
                    if (IsIntPtr)
                        return (T)(object)*(IntPtr*)pointer;

                    if (!TypeRequiresMarshal)
                    {
                        T generic = default(T);
                        void* ptr = GetPointer(ref generic);
                        Native.Imports.MoveMemory(ptr, pointer.ToPointer(), Size);
                        return generic;
                    }

                    return (T)Marshal.PtrToStructure(pointer, typeof(T));
                case TypeCode.Boolean:
                    return (T)(object)*(bool*)pointer;
                case TypeCode.SByte:
                    return (T)(object)*(sbyte*)pointer;
                case TypeCode.Byte:
                    return (T)(object)*(byte*)pointer;
                case TypeCode.Int16:
                    return (T)(object)*(short*)pointer;
                case TypeCode.UInt16:
                    return (T)(object)*(ushort*)pointer;
                case TypeCode.Int32:
                    return (T)(object)*(int*)pointer;
                case TypeCode.UInt32:
                    return (T)(object)*(uint*)pointer;
                case TypeCode.Int64:
                    return (T)(object)*(long*)pointer;
                case TypeCode.UInt64:
                    return (T)(object)*(ulong*)pointer;
                case TypeCode.Single:
                    return (T)(object)*(float*)pointer;
                case TypeCode.Double:
                    return (T)(object)*(double*)pointer;
                default:
                    throw new ArgumentException("Unsupported Type");
            }
        }

        public static byte[] GenericTypeToBytes(T generic)
        {
            switch (TypeCode)
            {
                case TypeCode.Object:
                    if (IsIntPtr)
                    {
                        switch (Size.ToInt32())
                        {
                            case 4:
                                return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt32());
                            case 8:
                                return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt64());
                        }
                    }

                    IntPtr ptr = Marshal.AllocHGlobal(Size);
                    Marshal.StructureToPtr(generic, ptr, false);
                    byte[] bytes = new byte[Size.ToInt32()];
                    Marshal.Copy(ptr, bytes, 0, Size.ToInt32());
                    return bytes;
                case TypeCode.Boolean:
                    return BitConverter.GetBytes((bool)(object)generic);
                case TypeCode.SByte:
                    return BitConverter.GetBytes((sbyte)(object)generic);
                case TypeCode.Byte:
                    return BitConverter.GetBytes((byte)(object)generic);
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)(object)generic);
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)(object)generic);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)(object)generic);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((uint)(object)generic);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)(object)generic);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)(object)generic);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)(object)generic);
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)(object)generic);
                default:
                    throw new ArgumentException("Unsupported Type");
            }
        }

        public static T BytesToGenericType(byte[] bytes)
        {
            switch (TypeCode)
            {
                case TypeCode.Object:
                    if (IsIntPtr)
                    {
                        switch (bytes.Length)
                        {
                            case 1:
                                return (T)(object)new IntPtr(bytes[0]);
                            case 2:
                                return (T)(object)new IntPtr(BitConverter.ToInt16(bytes, 0));
                            case 4:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(bytes, 0));
                            case 8:
                                return (T)(object)new IntPtr(BitConverter.ToInt64(bytes, 0));
                        }
                    }

                    IntPtr ptr = Marshal.AllocHGlobal(Size);
                    Marshal.Copy(bytes, 0, ptr, Size.ToInt32());
                    return (T)Marshal.PtrToStructure(ptr, typeof(T));
                case TypeCode.Boolean:
                    return (T)(object)BitConverter.ToBoolean(bytes, 0);
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return (T)(object)bytes[0];
                case TypeCode.Int16:
                    return (T)(object)BitConverter.ToInt16(bytes, 0);
                case TypeCode.UInt16:
                    return (T)(object)BitConverter.ToUInt16(bytes, 0);
                case TypeCode.Int32:
                    return (T)(object)BitConverter.ToInt32(bytes, 0);
                case TypeCode.UInt32:
                    return (T)(object)BitConverter.ToUInt32(bytes, 0);
                case TypeCode.Int64:
                    return (T)(object)BitConverter.ToInt64(bytes, 0);
                case TypeCode.UInt64:
                    return (T)(object)BitConverter.ToUInt64(bytes, 0);
                case TypeCode.Single:
                    return (T)(object)BitConverter.ToSingle(bytes, 0);
                case TypeCode.Double:
                    return (T)(object)BitConverter.ToDouble(bytes, 0);
                default:
                    throw new ArgumentException("Unsupported Type");
            }
        }
    }
}

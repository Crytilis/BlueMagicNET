using BlueMagic.Memory;
using System;
using System.Runtime.InteropServices;

namespace BlueMagic
{
    public static unsafe class TypeConverter
    {
        public static void* GenericTypeToPointer<T>(T generic) where T : struct
        {
            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return MarshalType<T>.GetPointer(ref generic);
                default:
                    throw new ArgumentException("Unsupported Type");
            }
        }

        public static T PointerToGenericType<T>(IntPtr pointer) where T : struct
        {
            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    if (MarshalType<T>.IsIntPtr)
                        return (T)(object)*(IntPtr*)pointer;

                    if (!MarshalType<T>.TypeRequiresMarshal)
                    {
                        T generic = default(T);
                        void* genericPtr = MarshalType<T>.GetPointer(ref generic);
                        Manager.Copy(genericPtr, pointer.ToPointer(), MarshalType<T>.Size);
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

        public static byte[] GenericTypeToBytes<T>(T generic) where T : struct
        {
            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    int size = MarshalType<T>.Size;

                    if (MarshalType<T>.IsIntPtr)
                    {
                        switch (size)
                        {
                            case 4:
                                return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt32());
                            case 8:
                                return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt64());
                        }
                    }

                    byte[] bytes = new byte[size];

                    if (!MarshalType<T>.TypeRequiresMarshal)
                    {
                        void* genericPtr = MarshalType<T>.GetPointer(ref generic);
                        fixed (byte* bytesPtr = bytes)
                        {
                            Manager.Copy(bytesPtr, genericPtr, size);
                            return bytes;
                        }
                    }

                    IntPtr ptr = Marshal.AllocHGlobal(size);

                    try
                    {
                        Marshal.StructureToPtr(generic, ptr, false);
                        Marshal.Copy(ptr, bytes, 0, size);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

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

        public static T BytesToGenericType<T>(byte[] bytes) where T : struct
        {
            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    if (MarshalType<T>.IsIntPtr)
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

                    int size = MarshalType<T>.Size;
                    T generic = default(T);

                    if (!MarshalType<T>.TypeRequiresMarshal)
                    {
                        void* genericPtr = MarshalType<T>.GetPointer(ref generic);
                        fixed (byte* bytesPtr = bytes)
                        {
                            Manager.Copy(genericPtr, bytesPtr, size);
                            return generic;
                        }
                    }

                    IntPtr ptr = Marshal.AllocHGlobal(size);

                    try
                    {
                        Marshal.Copy(bytes, 0, ptr, size);
                        generic = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

                    return generic;
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

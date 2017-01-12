using BlueMagic.Native;
using System;
using System.Runtime.InteropServices;

namespace BlueMagic.Memory
{
    public static class Manager
    {
        public static IntPtr Allocate(int size, [Optional] IntPtr address)
        {
            return Methods.Allocate(address, size, MemoryProtectionType.PAGE_EXECUTE_READWRITE);
        }

        public static IntPtr Allocate(int size, SafeMemoryHandle processHandle, [Optional] IntPtr address)
        {
            return Methods.Allocate(processHandle, address, size, MemoryProtectionType.PAGE_EXECUTE_READWRITE);
        }

        public static void Free(IntPtr address)
        {
            Methods.Free(address, 0, MemoryFreeType.MEM_RELEASE);
        }

        public static void Free(SafeMemoryHandle processHandle, IntPtr address)
        {
            Methods.Free(processHandle, address, 0, MemoryFreeType.MEM_RELEASE);
        }

        public static unsafe void Copy(void* destination, void* source, int size)
        {
            Imports.MoveMemory(destination, source, size);
        }
    }
}

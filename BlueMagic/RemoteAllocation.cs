using System;
using System.Runtime.InteropServices;

namespace BlueMagic
{
    public class RemoteAllocation : IDisposable
    {
        public int Size { get; private set; }
        public SafeMemoryHandle ProcessHandle { get; private set; }
        public IntPtr AllocationBase { get; private set; }

        public RemoteAllocation(int size, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = null;
            AllocationBase = MemoryManager.Allocate(Size, address);
        }

        public RemoteAllocation(int size, SafeMemoryHandle processHandle, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = processHandle;
            AllocationBase = MemoryManager.Allocate(Size, ProcessHandle, address);
        }

        ~RemoteAllocation()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (ProcessHandle == null)
                MemoryManager.Free(AllocationBase);
            else
                MemoryManager.Free(ProcessHandle, AllocationBase);

            ProcessHandle = null;
            AllocationBase = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
    }
}

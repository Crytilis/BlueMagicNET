using System;
using System.Runtime.InteropServices;

namespace BlueMagic.Memory
{
    public class ManagedAllocation : IDisposable
    {
        public int Size { get; private set; }
        public SafeMemoryHandle ProcessHandle { get; private set; }
        public IntPtr AllocationBase { get; private set; }

        public ManagedAllocation(int size, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = null;
            AllocationBase = Manager.Allocate(Size, address);
        }

        public ManagedAllocation(int size, SafeMemoryHandle processHandle, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = processHandle;
            AllocationBase = Manager.Allocate(Size, ProcessHandle, address);
        }

        ~ManagedAllocation()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (ProcessHandle == null)
                Manager.Free(AllocationBase);
            else
                Manager.Free(ProcessHandle, AllocationBase);

            GC.SuppressFinalize(this);
        }
    }
}

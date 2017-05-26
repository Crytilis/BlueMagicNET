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
            AllocationBase = NativeMethods.Allocate(address, Size);
        }

        public RemoteAllocation(int size, SafeMemoryHandle processHandle, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = processHandle;
            AllocationBase = NativeMethods.Allocate(ProcessHandle, address, Size);
        }

        ~RemoteAllocation()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (ProcessHandle == null)
                NativeMethods.Free(AllocationBase);
            else
                NativeMethods.Free(ProcessHandle, AllocationBase);

            ProcessHandle = null;
            AllocationBase = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
    }
}

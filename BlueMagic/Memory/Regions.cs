using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlueMagic.Memory
{
    public static class Regions
    {
        private static List<Native.MemoryBasicInformation> Scan(SafeMemoryHandle processHandle, ProcessModule[] processModules, int moduleIndex, int endModule)
        {
            List<Native.MemoryBasicInformation> regions = new List<Native.MemoryBasicInformation>();

            for (; moduleIndex < endModule; ++moduleIndex)
            {
                IntPtr start = processModules[moduleIndex].BaseAddress;
                IntPtr end = moduleIndex + 1 > processModules.Length - 1 ? new IntPtr(processModules[moduleIndex].ModuleMemorySize + 1) : processModules[moduleIndex + 1].BaseAddress;
                IntPtr seek = start;

                do
                {
                    Native.MemoryBasicInformation region = Native.Methods.Query(processHandle, seek, MarshalType<Native.MemoryBasicInformation>.Size);
                    if ((region.State & Native.MemoryAllocationState.MEM_COMMIT) != 0 && (region.Protect & (Native.MemoryProtectionType)0x701) == 0)
                        regions.Add(region);

                    seek = new IntPtr(region.BaseAddress.ToInt64() + region.RegionSize.ToInt64());
                }
                while (seek.ToInt64() < end.ToInt64());
            }

            return regions;
        }

        private static List<Native.MemoryBasicInformation> ScanAll(SafeMemoryHandle processHandle, ProcessModule[] processModules)
        {
            return Scan(processHandle, processModules, 0, processModules.Length);
        }

        public static List<Native.MemoryBasicInformation> Load(Process process, SafeMemoryHandle processHandle, ProcessModule processModule)
        {
            ProcessModule[] ProcessModules = GetProcessModules(process);
            int i = Array.FindIndex(ProcessModules, m => m.ModuleName == processModule.ModuleName);
            return Scan(processHandle, ProcessModules, i, i + 1);
        }

        public static List<Native.MemoryBasicInformation> LoadAll(Process process, SafeMemoryHandle processHandle)
        {
            ProcessModule[] ProcessModules = GetProcessModules(process);
            return ScanAll(processHandle, ProcessModules);
        }

        private static ProcessModule[] GetProcessModules(Process process)
        {
            ProcessModule[] ProcessModules = new ProcessModule[process.Modules.Count];
            process.Modules.CopyTo(ProcessModules, 0);
            return ProcessModules.OrderBy(p => p.BaseAddress.ToInt64()).ToArray();
        }
    }
}

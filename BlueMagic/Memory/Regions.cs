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
                long start = processModules[moduleIndex].BaseAddress.ToInt64();
                long end = moduleIndex + 1 > processModules.Length - 1 ? processModules[moduleIndex].ModuleMemorySize + 1 : processModules[moduleIndex + 1].BaseAddress.ToInt64();
                long seek = start;

                do
                {
                    Native.MemoryBasicInformation region = Native.Methods.Query(processHandle, new IntPtr(seek), MarshalType<Native.MemoryBasicInformation>.Size);
                    if ((region.State & Native.MemoryAllocationState.MEM_COMMIT) != 0 && (region.Protect & (Native.MemoryProtectionType)0x701) == 0)
                        regions.Add(region);

                    seek = region.BaseAddress.ToInt64() + region.RegionSize;
                }
                while (seek < end);
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

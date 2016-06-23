using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueMagic.Memory
{
    public static class Scanner
    {
        public static List<IntPtr> ScanForBytes(byte[] bytes, byte[] buffer)
        {
            List<IntPtr> results = new List<IntPtr>();
            int bytesLength = bytes.Length;
            int bufferLength = buffer.Length;
            int i, j, k;
            bool b;
            for (i = 0; i <= bufferLength - bytesLength; ++i)
            {
                if (buffer[i] == bytes[0])
                {
                    for (j = i, k = 1, b = true; k < bytesLength; ++k, ++i)
                    {
                        if (buffer[j + k] != bytes[k])
                        {
                            b = false;
                            break;
                        }
                    }

                    if (b)
                        results.Add(new IntPtr(j));
                }
            }

            return results;
        }

        public static List<IntPtr> ScanForGeneric<T>(T value, byte[] buffer) where T : struct
        {
            return ScanForBytes(TypeConverter.GenericTypeToBytes(value), buffer);
        }

        public static List<IntPtr> ScanForSignature(Signature signature, byte[] buffer)
        {
            if (signature.Bytes != null)
                return ScanForBytes(signature.Bytes, buffer);

            List<IntPtr> results = new List<IntPtr>();
            string bufferString = BitConverter.ToString(buffer).Replace("-", string.Empty);
            int signatureLength = signature.String.Length;
            int bufferLength = bufferString.Length;
            int i, j, k;
            bool b;
            for (i = 0; i <= bufferLength - signatureLength; i += 2)
            {
                if ((signature.String[0] == '?') || bufferString[i] == signature.String[0] &&
                    (signature.String[1] == '?') || bufferString[i + 1] == signature.String[1])
                {
                    for (j = i, k = 2, b = true; k < signatureLength; k += 2, i += 2)
                    {
                        if ((signature.String[k] != '?' && bufferString[j + k] != signature.String[k]) ||
                            (signature.String[k + 1] != '?' && bufferString[j + k + 1] != signature.String[k + 1]))
                        {
                            b = false;
                            break;
                        }
                    }

                    if (b)
                        results.Add(new IntPtr(j / 2));
                }
            }

            return results;
        }

        public static List<IntPtr> ScanRegionForBytes(SafeMemoryHandle processHandle, byte[] bytes, Native.MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForBytes(bytes, Literate.ReadBytes(processHandle, region.BaseAddress, region.RegionSize));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionForGeneric<T>(SafeMemoryHandle processHandle, T value, Native.MemoryBasicInformation region) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForGeneric(value, Literate.ReadBytes(processHandle, region.BaseAddress, region.RegionSize));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionForSignature(SafeMemoryHandle processHandle, Signature signature, Native.MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForSignature(signature, Literate.ReadBytes(processHandle, region.BaseAddress, region.RegionSize));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionsForBytes(SafeMemoryHandle processHandle, byte[] bytes, List<Native.MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (Native.MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForBytes(processHandle, bytes, region));

            return results;
        }

        public static List<IntPtr> ScanRegionsForGeneric<T>(SafeMemoryHandle processHandle, T value, List<Native.MemoryBasicInformation> regions) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (Native.MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForGeneric(processHandle, value, region));

            return results;
        }

        public static List<IntPtr> ScanRegionsForSignature(SafeMemoryHandle processHandle, Signature signature, List<Native.MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (Native.MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForSignature(processHandle, signature, region));

            return results;
        }

        public static List<IntPtr> ScanModuleForBytes(Process process, SafeMemoryHandle processHandle, byte[] bytes, ProcessModule module)
        {
            return ScanRegionsForBytes(processHandle, bytes, Regions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanModuleForGeneric<T>(Process process, SafeMemoryHandle processHandle, T value, ProcessModule module) where T : struct
        {
            return ScanRegionsForGeneric(processHandle, value, Regions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanModuleForSignature(Process process, SafeMemoryHandle processHandle, Signature signature, ProcessModule module)
        {
            return ScanRegionsForSignature(processHandle, signature, Regions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanAllModulesForBytes(Process process, SafeMemoryHandle processHandle, byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForBytes(process, processHandle, bytes, module))
                    results.Add(address);

            return results;
        }

        public static List<IntPtr> ScanAllModulesForGeneric<T>(Process process, SafeMemoryHandle processHandle, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForGeneric(process, processHandle, value, module))
                    results.Add(address);

            return results;
        }

        public static List<IntPtr> ScanAllModulesForSignature(Process process, SafeMemoryHandle processHandle, Signature signature)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForSignature(process, processHandle, signature, module))
                    results.Add(address);

            return results;
        }

        public static IntPtr RescanForBytes(SafeMemoryHandle processHandle, byte[] bytes, IntPtr address)
        {
            if (Literate.ReadBytes(processHandle, address, bytes.Length) == bytes)
                return address;

            return IntPtr.Zero;
        }

        public static IntPtr RescanForGeneric<T>(SafeMemoryHandle processHandle, T value, IntPtr address) where T : struct
        {
            return RescanForBytes(processHandle, TypeConverter.GenericTypeToBytes(value), address);
        }

        public static IntPtr RescanForSignature(SafeMemoryHandle processHandle, Signature signature, IntPtr address)
        {
            if (signature.Bytes != null)
                return RescanForBytes(processHandle, signature.Bytes, address);

            byte[] buffer = Literate.ReadBytes(processHandle, address, signature.String.Length / 2);
            string bufferString = BitConverter.ToString(buffer).Replace("-", string.Empty);
            int bufferLength = bufferString.Length;

            for (int i = 0; i < bufferLength; i += 2)
            {
                if ((signature.String[i] != '?' && bufferString[i] != signature.String[i]) ||
                    (signature.String[i + 1] != '?' && bufferString[i + 1] != signature.String[i + 1]))
                    return IntPtr.Zero;
            }

            return address;
        }

        public static List<IntPtr> RescanForBytes(SafeMemoryHandle processHandle, byte[] bytes, List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForBytes(processHandle, bytes, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForGeneric<T>(SafeMemoryHandle processHandle, T value, List<IntPtr> addresses) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForGeneric(processHandle, value, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForSignature(SafeMemoryHandle processHandle, Signature signature, List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForSignature(processHandle, signature, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }
    }
}

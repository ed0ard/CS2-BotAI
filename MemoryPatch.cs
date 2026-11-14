using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Common;

internal sealed partial class MemoryPatch(nint address, int length, MemoryPatch.Region[] regions, bool linux)
{
    private const int LinuxRead = 1, LinuxWrite = 2, LinuxExecute = 4;
    private const uint WindowsCommitted = 0x1000, WindowsGuard = 0x100;
    private const int WindowsReadWrite = 0x04, WindowsExecuteReadWrite = 0x40;

    internal readonly record struct Region(nint Address, nuint Length, int Protection, int WritableProtection);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsMemoryInfo
    {
        public nint BaseAddress, AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public nuint RegionSize;
        public uint State, Protect, Type;
    }

    [LibraryImport("libc", EntryPoint = "mprotect", SetLastError = true)]
    private static partial int MProtect(nint address, nuint length, int protect);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualProtect(nint address, nuint length, int protect, out uint oldProtect);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nuint VirtualQuery(nint address, out WindowsMemoryInfo info, nuint length);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FlushInstructionCache(nint process, nint address, nuint length);

    public static MemoryPatch Capture(nint address, int length)
    {
        nint pageMask = Environment.SystemPageSize - 1;
        nint start = address & ~pageMask;
        nint end = checked(address + length + pageMask) & ~pageMask;
        bool linux = OperatingSystem.IsLinux();
        var regions = linux ? LinuxRegions(start, end) : WindowsRegions(start, end);
        return new MemoryPatch(address, length, regions.ToArray(), linux);
    }

    private static IEnumerable<Region> WindowsRegions(nint start, nint end)
    {
        while (start < end)
        {
            if (VirtualQuery(start, out var info, (nuint)Marshal.SizeOf<WindowsMemoryInfo>()) == 0)
                throw NativeError($"VirtualQuery at 0x{start:X}");

            uint access = info.Protect & 0xFF;
            if (info.State != WindowsCommitted || (info.Protect & WindowsGuard) != 0 ||
                access is not (0x02 or 0x04 or 0x08 or 0x20 or 0x40 or 0x80))
                throw new InvalidOperationException($"Patch range is not readable at 0x{start:X}.");

            nint regionEnd = info.BaseAddress + (nint)info.RegionSize;
            nint stop = regionEnd < end ? regionEnd : end;
            yield return new Region(start, (nuint)(stop - start), (int)info.Protect,
                access >= 0x20 ? WindowsExecuteReadWrite : WindowsReadWrite);
            start = stop;
        }
    }

    private static IEnumerable<Region> LinuxRegions(nint start, nint end)
    {
        // /proc/self/maps provides the original permissions.
        foreach (var line in File.ReadLines("/proc/self/maps"))
        {
            int dash = line.IndexOf('-');
            int space = line.IndexOf(' ');
            nint first = nint.Parse(line.AsSpan(0, dash), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            nint last = nint.Parse(line.AsSpan(dash + 1, space - dash - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (last <= start) continue;
            if (first > start || line[space + 1] != 'r') break;

            int protection = LinuxRead |
                (line[space + 2] == 'w' ? LinuxWrite : 0) |
                (line[space + 3] == 'x' ? LinuxExecute : 0);
            nint stop = last < end ? last : end;
            yield return new Region(start, (nuint)(stop - start), protection, protection | LinuxWrite);
            start = stop;
            if (start == end) yield break;
        }
        throw new InvalidOperationException($"Patch range is not readable at 0x{start:X}.");
    }

    public void Write(byte[] bytes)
    {
        int changed = 0;
        List<Exception> errors = [];
        try
        {
            for (; changed < regions.Length; changed++)
                SetProtection(regions[changed], regions[changed].WritableProtection, "make writable");

            Marshal.Copy(bytes, 0, address, length);
            if (!linux && !FlushInstructionCache(-1, address, (nuint)length))
                throw NativeError($"FlushInstructionCache at 0x{address:X}");
        }
        catch (Exception ex)
        {
            errors.Add(ex);
        }
        finally
        {
            for (int i = changed - 1; i >= 0; i--)
            {
                try { SetProtection(regions[i], regions[i].Protection, "restore protection"); }
                catch (Exception ex) { errors.Add(ex); }
            }
        }
        if (errors.Count != 0)
            throw new AggregateException($"Memory write at 0x{address:X} did not complete cleanly.", errors);
    }

    private void SetProtection(Region region, int protection, string operation)
    {
        bool ok = linux
            ? MProtect(region.Address, region.Length, protection) == 0
            : VirtualProtect(region.Address, region.Length, protection, out _);
        if (!ok)
            throw NativeError($"Could not {operation} at 0x{region.Address:X}");
    }

    private static Win32Exception NativeError(string operation)
    {
        int error = Marshal.GetLastPInvokeError();
        return new Win32Exception(error, $"{operation} (OS error {error}).");
    }
}

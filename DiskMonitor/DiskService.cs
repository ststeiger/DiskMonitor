
namespace DiskMonitor;

using System.Linq;

// ── Data Model ────────────────────────────────────────────────────────────────

record DiskInfo(
    string Filesystem,
    string FsType,
    string MountPoint,
    long TotalBytes,
    long UsedBytes,
    long FreeBytes
)
{
    public double UsedPercent => TotalBytes > 0 ? UsedBytes * 100.0 / TotalBytes : 0;
}

// ── Disk Service ──────────────────────────────────────────────────────────────

static class DiskService
{
    // Filesystem types that are virtual / not real storage
    private static readonly System.Collections.Generic.HashSet<string> VirtualTypes = 
        new(System.StringComparer.OrdinalIgnoreCase)
    {
        "tmpfs","devtmpfs","efivarfs","sysfs","proc","cgroup","cgroup2",
        "overlay","overlayfs","squashfs","devpts","hugetlbfs","mqueue",
        "debugfs","securityfs","fusectl","binfmt_misc","pstore","autofs",
        "ramfs","rpc_pipefs","nfsd","configfs","tracefs","nsfs","udev",
        "fuse.lxcfs","fuse.gvfsd-fuse","iso9660","sysfs","bpf","cgroup2fs",
    };

    public static System.Collections.Generic.List<DiskInfo> GetDisks() =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
            ? GetWindowsDisks()
            : GetLinuxDisks();

    // ── Windows ───────────────────────────────────────────────────────────────

    private static System.Collections.Generic.List<DiskInfo> GetWindowsDisks() =>
        System.IO.DriveInfo.GetDrives()
            .Where(d => d.IsReady &&
                (
                       d.DriveType == System.IO.DriveType.Fixed 
                    || d.DriveType == System.IO.DriveType.Network
                )
            )
            .Select(d => new DiskInfo(
                Filesystem: d.Name.TrimEnd('\\', '/'),
                FsType: d.DriveFormat,
                MountPoint: d.RootDirectory.FullName,
                TotalBytes: d.TotalSize,
                UsedBytes: d.TotalSize - d.AvailableFreeSpace,
                FreeBytes: d.AvailableFreeSpace))
            .ToList();

    // ── Linux ─────────────────────────────────────────────────────────────────

    private static System.Collections.Generic.List<DiskInfo> GetLinuxDisks()
    {
        System.Collections.Generic.Dictionary<string, string> fsTypes =
            ReadProcMounts()
        ;

        System.Collections.Generic.List<DiskInfo> result = 
            new System.Collections.Generic.List<DiskInfo>();
        System.Collections.Generic.HashSet<string> seen = 
            new System.Collections.Generic.HashSet<string>();

        try
        {
            System.Diagnostics.ProcessStartInfo psi = 
                new System.Diagnostics.ProcessStartInfo("df", "-P -B1")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            using System.Diagnostics.Process proc = 
                System.Diagnostics.Process.Start(psi)!;

            System.Collections.Generic.IEnumerable<string> lines = proc.StandardOutput.ReadToEnd()
                .Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
                .Skip(1) // skip header
            ;   

            proc.WaitForExit();

            foreach (string line in lines)
            {
                // POSIX df -P columns:
                //  0:Filesystem  1:1B-blocks  2:Used  3:Available  4:Use%  5+:MountedOn
                string[] cols = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 6) continue;

                string fs = cols[0];

                // Only real block devices
                if (!fs.StartsWith("/dev/")) continue;

                // Deduplicate (bind-mounts etc.)
                if (!seen.Add(fs)) 
                    continue;

                string fsType = fsTypes.TryGetValue(fs, out string? ft) ? ft : string.Empty;
                if (VirtualTypes.Contains(fsType)) 
                    continue;

                if (!long.TryParse(cols[1], out long total) ||
                    !long.TryParse(cols[2], out long used) ||
                    !long.TryParse(cols[3], out long free)) continue;

                string mount = string.Join(" ", cols.Skip(5));
                result.Add(new DiskInfo(fs, fsType, mount, total, used, free));
            }
        }
        catch { /* best-effort; return what we have */ }

        return result;
    }

    /// <summary>Reads /proc/mounts to build a filesystem→type map.</summary>
    private static System.Collections.Generic.Dictionary<string, string> 
        ReadProcMounts()
    {
        System.Collections.Generic.Dictionary<string, string> map = 
            new System.Collections.Generic.Dictionary<string, string>()
        ;

        try
        {
            foreach (string line in System.IO.File.ReadAllLines("/proc/mounts"))
            {
                string[] p = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (p.Length >= 3)
                    map[p[0]] = p[2];          // device → fstype
            }
        }
        catch { }
        return map;
    }
}

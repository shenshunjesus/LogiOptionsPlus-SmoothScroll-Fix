using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace LogiSmoothScroll
{
    class Program
    {
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        // Patch 1: set_foreground_process_info (RVA 0x18303A0)
        // Forces foreground whitelist flag [r14+0x28] = 1 for all applications
        private static readonly byte[] FgUnpatched = new byte[] {
            0x75, 0x04, 0xB0, 0x01, 0xEB, 0x02, 0x32, 0xC0, 0x41, 0x88, 0x46, 0x28
        };
        private static readonly byte[] FgPatched = new byte[] {
            0x75, 0x04, 0xB0, 0x01, 0xEB, 0x02, 0xB0, 0x01, 0x41, 0x88, 0x46, 0x28
        };
        private const int KnownFgRva = 0x18303A0;

        // Patch 2: vertical_scroll (RVA 0x1830510)
        // Original: 84 D2 74 2D 80 79 28 00 74 27
        // Patched : 84 D2 74 2D 83 FF 01 90 74 27
        // When wheel is in physical Ratchet/Rigid mode (edi == 1), jumps to Rigid scroll path (0x1830541).
        // When wheel is in Free-spin mode (edi != 1), uses SmoothScrollManager.
        private static readonly byte[] VsUnpatched = new byte[] {
            0x84, 0xD2, 0x74, 0x2D, 0x80, 0x79, 0x28, 0x00, 0x74, 0x27
        };
        private static readonly byte[] VsPatched = new byte[] {
            0x84, 0xD2, 0x74, 0x2D, 0x83, 0xFF, 0x01, 0x90, 0x74, 0x27
        };
        private const int KnownVsRva = 0x1830510;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static bool MatchAt(byte[] buf, int offset, byte[] pattern)
        {
            if (offset + pattern.Length > buf.Length) return false;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (buf[offset + i] != pattern[i]) return false;
            }
            return true;
        }

        private static bool ApplySinglePatch(
            IntPtr hProc,
            IntPtr baseAddr,
            int moduleSize,
            int knownRva,
            byte[] unpatched,
            byte[] patched,
            int patchOffset,
            byte[] writeBytes,
            bool restore)
        {
            byte[] targetPattern = restore ? patched : unpatched;
            byte[] alreadyPattern = restore ? unpatched : patched;

            IntPtr matchAddr = IntPtr.Zero;
            byte[] probe = new byte[unpatched.Length];
            int bytesRead;
            IntPtr fastAddr = IntPtr.Add(baseAddr, knownRva);

            if (ReadProcessMemory(hProc, fastAddr, probe, probe.Length, out bytesRead) && bytesRead == probe.Length)
            {
                if (MatchAt(probe, 0, alreadyPattern)) return true;
                if (MatchAt(probe, 0, targetPattern)) matchAddr = fastAddr;
            }

            if (matchAddr == IntPtr.Zero)
            {
                int chunkSize = 1024 * 1024;
                byte[] chunk = new byte[chunkSize];
                int scanLimit = Math.Min(moduleSize, 0x03000000);
                for (int offset = 0x1000; offset < scanLimit; offset += (chunkSize - 32))
                {
                    IntPtr readPtr = IntPtr.Add(baseAddr, offset);
                    if (ReadProcessMemory(hProc, readPtr, chunk, chunkSize, out bytesRead) && bytesRead > unpatched.Length)
                    {
                        for (int i = 0; i <= bytesRead - unpatched.Length; i++)
                        {
                            if (MatchAt(chunk, i, targetPattern))
                            {
                                matchAddr = IntPtr.Add(readPtr, i);
                                break;
                            }
                            if (MatchAt(chunk, i, alreadyPattern))
                            {
                                return true;
                            }
                        }
                    }
                    if (matchAddr != IntPtr.Zero) break;
                }
            }

            if (matchAddr != IntPtr.Zero)
            {
                IntPtr patchAddr = IntPtr.Add(matchAddr, patchOffset);
                uint oldProtect;
                if (VirtualProtectEx(hProc, patchAddr, (UIntPtr)writeBytes.Length, PAGE_EXECUTE_READWRITE, out oldProtect))
                {
                    int written;
                    bool ok = WriteProcessMemory(hProc, patchAddr, writeBytes, writeBytes.Length, out written) && written == writeBytes.Length;
                    uint dummy;
                    VirtualProtectEx(hProc, patchAddr, (UIntPtr)writeBytes.Length, oldProtect, out dummy);
                    return ok;
                }
            }
            return false;
        }

        public static int PatchAllProcesses(bool restore)
        {
            int count = 0;
            Process[] procs = Process.GetProcessesByName("logioptionsplus_agent");
            byte[] fgBytes = restore ? new byte[] { 0x32, 0xC0 } : new byte[] { 0xB0, 0x01 };
            byte[] vsBytes = restore ? new byte[] { 0x80, 0x79, 0x28, 0x00 } : new byte[] { 0x83, 0xFF, 0x01, 0x90 };

            foreach (Process p in procs)
            {
                IntPtr hProc = IntPtr.Zero;
                try
                {
                    IntPtr baseAddr = p.MainModule.BaseAddress;
                    int moduleSize = p.MainModule.ModuleMemorySize;
                    hProc = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION, false, p.Id);
                    if (hProc == IntPtr.Zero) continue;

                    bool ok1 = ApplySinglePatch(hProc, baseAddr, moduleSize, KnownFgRva, FgUnpatched, FgPatched, 6, fgBytes, restore);
                    bool ok2 = ApplySinglePatch(hProc, baseAddr, moduleSize, KnownVsRva, VsUnpatched, VsPatched, 4, vsBytes, restore);

                    if (ok1 && ok2) count++;
                }
                catch
                {
                }
                finally
                {
                    if (hProc != IntPtr.Zero) CloseHandle(hProc);
                }
            }
            return count;
        }

        private static void TriggerForegroundRefresh()
        {
            try
            {
                IntPtr fg = GetForegroundWindow();
                IntPtr desktop = GetDesktopWindow();
                if (fg != IntPtr.Zero && desktop != IntPtr.Zero)
                {
                    SetForegroundWindow(desktop);
                    Thread.Sleep(30);
                    SetForegroundWindow(fg);
                }
            }
            catch { }
        }

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--restore")
            {
                int restored = PatchAllProcesses(true);
                TriggerForegroundRefresh();
                return restored > 0 ? 0 : 1;
            }

            if (args.Length > 0 && args[0] == "--once")
            {
                int patched = PatchAllProcesses(false);
                TriggerForegroundRefresh();
                return patched > 0 ? 0 : 1;
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "Local\\LogiSmoothScrollRamPatcherMutex", out createdNew))
            {
                if (!createdNew) return 0;

                int lastPid = -1;
                while (true)
                {
                    try
                    {
                        Process[] procs = Process.GetProcessesByName("logioptionsplus_agent");
                        if (procs.Length > 0)
                        {
                            int pid = procs[0].Id;
                            if (pid != lastPid)
                            {
                                if (PatchAllProcesses(false) > 0)
                                {
                                    TriggerForegroundRefresh();
                                    lastPid = pid;
                                }
                            }
                            else
                            {
                                PatchAllProcesses(false);
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(3000);
                }
            }
        }
    }
}

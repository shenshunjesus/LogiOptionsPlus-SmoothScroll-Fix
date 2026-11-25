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

        // Cached offsets for the active agent PID so steady-state checks are O(1)
        private static int _cachedPid = -1;
        private static int _cachedFgPatchRva = 0;
        private static int _cachedVsPatchRva = 0;
        private static byte _cachedFieldOffset = 0x30;

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

        private static bool WriteBytesProtected(IntPtr hProc, IntPtr addr, byte[] data)
        {
            uint oldProtect;
            if (!VirtualProtectEx(hProc, addr, (UIntPtr)data.Length, PAGE_EXECUTE_READWRITE, out oldProtect))
                return false;

            int written;
            bool ok = WriteProcessMemory(hProc, addr, data, data.Length, out written) && written == data.Length;

            uint dummy;
            VirtualProtectEx(hProc, addr, (UIntPtr)data.Length, oldProtect, out dummy);
            return ok;
        }

        private static void GetTextSectionBounds(IntPtr hProc, IntPtr baseAddr, int moduleSize, out int textRva, out int textSize)
        {
            textRva = 0x1000;
            textSize = Math.Min(moduleSize - 0x1000, 0x04000000);

            byte[] hdr = new byte[4096];
            int read;
            if (!ReadProcessMemory(hProc, baseAddr, hdr, hdr.Length, out read) || read < 512)
                return;

            int e_lfanew = BitConverter.ToInt32(hdr, 0x3C);
            if (e_lfanew <= 0 || e_lfanew + 264 >= read)
                return;

            short numSections = BitConverter.ToInt16(hdr, e_lfanew + 6);
            short optHdrSize = BitConverter.ToInt16(hdr, e_lfanew + 20);
            int secOffset = e_lfanew + 24 + optHdrSize;

            for (int i = 0; i < numSections && secOffset + 40 <= read; i++, secOffset += 40)
            {
                // Match ".text\0"
                if (hdr[secOffset] == 0x2E && hdr[secOffset + 1] == 0x74 &&
                    hdr[secOffset + 2] == 0x65 && hdr[secOffset + 3] == 0x78 &&
                    hdr[secOffset + 4] == 0x74)
                {
                    int vsize = BitConverter.ToInt32(hdr, secOffset + 8);
                    int vaddr = BitConverter.ToInt32(hdr, secOffset + 12);
                    if (vaddr > 0 && vsize > 0 && vaddr + vsize <= moduleSize)
                    {
                        textRva = vaddr;
                        textSize = vsize;
                    }
                    return;
                }
            }
        }

        // Signature 1: logi::scrolling::scroll_manager::set_foreground_process_info
        // 48 83 FB 0B 75 17 4C 8B C3 48 8D 15 [disp32] E8 [disp32] 85 C0 75 04 B0 01 EB 02 [32 C0 | B0 01] 41 88 46 [off]
        // Total length: 35 bytes. Patch target is at +29 (2 bytes). Struct field offset is at +34 (0x28 in v2.9, 0x30 in v3.0+).
        private static bool MatchFgSignature(byte[] buf, int i)
        {
            if (i + 35 > buf.Length) return false;
            return buf[i] == 0x48 && buf[i + 1] == 0x83 && buf[i + 2] == 0xFB && buf[i + 3] == 0x0B &&
                   buf[i + 4] == 0x75 && buf[i + 5] == 0x17 &&
                   buf[i + 6] == 0x4C && buf[i + 7] == 0x8B && buf[i + 8] == 0xC3 &&
                   buf[i + 9] == 0x48 && buf[i + 10] == 0x8D && buf[i + 11] == 0x15 &&
                   buf[i + 16] == 0xE8 &&
                   buf[i + 21] == 0x85 && buf[i + 22] == 0xC0 &&
                   buf[i + 23] == 0x75 && buf[i + 24] == 0x04 &&
                   buf[i + 25] == 0xB0 && buf[i + 26] == 0x01 &&
                   buf[i + 27] == 0xEB && buf[i + 28] == 0x02 &&
                   ((buf[i + 29] == 0x32 && buf[i + 30] == 0xC0) || (buf[i + 29] == 0xB0 && buf[i + 30] == 0x01)) &&
                   buf[i + 31] == 0x41 && buf[i + 32] == 0x88 && buf[i + 33] == 0x46;
        }

        // Signature 2: logi::scrolling::scroll_manager::vertical_scroll
        // 41 8B F8 48 8B D9 84 D2 74 2D [80 79 <off> 00 | 83 FF 01 90] 74 27 0F 28 C3
        // Total length: 19 bytes. Patch target is at +10 (4 bytes).
        private static bool MatchVsSignature(byte[] buf, int i)
        {
            if (i + 19 > buf.Length) return false;
            bool prefixOk = buf[i] == 0x41 && buf[i + 1] == 0x8B && buf[i + 2] == 0xF8 &&
                            buf[i + 3] == 0x48 && buf[i + 4] == 0x8B && buf[i + 5] == 0xD9 &&
                            buf[i + 6] == 0x84 && buf[i + 7] == 0xD2 &&
                            buf[i + 8] == 0x74 && buf[i + 9] == 0x2D;
            if (!prefixOk) return false;

            bool midOk = (buf[i + 10] == 0x80 && buf[i + 11] == 0x79 && buf[i + 13] == 0x00) ||
                         (buf[i + 10] == 0x83 && buf[i + 11] == 0xFF && buf[i + 12] == 0x01 && buf[i + 13] == 0x90);
            if (!midOk) return false;

            return buf[i + 14] == 0x74 && buf[i + 15] == 0x27 &&
                   buf[i + 16] == 0x0F && buf[i + 17] == 0x28 && buf[i + 18] == 0xC3;
        }

        private static bool LocateTargets(IntPtr hProc, IntPtr baseAddr, int moduleSize, int pid,
            out int fgPatchRva, out int vsPatchRva, out byte fieldOffset)
        {
            if (pid == _cachedPid && _cachedFgPatchRva != 0 && _cachedVsPatchRva != 0)
            {
                fgPatchRva = _cachedFgPatchRva;
                vsPatchRva = _cachedVsPatchRva;
                fieldOffset = _cachedFieldOffset;
                return true;
            }

            fgPatchRva = 0;
            vsPatchRva = 0;
            fieldOffset = 0x30;

            int textRva, textSize;
            GetTextSectionBounds(hProc, baseAddr, moduleSize, out textRva, out textSize);

            int chunkSize = 1024 * 1024; // 1 MB chunks
            byte[] chunk = new byte[chunkSize];
            int endRva = textRva + textSize;

            for (int rva = textRva; rva < endRva; rva += (chunkSize - 64))
            {
                int toRead = Math.Min(chunkSize, endRva - rva);
                int bytesRead;
                if (!ReadProcessMemory(hProc, IntPtr.Add(baseAddr, rva), chunk, toRead, out bytesRead) || bytesRead < 40)
                    continue;

                for (int i = 0; i <= bytesRead - 35; i++)
                {
                    if (fgPatchRva == 0 && MatchFgSignature(chunk, i))
                    {
                        fgPatchRva = rva + i + 29;
                        fieldOffset = chunk[i + 34];
                    }
                    if (vsPatchRva == 0 && MatchVsSignature(chunk, i))
                    {
                        vsPatchRva = rva + i + 10;
                    }
                    if (fgPatchRva != 0 && vsPatchRva != 0)
                    {
                        _cachedPid = pid;
                        _cachedFgPatchRva = fgPatchRva;
                        _cachedVsPatchRva = vsPatchRva;
                        _cachedFieldOffset = fieldOffset;
                        return true;
                    }
                }
            }
            return false;
        }

        public static int PatchAllProcesses(bool restore)
        {
            int count = 0;
            Process[] procs = Process.GetProcessesByName("logioptionsplus_agent");

            foreach (Process p in procs)
            {
                IntPtr hProc = IntPtr.Zero;
                try
                {
                    IntPtr baseAddr = p.MainModule.BaseAddress;
                    int moduleSize = p.MainModule.ModuleMemorySize;
                    hProc = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION, false, p.Id);
                    if (hProc == IntPtr.Zero) continue;

                    int fgPatchRva, vsPatchRva;
                    byte fieldOffset;
                    if (!LocateTargets(hProc, baseAddr, moduleSize, p.Id, out fgPatchRva, out vsPatchRva, out fieldOffset))
                        continue;

                    byte[] fgDesired = restore ? new byte[] { 0x32, 0xC0 } : new byte[] { 0xB0, 0x01 };
                    byte[] vsDesired = restore
                        ? new byte[] { 0x80, 0x79, fieldOffset, 0x00 }
                        : new byte[] { 0x83, 0xFF, 0x01, 0x90 };

                    byte[] curFg = new byte[2];
                    byte[] curVs = new byte[4];
                    int r1, r2;
                    IntPtr fgAddr = IntPtr.Add(baseAddr, fgPatchRva);
                    IntPtr vsAddr = IntPtr.Add(baseAddr, vsPatchRva);

                    bool ok1 = ReadProcessMemory(hProc, fgAddr, curFg, 2, out r1) && r1 == 2;
                    bool ok2 = ReadProcessMemory(hProc, vsAddr, curVs, 4, out r2) && r2 == 4;

                    if (ok1 && (curFg[0] != fgDesired[0] || curFg[1] != fgDesired[1]))
                    {
                        ok1 = WriteBytesProtected(hProc, fgAddr, fgDesired);
                    }
                    if (ok2 && (curVs[0] != vsDesired[0] || curVs[1] != vsDesired[1] || curVs[2] != vsDesired[2] || curVs[3] != vsDesired[3]))
                    {
                        ok2 = WriteBytesProtected(hProc, vsAddr, vsDesired);
                    }

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
                            if (PatchAllProcesses(false) > 0 && pid != lastPid)
                            {
                                TriggerForegroundRefresh();
                                lastPid = pid;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMSIAnalyzer
{
    // Checking the digital signature of an MSI file
    class Signature
    {
        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, WinTrustData pWvtData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class WinTrustFileInfo
        {
            public uint StructSize = (uint)Marshal.SizeOf(typeof(WinTrustFileInfo));
            public IntPtr pszFilePath; 
            public IntPtr hFile = IntPtr.Zero; 
            public IntPtr pgKnownSubject = IntPtr.Zero; 

            public WinTrustFileInfo(string _filePath)
            {
                pszFilePath = Marshal.StringToCoTaskMemAuto(_filePath);
            }

            ~WinTrustFileInfo()
            {
                Marshal.FreeCoTaskMem(pszFilePath);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class WinTrustData
        {
            public uint StructSize = (uint)Marshal.SizeOf(typeof(WinTrustData));
            public IntPtr PolicyCallbackData = IntPtr.Zero;
            public IntPtr SIPClientData = IntPtr.Zero;
            public UIChoice UIChoice = UIChoice.None;
            public RevocationChecks RevocationChecks = RevocationChecks.None;
            public UnionChoice UnionChoice = UnionChoice.File;
            public IntPtr FileInfoPtr;
            public StateAction StateAction = StateAction.Ignore;
            public IntPtr StateData = IntPtr.Zero;
            public string URLReference = null;
            public ProvFlags ProvFlags = ProvFlags.RevocationCheckChainExcludeRoot;
            public UIContext UIContext = UIContext.Execute;

            public WinTrustData(string fileName)
            {
                var wtfiData = new WinTrustFileInfo(fileName);
                FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
                Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
            }

            ~WinTrustData()
            {
                Marshal.FreeCoTaskMem(FileInfoPtr);
            }
        }

        private enum UIChoice : uint
        {
            All = 1,
            None = 2,
            NoBad = 3,
            NoGood = 4
        }

        private enum RevocationChecks : uint
        {
            None = 0x00000000,
            WholeChain = 0x00000001
        }

        private enum UnionChoice : uint
        {
            File = 1,
            Catalog,
            Blob,
            Signer,
            Certificate
        }

        private enum StateAction : uint
        {
            Ignore = 0x00000000,
            Verify = 0x00000001,
            Close = 0x00000002,
            AutoCache = 0x00000003,
            AutoCacheFlush = 0x00000004
        }

        [Flags]
        private enum ProvFlags : uint
        {
            UseIE4TrustFlag = 0x00000001,
            NoIE4ChainFlag = 0x00000002,
            NoPolicyUsageFlag = 0x00000004,
            RevocationCheckNone = 0x00000010,
            RevocationCheckEndCert = 0x00000020,
            RevocationCheckChain = 0x00000040,
            RevocationCheckChainExcludeRoot = 0x00000080,
            SaferFlag = 0x00000100,
            HashOnlyFlag = 0x00000200,
            RequireStrongSignatures = 0x00000400
        }

        private enum UIContext : uint
        {
            Execute = 0,
            Install = 1
        }

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private static readonly Guid WIN_TRUST_ACTION_GENERIC_VERIFY_V2 = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        public static bool VerifySignature(string fileName)
        {
            var wtd = new WinTrustData(fileName);
            var result = WinVerifyTrust(INVALID_HANDLE_VALUE, WIN_TRUST_ACTION_GENERIC_VERIFY_V2, wtd);
            return result == 0;
        }
    }
}

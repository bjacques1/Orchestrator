using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace TeamFoundationServerIntegrationPack
{
    #region Logon parameters

    enum LogonType : int
    {
        Interactive = 2,
        Network = 3,
        Batch = 4,
        Service = 5,
        NetworkClearText = 8,
        NewCredential = 9,
    }

    enum LogonProvider : int
    {
        Default = 0,
    }

    #endregion

    abstract class NativeMethods
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LogonUser(
            string userName,
            string domain,
            string password,
            LogonType logonType,
            LogonProvider logonProvider,
            out SafeTokenHandle token);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        // Performance hit:
        // [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(
            IntPtr handle);
    }

    sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle() :
            base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
    }

    class Impersonation : IDisposable
    {
        private SafeTokenHandle userHandle = null;
        private WindowsImpersonationContext impersonationContext;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle"), SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public Impersonation(
            string domain,
            string userName,
            string password)
        {
            // If the user name is empty, do not impersonate.
            if (string.IsNullOrEmpty(userName))
                return;

            bool loggedOn = NativeMethods.LogonUser(
                userName,
                domain,
                password,
                LogonType.NetworkClearText,
                LogonProvider.Default,
                out userHandle);

            if (!loggedOn)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            impersonationContext = WindowsIdentity.Impersonate(userHandle.DangerousGetHandle());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (userHandle != null)
            {
                impersonationContext.Undo();

                userHandle.Dispose();
                userHandle = null;
            }
        }

        ~Impersonation()
        {
            Dispose(false);
        }
    }
}

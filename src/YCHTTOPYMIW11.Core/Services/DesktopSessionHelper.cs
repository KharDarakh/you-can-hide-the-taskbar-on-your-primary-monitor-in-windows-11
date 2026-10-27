using System;
using System.Threading;
using System.Runtime.InteropServices;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Core.Services
{
    /// <summary>
    /// Ensures Win32 window queries and hooks execute on the interactive "Default" desktop.
    /// This provides seamless compatibility when running as standard desktop app, background service, or terminal tool.
    /// </summary>
    public static class DesktopSessionHelper
    {
        public static void RunOnInteractiveDesktop(Action action)
        {
            Exception? caughtException = null;

            Thread thread = new Thread(() =>
            {
                IntPtr hDesk = NativeMethods.OpenDesktop("Default", 0, false, NativeConstants.GENERIC_ALL_DESKTOP);
                if (hDesk != IntPtr.Zero)
                {
                    NativeMethods.SetThreadDesktop(hDesk);
                }

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (hDesk != IntPtr.Zero)
                    {
                        NativeMethods.CloseDesktop(hDesk);
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();

            if (caughtException != null)
            {
                throw new InvalidOperationException("Exception executing on interactive desktop", caughtException);
            }
        }

        public static T RunOnInteractiveDesktop<T>(Func<T> func)
        {
            T result = default!;
            Exception? caughtException = null;

            Thread thread = new Thread(() =>
            {
                IntPtr hDesk = NativeMethods.OpenDesktop("Default", 0, false, NativeConstants.GENERIC_ALL_DESKTOP);
                if (hDesk != IntPtr.Zero)
                {
                    NativeMethods.SetThreadDesktop(hDesk);
                }

                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (hDesk != IntPtr.Zero)
                    {
                        NativeMethods.CloseDesktop(hDesk);
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();

            if (caughtException != null)
            {
                throw new InvalidOperationException("Exception executing on interactive desktop", caughtException);
            }

            return result;
        }
    }
}


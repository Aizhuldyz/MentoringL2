using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedPowerManagement
{
    [ComVisible(true)]
    [Guid("057CDBDE-159A-4F23-B08D-DC85CD359F1D")]
    [ClassInterface(ClassInterfaceType.None)]
    public class PowerManager : IManagePower
    {
        public ulong GetLastSleepTime()
        {
            IntPtr status = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ulong)));
            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.LastSleepTime,
                (IntPtr)null,
                0,
                status,
                (uint)Marshal.SizeOf(typeof(SystemBatteryState))
            );

            ulong lst = (ulong)Marshal.PtrToStructure(status, typeof(ulong));
            return lst;
        }

        public ulong GetLastWakeTime()
        {
            IntPtr status = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ulong)));
            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.LastWakeTime,
                (IntPtr)null,
                0,
                status,
                (uint)Marshal.SizeOf(typeof(SystemBatteryState))
            );

            ulong lwt = (ulong)Marshal.PtrToStructure(status, typeof(ulong));
            return lwt;
        }

        public SystemBatteryState GetSystemBatteryState()
        {
            IntPtr status = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SystemBatteryState)));
            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.SystemBatteryState, 
                (IntPtr)null,
                0,
                status,
                (uint)Marshal.SizeOf(typeof(SystemBatteryState))
            );

            SystemBatteryState sbs = (SystemBatteryState)Marshal.PtrToStructure(status, typeof(SystemBatteryState));
            return sbs;
        }

        public SystemPowerInformation GetSystemPowerInformation()
        {
            IntPtr status = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SystemPowerInformation)));

            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.SystemPowerInformation,
                IntPtr.Zero,
                0,
                status,
                (uint)Marshal.SizeOf(typeof(SystemPowerInformation))
                );
            var spi = (SystemPowerInformation)Marshal.PtrToStructure(status, typeof(SystemPowerInformation));
            return spi;
        }

        public void ReserveHibernationFile()
        {
            IntPtr inbool = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Int32)));
            Marshal.WriteInt32(inbool, 1);

            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.SystemReserveHiberFile,
                inbool,
                0,
                IntPtr.Zero,
                (uint)Marshal.SizeOf(typeof(SystemPowerInformation))
            );
        }


        public void DeleteHibernationFile()
        {
            IntPtr inbool = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Int32)));
            Marshal.WriteInt32(inbool, 0);

            NativePowrProfInterop.CallNtPowerInformation(
                (int)InformationLevel.SystemReserveHiberFile,
                inbool,
                0,
                IntPtr.Zero,
                (uint)Marshal.SizeOf(typeof(SystemPowerInformation))
            );
        }

        public bool HIbernateSystem()
        {
            return NativePowrProfInterop.SetSuspendState(false, false, false);
        }
    }
}

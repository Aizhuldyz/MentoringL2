using System;
using System.Runtime.InteropServices;

namespace ManagedPowerManagement
{
    [ComVisible(true)]
    [Guid("3CDB6584-8F2D-48C5-8565-933177CDD9F5")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IManagePower
    {
        ulong GetLastSleepTime();
        ulong GetLastWakeTime();
        SystemBatteryState GetSystemBatteryState();
        SystemPowerInformation GetSystemPowerInformation();
        void ReserveHibernationFile();
        void DeleteHibernationFile();
        bool HIbernateSystem();
    }

    public enum InformationLevel
    {
        LastSleepTime = 15,
        LastWakeTime = 14,
        SystemBatteryState = 5,
        SystemPowerInformation = 12,
        SystemReserveHiberFile = 10
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential)]
    [Guid("402E4F7E-E2AA-48BB-8F75-B86C7DAD32ED")]
    public struct SystemBatteryState
    {
        public byte AcOnLine;
        public byte BatteryPresent;
        public byte Charging;
        public byte Discharging;
        public byte spare1;
        public byte spare2;
        public byte spare3;
        public byte spare4;
        public int MaxCapacity;
        public int RemainingCapacity;
        public int Rate;
        public int EstimatedTime;
        public int DefaultAlert1;
        public int DefaultAlert2;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential)]
    [Guid("536AEBFE-A244-49B5-AC14-FE9922ABF263")]
    public struct SystemPowerInformation
    {
        public long MaxIdlenessAllowed;
        public long Idleness;
        public long TimeRemaining;
        public byte CoolingMode;
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedPowerManagement
{

    internal class NativePowrProfInterop
    {
        [DllImport("powrprof.dll", SetLastError = true)]

        internal static extern uint CallNtPowerInformation(
            int InformationLevel,
            IntPtr lpInputBuffer,
            uint nInputBufferSize,
            IntPtr lpOutputBuffer,
            uint nOutputBufferSize
        );

        [DllImport("Powrprof.dll", SetLastError = true)]
        internal static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);


    }
}

using System;
using System.Runtime.InteropServices;

namespace Game.Bootstrap
{
    partial class Internal
    {
        public static int Bootstrap(IntPtr arg, int argLength)
        {
            if (arg == (IntPtr)0)
            {
                Console.WriteLine("Received NULL bootstrap structure");
                return 1;
            }
            int expectedLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NWN.Internal.BootstrapArgs));
            if (argLength < expectedLength)
            {
                Console.WriteLine($"Received bootstrap structure too small - actual={argLength}, expected={expectedLength}");
                return 1;
            }
            if (argLength > expectedLength)
            {
                Console.WriteLine($"WARNING: Received bootstrap structure bigger than expected - actual={argLength}, expected={expectedLength}");
                Console.WriteLine($"         This usually means that native code version is ahead of the managed code");
            }

            NWN.Internal.Init(Marshal.PtrToStructure<NWN.Internal.BootstrapArgs>(arg));
            NWN.Internal.OnMainLoop += Entrypoints.OnMainLoop;
            NWN.Internal.OnRunScript += Entrypoints.OnRunScript;

            return 0;
        }
    }
}

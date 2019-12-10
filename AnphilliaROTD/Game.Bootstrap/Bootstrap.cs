using System;
using System.Diagnostics;

namespace Game.Bootstrap
{
    partial class Internal
    {
        public static int Bootstrap(IntPtr arg, int argLength)
        {
            if (!NWN.Internal.Init(arg, argLength))
            {
                Console.WriteLine("Fatal error: consult log. Closing process.");
                Process.GetCurrentProcess().Kill();
            }

            // Init code here... call to game project.

            NWN.Internal.OnMainLoop += Entrypoints.OnMainLoop;
            NWN.Internal.OnRunScript += Entrypoints.OnRunScript;

            return 0;
        }
    }
}

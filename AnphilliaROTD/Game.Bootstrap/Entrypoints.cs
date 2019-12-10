using System;
using Game.NWN;

namespace Game.Bootstrap
{
    public class Entrypoints
    {
        //
        // This is called once every main loop frame, outside of object context
        //
        public static void OnMainLoop(ulong frame)
        {
            Console.WriteLine($"MainLoop frame {frame}");
        }

        //
        // Return true if the script has been handled, or false if it should be forwarded to nwscript.
        //
        public static bool OnRunScript(string script)
        {
            Console.WriteLine($"Runscript '{script}' on oid {NWScript.OBJECT_SELF}");
            return false;
        }
    }
}

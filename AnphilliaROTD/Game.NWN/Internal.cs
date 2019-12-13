using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace Game.NWN
{
    public class Internal
    {
        public static uint OBJECT_SELF { get; private set; } = NWScript.OBJECT_INVALID;

        public delegate void MainLoopHandlerDelegate(ulong frame);
        public delegate bool RunScriptHandlerDelegate(string script);

        public static MainLoopHandlerDelegate OnMainLoop;
        public static RunScriptHandlerDelegate OnRunScript;

        private delegate int _RunScriptHandlerDelegate(string script, uint oid);
        private delegate void _ClosureHandlerDelegate(ulong eid, uint oid);

        [StructLayout(LayoutKind.Sequential)]
        private struct AllHandlers
        {
            public MainLoopHandlerDelegate MainLoop;
            public _RunScriptHandlerDelegate RunScript;
            public _ClosureHandlerDelegate Closure;
        }

        private static void MainLoopHandler(ulong frame)
        {
            OnMainLoop?.Invoke(frame);
        }

        private struct ScriptContext
        {
            public uint OwnerObject;
            public string ScriptName;
        }

        private static readonly Stack<ScriptContext> ScriptContexts = new Stack<ScriptContext>();

        private static int RunScriptHandler(string script, uint oidSelf)
        {
            int ret = -1;
            OBJECT_SELF = oidSelf;
            ScriptContexts.Push(new ScriptContext { OwnerObject = oidSelf, ScriptName = script });
            try
            {
                ret = OnRunScript(script) ? 0 : -1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            ScriptContexts.Pop();
            OBJECT_SELF = ScriptContexts.Count == 0 ? NWScript.OBJECT_INVALID : ScriptContexts.Peek().OwnerObject;
            return ret;
        }

        private struct Closure
        {
            public uint OwnerObject;
            public ActionDelegate Run;
        }

        private static ulong NextEventId = 0;
        private static readonly Dictionary<ulong, Closure> Closures = new Dictionary<ulong, Closure>();

        private static void ClosureHandler(ulong eid, uint oidSelf)
        {
            uint old = OBJECT_SELF;
            OBJECT_SELF = oidSelf;
            try
            {
                Closures[eid].Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Closures.Remove(eid);
            OBJECT_SELF = old;
        }

        public static void ClosureAssignCommand(uint obj, ActionDelegate func)
        {
            if (NativeFunctions.ClosureAssignCommand(obj, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }

        public static void ClosureDelayCommand(uint obj, float duration, ActionDelegate func)
        {
            if (NativeFunctions.ClosureDelayCommand(obj, duration, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }

        public static void ClosureActionDoCommand(uint obj, ActionDelegate func)
        {
            if (NativeFunctions.ClosureActionDoCommand(obj, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetFunctionPointerDelegate(string name);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RegisterHandlersDelegate(IntPtr handlers, uint size);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallBuiltInDelegate(int id);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushIntegerDelegate(int value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushFloatDelegate(float value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushStringDelegate(string value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushStringUTF8Delegate(string value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushObjectDelegate(uint value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushVectorDelegate(NWN.Vector value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushEffectDelegate(IntPtr value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushEventDelegate(IntPtr value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushLocationDelegate(IntPtr value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushTalentDelegate(IntPtr value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StackPushItemPropertyDelegate(IntPtr value);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int StackPopIntegerDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate float StackPopFloatDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string StackPopStringDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string StackPopStringUTF8Delegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint StackPopObjectDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NWN.Vector StackPopVectorDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr StackPopEffectDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr StackPopEventDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr StackPopLocationDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr StackPopTalentDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr StackPopItemPropertyDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeEffectDelegate(IntPtr ptr);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeEventDelegate(IntPtr ptr);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeLocationDelegate(IntPtr ptr);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeTalentDelegate(IntPtr ptr);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeItemPropertyDelegate(IntPtr ptr);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ClosureAssignCommandDelegate(uint oid, ulong eventId);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ClosureDelayCommandDelegate(uint oid, float duration, ulong eventId);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ClosureActionDoCommandDelegate(uint oid, ulong eventId);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxSetFunctionDelegate(string plugin, string function);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushIntDelegate(int n);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushFloatDelegate(float f);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushObjectDelegate(uint o);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushStringDelegate(string s);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushStringUTF8Delegate(string s);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushEffectDelegate(IntPtr e);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxPushItemPropertyDelegate(IntPtr ip);
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int nwnxPopIntDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate float nwnxPopFloatDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint nwnxPopObjectDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string nwnxPopStringDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string nwnxPopStringUTF8Delegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr nwnxPopEffectDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr nwnxPopItemPropertyDelegate();
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void nwnxCallFunctionDelegate();

        [StructLayout(LayoutKind.Sequential)]
        public struct BootstrapArgs
        {
            public GetFunctionPointerDelegate GetFunctionPointer;
            public RegisterHandlersDelegate RegisterHandlers;
            public CallBuiltInDelegate CallBuiltIn;
            public StackPushIntegerDelegate StackPushInteger;
            public StackPushFloatDelegate StackPushFloat;
            public StackPushStringDelegate StackPushString;
            public StackPushStringUTF8Delegate StackPushStringUTF8;
            public StackPushObjectDelegate StackPushObject;
            public StackPushVectorDelegate StackPushVector;
            public StackPushEffectDelegate StackPushEffect;
            public StackPushEventDelegate StackPushEvent;
            public StackPushLocationDelegate StackPushLocation;
            public StackPushTalentDelegate StackPushTalent;
            public StackPushItemPropertyDelegate StackPushItemProperty;
            public StackPopIntegerDelegate StackPopInteger;
            public StackPopFloatDelegate StackPopFloat;
            public StackPopStringDelegate StackPopString;
            public StackPopStringUTF8Delegate StackPopStringUTF8;
            public StackPopObjectDelegate StackPopObject;
            public StackPopVectorDelegate StackPopVector;
            public StackPopEffectDelegate StackPopEffect;
            public StackPopEventDelegate StackPopEvent;
            public StackPopLocationDelegate StackPopLocation;
            public StackPopTalentDelegate StackPopTalent;
            public StackPopItemPropertyDelegate StackPopItemProperty;
            public FreeEffectDelegate FreeEffect;
            public FreeEventDelegate FreeEvent;
            public FreeLocationDelegate FreeLocation;
            public FreeTalentDelegate FreeTalent;
            public FreeItemPropertyDelegate FreeItemProperty;
            public ClosureAssignCommandDelegate ClosureAssignCommand;
            public ClosureDelayCommandDelegate ClosureDelayCommand;
            public ClosureActionDoCommandDelegate ClosureActionDoCommand;
            public nwnxSetFunctionDelegate nwnxSetFunction;
            public nwnxPushIntDelegate nwnxPushInt;
            public nwnxPushFloatDelegate nwnxPushFloat;
            public nwnxPushObjectDelegate nwnxPushObject;
            public nwnxPushStringDelegate nwnxPushString;
            public nwnxPushStringUTF8Delegate nwnxPushStringUTF8;
            public nwnxPushEffectDelegate nwnxPushEffect;
            public nwnxPushItemPropertyDelegate nwnxPushItemProperty;
            public nwnxPopIntDelegate nwnxPopInt;
            public nwnxPopFloatDelegate nwnxPopFloat;
            public nwnxPopObjectDelegate nwnxPopObject;
            public nwnxPopStringDelegate nwnxPopString;
            public nwnxPopStringUTF8Delegate nwnxPopStringUTF8;
            public nwnxPopEffectDelegate nwnxPopEffect;
            public nwnxPopItemPropertyDelegate nwnxPopItemProperty;
            public nwnxCallFunctionDelegate nwnxCallFunction;
        }

        public static BootstrapArgs NativeFunctions { get; private set; }

        public static bool Init(IntPtr arg, int argLength)
        {
            if (arg == (IntPtr)0)
            {
                Console.WriteLine("Received NULL bootstrap structure");
                return false;
            }
            int expectedLength = Marshal.SizeOf(typeof(Internal.BootstrapArgs));
            if (argLength < expectedLength)
            {
                Console.WriteLine($"Received bootstrap structure too small - actual={argLength}, expected={expectedLength}");
                return false;
            }
            if (argLength > expectedLength)
            {
                Console.WriteLine($"WARNING: Received bootstrap structure bigger than expected - actual={argLength}, expected={expectedLength}");
                Console.WriteLine($"         This usually means that native code version is ahead of the managed code");
            }

            NativeFunctions = Marshal.PtrToStructure<Internal.BootstrapArgs>(arg);

            AllHandlers handlers;
            handlers.MainLoop = MainLoopHandler;
            handlers.RunScript = RunScriptHandler;
            handlers.Closure = ClosureHandler;

            var size = Marshal.SizeOf(typeof(AllHandlers));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(handlers, ptr, false);
            NativeFunctions.RegisterHandlers(ptr, (uint)size);
            Marshal.FreeHGlobal(ptr);

            return true;
        }
    }
}

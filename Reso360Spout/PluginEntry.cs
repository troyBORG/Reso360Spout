// KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout
//
// Note: KlakSpout.dll must be placed in the rml_mods folder alongside Reso360Spout.dll
// Since Unity rendering is now in a separate Renderer process, the DLL path is resolved
// relative to the Renderer process working directory. The rml_mods folder should be
// accessible from the Renderer process (typically at Resonite\rml_mods).

using UnityEngine;
using System.Runtime.InteropServices;

namespace Reso360Spout
{
    static class PluginEntry
    {
        internal enum Event { Update, Dispose }

        internal static bool IsAvailable
        {
            get
            {
                return SystemInfo.graphicsDeviceType ==
                    UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
            }
        }

        // Note: Since Unity rendering is now in a separate Renderer process, KlakSpout.dll
        // must be accessible from the Renderer. Unity automatically looks for native plugins in:
        // 1. Renderite.Renderer_Data\Plugins\x86_64\ (RECOMMENDED - copy KlakSpout.dll here)
        // 2. Next to Renderite.Renderer.exe
        // 
        // IMPORTANT: For the separated Renderer architecture, you should copy KlakSpout.dll
        // from rml_mods to: Renderer\Renderite.Renderer_Data\Plugins\x86_64\KlakSpout.dll
        // 
        // The DllImport path below uses just the filename, which will work if the DLL is in
        // one of Unity's standard plugin search locations.
        
        [DllImport("KlakSpout", EntryPoint = "GetRenderEventFunc")]
        internal static extern System.IntPtr GetRenderEventFunc();

        [DllImport("KlakSpout", EntryPoint = "CreateSender")]
        internal static extern System.IntPtr CreateSender(string name, int width, int height);

        [DllImport("KlakSpout", EntryPoint = "CreateReceiver")]
        internal static extern System.IntPtr CreateReceiver(string name);

        [DllImport("KlakSpout", EntryPoint = "GetTexturePointer")]
        internal static extern System.IntPtr GetTexturePointer(System.IntPtr ptr);

        [DllImport("KlakSpout", EntryPoint = "GetTextureWidth")]
        internal static extern int GetTextureWidth(System.IntPtr ptr);

        [DllImport("KlakSpout", EntryPoint = "GetTextureHeight")]
        internal static extern int GetTextureHeight(System.IntPtr ptr);

        [DllImport("KlakSpout", EntryPoint = "CheckValid")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CheckValid(System.IntPtr ptr);

        [DllImport("KlakSpout", EntryPoint = "ScanSharedObjects")]
        internal static extern int ScanSharedObjects();

        [DllImport("KlakSpout", EntryPoint = "GetSharedObjectName")]
        internal static extern System.IntPtr GetSharedObjectName(int index);

        internal static string GetSharedObjectNameString(int index)
        {
            var ptr = GetSharedObjectName(index);
            return ptr != System.IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;
        }

    }
}

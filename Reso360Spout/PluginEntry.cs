// KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout

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

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern System.IntPtr GetRenderEventFunc();

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern System.IntPtr CreateSender(string name, int width, int height);

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern System.IntPtr CreateReceiver(string name);

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern System.IntPtr GetTexturePointer(System.IntPtr ptr);

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern int GetTextureWidth(System.IntPtr ptr);

        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern int GetTextureHeight(System.IntPtr ptr);


        [DllImport(@"rml_mods\KlakSpout.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CheckValid(System.IntPtr ptr);


        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern int ScanSharedObjects();


        [DllImport(@"rml_mods\KlakSpout.dll")]
        internal static extern System.IntPtr GetSharedObjectName(int index);

        internal static string GetSharedObjectNameString(int index)
        {
            var ptr = GetSharedObjectName(index);
            return ptr != System.IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;
        }

    }
}

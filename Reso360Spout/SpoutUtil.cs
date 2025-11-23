using UnityEngine.Rendering;
using UnityEngine;

namespace Reso360Spout
{
    static class SpoutUtil
    {
        internal static void Destroy(UnityEngine.Object? obj)
        {
            if (obj == null) return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        static CommandBuffer? _commandBuffer;

        internal static void IssuePluginEvent(PluginEntry.Event pluginEvent, System.IntPtr ptr)
        {
            _commandBuffer ??= new CommandBuffer();

            _commandBuffer.IssuePluginEventAndData(
                PluginEntry.GetRenderEventFunc(), (int)pluginEvent, ptr
            );

            Graphics.ExecuteCommandBuffer(_commandBuffer);

            _commandBuffer.Clear();
        }
    }
}

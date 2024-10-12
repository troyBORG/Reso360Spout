using HarmonyLib;
using ResoniteModLoader;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using FrooxEngine;

namespace Reso360Spout {

    static class SpoutUtil
    {
        internal static void Destroy(UnityEngine.Object obj)
        {
            if (obj == null) return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        static CommandBuffer _commandBuffer;

        internal static void
            IssuePluginEvent(PluginEntry.Event pluginEvent, System.IntPtr ptr)
        {
            if (_commandBuffer == null) _commandBuffer = new CommandBuffer();

            _commandBuffer.IssuePluginEventAndData(
                PluginEntry.GetRenderEventFunc(), (int)pluginEvent, ptr
            );

            Graphics.ExecuteCommandBuffer(_commandBuffer);

            _commandBuffer.Clear();
        }
    }

    public class UnityEntry : MonoBehaviour
    {
        GameObject root;
        GameObject CameraRoot;

        public static UnityEngine.Shader cubemapShader;
        public static UnityEngine.Shader cubemapRenderer;

        UnityEngine.Camera CameraComponent;
        IntPtr Plugin;
        UnityEngine.RenderTexture _SourceTexture;
        UnityEngine.RenderTexture SourceTexture;
        UnityEngine.Texture2D SharedTexture;

        void SendRenderTexture(UnityEngine.RenderTexture source)
        {
            // Plugin lazy initialization
            if (Plugin == System.IntPtr.Zero)
            {
                Plugin = PluginEntry.CreateSender(name, source.width, source.height);
                if (Plugin == System.IntPtr.Zero)
                {
                    Debug.Log("Spout may not be ready.");
                    return;
                }; // Spout may not be ready.
            }

            // Shared texture lazy initialization
            if (SharedTexture == null)
            {
                var ptr = PluginEntry.GetTexturePointer(Plugin);
                if (ptr != System.IntPtr.Zero)
                {
                    SharedTexture = UnityEngine.Texture2D.CreateExternalTexture(
                        PluginEntry.GetTextureWidth(Plugin),
                        PluginEntry.GetTextureHeight(Plugin),
                        TextureFormat.ARGB32, false, false, ptr
                    );
                    SharedTexture.hideFlags = HideFlags.DontSave;
                }
            }

            // Shared texture update
            if (SharedTexture != null)
            {
                var tempRT = UnityEngine.RenderTexture.GetTemporary
                    (SharedTexture.width, SharedTexture.height);
                Graphics.Blit(source, tempRT, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
                Graphics.CopyTexture(tempRT, SharedTexture);
                UnityEngine.RenderTexture.ReleaseTemporary(tempRT);
            }
        }



        void Start()
        {
            var assets = AssetBundle.LoadFromFile(@"rml_mods\cubeto360");
            assets.LoadAllAssets();

            var allAssets = assets.LoadAllAssets<UnityEngine.Shader>();
            foreach (var shader in allAssets)
            {
                if(shader.name == "Unlit/CubemapToOtherProjection")
                {
                    UnityEntry.cubemapShader = shader;
                }
                if (shader.name == "Unlit/CubemapRenderer")
                {
                    UnityEntry.cubemapRenderer = shader;
                }
            }

            Console.WriteLine("kokopi start");

            root = this.gameObject;

            CameraRoot = new GameObject();
            CameraRoot.name = "CameraRoot";
            CameraRoot.transform.position = new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);
            CameraRoot.transform.rotation = UnityEngine.Quaternion.identity;
            CameraRoot.transform.parent = root.transform;

            Plugin = PluginEntry.CreateSender("VR180Cam", 6144, 3072);
            CameraComponent = CameraRoot.AddComponent<UnityEngine.Camera>();
            CameraComponent.depth = -128;
            SourceTexture = new UnityEngine.RenderTexture(6144, 3072, 24);
            _SourceTexture = new UnityEngine.RenderTexture(6144, 3072, 24);
            CameraComponent.targetTexture = _SourceTexture;
            var cubeComponent = CameraRoot.AddComponent<CubemapToOtherProjection>();
            cubeComponent.RenderTarget = SourceTexture;
            cubeComponent.ProjectionType = ProjectionType.Equirectangular_180;
            cubeComponent.RenderInStereo = true;
        }

        void Update()
        {
            root.transform.position = Main.CameraOrigin;
            root.transform.rotation = Main.CameraRotation;
            root.transform.localScale = Main.CameraScale;

            // Update the plugin internal state.
            if (Plugin != System.IntPtr.Zero)
                SpoutUtil.IssuePluginEvent(PluginEntry.Event.Update, Plugin);

            // Render texture mode update
            if (SourceTexture != null)
                SendRenderTexture(SourceTexture);
        }
    }

    public class Main : ResoniteMod
    {
        public override string Name => "Reso360Spout";
        public override string Author => "kka429";
        public override string Version => "0.0.1";
        public override string Link => "https://github.com/rassi0429/Reso360Spout";


        public static Vector3 CameraOrigin;
        public static Quaternion CameraRotation;
        public static Vector3 CameraScale;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("dev.kokoa.Reso360Spout");
            harmony.PatchAll();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Msg("scene name: " + scene.name);
            var go = new UnityEngine.GameObject();
            go.name = "MODEntry";
            go.AddComponent<UnityEntry>();


            CameraOrigin = new Vector3(0.0f, 0.0f, 0.0f);
            CameraRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            CameraScale = new Vector3(1.0f, 1.0f, 1.0f);
        }


        [HarmonyPatch(typeof(FrooxEngine.Engine), "RunUpdateLoop")]
        class Patch
        {
            static bool Prefix(FrooxEngine.Engine __instance)
            {
                if (__instance.WorldManager.FocusedWorld == null) return true;

                __instance.WorldManager.FocusedWorld.RunSynchronously(() =>
                {
                    // 重そうなのであとでなおす
                    var origin = __instance.WorldManager.FocusedWorld.RootSlot.FindChildInHierarchy("#Camera");

                    if (origin != null)
                    {
                        CameraOrigin.Set(origin.GlobalPosition.x, origin.GlobalPosition.y, origin.GlobalPosition.z);
                        CameraRotation.Set(origin.GlobalRotation.x, origin.GlobalRotation.y, origin.GlobalRotation.z, origin.GlobalRotation.w);
                        CameraScale.Set(origin.GlobalScale.x, origin.GlobalScale.y, origin.GlobalScale.z);
                    }
                });

                return true;
            }
        }
    }
}

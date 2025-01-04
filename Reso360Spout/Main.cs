using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Reso360Spout {


    public class UnityEntry : MonoBehaviour
    {
        GameObject cube;

        GameObject root;
        GameObject CameraRoot;

        public static UnityEngine.Shader cubemapShader;
        public static UnityEngine.Shader cubemapRenderer;

        public UnityEngine.Camera CameraComponent;
        public CubemapToOtherProjection cubeComponent;
        public IntPtr Plugin = IntPtr.Zero;       
        public UnityEngine.RenderTexture SourceTexture;
        public UnityEngine.Texture2D SharedTexture;



        // load assetbundle
        void SendRenderTexture()
        {
            // Plugin lazy initialization
            if (Plugin == System.IntPtr.Zero)
            {
                // Plugin = PluginEntry.CreateSender(name, source.width, source.height);
                if (Plugin == System.IntPtr.Zero)
                {
                    Debug.Log("Spout may not be ready.");
                    return;
                }; // Spout may not be ready.
            }

            if (SharedTexture != null)
            {
                if (SharedTexture.height != SourceTexture.height || SharedTexture.width != SourceTexture.width)
                {
                    if (SharedTexture != null)
                    {
                        UnityEngine.Object.Destroy(SharedTexture);
                    }
                    SharedTexture = null;
                }
            }

            // Shared texture lazy initialization
            if (SharedTexture == null && Main.Config.GetValue(Main.SPOUT_ENABLE))
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
            //if (SharedTexture != null)
            //{
            //    var tempRT = UnityEngine.RenderTexture.GetTemporary
            //        (SharedTexture.width, SharedTexture.height);
            //    Graphics.Blit(SourceTexture, tempRT, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
            //    Graphics.CopyTexture(tempRT, SharedTexture);
            //    UnityEngine.RenderTexture.ReleaseTemporary(tempRT);
            //}

            // コマンドバッファを生成
            var cmd = new CommandBuffer();
            cmd.name = "SpoutSend";

            // 一時的なRTのIDを取得
            int tempRTId = UnityEngine.Shader.PropertyToID("_TempSpoutRT");

            // 一時的なRTをコマンドバッファで確保
            cmd.GetTemporaryRT(
                tempRTId,
                SharedTexture.width,
                SharedTexture.height,
                0,                      // Depth Buffer
                FilterMode.Bilinear,
                RenderTextureFormat.ARGB32
            );

            // Blit 相当
            // Unityのバージョンによっては cmd.Blit でスケール・オフセット付きが無い場合があります。
            // その場合はマテリアルを用意してUVを反転させるなど別途工夫が必要です。
            // ここではスケールオフセット付きが使えるという前提で記述しています。
            cmd.Blit(
                SourceTexture,                    // 元テクスチャ
                tempRTId,                         // 出力先(一時RT)
                new Vector2(1.0f, -1.0f),         // scale
                new Vector2(0.0f, 1.0f)           // offset
            );

            // CopyTexture 相当
            // SharedTexture は Texture2D なので、コマンドバッファでも CopyTexture 可能
            cmd.CopyTexture(tempRTId, 0, 0, SharedTexture, 0, 0);

            // 一時RTを解放
            cmd.ReleaseTemporaryRT(tempRTId);

            // コマンドバッファを実行
            Graphics.ExecuteCommandBuffer(cmd);

            // コマンドバッファを破棄（使い回さない場合）
            cmd.Release();
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

            Console.WriteLine("Shader Loaded.");

            root = this.gameObject;

            CameraRoot = new GameObject();
            CameraRoot.name = "CameraRoot";
            CameraRoot.transform.position = new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);
            CameraRoot.transform.rotation = UnityEngine.Quaternion.identity;
            CameraRoot.transform.parent = root.transform;


            CameraComponent = CameraRoot.AddComponent<UnityEngine.Camera>();
            CameraComponent.depth = -128;
            CameraComponent.fieldOfView = 90.0f;
            cubeComponent = CameraRoot.AddComponent<CubemapToOtherProjection>();
            cubeComponent.UseUnityInternalCubemapRenderer = true;

            if (Main.Config.GetValue<bool>(Main.SPOUT_ENABLE))
            {
                var resolution = Main.Config.GetValue(Main.OUTPUT_SIZE);
                Plugin = PluginEntry.CreateSender("VRCam", resolution.x, resolution.y);
                SourceTexture = new UnityEngine.RenderTexture(resolution.x, resolution.y, 24);
                cubeComponent.RenderTarget = SourceTexture;
            }

            cubeComponent.RenderInStereo = Main.Config.GetValue<bool>(Main.RENDER_IN_STEREO);
            cubeComponent.ProjectionType = Main.Config.GetValue<ProjectionType>(Main.PROJECTION_TYPE);
            cubeComponent.CubemapSize = (int)Main.Config.GetValue<Main.CubeMapSize>(Main.CUBEMAP_SIZE);


        }



        void Update()
        {
            try
            {
                CameraRoot.transform.position = new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);
                CameraRoot.transform.rotation = UnityEngine.Quaternion.identity;

                root.transform.position = Main.CameraOrigin;
                root.transform.rotation = Main.CameraRotation;
                root.transform.localScale = Main.CameraScale;

                // Update the plugin internal state.
                if (Plugin != System.IntPtr.Zero)
                    SpoutUtil.IssuePluginEvent(PluginEntry.Event.Update, Plugin);

                // Render texture mode update
                if (SourceTexture != null)
                    SendRenderTexture();

            } catch (Exception e)
            {
                Console.WriteLine(e);
                // Console.Error.WriteLine(e);
            }
        }
    }

    public class Main : ResoniteMod
    {
        public override string Name => "Reso360Spout";
        public override string Author => "kka429";
        public override string Version => "0.0.1";
        public override string Link => "https://github.com/rassi0429/Reso360Spout";

        public static ModConfiguration Config;

        public static Vector3 CameraOrigin;
        public static Quaternion CameraRotation;
        public static Vector3 CameraScale;

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> SPOUT_ENABLE = new ModConfigurationKey<bool>("SPOUT_ENABLE", "spout enable", () => true);


        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<ProjectionType> PROJECTION_TYPE = new ModConfigurationKey<ProjectionType>("PROJECTION_TYPE", "Projection Type", () => ProjectionType.Equirectangular_180);


        public enum CubeMapSize: int { Low=512, Mid=1024, High=2048, Ultra=3072 };

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<CubeMapSize> CUBEMAP_SIZE = new ModConfigurationKey<CubeMapSize>("CUBEMAP_SIZE", "cubemap size 512,1024,2048,3072", () => CubeMapSize.High);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<int2> OUTPUT_SIZE = new ModConfigurationKey<int2>("OUTPUT_SIZE", "output size", () => new int2(6144, 3072));

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> RENDER_IN_STEREO = new ModConfigurationKey<bool>("RENDER_IN_STEREO", "render in stereo", () => true);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> NEAR_CLIP = new ModConfigurationKey<float>("NEAR_CLIP", "near clip", () => 0.065f);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> FAR_CLIP = new ModConfigurationKey<float>("FAR_CLIP", "far clip", () => 3000f);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> HIDE_LOCAL = new ModConfigurationKey<bool>("HIDE_LOCAL", "localを映さないようにする", () => false);

        static GameObject modEntry;
        static UnityEntry unityEntry;


        static void OnConfigChanged(ConfigurationChangedEvent configChangedEvent)
        {
            Msg("Config changed: " + configChangedEvent.Key.Name);

            if (configChangedEvent.Key == HIDE_LOCAL)
            {
                if (Config.GetValue(HIDE_LOCAL))
                {
                    Msg("curring mask true");
                    unityEntry.CameraComponent.cullingMask &= ~((1 << 28) + (1 << 29) + (1 << 30) + (1 << 31));
                }
                else
                {
                    Msg("curring mask false");
                    unityEntry.CameraComponent.cullingMask |= ((1 << 28) + (1 << 29) + (1 << 30) + (1 << 31));
                }
            }

            if (configChangedEvent.Key == NEAR_CLIP)
            {
                unityEntry.CameraComponent.nearClipPlane = Config.GetValue(NEAR_CLIP);
            }

            if (configChangedEvent.Key == FAR_CLIP)
            {
                unityEntry.CameraComponent.farClipPlane = Config.GetValue(FAR_CLIP);
            }

            if (configChangedEvent.Key == CUBEMAP_SIZE)
            {
                unityEntry.cubeComponent.CubemapSize = (int)Config.GetValue(CUBEMAP_SIZE);
            }

            if (configChangedEvent.Key == PROJECTION_TYPE)
            {
                unityEntry.cubeComponent.ProjectionType = Config.GetValue(PROJECTION_TYPE);
            }

            if (configChangedEvent.Key == RENDER_IN_STEREO)
            {
                unityEntry.cubeComponent.RenderInStereo = Config.GetValue(RENDER_IN_STEREO);
            }

            if (configChangedEvent.Key == SPOUT_ENABLE)
            {
                if (Config.GetValue(SPOUT_ENABLE))
                {
                    var resolution = Config.GetValue(OUTPUT_SIZE);
                    unityEntry.Plugin = PluginEntry.CreateSender("VRCam", resolution.x, resolution.y);
                    unityEntry.SourceTexture = new UnityEngine.RenderTexture(resolution.x, resolution.y, 24);
                    unityEntry.cubeComponent.RenderTarget = unityEntry.SourceTexture;
                }
                else
                {
                    if(unityEntry.Plugin != System.IntPtr.Zero)
                        SpoutUtil.IssuePluginEvent(PluginEntry.Event.Dispose, unityEntry.Plugin);
                    unityEntry.Plugin = System.IntPtr.Zero;
                    unityEntry.SourceTexture = null;
                    unityEntry.cubeComponent.RenderTarget = null;
                }
            }
        }

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("dev.kokoa.Reso360Spout");
            harmony.PatchAll();


            Config = GetConfiguration();
            Config.OnThisConfigurationChanged += OnConfigChanged;

            Config.Save();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Msg("scene name: " + scene.name);

            modEntry = new UnityEngine.GameObject();
            modEntry.name = "___MODEntry";
            unityEntry = modEntry.AddComponent<UnityEntry>();


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

                __instance.WorldManager.FocusedWorld.RunInBackground(() =>
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

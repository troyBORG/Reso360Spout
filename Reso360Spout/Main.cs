using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Reso360Spout.Shared;

namespace Reso360Spout
{
    // Shared data structure for IPC between main and renderer processes
    // This is accessed from both processes if the DLL is loaded in both
    public static class SharedCameraData
    {
        public static Vector3 Origin = Vector3.zero;
        public static Quaternion Rotation = Quaternion.identity;
        public static Vector3 Scale = Vector3.one;
        public static bool IsDirty = false;
    }

    // Unity components - only runs in Renderer process
    public class UnityEntry : MonoBehaviour
    {
        // --- Public Fields --------------------------------------------------
        public UnityEngine.Camera? CameraComponent;
        public CubemapToOtherProjection? cubeComponent;
        public IntPtr Plugin = IntPtr.Zero;
        public UnityEngine.RenderTexture? SourceTexture;
        public UnityEngine.Texture2D? SharedTexture;

        // --- Static Fields (Shaders) ---------------------------------------
        public static UnityEngine.Shader? cubemapShader;
        public static UnityEngine.Shader? cubemapRenderer;

        // --- Static Fields (Singleton) -------------------------------------
        internal static GameObject? _modEntry;
        internal static UnityEntry? _unityEntry;

        // --- Private Fields ------------------------------------------------
        private GameObject? _root;
        private GameObject? _cameraRoot;
        private object? _messenger; // Use object to avoid type conflicts between InterprocessLib.Unity and InterprocessLib.FrooxEngine
        private System.Collections.Concurrent.ConcurrentQueue<System.Action> _mainQueue = new();
        
        // Current camera transform (received via IPC)
        private Vector3 _currentOrigin = Vector3.zero;
        private Quaternion _currentRotation = Quaternion.identity;
        private Vector3 _currentScale = Vector3.one;

        // --- Runtime Initialize (Runs in Renderer process) -----------------
        // This is called automatically when the DLL is loaded in the Renderer process
        // The DLL is loaded via BepInEx.Renderer bootstrap plugin (Reso360SpoutBootstrap.dll)
        // which calls Assembly.LoadFrom() to load this DLL, triggering RuntimeInitializeOnLoadMethod
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeUnityComponents()
        {
            // Only create if it doesn't exist yet
            if (_modEntry != null) return;

            try
            {
                UnityEngine.Debug.Log("[Reso360Spout] RuntimeInitializeOnLoadMethod called - attempting to create Unity components...");
                _modEntry = new GameObject("___MODEntry");
                UnityEngine.Object.DontDestroyOnLoad(_modEntry);
                _unityEntry = _modEntry.AddComponent<UnityEntry>();
                UnityEngine.Debug.Log("[Reso360Spout] Unity components initialized successfully in Renderer process!");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[Reso360Spout] Failed to initialize Unity components: {e}");
                UnityEngine.Debug.LogError($"[Reso360Spout] Stack trace: {e.StackTrace}");
            }
        }

        // --- MonoBehaviour Methods -----------------------------------------
        private void Start()
        {
            // Initialize InterprocessLib Messenger for receiving commands
            // Use reflection to avoid type conflicts between InterprocessLib.Unity and InterprocessLib.FrooxEngine
            try
            {
                var messengerType = Type.GetType("InterprocessLib.Messenger, InterprocessLib.Unity") 
                    ?? Type.GetType("InterprocessLib.Messenger, InterprocessLib.FrooxEngine");
                if (messengerType != null)
                {
                    var messenger = Activator.CreateInstance(messengerType, "dev.kokoa.Reso360Spout", new[] { typeof(Reso360Spout.Shared.CameraCommand) });
                    _messenger = messenger;
                    
                    var receiveMethod = messengerType.GetMethod("ReceiveObject", new[] { typeof(string), typeof(Action<Reso360Spout.Shared.CameraCommand>) });
                    if (receiveMethod != null)
                    {
                        receiveMethod.Invoke(messenger, new object[] { "CameraCommand", new Action<Reso360Spout.Shared.CameraCommand>((command) =>
                        {
                            _mainQueue.Enqueue(() => ProcessCameraCommand(command));
                        }) });
                    }
                    Debug.Log("[Reso360Spout] InterprocessLib Messenger initialized in Renderer");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Reso360Spout] Failed to initialize Messenger: {e}");
                // Fall back to static fields if Messenger fails
            }

            LoadShadersFromAssetBundle();

            // ルートオブジェクトの参照
            _root = gameObject;

            // カメラ用の GameObject 生成
            CreateCameraRoot();

            // CubemapToOtherProjection の設定
            if (_cameraRoot != null)
            {
                cubeComponent = _cameraRoot.AddComponent<CubemapToOtherProjection>();
            }

            // Spout（プラグイン）有効時の初期化
            if (Main.Config != null && Main.Config.GetValue<bool>(Main.SPOUT_ENABLE))
            {
                InitSpoutPlugin();
            }

            // 各種設定の反映
            if (cubeComponent != null && CameraComponent != null && Main.Config != null)
            {
                cubeComponent.RenderInStereo = Main.Config.GetValue<bool>(Main.RENDER_IN_STEREO);
                cubeComponent.ProjectionType = Main.Config.GetValue<ProjectionType>(Main.PROJECTION_TYPE);
                cubeComponent.CubemapSize = (int)Main.Config.GetValue<Main.CubeMapSize>(Main.CUBEMAP_SIZE);

                CameraComponent.nearClipPlane = Main.Config.GetValue<float>(Main.NEAR_CLIP);
                CameraComponent.farClipPlane = Main.Config.GetValue<float>(Main.FAR_CLIP);

                // Ensure bit 28 is off at startup
                CameraComponent.cullingMask &= ~(1 << 28);
            }
        }

        private void Update()
        {
            try
            {
                // Process queued commands from main process
                while (_mainQueue.TryDequeue(out var action))
                {
                    try { action(); }
                    catch (Exception e) { Debug.LogError($"[Reso360Spout] Error processing command: {e}"); }
                }

                // カメラのトランスフォームを毎フレーム更新
                // Prefer IPC data, fall back to static fields for backward compatibility
                if (_root != null)
                {
                    // Use IPC data if available, otherwise fall back to static fields
                    if (_messenger != null)
                    {
                        _root.transform.position = _currentOrigin;
                        _root.transform.rotation = _currentRotation;
                        _root.transform.localScale = _currentScale;
                    }
                    else
                    {
                        // Fallback to static fields
                        _root.transform.position = SharedCameraData.Origin;
                        _root.transform.rotation = SharedCameraData.Rotation;
                        _root.transform.localScale = SharedCameraData.Scale;
                    }
                }

                // プラグイン内部状態を更新
                if (Plugin != IntPtr.Zero)
                {
                    SpoutUtil.IssuePluginEvent(PluginEntry.Event.Update, Plugin);
                }

                // RenderTexture 送出
                if (SourceTexture != null)
                {
                    SendRenderTexture();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Reso360Spout] " + e);
            }
        }

        // --- Private Methods -----------------------------------------------
        /// <summary>
        /// AssetBundle からシェーダーをロードして static フィールドに設定する
        /// </summary>
        private void LoadShadersFromAssetBundle()
        {
            // Try multiple possible paths for the AssetBundle
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "rml_mods", "cubeto360"),
                System.IO.Path.Combine(Application.dataPath, "..", "rml_mods", "cubeto360"),
                System.IO.Path.Combine(Application.dataPath, "..", "..", "rml_mods", "cubeto360"),
                @"rml_mods\cubeto360",
                "rml_mods/cubeto360"
            };

            AssetBundle? assets = null;
            foreach (var path in possiblePaths)
            {
                var normalizedPath = System.IO.Path.GetFullPath(path);
                if (System.IO.File.Exists(normalizedPath))
                {
                    assets = AssetBundle.LoadFromFile(normalizedPath);
                    if (assets != null)
                    {
                        Debug.Log($"[Reso360Spout] AssetBundle loaded from: {normalizedPath}");
                        break;
                    }
                }
            }

            if (assets == null)
            {
                Debug.LogWarning("[Reso360Spout] AssetBundle not found. Tried paths: " + string.Join(", ", possiblePaths));
                return;
            }

            // 全ての Shader をロード
            var allShaders = assets.LoadAllAssets<UnityEngine.Shader>();
            foreach (var shader in allShaders)
            {
                if (shader.name == "Unlit/CubemapToOtherProjection")
                {
                    cubemapShader = shader;
                }
                else if (shader.name == "Unlit/CubemapRenderer")
                {
                    cubemapRenderer = shader;
                }
            }

            Debug.Log("[Reso360Spout] Shaders loaded.");
        }

        /// <summary>
        /// カメラ用のゲームオブジェクトを作成し、初期化する
        /// </summary>
        private void CreateCameraRoot()
        {
            if (_root == null) return;

            _cameraRoot = new GameObject("CameraRoot");
            _cameraRoot.transform.SetParent(_root.transform, false);
            _cameraRoot.transform.localPosition = Vector3.zero;
            _cameraRoot.transform.localRotation = Quaternion.identity;

            // Camera の設定
            CameraComponent = _cameraRoot.AddComponent<UnityEngine.Camera>();
            if (CameraComponent != null)
            {
                CameraComponent.depth = -128;
                CameraComponent.fieldOfView = 90.0f;
                CameraComponent.stereoTargetEye = StereoTargetEyeMask.None;
                CameraComponent.stereoSeparation = 0.065f;
            }
        }

        /// <summary>
        /// Spout（プラグイン）を初期化する
        /// </summary>
        private void InitSpoutPlugin()
        {
            if (Main.Config == null) return;

            // 出力解像度を取得
            var resolution = Main.Config.GetValue(Main.OUTPUT_SIZE);
            Plugin = PluginEntry.CreateSender("VRCam", resolution.x, resolution.y);

            // RenderTexture の作成
            SourceTexture = new UnityEngine.RenderTexture(resolution.x, resolution.y, 24);
            if (cubeComponent != null)
            {
                cubeComponent.RenderTarget = SourceTexture;
            }
        }

        /// <summary>
        /// CommandBuffer を利用して RenderTexture の内容を SharedTexture に転送
        /// </summary>
        private void SendRenderTexture()
        {
            // Spout プラグイン未初期化時はスキップ
            if (Plugin == IntPtr.Zero)
            {
                return;
            }

            // SharedTexture の初期化
            if (SharedTexture == null && Main.Config != null && Main.Config.GetValue(Main.SPOUT_ENABLE))
            {
                var ptr = PluginEntry.GetTexturePointer(Plugin);
                if (ptr != IntPtr.Zero)
                {
                    SharedTexture = UnityEngine.Texture2D.CreateExternalTexture(
                        PluginEntry.GetTextureWidth(Plugin),
                        PluginEntry.GetTextureHeight(Plugin),
                        UnityEngine.TextureFormat.ARGB32,
                        false,
                        false,
                        ptr
                    );
                    SharedTexture.hideFlags = HideFlags.DontSave;
                }
            }

            // SharedTexture が存在しなければ処理を中断
            if (SharedTexture == null) return;

            // コマンドバッファ作成
            var cmd = new CommandBuffer { name = "SpoutSend" };

            // 一時的な RT の ID を確保
            int tempRTId = UnityEngine.Shader.PropertyToID("_TempSpoutRT");
            cmd.GetTemporaryRT(tempRTId,
                SharedTexture.width,
                SharedTexture.height,
                0,
                FilterMode.Bilinear,
                RenderTextureFormat.ARGB32
            );

            // scale, offset を使ってブリット
            cmd.Blit(
                SourceTexture,
                tempRTId,
                new Vector2(1.0f, -1.0f), // scale
                new Vector2(0.0f, 1.0f)   // offset
            );

            // CopyTexture
            cmd.CopyTexture(tempRTId, 0, 0, SharedTexture, 0, 0);

            // 一時RT解放
            cmd.ReleaseTemporaryRT(tempRTId);

            // 実行＆破棄
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

        // Process camera commands received via IPC
        private void ProcessCameraCommand(Reso360Spout.Shared.CameraCommand command)
        {
            switch (command.Type)
            {
                case Reso360Spout.Shared.CameraCommandType.UpdateTransform:
                    _currentOrigin = new Vector3(command.OriginX, command.OriginY, command.OriginZ);
                    _currentRotation = new Quaternion(command.RotationX, command.RotationY, command.RotationZ, command.RotationW);
                    _currentScale = new Vector3(command.ScaleX, command.ScaleY, command.ScaleZ);
                    break;
                case Reso360Spout.Shared.CameraCommandType.Initialize:
                    Debug.Log("[Reso360Spout] Received Initialize command from main process");
                    break;
                case Reso360Spout.Shared.CameraCommandType.Shutdown:
                    Debug.Log("[Reso360Spout] Received Shutdown command from main process");
                    break;
            }
        }
    }

    // Main mod class - runs in main process
    public class Main : ResoniteMod
    {
        // --- Properties (Override) ----------------------------------------
        public override string Name => "Reso360Spout";
        public override string Author => "kka429";
        public override string Version => "0.0.7";
        public override string Link => "https://github.com/rassi0429/Reso360Spout";

        // --- Public Fields ------------------------------------------------
        public static ModConfiguration? Config;
        public static object? _messenger; // Use object to avoid type conflicts

        // --- Config Keys --------------------------------------------------
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> SPOUT_ENABLE =
            new ModConfigurationKey<bool>("SPOUT_ENABLE", "Spout Enable", () => true);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<ProjectionType> PROJECTION_TYPE =
            new ModConfigurationKey<ProjectionType>("PROJECTION_TYPE", "Projection Type",
                () => ProjectionType.Equirectangular_180);

        public enum CubeMapSize : int { Low = 512, Mid = 1024, High = 2048, Ultra = 3072 }

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<CubeMapSize> CUBEMAP_SIZE =
            new ModConfigurationKey<CubeMapSize>("CUBEMAP_SIZE",
                "Cubemap Size (512, 1024, 2048, 3072)",
                () => CubeMapSize.High);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<int2> OUTPUT_SIZE =
            new ModConfigurationKey<int2>("OUTPUT_SIZE", "Output Size",
                () => new int2(6144, 3072));

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> RENDER_IN_STEREO =
            new ModConfigurationKey<bool>("RENDER_IN_STEREO", "Render in Stereo",
                () => true);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> NEAR_CLIP =
            new ModConfigurationKey<float>("NEAR_CLIP", "Near Clip",
                () => 0.01f);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> FAR_CLIP =
            new ModConfigurationKey<float>("FAR_CLIP", "Far Clip",
                () => 3000f);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> HIDE_LOCAL =
            new ModConfigurationKey<bool>("HIDE_LOCAL", "Hide Local User",
                () => true);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<string> CAMERA_SLOT_NAME =
            new ModConfigurationKey<string>("CAMERA_SLOT_NAME", "Camera Slot Name", () => "#Camera");

        // --- Methods (ResoniteMod) ----------------------------------------
        public override void OnEngineInit()
        {
            // Initialize InterprocessLib Messenger for IPC (main process)
            try
            {
                var messengerType = Type.GetType("InterprocessLib.Messenger, InterprocessLib.FrooxEngine");
                if (messengerType != null)
                {
                    var messenger = Activator.CreateInstance(messengerType, "dev.kokoa.Reso360Spout", new[] { typeof(CameraCommand) });
                    _messenger = messenger;
                    Msg("[Reso360Spout] InterprocessLib Messenger initialized");
                }
            }
            catch (Exception e)
            {
                Msg($"[Reso360Spout] Failed to initialize Messenger: {e}");
                // Fall back to static fields if Messenger fails
            }

            // Harmony パッチ
            Harmony harmony = new Harmony("dev.kokoa.Reso360Spout");
            harmony.PatchAll();

            // Config
            Config = GetConfiguration();
            Config.OnThisConfigurationChanged += OnConfigChanged;
            Config.Save();

            // カメラ座標等の初期値
            SharedCameraData.Origin = Vector3.zero;
            SharedCameraData.Rotation = Quaternion.identity;
            SharedCameraData.Scale = Vector3.one;
        }

        // --- Private Methods ----------------------------------------------
        private static void OnConfigChanged(ConfigurationChangedEvent configChangedEvent)
        {
            Main.Msg("[Reso360Spout] Config changed: " + configChangedEvent.Key.Name);

            try
            {
                // Config changes are handled in UnityEntry if needed
                // The Unity components will read from Config directly
            }
            catch (Exception e)
            {
                Main.Msg("[Reso360Spout] Config Error occurred: " + e);
            }
        }

        // --- Harmony Patch ------------------------------------------------
        [HarmonyPatch(typeof(FrooxEngine.Engine), "RunUpdateLoop")]
        class Patch
        {
            static void Postfix(FrooxEngine.Engine __instance)
            {
                if (__instance.WorldManager.FocusedWorld == null) return;

                __instance.WorldManager.FocusedWorld.RunSynchronously(() =>
                {
                    // 重そうなのであとで最適化を検討
                    if (Config == null) return;
                    string? cameraSlotName = Config.GetValue(CAMERA_SLOT_NAME);
                    if (cameraSlotName == null) return;
                    var origin = __instance.WorldManager.FocusedWorld.RootSlot.FindChildInHierarchy(cameraSlotName);
                    if (origin != null)
                    {
                        // Update static fields for backward compatibility
                        SharedCameraData.Origin.Set(
                            origin.GlobalPosition.x,
                            origin.GlobalPosition.y,
                            origin.GlobalPosition.z
                        );
                        SharedCameraData.Rotation = new Quaternion(
                            origin.GlobalRotation.x,
                            origin.GlobalRotation.y,
                            origin.GlobalRotation.z,
                            origin.GlobalRotation.w
                        );
                        SharedCameraData.Scale.Set(
                            origin.GlobalScale.x,
                            origin.GlobalScale.y,
                            origin.GlobalScale.z
                        );
                        SharedCameraData.IsDirty = true;

                        // Send via InterprocessLib Messenger (preferred method)
                        if (_messenger != null)
                        {
                            try
                            {
                                var command = new CameraCommand
                                {
                                    Type = CameraCommandType.UpdateTransform,
                                    OriginX = origin.GlobalPosition.x,
                                    OriginY = origin.GlobalPosition.y,
                                    OriginZ = origin.GlobalPosition.z,
                                    RotationX = origin.GlobalRotation.x,
                                    RotationY = origin.GlobalRotation.y,
                                    RotationZ = origin.GlobalRotation.z,
                                    RotationW = origin.GlobalRotation.w,
                                    ScaleX = origin.GlobalScale.x,
                                    ScaleY = origin.GlobalScale.y,
                                    ScaleZ = origin.GlobalScale.z
                                };
                                // Use reflection to call SendObject
                                var sendMethod = _messenger.GetType().GetMethod("SendObject", new[] { typeof(string), typeof(object) });
                                sendMethod?.Invoke(_messenger, new object[] { "CameraCommand", command });
                            }
                            catch (Exception e)
                            {
                                // Log but don't fail if Messenger has issues
                                if (SharedCameraData.IsDirty) // Only log once per update
                                {
                                    // Silent fallback to static fields
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}

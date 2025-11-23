// Reso360SpoutBootstrap.cs
// BepInEx renderer-side plugin to bootstrap Reso360Spout in the Unity Renderer process
// This plugin runs in the Renderer process and loads our managed DLL
// 
// NOTE: This requires BepInEx.Renderer to be installed separately
// BepisLoader alone is not enough - you need BepInExRenderer.dll from Thunderstore

using System;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace Reso360Spout
{
    [BepInPlugin("dev.kokoa.Reso360Spout.Bootstrap", "Reso360Spout Bootstrap", "1.0.0")]
    public class Reso360SpoutBootstrap : BaseUnityPlugin
    {
        private void Start()
        {
            Logger.LogInfo("[Reso360SpoutBootstrap] Starting bootstrap in Renderer process...");

            try
            {
                // Try to load the managed DLL from the Managed folder
                string managedPath = Path.Combine(Application.dataPath, "..", "Renderite.Renderer_Data", "Managed", "Reso360Spout.dll");
                
                // Also try alternative paths
                if (!File.Exists(managedPath))
                {
                    string[] alternativePaths = new[]
                    {
                        Path.Combine(Directory.GetCurrentDirectory(), "Renderite.Renderer_Data", "Managed", "Reso360Spout.dll"),
                        Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "Renderite.Renderer_Data", "Managed", "Reso360Spout.dll"),
                    };
                    
                    foreach (var path in alternativePaths)
                    {
                        if (File.Exists(path))
                        {
                            managedPath = path;
                            break;
                        }
                    }
                }

                if (File.Exists(managedPath))
                {
                    Logger.LogInfo($"[Reso360SpoutBootstrap] Loading managed DLL from: {managedPath}");
                    var asm = Assembly.LoadFrom(managedPath);
                    Logger.LogInfo($"[Reso360SpoutBootstrap] Assembly loaded: {asm.FullName}");
                    
                    // The RuntimeInitializeOnLoadMethod should execute automatically now
                    // Verify that UnityEntry type exists
                    var entryType = asm.GetType("Reso360Spout.UnityEntry");
                    if (entryType != null)
                    {
                        Logger.LogInfo("[Reso360SpoutBootstrap] UnityEntry type found - RuntimeInitializeOnLoadMethod should run automatically");
                    }
                    else
                    {
                        Logger.LogWarning("[Reso360SpoutBootstrap] UnityEntry type not found in loaded assembly");
                    }
                }
                else
                {
                    Logger.LogError($"[Reso360SpoutBootstrap] Managed DLL not found. Tried: {managedPath}");
                    Logger.LogError("[Reso360SpoutBootstrap] Please ensure Reso360Spout.dll is copied to Renderer\\Renderite.Renderer_Data\\Managed\\");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[Reso360SpoutBootstrap] Failed to initialize: {e}");
                Logger.LogError($"[Reso360SpoutBootstrap] Stack trace: {e.StackTrace}");
            }
        }
    }
}

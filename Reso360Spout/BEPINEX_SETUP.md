# BepInEx.Renderer Setup for Reso360Spout

## Overview

Reso360Spout requires BepInEx.Renderer to bootstrap the mod in the Unity Renderer process. This breaks the circular dependency by providing managed code that runs in the Renderer and can load our managed DLL.

**Important:** BepisLoader alone is NOT enough - you need BepInEx.Renderer specifically for renderer-side plugins. BepisLoader can load RML mods in the main process, but BepInEx.Renderer is what injects into the Renderer process.

## Prerequisites

You need to install these mods from Thunderstore (or manually):

1. **BepisLoader** - https://thunderstore.io/c/resonite/p/ResoniteModding/BepisLoader/
   - This loads in the main process and can load RML mods
   - Place in `rml_mods` folder

2. **BepInEx.Renderer** - https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExRenderer/
   - This is what injects into the Renderer process
   - Place in `rml_mods` folder (it will set up the Renderer\BepInEx structure automatically)

3. **InterprocessLib** - Required for IPC between main and renderer processes
   - **Main process**: Install `InterprocessLib.BepisLoader` from Thunderstore
     - Place in `rml_mods` folder (or wherever BepisLoader plugins go)
   - **Renderer process**: Install `InterprocessLib.BepInEx` from Thunderstore
     - Place in `Renderer\BepInEx\plugins\` folder
   - These enable the Messenger system for inter-process communication
   - Without these, the mod will fall back to static field IPC (less reliable)

## Installation Steps

1. **Install BepInEx dependencies:**
   - Download and install `BepisLoader.dll` and `BepInExRenderer.dll` to `rml_mods` folder
   - Restart Resonite to ensure BepInEx.Renderer is initialized

2. **Build the BepInEx plugin:**
   - The `Reso360SpoutBootstrap.cs` file needs to be compiled as a BepInEx plugin
   - It should reference BepInEx.dll (from BepInEx.Renderer installation)
   - Output should be `Reso360SpoutBootstrap.dll`

3. **Place files:**
   - `Reso360SpoutBootstrap.dll` → `Renderer\BepInEx\plugins\` (BepInEx will load it automatically)
   - `Reso360SpoutLoader.dll` → `Renderer\Renderite.Renderer_Data\Plugins\x86_64\` (native plugin)
   - `Reso360Spout.dll` → `Renderer\Renderite.Renderer_Data\Managed\` (managed mod DLL)

4. **Restart Resonite**

## How It Works

1. BepInEx.Renderer injects into the Unity Renderer process
2. BepInEx loads `Reso360SpoutBootstrap.dll` from `Renderer\BepInEx\plugins\`
3. The bootstrap plugin's `Start()` method calls `Reso360Spout_Initialize()` via DllImport
4. Unity loads `Reso360SpoutLoader.dll` to resolve the DllImport
5. `Reso360Spout_Initialize()` uses Mono embedding API to load `Reso360Spout.dll`
6. `RuntimeInitializeOnLoadMethod` in `Reso360Spout.dll` runs and initializes Unity components

## Alternative: Direct Assembly Loading

If you prefer not to use the native plugin, you can modify `Reso360SpoutBootstrap.cs` to directly load the managed DLL:

```csharp
private void Start()
{
    try
    {
        string managedPath = Path.Combine(Application.dataPath, "..", "Renderite.Renderer_Data", "Managed", "Reso360Spout.dll");
        var asm = Assembly.LoadFrom(managedPath);
        var entryType = asm.GetType("Reso360Spout.UnityEntry");
        // Call initialization if needed
    }
    catch (Exception e)
    {
        Logger.LogError($"Failed to load Reso360Spout: {e}");
    }
}
```

This eliminates the need for the native plugin entirely, but you'll still need BepInEx.Renderer as the bootstrap.


# Reso360Spout Installation Guide

## Overview

Reso360Spout works with the splittening update by using **BepInEx.Renderer** to load the mod DLL in the Renderer process. This is the recommended and most reliable method.

## Prerequisites

1. **BepisLoader** - https://thunderstore.io/c/resonite/p/ResoniteModding/BepisLoader/
   - Loads RML mods in the main process
   - Place in `rml_mods` folder

2. **BepInEx.Renderer** - https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExRenderer/
   - Injects into the Renderer process and loads plugins
   - Place in `rml_mods` folder (it will set up the `Renderer\BepInEx` structure automatically)

## Installation Steps

1. **Install BepInEx dependencies**:
   - Download and install `BepisLoader.dll` and `BepInExRenderer.dll` to `rml_mods` folder
   - Restart Resonite to ensure BepInEx.Renderer is initialized

2. **Build the mod**:
   - Build `Reso360Spout.csproj` - it will automatically copy `Reso360Spout.dll` to:
     - `rml_mods\Reso360Spout.dll` (for main process)
     - `Renderer\Renderite.Renderer_Data\Managed\Reso360Spout.dll` (for renderer process)

3. **Build the BepInEx bootstrap plugin**:
   - Build `Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj`
   - It will automatically copy `Reso360SpoutBootstrap.dll` to `Renderer\BepInEx\plugins\`

4. **Copy required files**:
   - `KlakSpout.dll` → `Renderer\Renderite.Renderer_Data\Plugins\x86_64\KlakSpout.dll`
     - ✅ **Automatically copied during build** - The DLL is included in the repository and will be copied automatically
     - If you need to rebuild it, source code is in `Reso360Spout/KlakSpout/`
   - `cubeto360` (asset bundle) → `rml_mods\cubeto360`
     - **Note**: This is the shader asset bundle from the original release (not included in repository)

5. **Restart Resonite**

## How It Works

1. **Main Process**: RML loads `Reso360Spout.dll` from `rml_mods\`
   - `Main.OnEngineInit()` runs and sets up Harmony patches
   - Camera tracking updates `SharedCameraData` static fields

2. **Renderer Process**: BepInEx.Renderer injects into Unity
   - BepInEx loads `Reso360SpoutBootstrap.dll` from `Renderer\BepInEx\plugins\`
   - The bootstrap plugin's `Start()` method loads `Reso360Spout.dll` from `Renderer\Renderite.Renderer_Data\Managed\`
   - `RuntimeInitializeOnLoadMethod` runs and initializes Unity components
   - Unity components read from `SharedCameraData` and render to Spout

## File Locations Summary

```
Resonite/
├── rml_mods/
│   ├── Reso360Spout.dll          (main process)
│   └── cubeto360                 (asset bundle)
└── Renderer/
    └── Renderite.Renderer_Data/
        ├── Managed/
        │   └── Reso360Spout.dll  (renderer process)
        └── Plugins/
            └── x86_64/
                ├── Reso360SpoutLoader.dll  (native plugin - loads managed DLL)
                └── KlakSpout.dll          (Spout plugin)
```

## Troubleshooting

If the mod doesn't work:

1. **Check that the native plugin is loaded**:
   - Look for `[Reso360SpoutLoader]` messages in `Player.log`
   - The log file is usually at: `%USERPROFILE%\AppData\LocalLow\Resonite\Player.log`

2. **Verify file locations**:
   - Ensure `Reso360Spout.dll` is in both `rml_mods\` and `Renderer\Renderite.Renderer_Data\Managed\`
   - Ensure `Reso360SpoutLoader.dll` is in `Renderer\Renderite.Renderer_Data\Plugins\x86_64\`

3. **Check Unity logs**:
   - Look for `[Reso360Spout] RuntimeInitializeOnLoadMethod called` in the Renderer log
   - If you don't see this, the DLL isn't being loaded in the Renderer process

## Building the Native Plugin

You'll need:
- Visual Studio 2019 or later with C++ support
- Unity 2019.4 headers (or compatible version)

See `NATIVE_PLUGIN_README.md` for detailed build instructions.


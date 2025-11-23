# Reso360Spout - BepInEx.Renderer Integration Status

## Current Status: BLOCKED - Missing RML Mod DLL + Need to Refactor IPC

This branch contains work to adapt Reso360Spout for Resonite's "splittening" update, which separated FrooxEngine into a separate process from Unity. The mod now requires BepInEx.Renderer to load in the Unity Renderer process.

**UPDATE:** After examining [ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout) (a working Spout mod for Resonite), we discovered that our IPC approach needs to be refactored. ResoniteSpout uses:
- **InterprocessLib** for IPC (not static fields)
- **Renderite.Shared's RendererCommand** for sending commands to the renderer
- Separate Engine and Renderer BepInEx plugins

This is the proper way to handle IPC in Resonite's split architecture.

## What's Working

✅ **Main Process Mod (RML)**
- `Reso360Spout.dll` loads in the main FrooxEngine process via RML
- Harmony patches capture camera data and update `SharedCameraData` static class
- IPC mechanism using static fields works correctly

✅ **BepInEx Bootstrap Plugin**
- `Reso360SpoutBootstrap.dll` built and ready
- Located at: `Renderer\BepInEx\plugins\Reso360SpoutBootstrap.dll`
- Will load `Reso360Spout.dll` in the Renderer process when BepInEx is active

✅ **BepInEx Core Files**
- BepInEx core DLLs installed at: `Renderer\BepInEx\core\`
- `winhttp.dll` preloader installed at: `Renderer\winhttp.dll`

✅ **Build System**
- Main mod builds successfully
- Bootstrap plugin builds successfully
- Automatic file copying configured in `.csproj` files

## What's Blocking Us

❌ **Missing RML Mod DLL for BepInEx.Renderer**

The BepInEx.Renderer package from Thunderstore only contains:
- BepInEx core files (DLLs, XMLs)
- `manifest.json`
- `README.md`

**What's Missing:**
- The RML mod DLL that needs to be in `rml_mods\` folder
- This DLL is what tells Resonite to:
  1. Load BepInEx in the Renderer process
  2. Set up the BepInEx structure
  3. Initialize the preloader (`winhttp.dll`)

**Where It Should Come From:**
- The RML mod DLL is likely:
  1. Part of BepisLoader (if BepisLoader handles BepInEx.Renderer installation)
  2. Created by Thunderstore Mod Manager when installing the package
  3. A separate component that needs to be built from source

**Current Investigation:**
- The [BepInEx.Renderer GitHub repository](https://github.com/ResoniteModding/BepInEx.Renderer) only contains build scripts that package BepInEx files
- It does not contain the source code for the RML mod DLL
- The RML mod DLL must be provided by another component or created separately

## File Locations

### Main Process (RML)
- `Reso360Spout.dll` → `rml_mods\Reso360Spout.dll`

### Renderer Process (BepInEx)
- `Reso360Spout.dll` → `Renderer\Renderite.Renderer_Data\Managed\Reso360Spout.dll`
- `Reso360SpoutBootstrap.dll` → `Renderer\BepInEx\plugins\Reso360SpoutBootstrap.dll`
- `KlakSpout.dll` → `Renderer\Renderite.Renderer_Data\Plugins\x86_64\KlakSpout.dll`

### BepInEx.Renderer (Installed)
- BepInEx core files → `Renderer\BepInEx\core\`
- `winhttp.dll` → `Renderer\winhttp.dll`

### Missing
- RML mod DLL → `rml_mods\BepInExRenderer.dll` (or similar name)

## Next Steps

1. **Refactor IPC to Use InterprocessLib** (HIGH PRIORITY)
   - Replace static field IPC with InterprocessLib's `Messenger` class
   - Use `Renderite.Shared.RendererCommand` for sending camera data to renderer
   - Follow the pattern from [ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout)
   - Add InterprocessLib dependency to both Engine and Renderer projects

2. **Restructure Projects**
   - Split into separate projects:
     - `Reso360Spout.Engine` - BepInEx plugin for main process (using BepisResoniteWrapper)
     - `Reso360Spout.Renderer` - BepInEx plugin for renderer process
     - `Reso360Spout.Shared` - Shared command classes (extending RendererCommand)
   - Update build configuration to match ResoniteSpout's structure

3. **Find the RML Mod DLL Source**
   - Check if BepisLoader provides it
   - Check if there's a separate repository for the RML mod component
   - Check Thunderstore package structure more carefully

4. **Test Once Complete**
   - Install the RML mod DLL in `rml_mods\`
   - Restart Resonite
   - Check `Renderer\BepInEx\LogOutput.log` for BepInEx initialization
   - Verify `Reso360SpoutBootstrap` loads and initializes

## Technical Details

### Architecture
- **Main Process**: FrooxEngine (.NET 9) - Runs `Reso360Spout.dll` via RML/BepInEx
- **Renderer Process**: Unity (2019.4.19f1) - Runs `Reso360Spout.dll` via BepInEx bootstrap
- **IPC**: Currently using static fields in `SharedCameraData` class (needs refactoring)
  - **Should use**: InterprocessLib's `Messenger` + `Renderite.Shared.RendererCommand`
  - **Reference**: See [ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout) for proper implementation

### Build Requirements
- .NET 10.0 SDK
- BepInEx.Renderer installed (for bootstrap plugin build)
- Unity references from Renderer process

### Dependencies
- **KlakSpout**: External Unity plugin for Spout video sharing
  - Source: https://github.com/keijiro/KlakSpout
  - Binary: `KlakSpout.dll` (included in repo)

## Related Issues

- Main process crashes with duplicate key error (unrelated to this mod, but prevents testing)
- BepInEx.Renderer installation via Thunderstore Mod Manager may not be working correctly
- Need to verify if BepisLoader is required before BepInEx.Renderer

## Resources

- [BepInEx.Renderer on Thunderstore](https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExRenderer/)
- [BepInEx.Renderer GitHub](https://github.com/ResoniteModding/BepInEx.Renderer)
- [BepisLoader on Thunderstore](https://thunderstore.io/c/resonite/p/ResoniteModding/BepisLoader/)
- [Resonite Modding Wiki](https://modding.resonite.net/)
- **[ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout)** - Working reference implementation for Spout mods in Resonite
  - Shows proper IPC using InterprocessLib
  - Shows proper project structure for Engine/Renderer split
  - Uses Renderite.Shared.RendererCommand for IPC

## Notes

- All code changes for splittening update are complete
- The mod should work once BepInEx.Renderer is properly installed
- The blocker is finding/creating the RML mod DLL component


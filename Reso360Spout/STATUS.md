# Reso360Spout - IPC Refactoring Complete

## Current Status: IPC REFACTORED - Ready for Testing

This branch contains work to adapt Reso360Spout for Resonite's "splittening" update, which separated FrooxEngine into a separate process from Unity. The mod now requires BepInEx.Renderer to load in the Unity Renderer process.

**UPDATE:** IPC has been refactored to use InterprocessLib, following the pattern from [ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout).

## What's Working

✅ **IPC Refactoring Complete**
- Created `Reso360Spout.Shared` project with `CameraCommand` extending `Renderite.Shared.RendererCommand`
- Main process (Engine) sends camera data via `InterprocessLib.Messenger.SendObject()`
- Renderer process (UnityEntry) receives commands via `InterprocessLib.Messenger.ReceiveObject()`
- Commands are queued and processed on the Unity main thread
- Static field fallback maintained for backward compatibility

✅ **Main Process Mod (RML)**
- `Reso360Spout.dll` loads in the main FrooxEngine process via RML
- Harmony patches capture camera data
- Sends `CameraCommand` via InterprocessLib Messenger

✅ **BepInEx Bootstrap Plugin**
- `Reso360SpoutBootstrap.dll` built and ready
- Located at: `Renderer\BepInEx\plugins\Reso360SpoutBootstrap.dll`
- Will load `Reso360Spout.dll` in the Renderer process when BepInEx is active

✅ **Build System**
- Main mod builds successfully
- Bootstrap plugin builds successfully
- Automatic file copying configured in `.csproj` files
- Shared project files properly included

## What's Blocking Us

❌ **Missing BepInExRenderer.dll RML mod**

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

❌ **InterprocessLib Installation Required**

InterprocessLib must be installed in both processes:
- Main process: `BepInEx\Plugins\InterprocessLib.BepisLoader\`
- Renderer process: `BepInEx\plugins\InterprocessLib.BepInEx\`

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
- InterprocessLib → Both main and renderer processes

## Next Steps

1. **Install InterprocessLib** (REQUIRED)
   - Install in main process: `BepInEx\Plugins\InterprocessLib.BepisLoader\`
   - Install in renderer process: `BepInEx\plugins\InterprocessLib.BepInEx\`
   - These are likely available on Thunderstore

2. **Find the RML Mod DLL Source**
   - Check if BepisLoader provides it
   - Check if there's a separate repository for the RML mod component
   - Check Thunderstore package structure more carefully

3. **Test Once Complete**
   - Install InterprocessLib in both processes
   - Install the RML mod DLL in `rml_mods\`
   - Restart Resonite
   - Check `Renderer\BepInEx\LogOutput.log` for BepInEx initialization
   - Verify `Reso360SpoutBootstrap` loads and initializes
   - Verify IPC communication works

## Technical Details

### IPC Implementation
- **Primary**: Uses `InterprocessLib.Messenger` for IPC between main and renderer processes
  - Main process sends `CameraCommand` via `Messenger.SendObject()`
  - Renderer process receives commands via `Messenger.ReceiveObject()` callback
  - Commands are queued and processed on the main Unity thread
- **Fallback**: Static fields (`SharedCameraData`) maintained for backward compatibility
  - Used if InterprocessLib is not available or fails to initialize
- **Shared Project**: `Reso360Spout.Shared\CameraCommand.cs` extends `Renderite.Shared.RendererCommand`
  - Implements `Pack()` and `Unpack()` for serialization
  - Contains camera transform data (position, rotation, scale)

### Architecture
- **Main Process**: FrooxEngine (.NET 9) - Runs `Reso360Spout.dll` via RML
- **Renderer Process**: Unity (2019.4.19f1) - Runs `Reso360Spout.dll` via BepInEx bootstrap
- **IPC**: InterprocessLib's `Messenger` + `Renderite.Shared.RendererCommand`
  - **Reference**: See [ResoniteSpout](https://github.com/Zozokasu/ResoniteSpout) for implementation details

### Build Requirements
- .NET 10.0 SDK
- BepInEx.Renderer installed (for bootstrap plugin build)
- Unity references from Renderer process
- Renderite.Shared.dll (for CameraCommand)

### Dependencies
- **KlakSpout**: External Unity plugin for Spout video sharing
  - Source: https://github.com/keijiro/KlakSpout
  - Binary: `KlakSpout.dll` (included in repo)
- **InterprocessLib**: Required for IPC
  - Main process: `InterprocessLib.BepisLoader`
  - Renderer process: `InterprocessLib.BepInEx`

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

- IPC refactoring is complete and follows the pattern from ResoniteSpout
- The mod should work once InterprocessLib is installed and BepInEx.Renderer is properly set up
- Static field fallback ensures the mod can still function if InterprocessLib is unavailable

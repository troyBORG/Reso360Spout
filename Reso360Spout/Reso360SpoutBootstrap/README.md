# Reso360SpoutBootstrap - BepInEx Renderer Plugin

This is a BepInEx plugin that runs in the Unity Renderer process to bootstrap Reso360Spout.

## Prerequisites

1. **BepInEx.Renderer must be installed** - This creates the `Renderer\BepInEx\` directory structure
2. The BepInEx DLLs should be in `Renderer\BepInEx\core\` after installation

## Building

```powershell
dotnet build "Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj" /p:ResoniteRendererPath="G:\SteamLibrary\steamapps\common\Resonite\Renderer"
```

Or set the environment variable:
```powershell
$env:ResoniteRendererPath = "G:\SteamLibrary\steamapps\common\Resonite\Renderer"
dotnet build "Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj"
```

## Output

The built plugin (`Reso360SpoutBootstrap.dll`) will be automatically copied to:
- `Renderer\BepInEx\plugins\Reso360SpoutBootstrap.dll`

## How It Works

1. BepInEx.Renderer injects into the Unity Renderer process
2. BepInEx discovers and loads `Reso360SpoutBootstrap.dll` from `Renderer\BepInEx\plugins\`
3. The plugin's `Start()` method runs in the Renderer process
4. It loads `Reso360Spout.dll` from `Renderer\Renderite.Renderer_Data\Managed\`
5. `RuntimeInitializeOnLoadMethod` in `Reso360Spout.dll` executes and initializes Unity components

## Troubleshooting

If the build fails with "BepInEx.dll not found":
- Make sure BepInEx.Renderer is installed
- Check that `Renderer\BepInEx\core\BepInEx.dll` exists
- Update the `ResoniteRendererPath` in the .csproj file or pass it as a build parameter


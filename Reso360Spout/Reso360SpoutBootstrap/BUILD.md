# Building Reso360SpoutBootstrap

## Prerequisites

1. **Install BepInEx.Renderer** from Thunderstore:
   - https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExRenderer/
   - This will create `Renderer\BepInEx\core\` with BepInEx.dll

2. **Verify BepInEx is installed:**
   ```
   G:\SteamLibrary\steamapps\common\Resonite\Renderer\BepInEx\core\BepInEx.dll
   ```

## Building

### Option 1: Build with explicit path
```powershell
dotnet build "Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj" /p:ResoniteRendererPath="G:\SteamLibrary\steamapps\common\Resonite\Renderer"
```

### Option 2: Set environment variable
```powershell
$env:ResoniteRendererPath = "G:\SteamLibrary\steamapps\common\Resonite\Renderer"
dotnet build "Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj"
```

### Option 3: Edit the .csproj file
Edit `Reso360SpoutBootstrap.csproj` and change line 62:
```xml
<ResoniteRendererPath Condition="'$(ResoniteRendererPath)' == ''">G:\SteamLibrary\steamapps\common\Resonite\Renderer</ResoniteRendererPath>
```

Then just run:
```powershell
dotnet build "Reso360SpoutBootstrap\Reso360SpoutBootstrap.csproj"
```

## Output

After building, the plugin will be automatically copied to:
```
G:\SteamLibrary\steamapps\common\Resonite\Renderer\BepInEx\plugins\Reso360SpoutBootstrap.dll
```

## Troubleshooting

**Error: "BepInEx.dll not found"**
- Make sure BepInEx.Renderer is installed
- Check that `Renderer\BepInEx\core\BepInEx.dll` exists
- Verify the `ResoniteRendererPath` is correct

**Error: "UnityEngine.dll not found"**
- Check that `Renderer\Renderite.Renderer_Data\Managed\UnityEngine.dll` exists
- Verify the `ResoniteRendererPath` points to the Renderer directory (not the Resonite root)


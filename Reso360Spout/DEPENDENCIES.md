# External Dependencies

This document lists external dependencies that are NOT built by this project and must be obtained separately.

## KlakSpout.dll

**Source**: https://github.com/keijiro/KlakSpout

**What it is**: A native Unity plugin for Spout video frame sharing. This is a third-party library that provides the Spout functionality.

**Status**: ✅ **Included in repository** - The pre-built `KlakSpout.dll` is now included in the `Reso360Spout/` directory.

**Build process**:
- The DLL is automatically copied to `Renderer\Renderite.Renderer_Data\Plugins\x86_64\KlakSpout.dll` during build
- Source code is available in `Reso360Spout/KlakSpout/` if you need to rebuild

**Note**: This is a native plugin (C++ DLL), not a managed .NET assembly. It must be in the Plugins folder for Unity to load it.

## cubeto360 (Asset Bundle)

**What it is**: A Unity AssetBundle containing the shaders needed for cubemap rendering and projection.

**Where to get it**:
- Download from the original Reso360Spout release
- This was likely built from shader source code (if available in the repository)

**Installation**:
- Copy `cubeto360` to: `rml_mods\cubeto360`

**Note**: This is a Unity AssetBundle file (no extension), not a DLL. It contains the shaders that the mod loads at runtime.

## Summary

When building this mod, you will get:
- ✅ `Reso360Spout.dll` (built)
- ✅ `Reso360SpoutBootstrap.dll` (built)

You still need to manually copy:
- ❌ `KlakSpout.dll` (external dependency)
- ❌ `cubeto360` (external dependency)

These files are typically included in the mod's release packages but are not part of the source code repository.


# KlakSpout Native Plugin

This directory contains the KlakSpout native plugin source code and pre-built DLL.

## Pre-built DLL

The `KlakSpout.dll` file in the parent directory is a pre-built version from the KlakSpout repository.

## Building from Source

To build from source, you need:

1. **Mingw-w64** toolchain (x86_64-w64-mingw32-g++-posix)
   - Or use Visual Studio with C++ support

2. **Build using Makefile** (if Mingw is available):
   ```bash
   cd KlakSpout
   make
   ```

3. **Or create a Visual Studio project**:
   - Create a new C++ DLL project
   - Add all .cpp files from Plugin/ and Spout/ directories
   - Add all .h files as includes
   - Link against: dxgi.lib, d3d12.lib, d3d11.lib, ole32.lib
   - Set to x64 platform
   - Build as DLL

## Installation

The built `KlakSpout.dll` should be copied to:
- `Renderer\Renderite.Renderer_Data\Plugins\x86_64\KlakSpout.dll`

This is handled automatically by the build process or can be done manually.

## Source

Original repository: https://github.com/keijiro/KlakSpout


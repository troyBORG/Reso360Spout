# Reso360Spout
ResoniteでVR180動画や、360度動画を撮影できるMODです。  
[このような動画](https://deovr.com/voqxc9)を撮影できます。

## インストール
1. Resonite Mod Loaderをインストールしてください。
1. [最新のリリース](https://github.com/rassi0429/Reso360Spout/releases/latest)をダウンロードし、``cubeto360``,``KlakSpout.dll``,``Reso360Spout.dll``をすべて``rml_mods``フォルダに配置してください。
1. OBSに[Spoutプラグイン](https://github.com/Off-World-Live/obs-spout2-plugin)をインストールしてください。

## OBSのセットアップ
1. キャンバス解像度とスケーリング解像度を6144x3072にします。
1. シーンにSpoutソースを配置します。

セットアップ済みのシーンコレクション・プロファイルが欲しい場合は、[こちらからダウンロード](https://drive.google.com/drive/folders/1ZkWt8Ff8cR0690dlRtUjBLjwp9gejyir?usp=drive_link)できます。

## Resoniteのセットアップ
1. Resoniteを起動すると、自動的にSpoutを介してOBSに映像が表示されます。
1. Resonite Mod Settingsでは、カメラが追従するスロットの名前を設定し、投影モードを変更し、カメラ設定を調整できます。
1. 撮影をお楽しみください！

## License

[zlib license](http://zlib.net/zlib_license.html)

* CubemapToOtherProjection.shader: "CubemapToEquirectangular" by [Bartosz](https://stackoverflow.com/users/1531778/bartosz) is licensed under CC BY-SA 3.0. ([Converting a Cubemap into Equirectangular Panorama](https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama))


---

# Reso360Spout

A mod for Resonite that allows you to record VR180 and 360° videos.  
You can capture videos like [this one](https://deovr.com/voqxc9).

## Installation

**Important**: This mod requires the splittening update and BepInEx.Renderer to work.

1. Install the Resonite Mod Loader.
2. Install **BepInEx.Renderer** from Thunderstore:
   - [BepisLoader](https://thunderstore.io/c/resonite/p/ResoniteModding/BepisLoader/)
   - [BepInEx.Renderer](https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExRenderer/)
3. Build the mod (see BUILD_INSTRUCTIONS.md) or download the [latest release](https://github.com/rassi0429/Reso360Spout/releases/latest)
4. Copy files to the following locations:
   - `Reso360Spout.dll` → `rml_mods\` (main process) - **built by project, auto-copied**
   - `Reso360Spout.dll` → `Renderer\Renderite.Renderer_Data\Managed\` (renderer process) - **built by project, auto-copied**
   - `Reso360SpoutBootstrap.dll` → `Renderer\BepInEx\plugins\` (BepInEx plugin) - **built by project, auto-copied**
   - `KlakSpout.dll` → `Renderer\Renderite.Renderer_Data\Plugins\x86_64\` - **included in repo, auto-copied during build**
   - `cubeto360` → `rml_mods\` - **external dependency, copy from original release**
5. Install the [Spout plugin for OBS](https://github.com/Off-World-Live/obs-spout2-plugin).

For detailed installation instructions, see [INSTALLATION.md](Reso360Spout/INSTALLATION.md).

## OBS Setup

1. Set both the Canvas Resolution and the Output (Scaling) Resolution to **6144x3072**.
2. Add a Spout source to your scene.

If you would like a pre-configured Scene Collection and Profile for OBS, you can download them [here](https://drive.google.com/drive/folders/1ZkWt8Ff8cR0690dlRtUjBLjwp9gejyir?usp=drive_link).

## Resonite Setup

1. Launch Resonite. Your video feed will automatically be sent to OBS via Spout.
2. In Resonite Mod Settings, you can set the name of the slot the camera will follow, change the projection mode, and adjust camera settings.
3. Have fun recording!

## License

[zlib license](http://zlib.net/zlib_license.html)

- **CubemapToOtherProjection.shader**: "CubemapToEquirectangular" by [Bartosz](https://stackoverflow.com/users/1531778/bartosz) is licensed under CC BY-SA 3.0.  
  ([Converting a Cubemap into Equirectangular Panorama](https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama))

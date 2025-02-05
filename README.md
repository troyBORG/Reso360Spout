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
1. カメラが追従するSlotの名前を自由に設定できるようになりました。これにより、複数のユーザーが同時にMODを利用できます。
1. Resonite Mod Settingsのメニューから投影モードやカメラの設定を変更できます。
1. 撮影をお楽しみください！

## License

[zlib license](http://zlib.net/zlib_license.html)

* CubemapToOtherProjection.shader: "CubemapToEquirectangular" by [Bartosz](https://stackoverflow.com/users/1531778/bartosz) is licensed under CC BY-SA 3.0. ([Converting a Cubemap into Equirectangular Panorama](https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama))


---

# Reso360Spout

A mod for Resonite that allows you to record VR180 and 360° videos.  
You can capture videos like [this one](https://deovr.com/voqxc9).

## Installation

1. Install the Resonite Mod Loader.
2. Download the [latest release](https://github.com/rassi0429/Reso360Spout/releases/latest) and place all of the following files into your `rml_mods` folder:
   - `cubeto360`
   - `KlakSpout.dll`
   - `Reso360Spout.dll`
3. Install the [Spout plugin for OBS](https://github.com/Off-World-Live/obs-spout2-plugin).

## OBS Setup

1. Set both the Canvas Resolution and the Output (Scaling) Resolution to **6144x3072**.
2. Add a Spout source to your scene.

If you would like a pre-configured Scene Collection and Profile for OBS, you can download them [here](https://drive.google.com/drive/folders/1ZkWt8Ff8cR0690dlRtUjBLjwp9gejyir?usp=drive_link).

## Resonite Setup

1. Launch Resonite. Your video feed will automatically be sent to OBS via Spout.
2. You can now choose a custom name for the Slot where the camera follows. This allows multiple users to use the mod at the same time.
3. You can change the projection mode and camera settings in the Resonite Mod Settings menu.
4. Have fun recording!

## License

[zlib license](http://zlib.net/zlib_license.html)

- **CubemapToOtherProjection.shader**: "CubemapToEquirectangular" by [Bartosz](https://stackoverflow.com/users/1531778/bartosz) is licensed under CC BY-SA 3.0.  
  ([Converting a Cubemap into Equirectangular Panorama](https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama))

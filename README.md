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
1. ``#Camera``という名前のSlotを作成すると、そのSlotにカメラが追従します。
1. Resonite Mod Settingsのメニューから投影モードやカメラの設定を変更できます。
1. 撮影をお楽しみください！

## License

[zlib license](http://zlib.net/zlib_license.html)

* CubemapToOtherProjection.shader: "CubemapToEquirectangular" by [Bartosz](https://stackoverflow.com/users/1531778/bartosz) is licensed under CC BY-SA 3.0. ([Converting a Cubemap into Equirectangular Panorama](https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama))

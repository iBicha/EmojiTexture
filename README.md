# EmojiTexture
A Unity plugin to render Emojis ☺ ❤ 🍆 🍑 to a texture. Currently for iOS and Android only.

Please note that the editor is not supported. It will only render on device (should work on simulator as well)

## Preview
<img src="https://raw.github.com/iBicha/EmojiTexture/master/preview.gif">


## Usage
As simple as:
```csharp
material.mainTexture = new EmojiTexture("❤");
```
You can also do these things:
```csharp
//Create an EmojiTexture with a specific size (best if power of 2)
var emojiTexture = new EmojiTexture(128);

//Change an existing EmojiTexture
emojiTexture.Text = "❤"; 

//Get the texture as an array of bytes, in case you want to do something with it
var bytes = emojiTexture.ByteBuffer;

//Know the code of the emoji? Set it directly as an integer!
//E.g. https://emojipedia.org/smiling-face-with-smiling-eyes/
emojiTexture.Unicode = 0x1F60A; //😊 Smiling Face With Smiling Eyes

```
## TextMesh Pro support
[TextMesh Pro](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126) already supports emojis as sprites, but they need to be prepared beforehand, which makes it troublesome in terms of build size (and also a lot of manual work), if you want to support as many emojis as possible.
## Setup
- Import TextMesh Pro from the [Asset Store](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126)
- In the Player Settings, add `TMPRO_EMOJIS` to the [Scripting Define Symbols](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)

![Scripting Define Symbols](https://docs.unity3d.com/uploads/Main/ScriptDefines.png)
- Use the extension method `SetTextEx` to set the text of your `TextMeshPro` or `TextMeshProUGUI` component
```csharp
textMesh.SetTextEx("TextMesh Pro + EmojiTexture = ❤");
```
- That's it! `SetTextEx` will scan the text for emojis, load them into sprite sheets and make them available for TextMesh Pro.
## Optimizations
Few pointers to consider:

- `SetTextEx` will link EmojiTexture with `TextMeshPro` or `TextMeshProUGUI` component, scan the text, load emojis, and then apply the text. If you are doing this often, consider the following:
  - call `TMProEmojiAsset.HookTMP(textMesh)` on start with each text component you want to use with `EmojiTexture`
  - everytime you want to check if new emojis are being used, call `TMProEmojiAsset.Process(text)` (usually whenever the text changes if it is a user input, or when you know you are displaying an emoji for the first time). Call this before setting the text of the `TextMeshPro` or `TextMeshProUGUI` component to see the changes.
  
- Emojis are stored in sprite sheets of the size 4 by 4 (16 emojis) with an emoji texture of `128x128` pixels (which makes a `512x512` per sprite sheet.) These are constants defined in `TMProEmojiAsset.EMOJI_SIZE` and `TMProEmojiAsset.SHEET_TILES`. Optimize these values according to your use case.
- When a sprite sheet is full, a new one is created.
- Currently cleaning up unused emojis is not yet supported. This will be added in the future.

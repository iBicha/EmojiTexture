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
## Github emoji API (Experimental)
When not running Android or iOS, EmojiTexture can use [Github emoji API](https://developer.github.com/v3/emojis/).
This require network connection. While the list of emojis is cached, individual images are not.

Setting emojis from Github needs to run in a `Coroutine`, because it is an async operation.
It is possible to set the emoji text (same as above)
```
yield return emojiTexture.SetGithubEmoji("❤");
```
Or using keywords
```
yield return emojiTexture.SetGithubEmoji(":heart:");
```

Please check out example scene for usage (it includes a native emoji, a github emoji and TextMesh Pro examples, that shuffle emojis from script or read user imput with touch screen).


## TextMesh Pro support (Experimental)
[TextMesh Pro](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126) already supports emojis as sprites, but they need to be prepared beforehand, which makes it troublesome in terms of build size (and also a lot of manual work), if you want to support as many emojis as possible. This is where EmojiTexture comes in. It generates these sprites on the fly as they are needed.

## Setup
- Import TextMesh Pro from the [Asset Store](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126)
- In the Player Settings, add `TMPRO_EMOJIS` to the [Scripting Define Symbols](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)

![Scripting Define Symbols](https://docs.unity3d.com/uploads/Main/ScriptDefines.png)
- Add the component `TMP_EmojiSupport` on the same game object as your `TextMeshPro` or `TextMeshProUGUI` component.
That's about it. You should have emoji support out of the box.
- On the `TMP_EmojiSupport` component, if `Github fallback` is checked, the emojis will be downloaded from github if you are not running on Android or iOS (and if you have network)

If you want to "process" the text to display emojis manually on each change, use:
```csharp
textMesh.SetTextEx("TextMesh Pro + EmojiTexture = ❤");
```

## Optimizations
Few pointers to consider:

- `SetTextEx` will link EmojiTexture with `TextMeshPro` or `TextMeshProUGUI` component, scan the text, load emojis, and then apply the text. If you are doing this often, consider the following:
  - call `TMProEmojiAsset.HookTMP(textMesh)` on start with each text component you want to use with `EmojiTexture`
  - everytime you want to check if new emojis are being used, call `TMProEmojiAsset.Process(text)` (usually whenever the text changes if it is a user input, or when you know you are displaying an emoji for the first time). Call this before setting the text of the `TextMeshPro` or `TextMeshProUGUI` component to see the changes.
  - Of course `TMP_EmojiSupport` should take care of all this, but this is just in case you want to be more flexible
  
- Emojis are stored in sprite sheets of the size 4 by 4 (16 emojis) with an emoji texture of `128x128` pixels (which makes a `512x512` per sprite sheet.) These are constants defined in `TMProEmojiAsset.EMOJI_SIZE` and `TMProEmojiAsset.SHEET_TILES`. Optimize these values according to your use case. **BE AWARE** currently, github returns enoji images as `128x128`, and they share the same sprite sheet. Changing these constants will probably break emojis from github. (A resizing method needs to be implemented for that purpose, but that would be at the cost of performance)

- When a sprite sheet is full, a new one is created.
- Currently cleaning up unused emojis is not yet supported. This will be added in the future.

## Known issues.
- While the native emoji renderer (Android/iOS) tries to estimate the correct rendering, Github emojis and TMP both have trouble with complex emojis (such as flags, emojis with skintones, etc). These will be addressed in the future, as there is room for improvement.
- Some rendering issues are due to how TMP works, because it scans for unicode characters, and we get to feed it sprites. This needs to be fixed on TMP's side.


# EmojiTexture
A Unity plugin to render Emojis ☺ ❤ 🍆 🍑 to a texture. Currently for iOS only.

## Preview
<img src="https://raw.github.com/iBicha/EmojiTexture/master/preview.gif">


## Usage
```csharp
public class EmojiTextureExample : MonoBehaviour {

    public Material material;

    private EmojiTexture emojiTexture;

    private string[] emojis =  { "☺" ,"❤" , "🍆", "🍑" };
    
    void Start () {
        //Create a new EmojiTexture
        emojiTexture = new EmojiTexture();
        //Assign it to a material
        material.mainTexture = emojiTexture;
        //Invoke method to change emoji every second
        InvokeRepeating("ChangeEmoji", 0f, 1f);
    }

    void ChangeEmoji() {
        //Setting the .Text property of an EmojiTexture will update it's texture.
        emojiTexture.Text = emojis[Mathf.RoundToInt(Time.time) % emojis.Length];
    }

}

```
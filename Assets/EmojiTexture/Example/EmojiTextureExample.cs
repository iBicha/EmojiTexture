using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EmojiTextureExample : MonoBehaviour {

    public Material material;

    private EmojiTexture emojiTexture;

    private string[] emojis =  { "☺" ,"❤" , "🍆", "🍑" };

	void Start () {
        emojiTexture = new EmojiTexture();
        material.mainTexture = emojiTexture;
        InvokeRepeating("ChangeEmoji", 0f, 1f);
    }

    void ChangeEmoji() {
        emojiTexture.Text = emojis[Mathf.RoundToInt(Time.time) % emojis.Length];
    }

}

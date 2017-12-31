using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EmojiTexture {

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr _alloc(int size);

    [DllImport("__Internal")]
    private static extern void _free(IntPtr buffer);

    [DllImport("__Internal")]
    private static extern void _render(string text, IntPtr buffer, int width, int height);
#endif

    public string Text {
        get {
            return text;
        }
        set{
            if (value != text)
            {
                text = value;
#if UNITY_IOS && !UNITY_EDITOR
                _render(text, buffer, texture.width, texture.height);
                isbyteBufferDirty = true;
                texture.LoadRawTextureData(buffer, bufferSize);
                texture.Apply();
#endif
            }
        }
    }

    public byte[] ByteBuffer{
        get{
            if(isbyteBufferDirty || byteBuffer == null){
                if(buffer != IntPtr.Zero && bufferSize>0){
                    Marshal.Copy(buffer, byteBuffer, 0, bufferSize);
                    isbyteBufferDirty = false;
                }
            }
            return byteBuffer;
        }
    }

    private Texture2D texture;
    private string text;
    private IntPtr buffer;
    private int bufferSize;

    private byte[] byteBuffer;
    private bool isbyteBufferDirty;

    public EmojiTexture() : this(null) { }

    public EmojiTexture(string text) : this(text, 256, 256) { }

    public EmojiTexture(string text, int size) : this(text, size, size) { }

    public EmojiTexture(int size) : this(null, size, size) { }

    public EmojiTexture(int width, int height) : this(null, width, height) { }

    public EmojiTexture(string text, int width, int height){

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_IOS && !UNITY_EDITOR
        bufferSize = width * height * 4;
        buffer = _alloc(bufferSize);
#endif
        Text = text;
    }

    ~EmojiTexture()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if(buffer != IntPtr.Zero)
            _free(buffer);
#endif
    }

    static public implicit operator Texture(EmojiTexture emojiTexture)
    {
        return emojiTexture.texture;
    }

}

//
// Created by Brahim Hadriche on 2018-04-22.
//
#include <jni.h>
#include "Unity/IUnityRenderingExtensions.h"

typedef void* (*BUFFER_BY_INDEX_DELEGATE)(int index);
static BUFFER_BY_INDEX_DELEGATE s_getBufferByIndex;

extern "C" void UNITY_INTERFACE_EXPORT
EmojiTexture_SetBufferRefByIndexFunction(BUFFER_BY_INDEX_DELEGATE fn)
{
    s_getBufferByIndex = fn;
}

void TextureUpdateCallback(int eventID, void* data)
{
    auto event = static_cast<UnityRenderingExtEventType>(eventID);

    if (event == kUnityRenderingExtEventUpdateTextureBegin)
    {
        // UpdateTextureBegin: Generate and return texture image data.
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);

        if (s_getBufferByIndex == NULL)
            return;

        void* texData = s_getBufferByIndex((int)params->userData);
        params->texData = texData;
    }
}

extern "C" JNIEXPORT jlong JNICALL
Java_com_ibicha_emojitexture_EmojiTexture_GetTextureUpdateCallback(){
    return (jlong)TextureUpdateCallback;
}


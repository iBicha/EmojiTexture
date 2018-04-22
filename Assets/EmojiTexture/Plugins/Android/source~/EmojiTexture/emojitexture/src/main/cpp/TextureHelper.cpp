//
// Created by Brahim Hadriche on 2018-04-22.
//
#include <cstdint>
#include <cmath>

#include <jni.h>
#include "Unity/IUnityRenderingExtensions.h"

void TextureUpdateCallback(int eventID, void* data);

extern "C" JNIEXPORT jlong JNICALL
Java_com_ibicha_emojitexture_EmojiTexture_GetTextureUpdateCallback ();

uint32_t Plasma(uint32_t x, uint32_t y, uint32_t width, uint32_t height, uint32_t frame)
{
    auto px = (float)x / width;
    auto py = (float)y / height;
    auto time = frame / 60.0f;

    auto l = sinf(px * sinf(time * 1.3f) + sinf(py * 4 + time) * sinf(time));

    auto r = (uint32_t)(sinf(l *  6) * 127 + 127);
    auto g = (uint32_t)(sinf(l *  7) * 127 + 127);
    auto b = (uint32_t)(sinf(l * 10) * 127 + 127);

    return r + (g << 8) + (b << 16) + 0xff000000;
}

void TextureUpdateCallback(int eventID, void* data)
{
    auto event = static_cast<UnityRenderingExtEventType>(eventID);

    if (event == kUnityRenderingExtEventUpdateTextureBegin)
    {
        // UpdateTextureBegin: Generate and return texture image data.
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
        params->texData = reinterpret_cast<void*>(params->userData);
    }
    else if (event == kUnityRenderingExtEventUpdateTextureEnd)
    {
        // UpdateTextureEnd: Free up the temporary memory.
        //auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
        //delete[] reinterpret_cast<uint32_t*>(params->texData);
    }
}

jlong Java_com_ibicha_emojitexture_EmojiTexture_GetTextureUpdateCallback(){
    return (jlong)TextureUpdateCallback;
}
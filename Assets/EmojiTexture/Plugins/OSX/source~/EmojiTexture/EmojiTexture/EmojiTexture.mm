//
//  EmojiTexture.mm
//  EmojiTexture
//
//  Created by Brahim Hadriche on 2018-09-11.
//  Copyright © 2018 Brahim Hadriche. All rights reserved.
//

#import <CoreText/CoreText.h>
#import <Cocoa/Cocoa.h>
#import <CoreGraphics/CoreGraphics.h>
#import <QuartzCore/QuartzCore.h>

#include "IUnityRenderingExtensions.h"

CATextLayer * getTextLayer()
{
    static CATextLayer * caTextLayer = nil;
    if (caTextLayer == nil)
    {
        caTextLayer = [CATextLayer layer];
        caTextLayer.frame = CGRectMake(0, 0, 256, 256);

        caTextLayer.font = CFBridgingRetain([NSFont fontWithName:@"Apple Color Emoji" size:220].fontName);
        caTextLayer.fontSize = 220;
        caTextLayer.foregroundColor = [NSColor blackColor].CGColor;
        caTextLayer.backgroundColor = [NSColor clearColor].CGColor;
        caTextLayer.alignmentMode = kCAAlignmentCenter;
        caTextLayer.transform = CATransform3DScale(CATransform3DMakeRotation(M_PI / 2.0f, 0, 0, 1), -1, 1, 1);
    }
    return caTextLayer;
}

NSInteger GetGlyphCount(NSMutableAttributedString *attributedString)
{
    CTLineRef line = CTLineCreateWithAttributedString((CFAttributedStringRef)attributedString);
    NSInteger count = CTLineGetGlyphCount(line);
    CFRelease(line);
    return count;
}

extern "C" {
    int EmojiTexture_render(const char* text, unsigned char * buffer , int width, int height, int sanitize)
    {
        int textLength = 0;
        NSUInteger bytesPerPixel = 4;
        NSUInteger bytesPerRow = bytesPerPixel * width;
        NSUInteger bitsPerComponent = 8;
        CGColorSpaceRef colorSpace = CGColorSpaceCreateDeviceRGB();
        CGContextRef context = CGBitmapContextCreate(buffer, width, height,
                                                     bitsPerComponent, bytesPerRow, colorSpace,
                                                     kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);
        CGContextClearRect(context, CGRectMake(0, 0, width, height));
        if(text){
            CATextLayer * textLayer = getTextLayer();
            [textLayer setFrame:CGRectMake(0,0,width,height)];
            
            NSMutableAttributedString *attributedString =
            [[NSMutableAttributedString alloc] initWithString:[NSString stringWithUTF8String: text]];
            
            if(sanitize){
                while (GetGlyphCount(attributedString) > 1) {
                    [attributedString deleteCharactersInRange:NSMakeRange([attributedString length]-1, 1)];
                }
            }
            
            textLength = (int)[attributedString length];
            textLayer.string = [attributedString string];

            CGAffineTransform flipVertical = CGAffineTransformMake(1, 0, 0, -1, 0, height);
            CGContextConcatCTM(context, flipVertical);

            [textLayer renderInContext:context];
        }
        CGColorSpaceRelease(colorSpace);
        CGContextRelease(context);
        return textLength;
    }
}

typedef void* (*BUFFER_BY_INDEX_DELEGATE)(int index);
static BUFFER_BY_INDEX_DELEGATE s_getBufferByIndex;

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

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT
EmojiTexture_GetTextureUpdateCallback()
{
    return TextureUpdateCallback;
}

extern "C" void UNITY_INTERFACE_EXPORT
EmojiTexture_SetBufferRefByIndexFunction(BUFFER_BY_INDEX_DELEGATE fn)
{
    s_getBufferByIndex = fn;
}

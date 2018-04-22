#import <Foundation/Foundation.h>
#import <CoreText/CoreText.h>
#include "IUnityRenderingExtensions.h"

UILabel * getUILabel()
{
    static UILabel * label = nil;
    if (label == nil)
    {
        label = [[UILabel alloc]initWithFrame:CGRectMake(0, 0, 256, 256)];
        [label setOpaque:NO];
        label.font = [UIFont systemFontOfSize:999];
        label.textAlignment = NSTextAlignmentCenter;
        label.baselineAdjustment = UIBaselineAdjustmentAlignCenters;
        label.lineBreakMode = NSLineBreakByClipping;
        label.minimumScaleFactor = 0.0001;
        label.adjustsFontSizeToFitWidth = YES;
    }
    return label;
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
            UILabel * label = getUILabel();
            [label setFrame:CGRectMake(0,0,width,height)];
            
            NSMutableAttributedString *attributedString =
            [[NSMutableAttributedString alloc] initWithString:[NSString stringWithUTF8String: text]];
            
            if(sanitize){
                while (GetGlyphCount(attributedString) > 1) {
                    [attributedString deleteCharactersInRange:NSMakeRange([attributedString length]-1, 1)];
                }
            }
            
            textLength = (int)[attributedString length];
            label.attributedText = attributedString;
            
            [label.layer renderInContext:context];
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

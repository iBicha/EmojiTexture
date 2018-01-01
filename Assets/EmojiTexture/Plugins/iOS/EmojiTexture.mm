#import <Foundation/Foundation.h>

extern "C" {
    
    unsigned char * EmojiTexture_alloc(int size)
    {
        return (unsigned char*) calloc(size, sizeof(unsigned char));
    }
    
    void EmojiTexture_free(unsigned char * buffer)
    {
        free(buffer);
    }
    
    void EmojiTexture_render(const char* text, unsigned char * buffer , int width, int height)
    {
        NSUInteger bytesPerPixel = 4;
        NSUInteger bytesPerRow = bytesPerPixel * width;
        NSUInteger bitsPerComponent = 8;
        CGColorSpaceRef colorSpace = CGColorSpaceCreateDeviceRGB();
        CGContextRef context = CGBitmapContextCreate(buffer, width, height,
                                                     bitsPerComponent, bytesPerRow, colorSpace,
                                                     kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);
        CGContextClearRect(context, CGRectMake(0, 0, width, height));
        if(text){
            UILabel *label = [[UILabel alloc]initWithFrame:CGRectMake(0, 0, width, height)];
            
            [label setOpaque:NO];
            label.font = [UIFont systemFontOfSize:999];
            label.textAlignment = NSTextAlignmentCenter;
            label.baselineAdjustment = UIBaselineAdjustmentAlignCenters;
            label.lineBreakMode = NSLineBreakByClipping;
            label.minimumScaleFactor = 0.0001;
            label.adjustsFontSizeToFitWidth = YES;

            label.text = [NSString stringWithUTF8String: text];
            [label.layer renderInContext:context];
        }
        CGColorSpaceRelease(colorSpace);
        CGContextRelease(context);
    }
}


#import <Foundation/Foundation.h>

UILabel * getUILabel() {
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

extern "C" {
        
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
            UILabel * label = getUILabel();
            [label setFrame:CGRectMake(0,0,width,height)];
            label.text = [NSString stringWithUTF8String: text];
            [label.layer renderInContext:context];
        }
        CGColorSpaceRelease(colorSpace);
        CGContextRelease(context);
    }
}


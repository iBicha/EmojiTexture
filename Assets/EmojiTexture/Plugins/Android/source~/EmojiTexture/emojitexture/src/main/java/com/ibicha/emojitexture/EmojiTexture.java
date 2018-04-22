package com.ibicha.emojitexture;

import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.text.Layout;
import android.text.StaticLayout;
import android.text.TextPaint;
import android.util.DisplayMetrics;
import android.util.Log;
import android.util.TypedValue;

import java.nio.ByteBuffer;

/**
 * Created by ibicha on 2018-01-01.
 */

public class EmojiTexture {

    private static final int DEFAULT_MIN_TEXT_SIZE = 8;
    private static final int DEFAULT_MAX_TEXT_SIZE = 999;
    private static final float DEFAULT_PRECISION = 0.5f;


    static {
        Log.d("EmojiTexture", "static initializer: emojiTextureHelper");
        System.loadLibrary("emojiTextureHelper");
    }

    static long jGetTextureUpdateCallback(){
        Log.d("EmojiTexture", "jGetTextureUpdateCallback: " + GetTextureUpdateCallback());
        return GetTextureUpdateCallback();
    }

    public static native long GetTextureUpdateCallback();

    static int render(String text, ByteBuffer byteBuffer, int width, int height, boolean sanitize){
        TextPaint textPaint = new TextPaint(Paint.ANTI_ALIAS_FLAG);
        textPaint.setTextAlign(Paint.Align.CENTER);

        if(text == null)
            text = "";

        if(sanitize && text.length() > 0){
            Rect bounds = new Rect();
            for (int i = text.length(); i > 1 ; i--) {
                textPaint.getTextBounds(text,0, i, bounds);
                if((float)bounds.width()/(float)bounds.height() < 1.2){
                    text = text.substring(0, i);
                    break;
                }
            }
        }

        Resources r = Resources.getSystem();
        DisplayMetrics displayMetrics = r.getDisplayMetrics();

        float fontSize = getAutofitTextSize(text, textPaint,
                width, 1, DEFAULT_MIN_TEXT_SIZE, DEFAULT_MAX_TEXT_SIZE, DEFAULT_PRECISION,
                displayMetrics);

        textPaint.setTextSize(fontSize);

        Bitmap bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);

        Rect textBounds = new Rect();
        textPaint.getTextBounds(text, 0, text.length(), textBounds);

        canvas.scale(1f, -1f, width/2, height/2);
        canvas.drawText(text, width/2, height/2 - (textBounds.bottom + textBounds.top) / 2.0f, textPaint);

        //bitmap to buffer
        byteBuffer.rewind();
        bitmap.copyPixelsToBuffer(byteBuffer);

        return text.length();
    }

    //https://github.com/grantland/android-autofittextview/blob/master/library/src/main/java/me/grantland/widget/AutofitHelper.java#L141
    private static float getAutofitTextSize(CharSequence text, TextPaint paint,
                                            float targetWidth, int maxLines, float low, float high, float precision,
                                            DisplayMetrics displayMetrics) {

        float mid = (low + high) / 2.0f;
        int lineCount = 1;
        StaticLayout layout = null;

        paint.setTextSize(TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_PX, mid,
                displayMetrics));

        if (maxLines != 1) {
            layout = new StaticLayout(text, paint, (int)targetWidth, Layout.Alignment.ALIGN_CENTER,
                    1.0f, 0.0f, true);
            lineCount = layout.getLineCount();
        }


        if (lineCount > maxLines) {
            // For the case that `text` has more newline characters than `maxLines`.
            if ((high - low) < precision) {
                return low;
            }
            return getAutofitTextSize(text, paint, targetWidth, maxLines, low, mid, precision,
                    displayMetrics);
        }
        else if (lineCount < maxLines) {
            return getAutofitTextSize(text, paint, targetWidth, maxLines, mid, high, precision,
                    displayMetrics);
        }
        else {
            float maxLineWidth = 0;
            if (maxLines == 1) {
                maxLineWidth = paint.measureText(text, 0, text.length());
            } else {
                for (int i = 0; i < lineCount; i++) {
                    if (layout.getLineWidth(i) > maxLineWidth) {
                        maxLineWidth = layout.getLineWidth(i);
                    }
                }
            }

            if ((high - low) < precision) {
                return low;
            } else if (maxLineWidth > targetWidth) {
                return getAutofitTextSize(text, paint, targetWidth, maxLines, low, mid, precision,
                        displayMetrics);
            } else if (maxLineWidth < targetWidth) {
                return getAutofitTextSize(text, paint, targetWidth, maxLines, mid, high, precision,
                        displayMetrics);
            } else {
                return mid;
            }
        }
    }

}

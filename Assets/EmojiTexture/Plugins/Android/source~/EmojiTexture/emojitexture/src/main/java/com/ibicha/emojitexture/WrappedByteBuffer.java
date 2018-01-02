package com.ibicha.emojitexture;

import java.nio.ByteBuffer;

/**
 * Created by ibicha on 2018-01-02.
 */

public class WrappedByteBuffer {
    public ByteBuffer buffer;
    public WrappedByteBuffer(int capacity){
        buffer = ByteBuffer.allocateDirect(capacity);
    }
}

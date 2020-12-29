//
//  FreeTypeRenderer.cpp
//  EmojiTexture
//
//  Created by Brahim Hadriche on 2018-10-15.
//  Copyright © 2018 Brahim Hadriche. All rights reserved.
//

#include "FreeTypeRenderer.h"

void Render(){
    FT_Library library;
    FT_Face face;
    
    FT_Init_FreeType(&library);
    FT_New_Face(library, "/System/Library/Fonts/Apple Color Emoji.ttc", 0, &face);

    bool has_color = FT_HAS_COLOR(face);
    
    //debug(LOG_INFO, 0, "font has colors: %s", has_color ? "yes" : "no");
    printf("Has color: %d", has_color);
    
    std::string s = U"😀 😬 😁 😂 😃 😄 😅 😆";
    
    FT_GlyphSlot slot = face->glyph;
    for (auto c : s)
    {
        int glyph_index = FT_Get_Char_Index(face, c);
        
        FT_Error error = FT_Load_Glyph(face, glyph_index, FT_LOAD_DEFAULT|FT_LOAD_COLOR);
        if (error)
            continue;
        
        error = FT_Render_Glyph(slot, FT_RENDER_MODE_NORMAL);
        if (error)
            continue;
        
        if (slot->bitmap.pixel_mode == FT_PIXEL_MODE_BGRA)
            printf("glyph is colored");

    }

}

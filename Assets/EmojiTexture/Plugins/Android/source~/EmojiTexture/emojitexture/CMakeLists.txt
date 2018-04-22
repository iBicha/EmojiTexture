cmake_minimum_required(VERSION 3.4.1)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

add_library( emojiTextureHelper

             SHARED

             src/main/cpp/TextureHelper.cpp )

include_directories(src/main/cpp/)

target_link_libraries(emojiTextureHelper
                      android
                      log)

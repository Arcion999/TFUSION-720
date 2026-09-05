cmake_minimum_required(VERSION 3.28)

project(TFusionKernelNative VERSION 0.2.0 LANGUAGES C CXX)

if(NOT WIN32 OR NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "TFusion.Kernel.Native supports Windows x64 only.")
endif()

if(NOT MSVC)
    message(FATAL_ERROR "TFusion.Kernel.Native requires the supported Microsoft MSVC x64 toolchain.")
endif()

set(CMAKE_C_STANDARD 17)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

option(TFUSION_ENABLE_ASAN "Enable MSVC AddressSanitizer for project-owned native code" OFF)

find_package(OpenCASCADE CONFIG REQUIRED)

add_library(TFusionKernelNative SHARED
    include/tfusion_kernel.h
    src/abi.cpp
    src/context.cpp
    src/context.hpp
    src/diagnostic.cpp
    src/diagnostic.hpp
    src/handle_registry.cpp
    src/handle_registry.hpp
    src/utf8.cpp
    src/utf8.hpp)

target_compile_definitions(TFusionKernelNative PRIVATE TFUSION_KERNEL_NATIVE_EXPORTS)
target_include_directories(TFusionKernelNative
    PUBLIC "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>"
    PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/src")
target_link_libraries(TFusionKernelNative PRIVATE TKernel)
set_target_properties(TFusionKernelNative PROPERTIES
    OUTPUT_NAME "TFusion.Kernel.Native"
    WINDOWS_EXPORT_ALL_SYMBOLS OFF)

if(MSVC)
    target_compile_options(TFusionKernelNative PRIVATE /W4 /WX /permissive- /sdl /utf-8 /EHsc)
    target_link_options(TFusionKernelNative PRIVATE /guard:cf)
    if(TFUSION_ENABLE_ASAN)
        target_compile_options(TFusionKernelNative PRIVATE /fsanitize=address /Zi)
        target_link_options(TFusionKernelNative PRIVATE /INCREMENTAL:NO)
    endif()
endif()

add_custom_command(TARGET TFusionKernelNative POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "$<TARGET_FILE:TKernel>"
        "$<TARGET_FILE_DIR:TFusionKernelNative>"
    VERBATIM)

enable_testing()

add_executable(TFusionKernelNativeTests
    tests/native_tests.cpp)
target_link_libraries(TFusionKernelNativeTests PRIVATE TFusionKernelNative)
target_compile_definitions(TFusionKernelNativeTests PRIVATE TFUSION_NATIVE_TESTING=1)

add_executable(TFusionKernelCAbiSmoke
    tests/c_abi_smoke.c)
target_link_libraries(TFusionKernelCAbiSmoke PRIVATE TFusionKernelNative)

foreach(test_target IN ITEMS TFusionKernelNativeTests TFusionKernelCAbiSmoke)
    if(MSVC)
        target_compile_options(${test_target} PRIVATE /W4 /WX /sdl /utf-8)
        target_link_options(${test_target} PRIVATE /guard:cf)
        if(TFUSION_ENABLE_ASAN)
            target_compile_options(${test_target} PRIVATE /fsanitize=address /Zi)
            target_link_options(${test_target} PRIVATE /INCREMENTAL:NO)
        endif()
    endif()
endforeach()

add_test(NAME native-contract COMMAND TFusionKernelNativeTests)
add_test(NAME c-abi-smoke COMMAND TFusionKernelCAbiSmoke)

install(TARGETS TFusionKernelNative
    RUNTIME_DEPENDENCIES
        DIRECTORIES "$<TARGET_FILE_DIR:TKernel>"
        PRE_EXCLUDE_REGEXES "api-ms-.*" "ext-ms-.*"
        POST_EXCLUDE_REGEXES ".*[Ww][Ii][Nn][Dd][Oo][Ww][Ss][/\\\\][Ss]ystem32[/\\\\].*"
    RUNTIME DESTINATION .)

if(DEFINED VCPKG_INSTALLED_DIR AND DEFINED VCPKG_TARGET_TRIPLET)
    install(FILES
        "${VCPKG_INSTALLED_DIR}/${VCPKG_TARGET_TRIPLET}/share/opencascade/copyright"
        DESTINATION THIRD_PARTY-LICENSES
        RENAME OCCT.txt)
endif()

#pragma once

#include "tfusion_kernel.h"

#include <cstdint>
#include <string>
#include <string_view>

namespace tfusion
{
constexpr std::uint32_t maximum_text_bytes = 4096U;

[[nodiscard]] bool is_valid_utf8(const std::uint8_t* value, std::uint32_t byteCount) noexcept;
[[nodiscard]] TFusionStatus copy_utf8(std::string_view value, char* buffer, std::uint32_t bufferByteCount, std::uint32_t* requiredByteCount) noexcept;
}

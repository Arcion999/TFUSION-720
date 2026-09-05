#include "utf8.hpp"

#include <cstring>
#include <limits>

namespace tfusion
{
namespace
{
bool is_continuation(const std::uint8_t value) noexcept
{
    return (value & 0xC0U) == 0x80U;
}
}

bool is_valid_utf8(const std::uint8_t* value, const std::uint32_t byteCount) noexcept
{
    if (byteCount == 0U)
    {
        return true;
    }

    if (value == nullptr || byteCount > maximum_text_bytes)
    {
        return false;
    }

    std::uint32_t index = 0U;
    while (index < byteCount)
    {
        const auto first = value[index];
        if (first <= 0x7FU)
        {
            ++index;
            continue;
        }

        if (first >= 0xC2U && first <= 0xDFU)
        {
            if (index + 1U >= byteCount || !is_continuation(value[index + 1U]))
            {
                return false;
            }
            index += 2U;
            continue;
        }

        if (first >= 0xE0U && first <= 0xEFU)
        {
            if (index + 2U >= byteCount
                || !is_continuation(value[index + 1U])
                || !is_continuation(value[index + 2U]))
            {
                return false;
            }

            const auto second = value[index + 1U];
            if ((first == 0xE0U && second < 0xA0U) || (first == 0xEDU && second >= 0xA0U))
            {
                return false;
            }
            index += 3U;
            continue;
        }

        if (first >= 0xF0U && first <= 0xF4U)
        {
            if (index + 3U >= byteCount
                || !is_continuation(value[index + 1U])
                || !is_continuation(value[index + 2U])
                || !is_continuation(value[index + 3U]))
            {
                return false;
            }

            const auto second = value[index + 1U];
            if ((first == 0xF0U && second < 0x90U) || (first == 0xF4U && second > 0x8FU))
            {
                return false;
            }
            index += 4U;
            continue;
        }

        return false;
    }

    return true;
}

TFusionStatus copy_utf8(
    const std::string_view value,
    char* const buffer,
    const std::uint32_t bufferByteCount,
    std::uint32_t* const requiredByteCount) noexcept
{
    if (requiredByteCount == nullptr
        || value.size() >= static_cast<std::size_t>((std::numeric_limits<std::uint32_t>::max)()))
    {
        return TFUSION_STATUS_INVALID_ARGUMENT;
    }

    const auto required = static_cast<std::uint32_t>(value.size() + 1U);
    *requiredByteCount = required;

    if (buffer == nullptr || bufferByteCount < required)
    {
        if (buffer != nullptr && bufferByteCount > 0U)
        {
            buffer[0] = '\0';
        }
        return TFUSION_STATUS_BUFFER_TOO_SMALL;
    }

    if (!value.empty())
    {
        std::memcpy(buffer, value.data(), value.size());
    }
    buffer[value.size()] = '\0';
    return TFUSION_STATUS_SUCCESS;
}
}

#include "diagnostic.hpp"

#include "handle_registry.hpp"

#include <cstdio>
#include <memory>
#include <sstream>

namespace tfusion
{
namespace
{
thread_local std::string threadDiagnosticJson =
    R"({"status":"SUCCESS","code":"TFN-KRN-0000","severity":"information","userMessage":"No native failure has been recorded.","technicalMessage":"","operation":"","nativeDetail":""})";

const char* status_name(const TFusionStatus status) noexcept
{
    switch (status)
    {
    case TFUSION_STATUS_SUCCESS: return "SUCCESS";
    case TFUSION_STATUS_INVALID_ARGUMENT: return "INVALID_ARGUMENT";
    case TFUSION_STATUS_INVALID_HANDLE: return "INVALID_HANDLE";
    case TFUSION_STATUS_STALE_HANDLE: return "STALE_HANDLE";
    case TFUSION_STATUS_TYPE_MISMATCH: return "TYPE_MISMATCH";
    case TFUSION_STATUS_CONTEXT_MISMATCH: return "CONTEXT_MISMATCH";
    case TFUSION_STATUS_BUFFER_TOO_SMALL: return "BUFFER_TOO_SMALL";
    case TFUSION_STATUS_VERSION_MISMATCH: return "VERSION_MISMATCH";
    case TFUSION_STATUS_INTERNAL_ERROR: return "INTERNAL_ERROR";
    case TFUSION_STATUS_KERNEL_ERROR: return "KERNEL_ERROR";
    case TFUSION_STATUS_UNSUPPORTED: return "UNSUPPORTED";
    default: return "UNKNOWN_STATUS";
    }
}

std::string stable_code(const TFusionStatus status)
{
    char buffer[24]{};
    const int written = std::snprintf(buffer, sizeof(buffer), "TFN-KRN-%04u", static_cast<unsigned>(status));
    return written > 0 ? std::string(buffer, static_cast<std::size_t>(written)) : "TFN-KRN-9999";
}

std::string escape_json(const std::string_view value)
{
    std::ostringstream output;
    for (const unsigned char character : value)
    {
        switch (character)
        {
        case '"': output << "\\\""; break;
        case '\\': output << "\\\\"; break;
        case '\b': output << "\\b"; break;
        case '\f': output << "\\f"; break;
        case '\n': output << "\\n"; break;
        case '\r': output << "\\r"; break;
        case '\t': output << "\\t"; break;
        default:
            if (character < 0x20U)
            {
                char encoded[7]{};
                static_cast<void>(std::snprintf(encoded, sizeof(encoded), "\\u%04x", character));
                output << encoded;
            }
            else
            {
                output << static_cast<char>(character);
            }
            break;
        }
    }
    return output.str();
}

std::string make_json(
    const TFusionStatus status,
    const std::string_view operation,
    const std::string_view userMessage,
    const std::string_view technicalMessage,
    const std::string_view nativeDetail)
{
    std::ostringstream output;
    output << "{\"status\":\"" << status_name(status)
        << "\",\"code\":\"" << stable_code(status)
        << "\",\"severity\":\"error\""
        << ",\"userMessage\":\"" << escape_json(userMessage)
        << "\",\"technicalMessage\":\"" << escape_json(technicalMessage)
        << "\",\"operation\":\"" << escape_json(operation)
        << "\",\"nativeDetail\":\"" << escape_json(nativeDetail)
        << "\"}";
    return output.str();
}
}

TFusionStatus fail(
    const TFusionHandle contextHandle,
    const TFusionStatus status,
    const std::string_view operation,
    const std::string_view userMessage,
    const std::string_view technicalMessage,
    const std::string_view nativeDetail) noexcept
{
    try
    {
        auto value = make_json(status, operation, userMessage, technicalMessage, nativeDetail);
        threadDiagnosticJson = value;

        std::shared_ptr<KernelContext> context;
        if (contextHandle != 0U
            && HandleRegistry::instance().get_context(contextHandle, context) == TFUSION_STATUS_SUCCESS)
        {
            context->set_diagnostic(std::move(value));
        }
    }
    catch (...)
    {
        // Error reporting itself must never violate the exception boundary.
    }
    return status;
}

std::string diagnostic_json(const TFusionHandle contextHandle)
{
    if (contextHandle != 0U)
    {
        std::shared_ptr<KernelContext> context;
        if (HandleRegistry::instance().get_context(contextHandle, context) == TFUSION_STATUS_SUCCESS)
        {
            const auto value = context->diagnostic_json();
            if (!value.empty())
            {
                return value;
            }
        }
    }
    return threadDiagnosticJson;
}
}

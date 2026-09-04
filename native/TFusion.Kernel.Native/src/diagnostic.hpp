#pragma once

#include "tfusion_kernel.h"

#include <Standard_Failure.hxx>

#include <exception>
#include <string>
#include <string_view>
#include <utility>

namespace tfusion
{
[[nodiscard]] TFusionStatus fail(
    TFusionHandle contextHandle,
    TFusionStatus status,
    std::string_view operation,
    std::string_view userMessage,
    std::string_view technicalMessage,
    std::string_view nativeDetail = {}) noexcept;

[[nodiscard]] std::string diagnostic_json(TFusionHandle contextHandle);

template<typename Function>
TFusionStatus protect(
    const TFusionHandle contextHandle,
    const std::string_view operation,
    Function&& function) noexcept
{
    try
    {
        return std::forward<Function>(function)();
    }
    catch (const Standard_Failure& exception)
    {
        const char* const detail = exception.GetMessageString();
        return fail(
            contextHandle,
            TFUSION_STATUS_KERNEL_ERROR,
            operation,
            "The CAD kernel rejected the operation.",
            "An Open CASCADE exception was contained at the native ABI boundary.",
            detail == nullptr ? "Open CASCADE supplied no exception detail." : detail);
    }
    catch (const std::exception& exception)
    {
        return fail(
            contextHandle,
            TFUSION_STATUS_INTERNAL_ERROR,
            operation,
            "The native kernel bridge could not complete the operation.",
            "A standard C++ exception was contained at the native ABI boundary.",
            exception.what());
    }
    catch (...)
    {
        return fail(
            contextHandle,
            TFUSION_STATUS_INTERNAL_ERROR,
            operation,
            "The native kernel bridge could not complete the operation.",
            "An unknown C++ exception was contained at the native ABI boundary.",
            "No exception detail is available.");
    }
}
}

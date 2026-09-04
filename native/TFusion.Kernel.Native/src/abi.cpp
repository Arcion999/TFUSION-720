#include "tfusion_kernel.h"

#include "context.hpp"
#include "diagnostic.hpp"
#include "handle_registry.hpp"
#include "utf8.hpp"

#include <Standard_Failure.hxx>

#include <cstddef>
#include <cstdint>
#include <memory>
#include <stdexcept>
#include <string>

namespace
{
using tfusion::HandleRegistry;
using tfusion::KernelContext;
using tfusion::RuntimeProbe;

static_assert(sizeof(TFusionContextCreateInfo) == 32U);
static_assert(sizeof(TFusionKernelInfo) == 24U);
static_assert(sizeof(TFusionAllocationSnapshot) == 40U);

TFusionStatus validate_context(
    const TFusionHandle handle,
    std::shared_ptr<KernelContext>& context,
    const char* const operation)
{
    const auto status = HandleRegistry::instance().get_context(handle, context);
    if (status != TFUSION_STATUS_SUCCESS)
    {
        return tfusion::fail(
            0U,
            status,
            operation,
            "The native kernel context handle is not valid.",
            "Handle registry validation rejected the context handle.");
    }
    return TFUSION_STATUS_SUCCESS;
}

TFusionStatus invalid_argument(
    const TFusionHandle contextHandle,
    const char* const operation,
    const char* const detail)
{
    return tfusion::fail(
        contextHandle,
        TFUSION_STATUS_INVALID_ARGUMENT,
        operation,
        "The native kernel request is invalid.",
        detail);
}
}

TFusionStatus TFUSION_CALL tfusion_get_abi_version(std::uint32_t* const abiVersion)
{
    return tfusion::protect(0U, "tfusion_get_abi_version", [=]() -> TFusionStatus
    {
        if (abiVersion == nullptr)
        {
            return invalid_argument(0U, "tfusion_get_abi_version", "abiVersion must not be null.");
        }
        *abiVersion = TFUSION_ABI_VERSION;
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_negotiate_abi(
    const std::uint32_t requestedAbiVersion,
    std::uint32_t* const negotiatedAbiVersion)
{
    return tfusion::protect(0U, "tfusion_negotiate_abi", [=]() -> TFusionStatus
    {
        if (negotiatedAbiVersion == nullptr)
        {
            return invalid_argument(0U, "tfusion_negotiate_abi", "negotiatedAbiVersion must not be null.");
        }
        *negotiatedAbiVersion = TFUSION_ABI_VERSION;
        if (requestedAbiVersion != TFUSION_ABI_VERSION)
        {
            return tfusion::fail(
                0U,
                TFUSION_STATUS_VERSION_MISMATCH,
                "tfusion_negotiate_abi",
                "The requested native ABI version is not supported.",
                "TFusion.Kernel.Native currently requires an exact ABI v1 match.");
        }
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_context_create(
    const TFusionContextCreateInfo* const createInfo,
    TFusionHandle* const contextHandle)
{
    return tfusion::protect(0U, "tfusion_context_create", [=]() -> TFusionStatus
    {
        if (contextHandle == nullptr)
        {
            return invalid_argument(0U, "tfusion_context_create", "contextHandle must not be null.");
        }
        *contextHandle = 0U;
        if (createInfo == nullptr
            || createInfo->structSize < sizeof(TFusionContextCreateInfo)
            || createInfo->structVersion != TFUSION_CONTEXT_CREATE_INFO_VERSION)
        {
            return invalid_argument(0U, "tfusion_context_create", "Context create structure size or version is invalid.");
        }

        std::uint32_t negotiated = 0U;
        const auto versionStatus = tfusion_negotiate_abi(createInfo->requestedAbiVersion, &negotiated);
        if (versionStatus != TFUSION_STATUS_SUCCESS)
        {
            return versionStatus;
        }
        if (!tfusion::is_valid_utf8(createInfo->clientNameUtf8, createInfo->clientNameByteCount))
        {
            return invalid_argument(0U, "tfusion_context_create", "clientNameUtf8 is null, too long, or malformed UTF-8.");
        }

        const auto* const nameBytes = reinterpret_cast<const char*>(createInfo->clientNameUtf8);
        std::string clientName;
        if (createInfo->clientNameByteCount > 0U)
        {
            clientName.assign(nameBytes, createInfo->clientNameByteCount);
        }
        auto context = std::make_shared<KernelContext>(std::move(clientName));
        const auto handle = HandleRegistry::instance().add_context(std::move(context));
        if (handle == 0U)
        {
            return tfusion::fail(
                0U,
                TFUSION_STATUS_INTERNAL_ERROR,
                "tfusion_context_create",
                "The native kernel context could not be created.",
                "The validated handle registry could not allocate a slot.");
        }
        *contextHandle = handle;
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_context_destroy(const TFusionHandle contextHandle)
{
    return tfusion::protect(contextHandle, "tfusion_context_destroy", [=]() -> TFusionStatus
    {
        const auto status = HandleRegistry::instance().release_context(contextHandle);
        if (status != TFUSION_STATUS_SUCCESS)
        {
            return tfusion::fail(
                0U,
                status,
                "tfusion_context_destroy",
                "The native kernel context could not be destroyed.",
                "Handle registry validation rejected the context handle.");
        }
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_context_get_kernel_info(
    const TFusionHandle contextHandle,
    TFusionKernelInfo* const kernelInfo)
{
    return tfusion::protect(contextHandle, "tfusion_context_get_kernel_info", [=]() -> TFusionStatus
    {
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_context_get_kernel_info");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }
        if (kernelInfo == nullptr
            || kernelInfo->structSize < sizeof(TFusionKernelInfo)
            || kernelInfo->structVersion != TFUSION_KERNEL_INFO_VERSION)
        {
            return invalid_argument(contextHandle, "tfusion_context_get_kernel_info", "Kernel info structure size or version is invalid.");
        }

        kernelInfo->abiVersion = TFUSION_ABI_VERSION;
        kernelInfo->architecture = TFUSION_ARCHITECTURE_X64;
        kernelInfo->compiledOcctVersionRequiredBytes = static_cast<std::uint32_t>(context->compiled_occt_version().size() + 1U);
        kernelInfo->runtimeOcctVersionRequiredBytes = static_cast<std::uint32_t>(context->runtime_occt_version().size() + 1U);
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_context_set_client_name_utf8(
    const TFusionHandle contextHandle,
    const std::uint8_t* const value,
    const std::uint32_t valueByteCount)
{
    return tfusion::protect(contextHandle, "tfusion_context_set_client_name_utf8", [=]() -> TFusionStatus
    {
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_context_set_client_name_utf8");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }
        if (!tfusion::is_valid_utf8(value, valueByteCount))
        {
            return invalid_argument(contextHandle, "tfusion_context_set_client_name_utf8", "The value is null, too long, or malformed UTF-8.");
        }

        std::string name;
        if (valueByteCount > 0U)
        {
            name.assign(reinterpret_cast<const char*>(value), valueByteCount);
        }
        context->set_client_name(std::move(name));
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_context_get_text_utf8(
    const TFusionHandle contextHandle,
    const std::uint32_t field,
    char* const buffer,
    const std::uint32_t bufferByteCount,
    std::uint32_t* const requiredByteCount)
{
    return tfusion::protect(contextHandle, "tfusion_context_get_text_utf8", [=]() -> TFusionStatus
    {
        if (requiredByteCount == nullptr)
        {
            return invalid_argument(contextHandle, "tfusion_context_get_text_utf8", "requiredByteCount must not be null.");
        }

        if (field == TFUSION_TEXT_LAST_DIAGNOSTIC_JSON && contextHandle == 0U)
        {
            return tfusion::copy_utf8(tfusion::diagnostic_json(0U), buffer, bufferByteCount, requiredByteCount);
        }

        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_context_get_text_utf8");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }

        switch (field)
        {
        case TFUSION_TEXT_COMPILED_OCCT_VERSION:
            return tfusion::copy_utf8(context->compiled_occt_version(), buffer, bufferByteCount, requiredByteCount);
        case TFUSION_TEXT_RUNTIME_OCCT_VERSION:
            return tfusion::copy_utf8(context->runtime_occt_version(), buffer, bufferByteCount, requiredByteCount);
        case TFUSION_TEXT_CONTEXT_CLIENT_NAME:
            return tfusion::copy_utf8(context->client_name(), buffer, bufferByteCount, requiredByteCount);
        case TFUSION_TEXT_LAST_DIAGNOSTIC_JSON:
            return tfusion::copy_utf8(tfusion::diagnostic_json(contextHandle), buffer, bufferByteCount, requiredByteCount);
        default:
            return tfusion::fail(
                contextHandle,
                TFUSION_STATUS_UNSUPPORTED,
                "tfusion_context_get_text_utf8",
                "The requested native text field is not supported.",
                "The text field identifier is not defined by ABI v1.");
        }
    });
}

TFusionStatus TFUSION_CALL tfusion_probe_create(
    const TFusionHandle contextHandle,
    TFusionHandle* const probeHandle)
{
    return tfusion::protect(contextHandle, "tfusion_probe_create", [=]() -> TFusionStatus
    {
        if (probeHandle == nullptr)
        {
            return invalid_argument(contextHandle, "tfusion_probe_create", "probeHandle must not be null.");
        }
        *probeHandle = 0U;
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_probe_create");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }
        auto probe = std::make_shared<RuntimeProbe>(context->runtime_occt_version());
        const auto handle = HandleRegistry::instance().add_probe(contextHandle, std::move(probe));
        if (handle == 0U)
        {
            return tfusion::fail(
                contextHandle,
                TFUSION_STATUS_INTERNAL_ERROR,
                "tfusion_probe_create",
                "The native runtime probe could not be created.",
                "The validated handle registry could not allocate a slot.");
        }
        *probeHandle = handle;
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_probe_get_runtime_version_utf8(
    const TFusionHandle contextHandle,
    const TFusionHandle probeHandle,
    char* const buffer,
    const std::uint32_t bufferByteCount,
    std::uint32_t* const requiredByteCount)
{
    return tfusion::protect(contextHandle, "tfusion_probe_get_runtime_version_utf8", [=]() -> TFusionStatus
    {
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_probe_get_runtime_version_utf8");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }

        std::shared_ptr<RuntimeProbe> probe;
        const auto probeStatus = HandleRegistry::instance().get_probe(contextHandle, probeHandle, probe);
        if (probeStatus != TFUSION_STATUS_SUCCESS)
        {
            return tfusion::fail(
                contextHandle,
                probeStatus,
                "tfusion_probe_get_runtime_version_utf8",
                "The native runtime probe handle is not valid for this context.",
                "Handle registry validation rejected the probe handle.");
        }
        return tfusion::copy_utf8(probe->runtime_version(), buffer, bufferByteCount, requiredByteCount);
    });
}

TFusionStatus TFUSION_CALL tfusion_probe_release(
    const TFusionHandle contextHandle,
    const TFusionHandle probeHandle)
{
    return tfusion::protect(contextHandle, "tfusion_probe_release", [=]() -> TFusionStatus
    {
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_probe_release");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }

        const auto status = HandleRegistry::instance().release_probe(contextHandle, probeHandle);
        if (status != TFUSION_STATUS_SUCCESS)
        {
            return tfusion::fail(
                contextHandle,
                status,
                "tfusion_probe_release",
                "The native runtime probe could not be released.",
                "Handle registry validation rejected the probe handle.");
        }
        return TFUSION_STATUS_SUCCESS;
    });
}

TFusionStatus TFUSION_CALL tfusion_test_exception_boundary(
    const TFusionHandle contextHandle,
    const std::uint32_t exceptionProbe)
{
    return tfusion::protect(contextHandle, "tfusion_test_exception_boundary", [=]() -> TFusionStatus
    {
        std::shared_ptr<KernelContext> context;
        const auto contextStatus = validate_context(contextHandle, context, "tfusion_test_exception_boundary");
        if (contextStatus != TFUSION_STATUS_SUCCESS)
        {
            return contextStatus;
        }

        switch (exceptionProbe)
        {
        case TFUSION_EXCEPTION_PROBE_OCCT:
            Standard_Failure::Raise(u8"OCCT boundary probe — presisjon");
            break;
        case TFUSION_EXCEPTION_PROBE_STANDARD:
            throw std::runtime_error("standard boundary probe");
        case TFUSION_EXCEPTION_PROBE_UNKNOWN:
            throw 720;
        default:
            return invalid_argument(contextHandle, "tfusion_test_exception_boundary", "The exception probe identifier is invalid.");
        }
        return TFUSION_STATUS_INTERNAL_ERROR;
    });
}

TFusionStatus TFUSION_CALL tfusion_debug_get_allocation_snapshot(
    TFusionAllocationSnapshot* const snapshot)
{
    return tfusion::protect(0U, "tfusion_debug_get_allocation_snapshot", [=]() -> TFusionStatus
    {
        if (snapshot == nullptr
            || snapshot->structSize < sizeof(TFusionAllocationSnapshot)
            || snapshot->structVersion != TFUSION_ALLOCATION_SNAPSHOT_VERSION)
        {
            return invalid_argument(0U, "tfusion_debug_get_allocation_snapshot", "Allocation snapshot structure size or version is invalid.");
        }
        HandleRegistry::instance().snapshot(*snapshot);
        return TFUSION_STATUS_SUCCESS;
    });
}

#ifndef TFUSION_KERNEL_H
#define TFUSION_KERNEL_H

#include <stdint.h>

#if defined(_WIN32)
#  if defined(TFUSION_KERNEL_NATIVE_EXPORTS)
#    define TFUSION_API __declspec(dllexport)
#  else
#    define TFUSION_API __declspec(dllimport)
#  endif
#  define TFUSION_CALL __cdecl
#else
#  define TFUSION_API
#  define TFUSION_CALL
#endif

#ifdef __cplusplus
extern "C" {
#endif

#define TFUSION_ABI_VERSION UINT32_C(1)
#define TFUSION_CONTEXT_CREATE_INFO_VERSION UINT32_C(1)
#define TFUSION_KERNEL_INFO_VERSION UINT32_C(1)
#define TFUSION_ALLOCATION_SNAPSHOT_VERSION UINT32_C(1)

typedef uint64_t TFusionHandle;

typedef int32_t TFusionStatus;
#define TFUSION_STATUS_SUCCESS INT32_C(0)
#define TFUSION_STATUS_INVALID_ARGUMENT INT32_C(1)
#define TFUSION_STATUS_INVALID_HANDLE INT32_C(2)
#define TFUSION_STATUS_STALE_HANDLE INT32_C(3)
#define TFUSION_STATUS_TYPE_MISMATCH INT32_C(4)
#define TFUSION_STATUS_CONTEXT_MISMATCH INT32_C(5)
#define TFUSION_STATUS_BUFFER_TOO_SMALL INT32_C(6)
#define TFUSION_STATUS_VERSION_MISMATCH INT32_C(7)
#define TFUSION_STATUS_INTERNAL_ERROR INT32_C(8)
#define TFUSION_STATUS_KERNEL_ERROR INT32_C(9)
#define TFUSION_STATUS_UNSUPPORTED INT32_C(10)

typedef uint32_t TFusionArchitecture;
#define TFUSION_ARCHITECTURE_UNKNOWN UINT32_C(0)
#define TFUSION_ARCHITECTURE_X64 UINT32_C(1)

typedef uint32_t TFusionTextField;
#define TFUSION_TEXT_COMPILED_OCCT_VERSION UINT32_C(1)
#define TFUSION_TEXT_RUNTIME_OCCT_VERSION UINT32_C(2)
#define TFUSION_TEXT_CONTEXT_CLIENT_NAME UINT32_C(3)
#define TFUSION_TEXT_LAST_DIAGNOSTIC_JSON UINT32_C(4)

typedef uint32_t TFusionExceptionProbe;
#define TFUSION_EXCEPTION_PROBE_OCCT UINT32_C(1)
#define TFUSION_EXCEPTION_PROBE_STANDARD UINT32_C(2)
#define TFUSION_EXCEPTION_PROBE_UNKNOWN UINT32_C(3)

typedef struct TFusionContextCreateInfo {
    uint32_t structSize;
    uint32_t structVersion;
    uint32_t requestedAbiVersion;
    const uint8_t* clientNameUtf8;
    uint32_t clientNameByteCount;
    uint32_t reserved;
} TFusionContextCreateInfo;

typedef struct TFusionKernelInfo {
    uint32_t structSize;
    uint32_t structVersion;
    uint32_t abiVersion;
    uint32_t architecture;
    uint32_t compiledOcctVersionRequiredBytes;
    uint32_t runtimeOcctVersionRequiredBytes;
} TFusionKernelInfo;

typedef struct TFusionAllocationSnapshot {
    uint32_t structSize;
    uint32_t structVersion;
    uint64_t activeContexts;
    uint64_t activeProbes;
    uint64_t totalAllocations;
    uint64_t totalReleases;
} TFusionAllocationSnapshot;

TFUSION_API TFusionStatus TFUSION_CALL tfusion_get_abi_version(uint32_t* abiVersion);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_negotiate_abi(uint32_t requestedAbiVersion, uint32_t* negotiatedAbiVersion);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_context_create(const TFusionContextCreateInfo* createInfo, TFusionHandle* contextHandle);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_context_destroy(TFusionHandle contextHandle);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_context_get_kernel_info(TFusionHandle contextHandle, TFusionKernelInfo* kernelInfo);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_context_set_client_name_utf8(TFusionHandle contextHandle, const uint8_t* value, uint32_t valueByteCount);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_context_get_text_utf8(TFusionHandle contextHandle, uint32_t field, char* buffer, uint32_t bufferByteCount, uint32_t* requiredByteCount);

TFUSION_API TFusionStatus TFUSION_CALL tfusion_probe_create(TFusionHandle contextHandle, TFusionHandle* probeHandle);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_probe_get_runtime_version_utf8(TFusionHandle contextHandle, TFusionHandle probeHandle, char* buffer, uint32_t bufferByteCount, uint32_t* requiredByteCount);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_probe_release(TFusionHandle contextHandle, TFusionHandle probeHandle);

TFUSION_API TFusionStatus TFUSION_CALL tfusion_test_exception_boundary(TFusionHandle contextHandle, uint32_t exceptionProbe);
TFUSION_API TFusionStatus TFUSION_CALL tfusion_debug_get_allocation_snapshot(TFusionAllocationSnapshot* snapshot);

#ifdef __cplusplus
}
#endif

#endif

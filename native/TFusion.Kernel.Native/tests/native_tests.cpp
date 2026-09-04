#include "tfusion_kernel.h"

#include <array>
#include <cstdint>
#include <iostream>
#include <stdexcept>
#include <string>
#include <string_view>
#include <vector>

namespace
{
void require(const bool condition, const std::string_view message)
{
    if (!condition)
    {
        throw std::runtime_error(std::string(message));
    }
}

TFusionHandle create_context(const std::string_view name = "native-tests")
{
    const auto* const bytes = reinterpret_cast<const std::uint8_t*>(name.data());
    const TFusionContextCreateInfo createInfo{
        sizeof(TFusionContextCreateInfo),
        TFUSION_CONTEXT_CREATE_INFO_VERSION,
        TFUSION_ABI_VERSION,
        bytes,
        static_cast<std::uint32_t>(name.size()),
        0U};
    TFusionHandle handle = 0U;
    require(tfusion_context_create(&createInfo, &handle) == TFUSION_STATUS_SUCCESS, "Context creation failed.");
    require(handle != 0U, "Context handle was zero.");
    return handle;
}

std::string get_text(const TFusionHandle context, const std::uint32_t field)
{
    std::uint32_t required = 0U;
    require(
        tfusion_context_get_text_utf8(context, field, nullptr, 0U, &required) == TFUSION_STATUS_BUFFER_TOO_SMALL,
        "Text size query did not report BUFFER_TOO_SMALL.");
    require(required > 0U, "Text size query returned zero.");
    std::vector<char> buffer(required);
    require(
        tfusion_context_get_text_utf8(context, field, buffer.data(), required, &required) == TFUSION_STATUS_SUCCESS,
        "Text copy failed.");
    require(buffer.back() == '\0', "Text was not null terminated.");
    return std::string(buffer.data());
}

void test_abi()
{
    std::uint32_t version = 0U;
    require(tfusion_get_abi_version(&version) == TFUSION_STATUS_SUCCESS, "ABI query failed.");
    require(version == TFUSION_ABI_VERSION, "ABI query returned a wrong version.");
    require(tfusion_get_abi_version(nullptr) == TFUSION_STATUS_INVALID_ARGUMENT, "Null ABI output was accepted.");

    std::uint32_t negotiated = 0U;
    require(tfusion_negotiate_abi(1U, &negotiated) == TFUSION_STATUS_SUCCESS, "ABI v1 negotiation failed.");
    require(negotiated == 1U, "Negotiated version was wrong.");
    require(tfusion_negotiate_abi(0U, &negotiated) == TFUSION_STATUS_VERSION_MISMATCH, "ABI zero was accepted.");
    require(tfusion_negotiate_abi(2U, &negotiated) == TFUSION_STATUS_VERSION_MISMATCH, "Future ABI was accepted.");
    require(tfusion_negotiate_abi(UINT32_MAX, &negotiated) == TFUSION_STATUS_VERSION_MISMATCH, "Unsupported ABI was accepted.");
}

void test_context_and_occt()
{
    const auto context = create_context(u8"TFUSION — blå");
    TFusionKernelInfo info{
        sizeof(TFusionKernelInfo),
        TFUSION_KERNEL_INFO_VERSION,
        0U, 0U, 0U, 0U};
    require(tfusion_context_get_kernel_info(context, &info) == TFUSION_STATUS_SUCCESS, "Kernel info failed.");
    require(info.abiVersion == 1U, "Kernel info ABI was wrong.");
    require(info.architecture == TFUSION_ARCHITECTURE_X64, "Kernel architecture was not x64.");

    const auto compiled = get_text(context, TFUSION_TEXT_COMPILED_OCCT_VERSION);
    const auto runtime = get_text(context, TFUSION_TEXT_RUNTIME_OCCT_VERSION);
    require(compiled.find("8.0.1") != std::string::npos, "Compiled OCCT version did not come from the pinned OCCT headers.");
    require(runtime.find("8.0.1") != std::string::npos, "Runtime OCCT version did not come from the pinned OCCT runtime.");
    require(info.compiledOcctVersionRequiredBytes == compiled.size() + 1U, "Compiled version size metadata was wrong.");
    require(info.runtimeOcctVersionRequiredBytes == runtime.size() + 1U, "Runtime version size metadata was wrong.");

    require(get_text(context, TFUSION_TEXT_CONTEXT_CLIENT_NAME) == u8"TFUSION — blå", "Unicode client name did not round-trip.");

    std::array<char, 2> shortBuffer{'x', 'x'};
    std::uint32_t required = 0U;
    require(
        tfusion_context_get_text_utf8(context, TFUSION_TEXT_RUNTIME_OCCT_VERSION, shortBuffer.data(), 2U, &required)
            == TFUSION_STATUS_BUFFER_TOO_SMALL,
        "Short text buffer was accepted.");
    require(shortBuffer[0] == '\0', "Short text buffer was not safely terminated.");
    require(required > 2U, "Short-buffer required length was wrong.");

    const std::array<std::uint8_t, 2> malformed{0xC3U, 0x28U};
    require(
        tfusion_context_set_client_name_utf8(context, malformed.data(), static_cast<std::uint32_t>(malformed.size()))
            == TFUSION_STATUS_INVALID_ARGUMENT,
        "Malformed UTF-8 was accepted.");

    require(tfusion_context_destroy(context) == TFUSION_STATUS_SUCCESS, "Context destruction failed.");
    require(tfusion_context_destroy(context) == TFUSION_STATUS_STALE_HANDLE, "Repeated context destruction was not stale.");
    require(tfusion_context_destroy(0U) == TFUSION_STATUS_INVALID_HANDLE, "Zero context handle was accepted.");
    require(tfusion_context_destroy(UINT64_C(0x0123456789ABCDEF)) == TFUSION_STATUS_INVALID_HANDLE, "Random context handle was accepted.");
}

void test_handles()
{
    const auto contextA = create_context("context-a");
    const auto contextB = create_context("context-b");
    TFusionHandle probe = 0U;
    require(tfusion_probe_create(contextA, &probe) == TFUSION_STATUS_SUCCESS, "Probe creation failed.");
    require(tfusion_context_destroy(probe) == TFUSION_STATUS_TYPE_MISMATCH, "Wrong handle type was accepted.");

    std::uint32_t required = 0U;
    require(
        tfusion_probe_get_runtime_version_utf8(contextB, probe, nullptr, 0U, &required)
            == TFUSION_STATUS_CONTEXT_MISMATCH,
        "Cross-context probe use was accepted.");
    require(tfusion_probe_release(contextB, probe) == TFUSION_STATUS_CONTEXT_MISMATCH, "Cross-context probe release was accepted.");
    require(tfusion_probe_release(contextA, probe) == TFUSION_STATUS_SUCCESS, "Probe release failed.");
    require(tfusion_probe_release(contextA, probe) == TFUSION_STATUS_STALE_HANDLE, "Double probe release was not stale.");

    TFusionHandle cleanupProbe = 0U;
    require(tfusion_probe_create(contextA, &cleanupProbe) == TFUSION_STATUS_SUCCESS, "Cleanup probe creation failed.");
    require(tfusion_context_destroy(contextA) == TFUSION_STATUS_SUCCESS, "Owning context destruction failed.");
    require(
        tfusion_probe_get_runtime_version_utf8(contextB, cleanupProbe, nullptr, 0U, &required)
            == TFUSION_STATUS_STALE_HANDLE,
        "Context destruction did not invalidate child handles.");
    require(tfusion_context_destroy(contextB) == TFUSION_STATUS_SUCCESS, "Second context destruction failed.");
}

void test_exception_containment()
{
    const auto context = create_context("exception-tests");
    require(
        tfusion_test_exception_boundary(context, TFUSION_EXCEPTION_PROBE_OCCT) == TFUSION_STATUS_KERNEL_ERROR,
        "OCCT exception was not converted to KERNEL_ERROR.");
    const auto occtDiagnostic = get_text(context, TFUSION_TEXT_LAST_DIAGNOSTIC_JSON);
    require(occtDiagnostic.find("KERNEL_ERROR") != std::string::npos, "OCCT diagnostic lacks stable status.");
    require(occtDiagnostic.find(u8"presisjon") != std::string::npos, "Unicode OCCT detail was not preserved.");

    require(
        tfusion_test_exception_boundary(context, TFUSION_EXCEPTION_PROBE_STANDARD) == TFUSION_STATUS_INTERNAL_ERROR,
        "std::exception was not converted to INTERNAL_ERROR.");
    require(
        tfusion_test_exception_boundary(context, TFUSION_EXCEPTION_PROBE_UNKNOWN) == TFUSION_STATUS_INTERNAL_ERROR,
        "Unknown exception was not converted to INTERNAL_ERROR.");
    require(tfusion_context_destroy(context) == TFUSION_STATUS_SUCCESS, "Exception-test context destruction failed.");
}

void test_lifetime_stress()
{
    TFusionAllocationSnapshot before{
        sizeof(TFusionAllocationSnapshot),
        TFUSION_ALLOCATION_SNAPSHOT_VERSION,
        0U, 0U, 0U, 0U};
    require(tfusion_debug_get_allocation_snapshot(&before) == TFUSION_STATUS_SUCCESS, "Initial allocation snapshot failed.");
    require(before.activeContexts == 0U && before.activeProbes == 0U, "Tests leaked resources before stress began.");

    constexpr std::uint32_t iterations = 10000U;
    for (std::uint32_t iteration = 0U; iteration < iterations; ++iteration)
    {
        const auto context = create_context("lifetime-stress");
        TFusionHandle probe = 0U;
        require(tfusion_probe_create(context, &probe) == TFUSION_STATUS_SUCCESS, "Stress probe creation failed.");
        require(tfusion_probe_release(context, probe) == TFUSION_STATUS_SUCCESS, "Stress probe release failed.");
        require(tfusion_context_destroy(context) == TFUSION_STATUS_SUCCESS, "Stress context destruction failed.");
    }

    TFusionAllocationSnapshot after{
        sizeof(TFusionAllocationSnapshot),
        TFUSION_ALLOCATION_SNAPSHOT_VERSION,
        0U, 0U, 0U, 0U};
    require(tfusion_debug_get_allocation_snapshot(&after) == TFUSION_STATUS_SUCCESS, "Final allocation snapshot failed.");
    require(after.activeContexts == 0U && after.activeProbes == 0U, "Stress left native handles active.");
    require(after.totalAllocations - before.totalAllocations == iterations * 2U, "Stress allocation count was incomplete.");
    require(after.totalReleases - before.totalReleases == iterations * 2U, "Stress release count was incomplete.");
    std::cout << "lifetime_iterations=" << iterations
              << " active_contexts=" << after.activeContexts
              << " active_probes=" << after.activeProbes << '\n';
}
}

int main()
{
    try
    {
        test_abi();
        test_context_and_occt();
        test_handles();
        test_exception_containment();
        test_lifetime_stress();
        std::cout << "TFusion.Kernel.Native ABI v1 tests passed.\n";
        return 0;
    }
    catch (const std::exception& exception)
    {
        std::cerr << "Native test failure: " << exception.what() << '\n';
        return 1;
    }
}

#pragma once

#include "context.hpp"
#include "tfusion_kernel.h"

#include <cstdint>
#include <memory>
#include <mutex>
#include <vector>

namespace tfusion
{
enum class HandleType : std::uint8_t
{
    none = 0,
    context = 1,
    runtimeProbe = 2
};

class HandleRegistry final
{
public:
    static HandleRegistry& instance();

    TFusionHandle add_context(std::shared_ptr<KernelContext> context);
    TFusionHandle add_probe(TFusionHandle ownerContext, std::shared_ptr<RuntimeProbe> probe);

    TFusionStatus get_context(TFusionHandle handle, std::shared_ptr<KernelContext>& context) const;
    TFusionStatus get_probe(TFusionHandle ownerContext, TFusionHandle handle, std::shared_ptr<RuntimeProbe>& probe) const;
    TFusionStatus release_probe(TFusionHandle ownerContext, TFusionHandle handle);
    TFusionStatus release_context(TFusionHandle handle);

    void snapshot(TFusionAllocationSnapshot& snapshot) const;

private:
    struct Slot final
    {
        std::uint32_t generation{1U};
        HandleType type{HandleType::none};
        TFusionHandle ownerContext{0U};
        std::shared_ptr<void> value;
    };

    HandleRegistry() = default;

    TFusionHandle add(HandleType type, TFusionHandle ownerContext, std::shared_ptr<void> value);
    static bool decode(TFusionHandle handle, std::uint32_t& index, std::uint32_t& generation) noexcept;
    static TFusionHandle encode(std::uint32_t index, std::uint32_t generation) noexcept;
    static void retire(Slot& slot) noexcept;
    TFusionStatus validate_locked(TFusionHandle handle, HandleType expected, const Slot*& slot) const noexcept;
    TFusionStatus validate_locked(TFusionHandle handle, HandleType expected, Slot*& slot) noexcept;

    mutable std::mutex mutex_;
    std::vector<Slot> slots_;
    std::uint64_t activeContexts_{0U};
    std::uint64_t activeProbes_{0U};
    std::uint64_t totalAllocations_{0U};
    std::uint64_t totalReleases_{0U};
};
}

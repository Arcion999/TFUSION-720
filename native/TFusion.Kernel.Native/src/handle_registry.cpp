#include "handle_registry.hpp"

#include <limits>
#include <utility>

namespace tfusion
{
HandleRegistry& HandleRegistry::instance()
{
    static HandleRegistry registry;
    return registry;
}

TFusionHandle HandleRegistry::add_context(std::shared_ptr<KernelContext> context)
{
    return add(HandleType::context, 0U, std::move(context));
}

TFusionHandle HandleRegistry::add_probe(
    const TFusionHandle ownerContext,
    std::shared_ptr<RuntimeProbe> probe)
{
    return add(HandleType::runtimeProbe, ownerContext, std::move(probe));
}

TFusionHandle HandleRegistry::add(
    const HandleType type,
    const TFusionHandle ownerContext,
    std::shared_ptr<void> value)
{
    const std::scoped_lock lock(mutex_);
    std::uint32_t index = 0U;
    for (; index < slots_.size(); ++index)
    {
        if (slots_[index].type == HandleType::none)
        {
            break;
        }
    }

    if (index == slots_.size())
    {
        if (slots_.size() >= static_cast<std::size_t>((std::numeric_limits<std::uint32_t>::max)() - 1U))
        {
            return 0U;
        }
        slots_.emplace_back();
    }

    auto& slot = slots_[index];
    slot.type = type;
    slot.ownerContext = ownerContext;
    slot.value = std::move(value);
    ++totalAllocations_;
    if (type == HandleType::context)
    {
        ++activeContexts_;
    }
    else if (type == HandleType::runtimeProbe)
    {
        ++activeProbes_;
    }
    return encode(index, slot.generation);
}

TFusionStatus HandleRegistry::get_context(
    const TFusionHandle handle,
    std::shared_ptr<KernelContext>& context) const
{
    const std::scoped_lock lock(mutex_);
    const Slot* slot = nullptr;
    const auto status = validate_locked(handle, HandleType::context, slot);
    if (status != TFUSION_STATUS_SUCCESS)
    {
        return status;
    }

    context = std::static_pointer_cast<KernelContext>(slot->value);
    return context->is_active() ? TFUSION_STATUS_SUCCESS : TFUSION_STATUS_STALE_HANDLE;
}

TFusionStatus HandleRegistry::get_probe(
    const TFusionHandle ownerContext,
    const TFusionHandle handle,
    std::shared_ptr<RuntimeProbe>& probe) const
{
    const std::scoped_lock lock(mutex_);
    const Slot* slot = nullptr;
    const auto status = validate_locked(handle, HandleType::runtimeProbe, slot);
    if (status != TFUSION_STATUS_SUCCESS)
    {
        return status;
    }
    if (slot->ownerContext != ownerContext)
    {
        return TFUSION_STATUS_CONTEXT_MISMATCH;
    }
    probe = std::static_pointer_cast<RuntimeProbe>(slot->value);
    return TFUSION_STATUS_SUCCESS;
}

TFusionStatus HandleRegistry::release_probe(
    const TFusionHandle ownerContext,
    const TFusionHandle handle)
{
    const std::scoped_lock lock(mutex_);
    Slot* slot = nullptr;
    const auto status = validate_locked(handle, HandleType::runtimeProbe, slot);
    if (status != TFUSION_STATUS_SUCCESS)
    {
        return status;
    }
    if (slot->ownerContext != ownerContext)
    {
        return TFUSION_STATUS_CONTEXT_MISMATCH;
    }

    retire(*slot);
    --activeProbes_;
    ++totalReleases_;
    return TFUSION_STATUS_SUCCESS;
}

TFusionStatus HandleRegistry::release_context(const TFusionHandle handle)
{
    const std::scoped_lock lock(mutex_);
    Slot* contextSlot = nullptr;
    const auto status = validate_locked(handle, HandleType::context, contextSlot);
    if (status != TFUSION_STATUS_SUCCESS)
    {
        return status;
    }

    const auto context = std::static_pointer_cast<KernelContext>(contextSlot->value);
    context->mark_destroyed();
    for (auto& slot : slots_)
    {
        if (slot.type == HandleType::runtimeProbe && slot.ownerContext == handle)
        {
            retire(slot);
            --activeProbes_;
            ++totalReleases_;
        }
    }

    retire(*contextSlot);
    --activeContexts_;
    ++totalReleases_;
    return TFUSION_STATUS_SUCCESS;
}

void HandleRegistry::snapshot(TFusionAllocationSnapshot& snapshot) const
{
    const std::scoped_lock lock(mutex_);
    snapshot.activeContexts = activeContexts_;
    snapshot.activeProbes = activeProbes_;
    snapshot.totalAllocations = totalAllocations_;
    snapshot.totalReleases = totalReleases_;
}

bool HandleRegistry::decode(
    const TFusionHandle handle,
    std::uint32_t& index,
    std::uint32_t& generation) noexcept
{
    const auto encodedIndex = static_cast<std::uint32_t>(handle & UINT64_C(0xFFFFFFFF));
    generation = static_cast<std::uint32_t>((handle >> 32U) & UINT64_C(0x7FFFFFFF));
    if (encodedIndex == 0U || generation == 0U || (handle & UINT64_C(0x8000000000000000)) != 0U)
    {
        return false;
    }
    index = encodedIndex - 1U;
    return true;
}

TFusionHandle HandleRegistry::encode(const std::uint32_t index, const std::uint32_t generation) noexcept
{
    return (static_cast<TFusionHandle>(generation) << 32U)
        | static_cast<TFusionHandle>(index + 1U);
}

void HandleRegistry::retire(Slot& slot) noexcept
{
    slot.value.reset();
    slot.type = HandleType::none;
    slot.ownerContext = 0U;
    slot.generation = slot.generation == UINT32_C(0x7FFFFFFF) ? 1U : slot.generation + 1U;
}

TFusionStatus HandleRegistry::validate_locked(
    const TFusionHandle handle,
    const HandleType expected,
    const Slot*& slot) const noexcept
{
    std::uint32_t index = 0U;
    std::uint32_t generation = 0U;
    if (!decode(handle, index, generation) || index >= slots_.size())
    {
        return TFUSION_STATUS_INVALID_HANDLE;
    }

    slot = &slots_[index];
    if (slot->generation != generation || slot->type == HandleType::none)
    {
        return TFUSION_STATUS_STALE_HANDLE;
    }
    return slot->type == expected ? TFUSION_STATUS_SUCCESS : TFUSION_STATUS_TYPE_MISMATCH;
}

TFusionStatus HandleRegistry::validate_locked(
    const TFusionHandle handle,
    const HandleType expected,
    Slot*& slot) noexcept
{
    const Slot* constSlot = nullptr;
    const auto status = static_cast<const HandleRegistry*>(this)->validate_locked(handle, expected, constSlot);
    slot = const_cast<Slot*>(constSlot);
    return status;
}
}

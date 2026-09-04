#include "context.hpp"

#include <Standard_Version.hxx>
#include <Standard_VersionInfo.hxx>

#include <utility>

namespace tfusion
{
KernelContext::KernelContext(std::string clientName)
    : clientName_(std::move(clientName)),
      compiledOcctVersion_(OCC_VERSION_COMPLETE),
      runtimeOcctVersion_(OCCT_Version_String_Complete())
{
}

bool KernelContext::is_active() const noexcept
{
    return active_.load(std::memory_order_acquire);
}

void KernelContext::mark_destroyed() noexcept
{
    active_.store(false, std::memory_order_release);
}

std::string KernelContext::client_name() const
{
    const std::scoped_lock lock(mutex_);
    return clientName_;
}

void KernelContext::set_client_name(std::string value)
{
    const std::scoped_lock lock(mutex_);
    clientName_ = std::move(value);
}

const std::string& KernelContext::compiled_occt_version() const noexcept
{
    return compiledOcctVersion_;
}

const std::string& KernelContext::runtime_occt_version() const noexcept
{
    return runtimeOcctVersion_;
}

std::string KernelContext::diagnostic_json() const
{
    const std::scoped_lock lock(mutex_);
    return diagnosticJson_;
}

void KernelContext::set_diagnostic(std::string value)
{
    const std::scoped_lock lock(mutex_);
    diagnosticJson_ = std::move(value);
}

RuntimeProbe::RuntimeProbe(std::string runtimeVersion)
    : runtimeVersion_(std::move(runtimeVersion))
{
}

const std::string& RuntimeProbe::runtime_version() const noexcept
{
    return runtimeVersion_;
}
}

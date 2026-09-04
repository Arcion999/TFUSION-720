#pragma once

#include <atomic>
#include <mutex>
#include <string>

namespace tfusion
{
class KernelContext final
{
public:
    explicit KernelContext(std::string clientName);

    [[nodiscard]] bool is_active() const noexcept;
    void mark_destroyed() noexcept;

    [[nodiscard]] std::string client_name() const;
    void set_client_name(std::string value);

    [[nodiscard]] const std::string& compiled_occt_version() const noexcept;
    [[nodiscard]] const std::string& runtime_occt_version() const noexcept;

    [[nodiscard]] std::string diagnostic_json() const;
    void set_diagnostic(std::string value);

private:
    std::atomic_bool active_{true};
    mutable std::mutex mutex_;
    std::string clientName_;
    std::string diagnosticJson_;
    std::string compiledOcctVersion_;
    std::string runtimeOcctVersion_;
};

class RuntimeProbe final
{
public:
    explicit RuntimeProbe(std::string runtimeVersion);
    [[nodiscard]] const std::string& runtime_version() const noexcept;

private:
    std::string runtimeVersion_;
};
}

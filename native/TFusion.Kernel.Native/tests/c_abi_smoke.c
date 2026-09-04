#include "tfusion_kernel.h"

#include <stdint.h>
#include <stdio.h>

int main(void)
{
    uint32_t abi = 0U;
    if (tfusion_get_abi_version(&abi) != TFUSION_STATUS_SUCCESS || abi != TFUSION_ABI_VERSION)
    {
        fputs("C ABI version query failed.\n", stderr);
        return 1;
    }

    const uint8_t client_name[] = { 'c', '-', 'a', 'b', 'i' };
    TFusionContextCreateInfo create_info = {
        sizeof(TFusionContextCreateInfo),
        TFUSION_CONTEXT_CREATE_INFO_VERSION,
        TFUSION_ABI_VERSION,
        client_name,
        (uint32_t)sizeof(client_name),
        0U
    };
    TFusionHandle context = 0U;
    if (tfusion_context_create(&create_info, &context) != TFUSION_STATUS_SUCCESS || context == 0U)
    {
        fputs("C ABI context creation failed.\n", stderr);
        return 2;
    }
    if (tfusion_context_destroy(context) != TFUSION_STATUS_SUCCESS)
    {
        fputs("C ABI context destruction failed.\n", stderr);
        return 3;
    }
    return 0;
}

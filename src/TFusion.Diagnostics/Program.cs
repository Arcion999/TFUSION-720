using System.Text.Json;

namespace TFusion.Diagnostics;

public static class Program
{
    public static int Main(string[] args)
    {
        if (!args.SequenceEqual(["--self-test", "--format", "json"], StringComparer.Ordinal))
        {
            Console.Out.WriteLine(JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                status = "error",
                code = "TFN-DIAG-INVALID-ARGUMENTS",
                message = "Usage: TFusion.Diagnostics.exe --self-test --format json",
            }));
            return 2;
        }

        var command = new SelfTestCommand();
        return command.Execute(Console.Out, Console.Error);
    }
}

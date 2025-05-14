using Fujin;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
class Program {
    // export DYLD_FALLBACK_LIBRARY_PATH=$DYLD_FALLBACK_LIBRARY_PATH:$(brew --prefix)/lib
    static async Task Main(string[] args)
    {
        await using var client = await Client.ConnectAsync("localhost", 4848, opts => {
            opts.MaxInboundBidirectionalStreams = 1000;
            opts.MaxInboundUnidirectionalStreams = 1000;
            opts.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        });
        try {
            Console.WriteLine("connected!");
            await Task.Delay(10000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("disconnected");
    }
}
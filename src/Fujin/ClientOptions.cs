using System.Net.Security;

namespace Fujin;

public class ClientOptions
{
    public int MaxInboundBidirectionalStreams { get; set; } = 1000;
    public int MaxInboundUnidirectionalStreams { get; set; } = 1000;

    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }
}
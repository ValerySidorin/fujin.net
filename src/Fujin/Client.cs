using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.Versioning;


namespace Fujin
{

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public class Client : IAsyncDisposable
    {
        private readonly QuicConnection _connection;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _incomingHandler;

        public Client(QuicConnection connection)
        {
            _connection = connection;
            _incomingHandler = HandleIncomingStreamsAsync(_cts.Token);
        }

        public static async Task<Client> ConnectAsync(
            string host, int port,
            Action<ClientOptions>? configure = null)
        {
            var endpoint = new DnsEndPoint(host, port);
            var options = new ClientOptions();
            configure?.Invoke(options);

            var quicOptions = new QuicClientConnectionOptions
            {
                RemoteEndPoint = endpoint,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions
                {
                    ApplicationProtocols = [new SslApplicationProtocol("fujin")],
                    RemoteCertificateValidationCallback = options.RemoteCertificateValidationCallback
                },
                DefaultCloseErrorCode = 0,
                DefaultStreamErrorCode = 0,
                MaxInboundBidirectionalStreams = options.MaxInboundBidirectionalStreams,
                MaxInboundUnidirectionalStreams = options.MaxInboundUnidirectionalStreams
            };

            var connection = await QuicConnection.ConnectAsync(quicOptions);
            return new Client(connection);
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await _incomingHandler;
            await _connection.CloseAsync(0);
        }

        private async Task HandleIncomingStreamsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                QuicStream? stream;
                try
                {
                    stream = await _connection.AcceptInboundStreamAsync(cancellationToken);
                    byte[] buffer = new byte[1];
                    int read = await stream.ReadAsync(buffer, cancellationToken);
                    if (read == 1 && buffer[0] == 0x11) // PING
                    {
                        buffer[0] = 0x11; // PONG
                        await stream.WriteAsync(buffer, cancellationToken);
                    }
                    await stream.DisposeAsync();
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PING] error: {ex}");
                }
            }
        }
    }
}

using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TrafficCourts.Workflow.Service.Configuration;

namespace TrafficCourts.Workflow.Service.Services;

public partial class SmtpClientFactory : ISmtpClientFactory
{
    private readonly ILogger<SmtpClientFactory> _logger;
    private readonly SmtpConfiguration _stmpConfiguration;

    public SmtpClientFactory(ILogger<SmtpClientFactory> logger, SmtpConfiguration stmpConfiguration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stmpConfiguration = stmpConfiguration ?? throw new ArgumentNullException(nameof(stmpConfiguration));
    }

    public async Task<ISmtpClient> CreateAsync(CancellationToken cancellationToken)
    {
        using var operation = Instrumentation.Smtp.BeginOperation(nameof(ISmtpClient.ConnectAsync));

        try
        {
            SmtpClient smtp = new();

            if (_stmpConfiguration.IgnoreCertificateValidation)
            {
                LogCertificateValidationDisabled();
                smtp.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
            }

            await smtp.ConnectAsync(_stmpConfiguration.Host, _stmpConfiguration.Port, SecureSocketOptions.Auto, cancellationToken)
                .ConfigureAwait(false);
            
            return smtp;
        }
        catch (ArgumentNullException exception)
        {
            // host or message is null.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "Host or message is null");
            throw new SmtpConnectFailedException("Host or message is null", exception);
        }
        catch (ArgumentOutOfRangeException exception) 
        {
            // port is not between 0 and 65535.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "Port is not between 0 and 65535");
            throw new SmtpConnectFailedException("Port is not between 0 and 65535", exception);
        }
        catch (ArgumentException exception)
        {
            // The host is a zero-length string.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "The host is a zero-length string");
            throw new SmtpConnectFailedException("The host is a zero-length string", exception);
        }
        catch (ObjectDisposedException exception)
        {
            // The MailKit.Net.Smtp.SmtpClient has been disposed.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "SmtpClient has been disposed");
            throw new SmtpConnectFailedException("SmtpClient has been disposed", exception);
        }
        catch (NotSupportedException exception)
        {
            // options was set to MailKit.Security.SecureSocketOptions.StartTls and the SMTP
            // server does not support the STARTTLS extension.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "STMP server does not support the STARTTLS extension");
            throw new SmtpConnectFailedException("STMP server does not support the STARTTLS extension", exception);
        }
        /*catch (OperationCanceledException oce)
        {
            // The operation was canceled.
            _logger.LogError(oce, "The operation was canceled.");
            throw new SmtpConnectFailedException($"The operation was canceled", oce);
        }*/
        catch (System.Net.Sockets.SocketException exception)
        {
            // A socket error occurred trying to connect to the remote host.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "A socket error occurred trying to connect to the remote host");
            throw new SmtpConnectFailedException("A socket error occurred trying to connect to the remote host", exception);
        }
        catch (SslHandshakeException exception)
        {
            // An error occurred during the SSL/TLS negotiations.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "An error occurred during the SSL/TLS negotiations");
            throw new SmtpConnectFailedException("An error occurred during the SSL/TLS negotiations", exception);
        }
        catch (IOException exception)
        {
            // An I/O error occurred.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "An I/O error occurred");
            throw new SmtpConnectFailedException("An I/O error occurred", exception);
        }
        catch (SmtpCommandException exception)
        {
            // An SMTP command failed.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "An SMTP command failed");
            throw new SmtpConnectFailedException("An SMTP command failed", exception);
        }
        catch (SmtpProtocolException exception)
        {
            // An SMTP protocol error occurred.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "An SMTP protocol error occurred");
            throw new SmtpConnectFailedException("An SMTP protocol error occurred", exception);
        }
        catch (InvalidOperationException exception)
        {
            // The MailKit.Net.Smtp.SmtpClient is already connected.
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "The SmtpClient is already connected");
            throw new SmtpConnectFailedException("The SmtpClient is already connected", exception);
        }
        catch (Exception exception)
        {
            Instrumentation.Smtp.EndOperation(operation, exception);
            _logger.LogError(exception, "General smtp connection exception thrown");
            throw new SmtpConnectFailedException("General smtp connection exception thrown", exception);
        }
    }

    private bool RemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true; // Certificate is valid
        }

        LogCertificateValidationError(certificate, chain, sslPolicyErrors);
        return true;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "TLS certificate validation failed, will ignore certificate errors.", EventName = "CertificateValidationError")]
    private partial void LogCertificateValidationError(
        [TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)]
        X509Certificate? certificate,
        [TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)]
        X509Chain? chain,
        [TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)]
        SslPolicyErrors policyErrors);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "TLS certificate validation has been disabled", EventName = "CertificateValidationDisabled")]
    private partial void LogCertificateValidationDisabled();
}

internal static partial class TagProvider
{

    public static void RecordTags(ITagCollector collector, X509Certificate? certificate)
    {
        if (certificate?.Subject is not null)
        {
            collector.Add("CertificateSubject", certificate.Subject);
        }
    }

    public static void RecordTags(ITagCollector collector, X509Chain? chain)
    {
        if (chain is not null)
        {
            StringBuilder builder = new();
            foreach (X509ChainElement element in chain.ChainElements)
            {
                builder.AppendLine(element.Certificate.Subject);
            }
            string subjectChain = builder.ToString();
            collector.Add("CertificateChain", subjectChain);
        }
    }

    public static void RecordTags(ITagCollector collector, SslPolicyErrors sslPolicyErrors)
    {
        collector.Add("SslPolicyErrors", sslPolicyErrors);
    }
}

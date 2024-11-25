using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using nClam;
using System;
using System.IO;
using System.Threading.Tasks;
using TrafficCourts.Messaging.MessageContracts;
using TrafficCourts.Workflow.Service.Consumers;
using TrafficCourts.Workflow.Service.Services;
using Xunit;

namespace TrafficCourts.Test.Workflow.Service.Consumers;

public class ScanUploadedDocumentForVirusesConsumerTest
{
    private const string? skip = "Failing in githut actions but not locally"; // 

    [Fact(Skip = skip)]
    public async Task TestVirusScanDocumentConsumer_ConfirmScanResultClean()
    {
        Mock<IClamClient> clamClient = new();
        Mock<IWorkflowDocumentService> comsService = new();

        // Arrange
        var file = CreateFile();

        comsService
            .SetupGetFileWithAnyParameters()
            .Returns(Task.FromResult(file));

        clamClient
            .SendAndScanFileWithAnyParameters()
            .Returns(Task.FromResult(new ClamScanResult("ok")));

        // comsService.UpdateFileAsync ...

        await using var provider = GetServiceProvider(clamClient.Object, comsService.Object);

        // Act
        var harness = await PublishAsync(provider, new DocumentUploaded { Id = file.Id!.Value });
        // note the consumer will not execute until we this completes
        var consumed = await harness.Consumed.Any<DocumentUploaded>();
        
        // Assert
        Assert.True(consumed);

        comsService.VerifyGetFile(file.Id!.Value);

        clamClient.VerifyVirusScan(file.Data);

        // verify UpdateFile meta data
    }

    [Fact(Skip = skip)]
    public async Task TestVirusScanDocumentConsumer_ConfirmScanResultInfected()
    {
        Mock<IClamClient> virusScanClient = new();
        Mock<IWorkflowDocumentService> comsService = new();

        // Arrange
        var file = CreateFile();

        comsService
            .SetupGetFileWithAnyParameters()
            .Returns(Task.FromResult(file));

        virusScanClient
            .SendAndScanFileWithAnyParameters() //  
            .Returns(Task.FromResult(new ClamScanResult("cryptolocker found")));

        // comsService.UpdateFileAsync ...

        await using var provider = GetServiceProvider(virusScanClient.Object, comsService.Object);

        // Act
        var harness = await PublishAsync(provider, new DocumentUploaded { Id = file.Id!.Value });

        // note the consumer will not execute until we this completes
        var consumed = await harness.Consumed.Any<DocumentUploaded>();

        // Assert
        Assert.True(consumed);

        comsService.VerifyGetFile(file.Id!.Value);

        virusScanClient.VerifyVirusScan(file.Data);

        // verify UpdateFile meta data

    }

    [Fact(Skip = skip)]
    public async Task TestVirusScanDocumentConsumer_ConfirmScanResultUnknown()
    {
        Mock<IClamClient> virusScanClient = new();
        Mock<IWorkflowDocumentService> comsService = new();

        // Arrange
        var file = CreateFile();

        comsService
            .SetupGetFileWithAnyParameters()
            .Returns(Task.FromResult(file));

        virusScanClient
            .SendAndScanFileWithAnyParameters()
            .Returns(Task.FromResult(new ClamScanResult("error")));

        await using var provider = GetServiceProvider(virusScanClient.Object, comsService.Object);

        // Act
        var harness = await PublishAsync(provider, new DocumentUploaded { Id = file.Id!.Value });
        // note the consumer will not execute until we this completes
        var consumed = await harness.Consumed.Any<DocumentUploaded>();

        // Assert
        Assert.True(consumed);

        comsService.VerifyGetFile(file.Id!.Value);

        virusScanClient.VerifyVirusScan(file.Data);

        // verify UpdateFile meta data
    }

    /// <summary>
    /// Gets the ServiceProvider with all the registered services 
    /// </summary>
    /// <param name="clamClient"></param>
    /// <param name="comsService"></param>
    /// <returns></returns>
    private static ServiceProvider GetServiceProvider(IClamClient clamClient, IWorkflowDocumentService comsService)
    {
        return new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ScanUploadedDocumentForVirusesConsumer>();
                cfg.UsingInMemory((context, inMemoryConfig) =>
                {
                    inMemoryConfig.ConfigureEndpoints(context);
                });
            })
            .AddScoped(sp => comsService)
            .AddScoped(sp => clamClient)
            .AddScoped(sp => Mock.Of<ILogger<ScanUploadedDocumentForVirusesConsumer>>())
            .BuildServiceProvider(true);
    }

    /// <summary>
    /// Gets and starts the harness and published the message.
    /// </summary>
    private async Task<ITestHarness> PublishAsync<T>(ServiceProvider provider, T message) where T : class
    {
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        await harness.Bus.Publish(message);

        return harness;
    }

    /// <summary>
    /// Creates a random file
    /// </summary>
    private Coms.Client.File CreateFile()
    {
        Stream stream = new MemoryStream(Guid.NewGuid().ToByteArray());
        return new Coms.Client.File (Guid.NewGuid(), stream, "sample_file", null, null, null);
    }
}

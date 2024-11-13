using Moq;
using nClam;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrafficCourts.Workflow.Service.Services;

namespace TrafficCourts.Test;

public static class MockExtensions
{
    public static Moq.Language.Flow.ISetup<IClamClient, Task<ClamScanResult>> SendAndScanFileWithAnyParameters(this Mock<IClamClient> mock)
    {
        return mock.Setup(_ => _.SendAndScanFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()));
    }

    public static Moq.Language.Flow.ISetup<IWorkflowDocumentService, Task<Coms.Client.File>> SetupGetFileWithAnyParameters(this Mock<IWorkflowDocumentService> mock)
    {
        return mock.Setup(_ => _.GetFileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
    }


    /// <summary>
    /// Verify ICitizenDocumentService.GetFileAsync was called once with the specified id
    /// </summary>
    public static void VerifyGetFile(this Mock<IWorkflowDocumentService> mock, Guid id)
    {
        mock.Verify(_ => _.GetFileAsync(
            It.Is<Guid>((actual) => actual == id),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    public static void VerifySaveDocumentProperties(this Mock<IWorkflowDocumentService> mock, Times times)
    {
        mock.Verify(_ => _.SaveDocumentPropertiesAsync(
            It.IsAny<Guid>(),
            It.IsAny<Domain.Models.DocumentProperties>(),
            It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyVirusScan(this Mock<IClamClient> mock, Times times)
    {
        mock.Verify(_ => _.SendAndScanFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyVirusScan(this Mock<IClamClient> mock, Stream stream)
    {
        mock.Verify(_ => _.SendAndScanFileAsync(
            It.Is<Stream>(actual => actual == stream),
            It.IsAny<CancellationToken>()), Times.Once());
    }
}

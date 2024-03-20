using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrafficCourts.Citizen.Service.Features.Tickets;
using TrafficCourts.Citizen.Service.Services;
using TrafficCourts.Citizen.Service.Validators;
using TrafficCourts.Domain.Models;
using Xunit;

namespace TrafficCourts.Test.Citizen.Service.Features.Tickets;

public class AnalyseHandlerTests
{
    [Fact]
    public async void TestHandleReturnsResponse()
    {
        // Arrange
        var mockValidator = new Mock<IFormRecognizerValidator>();
        var mockLogger = new Mock<ILogger<AnalyseHandler.Handler>>();
        var mockRedisCacheService = new Mock<IRedisCacheService>();

        // calls AnalyzeImageAsync
        OcrViolationTicket expectedOcrViolationTicket = new();
        var mockService = new Mock<IFormRecognizerService>(MockBehavior.Strict);
        mockService.Setup(_ => _.AnalyzeImageAsync(It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(expectedOcrViolationTicket));

        // setup the mock IFilePersistenceService
        string expectedFilename = Guid.NewGuid().ToString("n") + ".jpg";

        var handler = new AnalyseHandler.Handler(
            mockService.Object, 
            mockValidator.Object, 
            new SimpleMemoryStreamManager(),
            mockRedisCacheService.Object,
            mockLogger.Object);

        var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
        IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

        var request = new AnalyseHandler.AnalyseRequest(file);

        // Act
        AnalyseHandler.AnalyseResponse response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedOcrViolationTicket, response.OcrViolationTicket);
        Assert.NotEqual(expectedFilename, response.OcrViolationTicket.ImageFilename);
    }
}

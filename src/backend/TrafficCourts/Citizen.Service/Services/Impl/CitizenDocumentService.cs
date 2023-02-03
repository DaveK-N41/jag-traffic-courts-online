﻿using MassTransit;
using TrafficCourts.Common.Models;
using TrafficCourts.Coms.Client;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Citizen.Service.Services.Impl;

/// <summary>
/// A service for file operations utilizing common object management service client
/// </summary>
public class CitizenDocumentService : ICitizenDocumentService
{
    private readonly IObjectManagementService _objectManagementService;
    private readonly IMemoryStreamManager _memoryStreamManager;
    private readonly IBus _bus;
    private readonly ILogger<CitizenDocumentService> _logger;

    public CitizenDocumentService(
        IObjectManagementService objectManagementService,
        IMemoryStreamManager memoryStreamManager,
        IBus bus,
        ILogger<CitizenDocumentService> logger)
    {
        _objectManagementService = objectManagementService ?? throw new ArgumentNullException(nameof(objectManagementService));
        _memoryStreamManager = memoryStreamManager ?? throw new ArgumentNullException(nameof(memoryStreamManager));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting the file through COMS");

        // find the file so we can get the ticket number and notice of dispute id
        FileSearchParameters searchParameters = new(fileId);

        IList<FileSearchResult> searchResults = await _objectManagementService.FileSearchAsync(searchParameters, cancellationToken);

        if (searchResults.Count == 0)
        {
            return; // file not found
        }

        FileSearchResult file = searchResults[0];

        file.Metadata.TryGetValue("notice-of-dispute-id", out string? noticeOfDisputeId);
        if (string.IsNullOrEmpty(noticeOfDisputeId))
        {
            _logger.LogDebug("notice-of-dispute-id value from metadata is empty. Cannot delete the file since it was not uploaded by a disputant");
            return;
        }

        file.Metadata.TryGetValue("ticket-number", out string? ticketNumber);
        if (string.IsNullOrEmpty(ticketNumber))
        {
            ticketNumber = "unknown";
            _logger.LogDebug("ticket-number value from metadata is empty");
        }

        await _objectManagementService.DeleteFileAsync(fileId, cancellationToken);

        // Save file delete event to file history
        SaveFileHistoryRecord fileHistoryRecord = new();
        fileHistoryRecord.TicketNumber = ticketNumber;
        fileHistoryRecord.Description = $"File: {file.FileName} was deleted by the Disputant.";
        await _bus.PublishWithLog(_logger, fileHistoryRecord, cancellationToken);
    }

    public async Task<Coms.Client.File> GetFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting the file through COMS");

        Coms.Client.File comsFile = await _objectManagementService.GetFileAsync(fileId, false, cancellationToken);

        Dictionary<string, string> metadata = comsFile.Metadata;

        if (!metadata.ContainsKey("virus-scan-status"))
        {
            _logger.LogError("Could not download the document because metadata does not contain the key: virus-scan-status");
            throw new ObjectManagementServiceException("File could not be downloaded due to the missing metadata key: virus-scan-status");
        }

        metadata.TryGetValue("virus-scan-status", out string? scanStatus);
        if (!string.IsNullOrEmpty(scanStatus) && scanStatus != "clean")
        {
            _logger.LogDebug("Trying to download unscanned or virus detected file");
            throw new ObjectManagementServiceException($"File could not be downloaded due to virus scan status. Virus scan status of the file is {scanStatus}");
        }

        return comsFile;
    }

    public async Task<List<FileMetadata>> GetFilesBySearchAsync(IDictionary<string, string>? metadata, IDictionary<string, string>? tags, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Searching files through COMS");

        FileSearchParameters searchParameters = new(null, metadata, tags);

        IList<FileSearchResult> searchResult = await _objectManagementService.FileSearchAsync(searchParameters, cancellationToken);

        List<FileMetadata> fileData = new();

        foreach (var result in searchResult)
        {
            FileMetadata fileMetadata = new()
            {
                FileId = result.Id,
                FileName = result.FileName
            };

            fileData.Add(fileMetadata);
        }

        return fileData;
    }

    public async Task<Guid> SaveFileAsync(IFormFile file, Dictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Saving file through COMS");

        metadata.Add("staff-review-status", "pending");

        using Coms.Client.File comsFile = new(GetStreamForFile(file), file.FileName, file.ContentType, metadata, null);

        Guid id = await _objectManagementService.CreateFileAsync(comsFile, cancellationToken);

        // Publish a message to virus scan the newly uploaded file
        DocumentUploaded virusScan = new()
        {
            Id = id
        };
        await _bus.PublishWithLog(_logger, virusScan, cancellationToken);

        metadata.TryGetValue("ticket-number", out string? ticketNumber);
        if (string.IsNullOrEmpty(ticketNumber))
        {
            ticketNumber = "unknown";
            _logger.LogDebug("ticket-number value from metadata is empty");
        }

        // Save file upload event to file history
        SaveFileHistoryRecord fileHistoryRecord = new();
        fileHistoryRecord.TicketNumber = ticketNumber;
        fileHistoryRecord.Description = $"File: {file.FileName} was uploaded by the Disputant.";
        await _bus.PublishWithLog(_logger, fileHistoryRecord, cancellationToken);

        return id;
    }

    private MemoryStream GetStreamForFile(IFormFile formFile)
    {
        MemoryStream memoryStream = _memoryStreamManager.GetStream();

        using var fileStream = formFile.OpenReadStream();
        fileStream.CopyTo(memoryStream);

        // Reset position to the beginning of the stream
        memoryStream.Position = 0;

        return memoryStream;
    }
}
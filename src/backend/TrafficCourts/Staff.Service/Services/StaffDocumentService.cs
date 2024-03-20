﻿using MassTransit;
using System.Security.Claims;
using TrafficCourts.Domain.Models;
using TrafficCourts.Coms.Client;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Staff.Service.Services;

/// <summary>
/// A service for file operations utilizing common object management service client
/// </summary>
public class StaffDocumentService : IStaffDocumentService
{
    private readonly IObjectManagementService _objectManagementService;
    private readonly IMemoryStreamManager _memoryStreamManager;
    private readonly IBus _bus;
    private readonly ILogger<StaffDocumentService> _logger;

    public StaffDocumentService(
        IObjectManagementService objectManagementService,
        IMemoryStreamManager memoryStreamManager,
        IBus bus,
        ILogger<StaffDocumentService> logger)
    {
        _objectManagementService = objectManagementService ?? throw new ArgumentNullException(nameof(objectManagementService));
        _memoryStreamManager = memoryStreamManager ?? throw new ArgumentNullException(nameof(memoryStreamManager));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Coms.Client.File> GetFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting the file through COMS");

        Coms.Client.File file = await _objectManagementService.GetFileAsync(fileId, cancellationToken);

        var properties = new Domain.Models.DocumentProperties(file.Metadata, file.Tags);

        if (!properties.VirusScanIsClean)
        {
            string scanStatus = properties.VirusScanStatus;
            _logger.LogInformation("Cannot download file that has not been successfully scanned for viruses, status is {VirusScanStatus}, expected clean.", scanStatus);
            throw new ObjectManagementServiceException($"File could not be downloaded due to virus scan status. Virus scan status of the file is {scanStatus}");
        }

        return file;
    }

    public async Task DeleteFileAsync(Guid fileId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting the file through COMS");

        // find the file so we can get the ticket number
        FileSearchParameters searchParameters = new FileSearchParameters(fileId);

        IList<FileSearchResult> searchResults = await _objectManagementService.FileSearchAsync(searchParameters, cancellationToken);

        if (searchResults.Count == 0)
        {
            _logger.LogInformation("File {FileId} not found", fileId);
            return; // file not found
        }

        FileSearchResult file = searchResults[0];

        var properties = new Domain.Models.DocumentProperties(file.Metadata, file.Tags);

        await _objectManagementService.DeleteFileAsync(fileId, cancellationToken);

        // Save file delete event to file history
        SaveFileHistoryRecord fileHistoryRecord = new SaveFileHistoryRecord
        {
            NoticeOfDisputeId = properties?.NoticeOfDisputeId?.ToString("d"),
            AuditLogEntryType = FileHistoryAuditLogEntryType.FDLS,
            ActionByApplicationUser = GetUserName(user)
        };

        await _bus.PublishWithLog(_logger, fileHistoryRecord, cancellationToken);
    }

    public async Task<List<TrafficCourts.Domain.Models.FileMetadata>> FindFilesAsync(Domain.Models.DocumentProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Searching files through COMS");

        var metadata = properties.ToMetadata();
        var tags = properties.ToTags();
        FileSearchParameters searchParameters = new(null, metadata, tags);

        IList<FileSearchResult> searchResult = await _objectManagementService.FileSearchAsync(searchParameters, cancellationToken);

        List<TrafficCourts.Domain.Models.FileMetadata> fileData = new();

        foreach (var result in searchResult)
        {
            properties = new Domain.Models.DocumentProperties(result.Metadata, result.Tags);

            TrafficCourts.Domain.Models.FileMetadata fileMetadata = new()
            {
                FileId = result.Id,
                //FileName = result.FileName, // this is always null;
                FileName = properties.DocumentName,
                DocumentType = properties.DocumentType,
                DocumentSource = properties.DocumentSource,
                DocumentStatus = properties.StaffReviewStatus,
                NoticeOfDisputeGuid = properties.NoticeOfDisputeId?.ToString("d"),
                VirusScanStatus = properties.VirusScanStatus,
                DisputeId = properties.TcoDisputeId
            };

            fileData.Add(fileMetadata);
        }

        return fileData;
    }

    public async Task<Guid> SaveFileAsync(IFormFile file, Domain.Models.DocumentProperties properties, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Saving file through COMS");

        properties.DocumentSource = TrafficCourts.Domain.Models.DocumentSource.Staff;

        // get the tags
        var metadata = properties.ToMetadata();
        var tags = properties.ToTags();

        using Coms.Client.File comsFile = new(GetStreamForFile(file), file.FileName, file.ContentType, metadata, tags);

        Guid id = await _objectManagementService.CreateFileAsync(comsFile, cancellationToken);

        // Publish a message to virus scan the newly uploaded file
        await _bus.PublishWithLog(_logger, new DocumentUploaded { Id = id } , cancellationToken);

        // Save file upload event to file history if valid notice of dispute
        // TODO: this check is not needed once we have proper test data.  Currently in test data we have supertickets with no associated notice of dispute
        // which cause this to error out
        if (properties.NoticeOfDisputeId is not null)
        {
            SaveFileHistoryRecord fileHistoryRecord = new()
            {
                NoticeOfDisputeId = properties.NoticeOfDisputeId?.ToString("d"),
                AuditLogEntryType = FileHistoryAuditLogEntryType.SUPL,
                ActionByApplicationUser = GetUserName(user)
            };

            await _bus.PublishWithLog(_logger, fileHistoryRecord, cancellationToken);
        }

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

    private static string GetUserName(ClaimsPrincipal user)
    {
        string? username = user.Identity?.Name;
        return username ?? string.Empty;
    }
}

﻿using MassTransit;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;
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
        _logger.LogDebug("Deleting the file {FileId} through COMS", fileId);

        // find the file so we can get the ticket number and notice of dispute id
        FileSearchParameters searchParameters = new(fileId);

        IList<FileSearchResult> searchResults = await _objectManagementService.FileSearchAsync(searchParameters, cancellationToken);

        if (searchResults.Count == 0)
        {
            return; // file not found
        }

        // TODO: check approval status before delete

        FileSearchResult file = searchResults[0];

        Domain.Models.DocumentProperties properties = new(file.Metadata, file.Tags);

        // Citizen Portal should only have access to Citizen documents
        if (properties.DocumentSource is not null && properties.DocumentSource is not TrafficCourts.Domain.Models.DocumentSource.Citizen) {
            // Should never happen since this file is not even available in the UI for selection to delete.
            throw new InvalidDataException("Requested file is not a citizen document");
        }

        if (properties.NoticeOfDisputeId is null)
        {
            _logger.LogDebug("notice-of-dispute-id value from metadata is empty. Cannot delete the file since it was not uploaded by a disputant");
            return;
        }

        await _objectManagementService.DeleteFileAsync(fileId, cancellationToken);

        // Save file delete event to file history
        SaveFileHistoryRecord fileHistoryRecord = new()
        {
            NoticeOfDisputeId = properties.NoticeOfDisputeId.Value.ToString("d"), // dashes
            ActionByApplicationUser = "Disputant",
            AuditLogEntryType = FileHistoryAuditLogEntryType.FDLD
        };

        await _bus.PublishWithLog(_logger, fileHistoryRecord, cancellationToken);
    }

    public async Task<Coms.Client.File> GetFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting the file {FileId} through COMS", fileId);

        Coms.Client.File file = await _objectManagementService.GetFileAsync(fileId, cancellationToken);

        var properties = new Domain.Models.DocumentProperties(file.Metadata, file.Tags);

        // Citizen Portal should only have access to Citizen documents
        if (properties.DocumentSource is not null && properties.DocumentSource is not TrafficCourts.Domain.Models.DocumentSource.Citizen) {
            // Should never happen since this file is not even available in the UI for selection.
            throw new InvalidDataException("Requested file is not a citizen document");
        }

        if (!properties.VirusScanIsClean)
        {
            string scanStatus = properties.VirusScanStatus;
            _logger.LogDebug("Trying to download unscanned or virus detected file {FileId}", fileId);
            throw new ObjectManagementServiceException($"File could not be downloaded due to virus scan status. Virus scan status of the file is {scanStatus}");
        }

        return file;
    }

    public async Task<List<Domain.Models.FileMetadata>> FindFilesAsync(Domain.Models.DocumentProperties properties, CancellationToken cancellationToken)
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

            if (properties.DocumentSource is not TrafficCourts.Domain.Models.DocumentSource.Staff) {
                TrafficCourts.Domain.Models.FileMetadata fileMetadata = new()
                {
                    FileId = result.Id,
                    //FileName = result.FileName, // this is always null;
                    FileName = properties.DocumentName,
                    DocumentType = properties.DocumentType,
                    DocumentSource = properties.DocumentSource,
                    NoticeOfDisputeGuid = properties.NoticeOfDisputeId?.ToString("d"),
                    VirusScanStatus = properties.VirusScanStatus,
                    DocumentStatus = properties.StaffReviewStatus,
                };

                fileData.Add(fileMetadata);
            }
        }

        return fileData;
    }

    public async Task<Guid> SaveFileAsync(string base64FileString, string fileName, Domain.Models.DocumentProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Saving file through COMS");

        properties.DocumentSource = TrafficCourts.Domain.Models.DocumentSource.Citizen;
        properties.StaffReviewStatus = DisputeUpdateRequestStatus.PENDING.ToString();

        var metadata = properties.ToMetadata();
        var tags = properties.ToTags();

        // JavaScript - URL.createObjectURL will include the file type in the base64 string and separate it with a comma
        string[] fileStringSplit = base64FileString.Split(",");
        byte[] bytes = Convert.FromBase64String(fileStringSplit.Last());
        MemoryStream stream = new MemoryStream(bytes);
        string contentType = GetStringBetween(base64FileString, "data:", ";base64");
        using Coms.Client.File comsFile = new(stream, fileName, contentType, metadata, tags);

        Guid id = await _objectManagementService.CreateFileAsync(comsFile, cancellationToken);

        // Publish a message to virus scan the newly uploaded file
        await _bus.PublishWithLog(_logger, new DocumentUploaded { Id = id }, cancellationToken);

        if (properties.NoticeOfDisputeId is null)
        {
            _logger.LogDebug("notice-of-dispute-id value from metadata is empty. Could not save document upload event to File History");
        } 
        else
        {
            // Save file upload event to file history
            SaveFileHistoryRecord fileHistoryRecord = new()
            {
                NoticeOfDisputeId = properties.NoticeOfDisputeId.Value.ToString("d"),
                ActionByApplicationUser = "Disputant",
                AuditLogEntryType = FileHistoryAuditLogEntryType.FUPD
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

    private string GetStringBetween(string input, string startString, string endString)
    {
        int startIndex = input.IndexOf(startString) + startString.Length;
        int endIndex = input.IndexOf(endString, startIndex);

        if (startIndex < 0 || endIndex < 0 || endIndex < startIndex)
        {
            return string.Empty;
        }

        return input.Substring(startIndex, endIndex - startIndex);
    }
}

﻿using Microsoft.Extensions.Logging;
using System.Linq;

namespace TrafficCourts.Coms.Client;

internal class ObjectManagementService : IObjectManagementService
{
    private readonly IObjectManagementClient _client;
    private readonly IMemoryStreamFactory _memoryStreamFactory;
    private readonly ILogger<ObjectManagementService> _logger;

    public ObjectManagementService(IObjectManagementClient client, IMemoryStreamFactory memoryStreamFactory, ILogger<ObjectManagementService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _memoryStreamFactory = memoryStreamFactory ?? throw new ArgumentNullException(nameof(memoryStreamFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a file. On successful creation, the file's <see cref="File.Id"/> will be set.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ObjectManagementServiceException"></exception>
    public async Task<Guid> CreateFileAsync(File file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Data is null)
        {
            throw new ArgumentException("Data is required for creating file", nameof(file));
        }

        _logger.LogDebug("Creating file");

        MetadataValidator.Validate(file.Metadata);
        TagValidator.Validate(file.Tags);

        FileParameter parameter = new(file.Data, file.FileName, file.ContentType);

        try
        {
            var created = await _client.CreateObjectsAsync(file.Metadata, file.Tags, parameter, cancellationToken)
                .ConfigureAwait(false);

            if (created.Count == 0)
            {
                _logger.LogError("Creating file did not return any created objects");
                throw new ObjectManagementServiceException("Could not create file");
            }

            if (created.Count != 1)
            {
                _logger.LogWarning("{Count} created objects were returned, expecting only 1. Returning the first object id.", created.Count);
            }

            Guid id = created[0].Id;
            file.Id = id;
            _logger.LogDebug("Created file {FileId}", id);
            return id;
        }
        catch (Exception exception)
        {
            throw ExceptionHandler("creating file", exception);
        }
    }

    public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting file {FileId}", id);

        try
        {
            ResponseObjectDeleted response = await _client.DeleteObjectAsync(id, versionId: null, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("File deleted {FileId}", id);
        }
        catch (ApiException<ResponseError> exception) when (exception.Result.Detail == "NotFoundError")
        {
            // it is ok if the specific file not found
            _logger.LogDebug("File not found {FileId}", id);
        }
        catch (Exception exception)
        {
            // if the file does not exist, this could return 502 Bad Gateway error
            // see https://github.com/bcgov/common-object-management-service/issues/89
            throw ExceptionHandler("deleting file", exception);
        }
    }

    public async Task<File> GetFileAsync(Guid id, bool includeTags, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting file {FileId}", id);

        try
        {
            FileResponse response = await _client.ReadObjectAsync(id, DownloadMode.Proxy, expiresIn: null, versionId: null, cancellationToken)
                .ConfigureAwait(false);

            string? contentType = GetHeader(response.Headers, "Content-Type");
            string? fileName = GetHeader(response.Headers, "x-amz-meta-name");

            var metadataValues = await GetMetadataAsync(id, cancellationToken);
            IDictionary<string, string>? metadata = Factory.CreateMetadata(metadataValues[id]);

            IDictionary<string, string>? tags = null;

            if (includeTags)
            {
                tags = await GetTagsAsync(id, cancellationToken);
            }

            // make a copy of the stream because the FileResponse will dispose of the stream
            var stream = _memoryStreamFactory.GetStream();
            await response.Stream.CopyToAsync(stream, cancellationToken);

            var file = new File(id, stream, fileName, contentType, metadata, tags);
            return file;
        }
        catch (Exception exception)
        {
            throw ExceptionHandler("getting file", exception);
        }
    }

    public async Task<List<FileSearchResult>> FileSearchAsync(FileSearchParameters parameters, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        _logger.LogDebug("Searching for files");

        MetadataValidator.Validate(parameters.Metadata);
        TagValidator.Validate(parameters.Tags);

        try
        {
            List<DBObject> files = await _client.SearchObjectsAsync(
                parameters.Metadata,
                parameters.Ids,
                parameters.Path,
                parameters.Active,
                parameters.Public,
                parameters.MimeType,
                parameters.Name,
                parameters.Tags,
                cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Found {Count} files", files.Count);

            // fetch all of the metadata for found files at once
            var foundIds = files.Select(_ => _.Id).ToList();
            var objectMetadataById = await GetMetadataAsync(foundIds, cancellationToken);

            List<FileSearchResult> results = new(files.Count);

            foreach (var file in files)
            {
                var metadata = new Dictionary<string, string>(objectMetadataById[file.Id]);
                var tags = await GetTagsAsync(file.Id, cancellationToken);

                var result = new FileSearchResult(
                    file.Id,
                    file.Path,
                    file.Active,
                    file.Public,
                    file.CreatedBy,
                    file.CreatedAt,
                    file.UpdatedBy ?? Guid.Empty,
                    file.UpdatedAt,
                    metadata,
                    tags);

                results.Add(result);
            }

            return results;
        }
        catch (Exception exception)
        {
            throw ExceptionHandler("searching files", exception);
        }
    }

    public async Task UpdateFileAsync(Guid id, File file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        
        if (file.Data is null)
        {
            throw new ArgumentException("Data is required for updating a file", nameof(file));
        }

        _logger.LogDebug("Updating file");


        MetadataValidator.Validate(file.Metadata);
        TagValidator.Validate(file.Tags);

        FileParameter parameter = new(file.Data, file.FileName, file.ContentType);

        try
        {
            await _client.UpdateObjectAsync(file.Metadata, id, file.Tags, parameter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw ExceptionHandler("updating file", exception);
        }
    }

    private static string? GetHeader(IReadOnlyDictionary<string, IEnumerable<string>> headers, string name)
    {
        string? value = null;
        if (headers.TryGetValue(name, out IEnumerable<string>? values))
        {
            if (values is not null)
            {
                value = values.FirstOrDefault();
            }
        }

        return value;
    }

    private async Task<ILookup<Guid, KeyValuePair<string, string>>> GetMetadataAsync(IList<Guid> ids, CancellationToken cancellationToken)
    {
        try
        {
            IList<ObjectMetadata> objectsMetadata = await _client.GetObjectMetadataAsync(ids, cancellationToken).ConfigureAwait(false);
            
            // not great we have to flatten out the data, but it makes using a ILookup easier to process on the caller
            var lookup = Flatten(objectsMetadata).ToLookup(_ => _.Item1, _ => _.Item2);
            return lookup;
        }
        catch (Exception exception)
        {
            throw ExceptionHandler("fetch metadata", exception);
        }
    }

    private async Task<ILookup<Guid, KeyValuePair<string, string>>> GetMetadataAsync(Guid id, CancellationToken cancellationToken)
    {
        return await GetMetadataAsync(new[] { id }, cancellationToken).ConfigureAwait(false);
    }

    private IEnumerable<Tuple<Guid, KeyValuePair<string, string>>> Flatten(IList<ObjectMetadata> items)
    {
        foreach (var objectItem in items)
        {
            foreach (var metaDataItem in objectItem.Metadata)
            {
                // do not return internal metadata
                if (!IsInternalMetadata(metaDataItem.Key))
                {
                    yield return Tuple.Create(objectItem.Id, new KeyValuePair<string, string>(metaDataItem.Key, metaDataItem.Value));
                }
            }
        }
    }

    private static bool IsInternalMetadata(string key) => key == "id" || key == "name";

    private Task<IDictionary<string, string>> GetTagsAsync(Guid id, CancellationToken cancellationToken)
    {
        // TODO: need to determine how to get tags, see: https://github.com/bcgov/common-object-management-service/issues/93
        // May need to query the database directly in the interim
        IDictionary<string, string> tags = Factory.CreateTags();
        return Task.FromResult(tags);
    }

    private Exception ExceptionHandler(string operation, Exception exception)
    {
        if (exception is ApiException)
        {
            _logger.LogError(exception, "API error {operation}", operation);
            return new ObjectManagementServiceException($"API error {operation}", exception);
        }

        _logger.LogError(exception, "Other error {operation}", operation);
        return new ObjectManagementServiceException($"Other error {operation}", exception);
    }
}

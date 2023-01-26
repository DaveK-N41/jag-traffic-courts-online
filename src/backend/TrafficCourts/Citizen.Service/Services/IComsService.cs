﻿using TrafficCourts.Coms.Client;

namespace TrafficCourts.Citizen.Service.Services;

public interface IComsService
{
    /// <summary>
    /// Saves the given file object with optional content type and metadata to object store through COMS service
    /// </summary>
    /// <param name="file"></param>
    /// <param name="metadata"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Id of newly inserted file to the object storage</returns>
    /// <exception cref="ArgumentNullException"><paramref name="file"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="file"/> has a null data property.</exception>
    /// <exception cref="MetadataInvalidKeyException">A key contains an invalid character</exception>
    /// <exception cref="MetadataTooLongException">The total length of the metadata is too long</exception>
    /// <exception cref="TagKeyTooLongException"></exception>
    /// <exception cref="TagValueTooLongException"></exception>
    /// <exception cref="TooManyTagsException"></exception>
    /// <exception cref="ObjectManagementServiceException">Other error.</exception>
    Task<Guid> SaveFileAsync(IFormFile file, Dictionary<string, string> metadata, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified file through COMS service for the given unique file ID
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ObjectManagementServiceException">Unable to delete the file through COMS</exception>
    Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a dictionary of IDs and file names of the documents found in object storage through COMS service based on the search parameters provided
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="tags"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException">Parameters is null.</exception>
    /// <exception cref="MetadataInvalidKeyException">An invalid metadata key was supplied.</exception>
    /// <exception cref="MetadataTooLongException">The total length of the metadata was too long.</exception>
    /// <exception cref="TooManyTagsException">Too many tags were supplied. Only 10 tags are allowed.</exception>
    /// <exception cref="TagKeyTooLongException">A tag key was too long. Maximum length of a tag key is 128.</exception>
    /// <exception cref="TagValueTooLongException">A tag value was too long. Maximum length of a tag value is 256.</exception>
    /// <exception cref="ObjectManagementServiceException">There was an error searching files in COMS</exception>
    /// <returns></returns>
    Task<Dictionary<Guid, string>> GetFilesBySearchAsync(IDictionary<string, string>? metadata, IDictionary<string, string>? tags, CancellationToken cancellationToken);
}
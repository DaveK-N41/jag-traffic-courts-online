using MassTransit;
using nClam;
using System.Diagnostics;
using TrafficCourts.Coms.Client;
using TrafficCourts.Messaging.MessageContracts;
using TrafficCourts.Workflow.Service.Services;

namespace TrafficCourts.Workflow.Service.Consumers
{
    /// <summary>
    /// Consumer for DocumentUploaded message.
    /// </summary>
    public class ScanUploadedDocumentForVirusesConsumer : IConsumer<DocumentUploaded>
    {
        private readonly IWorkflowDocumentService _documentService;
        private readonly IClamClient _clamClient;
        private readonly ILogger<ScanUploadedDocumentForVirusesConsumer> _logger;


        public ScanUploadedDocumentForVirusesConsumer(
            IWorkflowDocumentService comsService,
            IClamClient clamClient,
            ILogger<ScanUploadedDocumentForVirusesConsumer> logger)
        {
            _documentService = comsService ?? throw new ArgumentNullException(nameof(comsService));
            _clamClient = clamClient ?? throw new ArgumentNullException(nameof(clamClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<DocumentUploaded> context)
        {
            using var scope = _logger.BeginConsumeScope(context);

            _logger.LogDebug("Consuming VirusScanDocument message");
            var cancellationToken = context.CancellationToken;

            Guid documentId = context.Message.Id;

            // get the file
            Coms.Client.File file = await GetFileAsync(documentId, cancellationToken);
            Debug.Assert(file.Id == documentId);

            // scan the file for viruses
            ClamScanResult scanResult = await _clamClient.SendAndScanFileAsync(file.Data, cancellationToken);

            // check if we should update the meta data
            var newProperties = GetUpdatedDocumentProperties(scanResult, file);
            if (newProperties is null)
            {
                return; // nothing changed
            }

            // Update document's metadata
            await SaveFilePropertiesAsync(documentId, newProperties, cancellationToken);
        }

        private async Task<Coms.Client.File> GetFileAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                // Get the file to scan from COMS
                Coms.Client.File file = await _documentService.GetFileAsync(id, cancellationToken);
                return file;
            }
            catch (ObjectManagementServiceException exception)
            {
                _logger.LogError(exception, "Error retrieving file from document service");
                throw new DocumentVirusScanConsumerException("Error retrieving file from document service", exception);
            }
        }

        /// <summary>
        /// Gets the updated document properties or null of nothing was changed.
        /// </summary>
        /// <param name="scanResult"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private Domain.Models.DocumentProperties? GetUpdatedDocumentProperties(ClamScanResult scanResult, Coms.Client.File file)
        {
            var properties = new Domain.Models.DocumentProperties(file.Metadata, file.Tags);

            if (scanResult.Result == ClamScanResults.Clean)
            {
                _logger.LogDebug("No viruses detected as a result of the scan");
                properties.SetVirusScanNotInfected();
                return properties;
            }
            else if (scanResult.Result == ClamScanResults.VirusDetected)
            {
                string virusName = scanResult.InfectedFiles[0].VirusName;
                _logger.LogDebug("The document with id {documentId} is infected with virus {virusName}", file.Id, virusName);
                // Virus detected so add "infected" as virus-scan-status metadata as well as the virus name to the document
                properties.SetVirusScanInfected(virusName);
                return properties;
            }
            else if (scanResult.Result == ClamScanResults.Error)
            {
                _logger.LogDebug("Could not determine the virus status of the document");
                properties.SetVirusScanError();
                return properties;
            }

            // unknown status
            _logger.LogWarning("Unknown virus scan status {Status}, file metadata will not be updated.", scanResult.Result);
            return null; // did not update metadata
        }

        private async Task SaveFilePropertiesAsync(Guid id, Domain.Models.DocumentProperties properties, CancellationToken cancellationToken)
        {
            try
            {
                await _documentService.SaveDocumentPropertiesAsync(id, properties, cancellationToken);
            }
            catch (ObjectManagementServiceException exception)
            {
                _logger.LogError(exception, "Error saving file meta data in document service");
                throw new DocumentVirusScanConsumerException("Error saving file meta data in document service", exception);
            }
        }
    }
}

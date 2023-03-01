﻿using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;

namespace TrafficCourts.Staff.Service.Services;

/// <summary>
/// Summary description for FileHistoryService
/// </summary>
public class FileHistoryService : IFileHistoryService
{
    private readonly IOracleDataApiClient _oracleDataApi;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileHistoryService(
        IOracleDataApiClient oracleDataApi,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FileHistoryService> logger)
    {
        _oracleDataApi = oracleDataApi ?? throw new ArgumentNullException(nameof(oracleDataApi));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<ICollection<FileHistory>> GetFileHistoryForTicketAsync(String ticketNumber, CancellationToken cancellationToken)
    {
        return await _oracleDataApi.GetFileHistoryByTicketNumberAsync(ticketNumber, cancellationToken);
    }

    public async Task<long> SaveFileHistoryAsync(FileHistory fileHistory, CancellationToken cancellationToken)
    {
        fileHistory.ActionByApplicationUser = GetUserName();
        return await _oracleDataApi.InsertFileHistoryAsync(fileHistory, cancellationToken);
    }

    private string GetUserName()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        string? username = httpContext?.User.Identity?.Name;
        return username ?? string.Empty;
    }
}

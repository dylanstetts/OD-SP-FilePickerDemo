using System.Text;

namespace ScannerAndPicker.Services;

public interface IApiLogService
{
    Task LogRequestAsync(ApiRequestLog request);
    Task LogResponseAsync(ApiResponseLog response);
}

public class ApiLogService : IApiLogService
{
    private readonly string _logDirectory;
    private readonly string _requestLogPath;
    private readonly string _responseLogPath;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private readonly SemaphoreSlim _responseLock = new(1, 1);
    private readonly ILogger<ApiLogService> _logger;

    public ApiLogService(ILogger<ApiLogService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _logDirectory = Path.Combine(env.ContentRootPath, "Logs");
        
        // Ensure log directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        _requestLogPath = Path.Combine(_logDirectory, "API_request.log");
        _responseLogPath = Path.Combine(_logDirectory, "API_response.log");

        // Initialize CSV files with headers if they don't exist
        InitializeLogFiles();
    }

    private void InitializeLogFiles()
    {
        if (!File.Exists(_requestLogPath))
        {
            var requestHeaders = "Timestamp,SessionId,RequestId,ClientRequestId,Method,Endpoint,Headers\n";
            File.WriteAllText(_requestLogPath, requestHeaders);
            _logger.LogInformation("Created request log file: {Path}", _requestLogPath);
        }

        if (!File.Exists(_responseLogPath))
        {
            var responseHeaders = "Timestamp,SessionId,RequestId,ClientRequestId,RequestTimestamp,Endpoint,Status,StatusText,MsRequestId,SpRequestGuid,XMsClientRequestId,ContentType,Date,Error\n";
            File.WriteAllText(_responseLogPath, responseHeaders);
            _logger.LogInformation("Created response log file: {Path}", _responseLogPath);
        }
    }

    public async Task LogRequestAsync(ApiRequestLog request)
    {
        await _requestLock.WaitAsync();
        try
        {
            var line = new StringBuilder();
            line.Append(EscapeCsvField(request.Timestamp));
            line.Append(',');
            line.Append(EscapeCsvField(request.SessionId));
            line.Append(',');
            line.Append(EscapeCsvField(request.RequestId));
            line.Append(',');
            line.Append(EscapeCsvField(request.ClientRequestId));
            line.Append(',');
            line.Append(EscapeCsvField(request.Method));
            line.Append(',');
            line.Append(EscapeCsvField(request.Endpoint));
            line.Append(',');
            line.Append(EscapeCsvField(request.Headers));
            line.AppendLine();

            await File.AppendAllTextAsync(_requestLogPath, line.ToString());
            _logger.LogDebug("Logged API request: {Method} {Endpoint}", request.Method, request.Endpoint);
        }
        finally
        {
            _requestLock.Release();
        }
    }

    public async Task LogResponseAsync(ApiResponseLog response)
    {
        await _responseLock.WaitAsync();
        try
        {
            var line = new StringBuilder();
            line.Append(EscapeCsvField(response.Timestamp));
            line.Append(',');
            line.Append(EscapeCsvField(response.SessionId));
            line.Append(',');
            line.Append(EscapeCsvField(response.RequestId));
            line.Append(',');
            line.Append(EscapeCsvField(response.ClientRequestId));
            line.Append(',');
            line.Append(EscapeCsvField(response.RequestTimestamp));
            line.Append(',');
            line.Append(EscapeCsvField(response.Endpoint));
            line.Append(',');
            line.Append(response.Status.ToString());
            line.Append(',');
            line.Append(EscapeCsvField(response.StatusText));
            line.Append(',');
            line.Append(EscapeCsvField(response.MsRequestId));
            line.Append(',');
            line.Append(EscapeCsvField(response.SpRequestGuid));
            line.Append(',');
            line.Append(EscapeCsvField(response.XMsClientRequestId));
            line.Append(',');
            line.Append(EscapeCsvField(response.ContentType));
            line.Append(',');
            line.Append(EscapeCsvField(response.Date));
            line.Append(',');
            line.Append(EscapeCsvField(response.Error));
            line.AppendLine();

            await File.AppendAllTextAsync(_responseLogPath, line.ToString());
            _logger.LogDebug("Logged API response: {Status} {Endpoint}", response.Status, response.Endpoint);
        }
        finally
        {
            _responseLock.Release();
        }
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return "\"\"";

        // Escape quotes and wrap in quotes
        return $"\"{field.Replace("\"", "\"\"")}\"";
    }
}

public class ApiRequestLog
{
    public string Timestamp { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public string ClientRequestId { get; set; } = "";
    public string Method { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Headers { get; set; } = "";
}

public class ApiResponseLog
{
    public string Timestamp { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public string ClientRequestId { get; set; } = "";
    public string RequestTimestamp { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public string MsRequestId { get; set; } = "";
    public string SpRequestGuid { get; set; } = "";
    public string XMsClientRequestId { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string Date { get; set; } = "";
    public string Error { get; set; } = "";
}

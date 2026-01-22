using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScannerAndPicker.Services;

namespace ScannerAndPicker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiLogController : ControllerBase
{
    private readonly IApiLogService _logService;
    private readonly ILogger<ApiLogController> _logger;

    public ApiLogController(IApiLogService logService, ILogger<ApiLogController> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    [HttpPost("request")]
    public async Task<IActionResult> LogRequest([FromBody] ApiRequestLog request)
    {
        try
        {
            // Add session context from the authenticated user
            request.SessionId = User.Identity?.Name ?? HttpContext.Session?.Id ?? "anonymous";
            
            await _logService.LogRequestAsync(request);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API request");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("response")]
    public async Task<IActionResult> LogResponse([FromBody] ApiResponseLog response)
    {
        try
        {
            // Add session context from the authenticated user
            response.SessionId = User.Identity?.Name ?? HttpContext.Session?.Id ?? "anonymous";
            
            await _logService.LogResponseAsync(response);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API response");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> LogBatch([FromBody] BatchLogRequest batch)
    {
        try
        {
            var sessionId = User.Identity?.Name ?? HttpContext.Session?.Id ?? "anonymous";
            
            foreach (var request in batch.Requests ?? Array.Empty<ApiRequestLog>())
            {
                request.SessionId = sessionId;
                await _logService.LogRequestAsync(request);
            }

            foreach (var response in batch.Responses ?? Array.Empty<ApiResponseLog>())
            {
                response.SessionId = sessionId;
                await _logService.LogResponseAsync(response);
            }

            return Ok(new { 
                success = true, 
                requestsLogged = batch.Requests?.Length ?? 0,
                responsesLogged = batch.Responses?.Length ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API batch");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}

public class BatchLogRequest
{
    public ApiRequestLog[]? Requests { get; set; }
    public ApiResponseLog[]? Responses { get; set; }
}

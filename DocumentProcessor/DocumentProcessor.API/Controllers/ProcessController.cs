using Microsoft.AspNetCore.Mvc;
using DocumentProcessor.API.Services;
using DocumentProcessor.API.Models;

namespace DocumentProcessor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessController : ControllerBase
    {
        private readonly IProcessService _processService;
        private readonly ILogger<ProcessController> _logger;

        public ProcessController(IProcessService processService, ILogger<ProcessController> logger)
        {
            _processService = processService;
            _logger = logger;
        }

        /// <summary>
        /// Start a new document processing process
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartProcess([FromForm] ProcessStartRequest request)
        {
            try
            {
                if (request == null || request.Files == null || request.Files.Count == 0)
                {
                    return BadRequest(new { Error = "At least one file is required" });
                }

                var result = await _processService.StartProcessAsync(request.Files);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error starting process: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Business rule violation starting process: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error starting process");
                return StatusCode(500, new { Error = "Internal server error occurred while starting process" });
            }
        }

        /// <summary>
        /// Stop a running process
        /// </summary>
        [HttpPost("stop/{processId}")]
        public async Task<IActionResult> StopProcess(Guid processId)
        {
            try
            {
                if (processId == Guid.Empty)
                {
                    return BadRequest(new { Error = "Invalid process ID" });
                }

                var result = await _processService.StopProcessAsync(processId);
                return Ok(new { Message = "Process stopped successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Process not found: {ProcessId}, Error: {Error}", processId, ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot stop process {ProcessId}: {Error}", processId, ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error stopping process {ProcessId}", processId);
                return StatusCode(500, new { Error = "Internal server error occurred while stopping process" });
            }
        }

        /// <summary>
        /// Get process status
        /// </summary>
        [HttpGet("status/{processId}")]
        public async Task<IActionResult> GetProcessStatus(Guid processId)
        {
            try
            {
                if (processId == Guid.Empty)
                {
                    return BadRequest(new { Error = "Invalid process ID" });
                }

                var result = await _processService.GetProcessStatusAsync(processId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Process not found: {ProcessId}, Error: {Error}", processId, ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting process status for {ProcessId}", processId);
                return StatusCode(500, new { Error = "Internal server error occurred while getting process status" });
            }
        }

        /// <summary>
        /// List all processes
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> ListProcesses()
        {
            try
            {
                var result = await _processService.ListProcessesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error listing processes");
                return StatusCode(500, new { Error = "Internal server error occurred while listing processes" });
            }
        }

        /// <summary>
        /// Get process results
        /// </summary>
        [HttpGet("results/{processId}")]
        public async Task<IActionResult> GetProcessResults(Guid processId)
        {
            try
            {
                if (processId == Guid.Empty)
                {
                    return BadRequest(new { Error = "Invalid process ID" });
                }

                var result = await _processService.GetProcessResultsAsync(processId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Process not found: {ProcessId}, Error: {Error}", processId, ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot get results for process {ProcessId}: {Error}", processId, ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting process results for {ProcessId}", processId);
                return StatusCode(500, new { Error = "Internal server error occurred while getting process results" });
            }
        }
    }
}

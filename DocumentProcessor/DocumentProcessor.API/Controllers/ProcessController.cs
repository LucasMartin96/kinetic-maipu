using Microsoft.AspNetCore.Mvc;
using MassTransit;
using DocumentProcessor.Contracts.TestsRecords;

namespace DocumentProcessor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ProcessController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestPublish()
        {
            var processId = Guid.NewGuid();

            await _publishEndpoint.Publish(new ProcessStartedTestEvent(processId));

            return Ok(new { Message = "Event published", ProcessId = processId });
        }
    }
}

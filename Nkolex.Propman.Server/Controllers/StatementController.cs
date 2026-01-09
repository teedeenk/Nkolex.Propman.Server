using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatementController : Controller
    {
        private readonly IProcessCsvFileService _processCsvFileService;
        private readonly ILogger<StatementController> _logger;
        private readonly IUploadCsvService _uploadCsvService;

        public StatementController(IProcessCsvFileService processCsvFileService, ILogger<StatementController> logger, IUploadCsvService uploadCsvService)
        {
            _processCsvFileService = processCsvFileService;
            _logger = logger;
            _uploadCsvService = uploadCsvService;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file upload.");
                }

                using var stream = file.OpenReadStream();
                var statement = await _processCsvFileService.ProcessCsv(stream);

                if (statement == null)
                {
                    return BadRequest("file processing failed.");
                }

                var addStatement = await _uploadCsvService.AddAsync(statement);

                if(addStatement == 0)
                {
                    return StatusCode(400, "Statement add failed because some lines aren't valid, please check dates or discriptions.");
                }

                if(addStatement == 2)
                {
                    return StatusCode(400, "One of the statement lines is possibly a duplicate");
                }

                _logger.LogInformation("Processed {Count} records.", statement.StatementLines.Count);
                return Ok(statement);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error! while attempting to add statement");
                return StatusCode(400, new { message = "Statement add failed, please try again." });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetStatement()
        {
            _logger.LogInformation("Fetching all statement lines... | GetStatement controller" );
            var statement = await _uploadCsvService.GetAllAsync();
            if (statement == null || statement.Count == 0)
            {
                return StatusCode(404, "No statement found...");
            }
            _logger.LogInformation("Returning statement {statement} | GetAllAsync uploadCsvService", statement);
            return Ok(statement);
        }
    }
}
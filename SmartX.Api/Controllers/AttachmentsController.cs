using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Data;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/sensors/{sensorId}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly SensorStore _store;
    private readonly IWebHostEnvironment _env;

    public AttachmentsController(SensorStore store, IWebHostEnvironment env)
    {
        _store = store;
        _env = env;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(string sensorId, IFormFile file)
    {
        var sensor = _store.Get(sensorId);
        if (sensor == null)
        {
            return NotFound("sensor not registered");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("no file was sent");
        }

        var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", sensorId);
        Directory.CreateDirectory(uploadsFolder);

        // strip any folder info from the file name so it cant be used to write
        // somewhere outside the uploads folder
        var safeFileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsFolder, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        sensor.AttachmentFileNames.Add(safeFileName);

        return Ok(new { fileName = safeFileName });
    }

    [HttpGet]
    public ActionResult<List<string>> GetAll(string sensorId)
    {
        var sensor = _store.Get(sensorId);
        if (sensor == null)
        {
            return NotFound();
        }
        return Ok(sensor.AttachmentFileNames);
    }
}

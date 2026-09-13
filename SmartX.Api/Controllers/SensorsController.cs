using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Data;
using SmartX.Shared;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly SensorStore _store;

    public SensorsController(SensorStore store)
    {
        _store = store;
    }

    public class RegisterSensorRequest
    {
        public string MacAddress { get; set; } = "";
        public string Location { get; set; } = "";
        public SensorCategory Category { get; set; }
    }

    [HttpPost]
    public ActionResult<SensorRegistration> Register(RegisterSensorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MacAddress) || string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest("mac address and location are required");
        }

        var sensor = new SensorRegistration
        {
            MacAddress = request.MacAddress,
            Location = request.Location,
            Category = request.Category
        };

        _store.Add(sensor);
        return Ok(sensor);
    }

    [HttpGet]
    public ActionResult<List<SensorRegistration>> GetAll()
    {
        return Ok(_store.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<SensorRegistration> Get(string id)
    {
        var sensor = _store.Get(id);
        if (sensor == null)
        {
            return NotFound();
        }
        return Ok(sensor);
    }
}

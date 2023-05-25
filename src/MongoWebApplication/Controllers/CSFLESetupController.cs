using Microsoft.AspNetCore.Mvc;
using MongoWebApplication.Models;
using MongoWebApplication.Service;

namespace MongoWebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CSFLESetupController : ControllerBase
{
    private readonly PatientsService _patientsService;
    private readonly ILogger<PatientController> _logger;
    public CSFLESetupController(PatientsService patientsService, ILogger<PatientController> logger)
    {
        _patientsService = patientsService;
        _logger = logger;
    }
   
  
   // Use this method to setup CSFLE data encryption key
   //  Warning: Using this controller would wipe out existing encypted collection and keys
  

    [HttpPost]
    public async Task<IActionResult> Post(bool areYouSure)
    {
        if(areYouSure)
            await _patientsService.MakeKeyBasicCSFLE();
        return NoContent();
    }
    
}

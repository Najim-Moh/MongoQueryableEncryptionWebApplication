using Microsoft.AspNetCore.Mvc;
using MongoCSFLEWebApplication.Service;
using MongoWebApplication.Controllers;
using MongoWebApplication.Models;
using MongoWebApplication.Service;


namespace MongoCSFLEWebApplication.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PatientQuerableController : ControllerBase
    {
        private readonly PatientsQuerableService _patientQuerableService;
        private readonly ILogger<PatientQuerableController> _logger;

        public PatientQuerableController(PatientsQuerableService patientQuerableService, ILogger<PatientQuerableController> logger)
        {
            _patientQuerableService = patientQuerableService;
            _logger = logger;
        }
        /*
        [HttpGet]
        public async List<Patient> Get()
        {
            return await _patientQuerableService.GetPatients();

        }

        */
        [HttpPost]
        public async Task<IActionResult> Post(Patient newPatient)
        {
            await _patientQuerableService.CreateAsync(newPatient);

            return CreatedAtAction(nameof(Post), new { id = newPatient.Id }, newPatient);
            //
        }




    }
}

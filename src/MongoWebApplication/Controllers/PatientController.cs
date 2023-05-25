using Microsoft.AspNetCore.Mvc;
using MongoCSFLEWebApplication.Service;
using MongoWebApplication.Models;
using MongoWebApplication.Service;

namespace MongoWebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly PatientsService _patientsService;
    private readonly ILogger<PatientController> _logger;
    public PatientController(PatientsService patientsService, ILogger<PatientController> logger)
    {
        _patientsService = patientsService;
        _logger = logger;
    }

    // [HttpGet]
    // public async Task<List<Patient>> Get() =>
    //     await _patientsService.GetAsync();

    // [HttpGet("{id:length(24)}")]
    // public async Task<ActionResult<Patient>> Get(string id)
    // {
    //     var book = await _patientsService.GetAsync(id);

    //     if (book is null)
    //     {
    //         return NotFound();
    //     }

    //     return book;
    // }

    [HttpPost]
    public async Task<IActionResult> Post(Patient newPatient)
    {
        await _patientsService.CreateAsync(newPatient);
        return CreatedAtAction(nameof(Post), new { id = newPatient.Id }, newPatient);
    }      

    // [HttpPut("{id:length(24)}")]
    // public async Task<IActionResult> Update(string id, Patient updatedBook)
    // {
    //     var book = await _patientsService.GetAsync(id);

    //     if (book is null)
    //     {
    //         return NotFound();
    //     }

    //     updatedBook.Id = book.Id;

    //     await _patientsService.UpdateAsync(id, updatedBook);

    //     return NoContent();
    // }

    // [HttpDelete("{id:length(24)}")]
    // public async Task<IActionResult> Delete(string id)
    // {
    //     var book = await _patientsService.GetAsync(id);

    //     if (book is null)
    //     {
    //         return NotFound();
    //     }

    //     await _patientsService.RemoveAsync(id);

    //     return NoContent();
    // }
}

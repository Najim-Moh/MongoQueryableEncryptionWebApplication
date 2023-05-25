using Microsoft.AspNetCore.Mvc;
using MongoCSFLEWebApplication.Service;
using MongoWebApplication.Models;

namespace MongoCSFLEWebApplication.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class SetupQueryableEncryption : ControllerBase
    {
        private readonly SetupQueryableService _setupQueryableService;
        private readonly ILogger<SetupQueryableEncryption> _logger;

        public SetupQueryableEncryption(SetupQueryableService setupQueryableEncryption, ILogger<SetupQueryableEncryption> logger)
        {
            _setupQueryableService = setupQueryableEncryption;
            _logger = logger;
        }


        // Use this method to create a single key in the keyvault based on your master key, this would be used to encrypt the user data, run this before anything else and run it once
        [HttpPost]
        public async Task<IActionResult> Post(bool areYouSure)
        {

            if (areYouSure)
                await _setupQueryableService.CreateAsync();

            return NoContent();
        }


    }
}
using FoodTruckAppApi.Domain;
using FoodTruckAppApi.DTOs;
using FoodTruckAppApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodTruckAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserDto user)
        {
            
            var result = await _accountService.CreateUser(user);

            // Return appropriate response
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        // POST api/Account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _accountService.AuthenticateUser(loginDto.email_or_username, loginDto.password);

            // Return appropriate response
            if (result.Success)
            {
                return Ok(new { token = result.Data });
            }
            return Unauthorized();
        }

    }
}

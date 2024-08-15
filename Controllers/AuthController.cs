using AuthProductsAPI.Auth;
using AuthProductsAPI.DB;
using AuthProductsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthProductsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string _tokenKey;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _tokenKey = configuration["TokenKey"];
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        try
        {
            var userExists = _context.Users.Any(u => u.Username == user.Username);
            if (userExists) return BadRequest("User already exists!");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(User login)
    {
        try
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Username == login.Username && u.Password == login.Password);

            return user != null
                ? Ok(new { Token = JwtGenerator.GenerateJwt(user, _tokenKey, DateTime.UtcNow.AddMinutes(20)) })
                : Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers;
[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public class AuthenticationRequestBody
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class CityInfoUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }

        public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
        {
            UserId = userId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            City = city;
        }
    }

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
    {
        // Step 1: Validate user info
        var user = ValidateUserCredentials(
            authenticationRequestBody.UserName,
            authenticationRequestBody.Password);

        if( user is null )
        {
            return Unauthorized();
        }

        // Step 2: Create signature
        var securityKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));

        var signingCredentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        // Step 3: Create claims for payload
        var claimsForToken = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("city", user.City)
        };

        // Step 4: Create token
        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            claimsForToken,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(5),
            signingCredentials
         );

        var token = new JwtSecurityTokenHandler()
            .WriteToken(jwtSecurityToken);

        return Ok(token);
    }

    private CityInfoUser ValidateUserCredentials(string? userName, string? password)
    {
        return new CityInfoUser(
            1,
            userName ?? "",
            "Alex",
            "Gama",
            "New York City");
    }
}

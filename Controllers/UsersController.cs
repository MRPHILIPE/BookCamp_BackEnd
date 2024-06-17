using bookcamp.Data;
using bookcamp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace bookcamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BookCampDbContext dbContext;
        private readonly IConfiguration configuration;
        public UsersController(BookCampDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(UserDTO userDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userObj = dbContext.Users.FirstOrDefault(x => x.Email == userDTO.Email);
            if (userObj == null)
            {
                dbContext.Users.Add(new User
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    UserName = userDTO.UserName,
                    Email = userDTO.Email,
                    Password = userDTO.Password
                });
                dbContext.SaveChanges();
                return Ok("User Registered Successfully");
            }
            else
            {
                return BadRequest("User already exists with the same UserName");
            }

        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Email == loginDTO.Email && x.Password == loginDTO.Password);
            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("Email", user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: signIn
                    );
                string tokenvalue = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { Token = tokenvalue, User = user });
            }
            else
            {
                return NoContent();
            }

        }

        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(dbContext.Users.ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("User not found");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("EditUser")]
        public IActionResult EditUser(EditUserDTO editUserDTO)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = editUserDTO.FirstName;
            user.LastName = editUserDTO.LastName;
            user.UserName = editUserDTO.UserName;
            user.Email = editUserDTO.Email;
            dbContext.SaveChanges();

            return Ok("User details updated successfully");
        }

        [Authorize]
        [HttpPost]
        [Route("ChangePassword")]
        public IActionResult ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Password != changePasswordDTO.CurrentPassword)
            {
                return BadRequest("Incorrect current password");
            }

            user.Password = changePasswordDTO.NewPassword;
            dbContext.SaveChanges();

            return Ok("Password changed successfully");
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteUser")]
        public IActionResult DeleteUser()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);
            if (user != null)
            {
                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
                return Ok("User deleted successfully");
            }
            else
            {
                return NotFound("User not found");
            }
        }
    }
}

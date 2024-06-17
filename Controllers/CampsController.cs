using bookcamp.Data;
using bookcamp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace bookcamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly BookCampDbContext dbContext;
        public CampsController(BookCampDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("GetCamps")]
        public IActionResult GetCamps()
        {
            return Ok(dbContext.Camps.ToList());
        }

        [HttpGet]
        [Route("GetCamp/{id}")]
        public IActionResult GetCamp(int id)
        {
            var camp = dbContext.Camps.FirstOrDefault(x => x.CampId == id);
            if (camp == null)
            {
                return NotFound("Camp not found");
            }
            return Ok(camp);
        }

        [Authorize]
        [HttpPost]
        [Route("CreateCamp")]
        public IActionResult CreateCamp(CampDTO campDTO)
        {
            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var camp = new Camp
            {
                Name = campDTO.Name,
                Location = campDTO.Location,
                Description = campDTO.Description,
                Price = campDTO.Price,
                CreatedByEmail = userEmail
            };
            dbContext.Camps.Add(camp);
            dbContext.SaveChanges();
            return Ok("Camp created successfully");
        }

        [Authorize]
        [HttpPut]
        [Route("EditCamp/{id}")]
        public IActionResult EditCamp(int id, CampDTO campDTO)
        {
            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var camp = dbContext.Camps.FirstOrDefault(x => x.CampId == id && x.CreatedByEmail == userEmail);
            if (camp == null)
            {
                return NotFound("Camp not found or you do not have permission to edit this camp");
            }

            camp.Name = campDTO.Name;
            camp.Location = campDTO.Location;
            camp.Description = campDTO.Description;
            camp.Price = campDTO.Price;
            dbContext.SaveChanges();
            return Ok("Camp updated successfully");
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteCamp/{id}")]
        public IActionResult DeleteCamp(int id)
        {
            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var camp = dbContext.Camps.FirstOrDefault(x => x.CampId == id && x.CreatedByEmail == userEmail);
            if (camp == null)
            {
                return NotFound("Camp not found or you do not have permission to delete this camp");
            }

            dbContext.Camps.Remove(camp);
            dbContext.SaveChanges();
            return Ok("Camp deleted successfully");
        }

        [Authorize]
        [HttpPost]
        [Route("BookCamp/{id}")]
        public IActionResult BookCamp(int id)
        {
            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var camp = dbContext.Camps.FirstOrDefault(x => x.CampId == id);
            if (camp == null)
            {
                return NotFound("Camp not found");
            }

            if (camp.IsBooked)
            {
                return BadRequest("Camp is already booked");
            }

            camp.IsBooked = true;
            dbContext.SaveChanges();
            return Ok("Camp booked successfully");
        }

        [Authorize]
        [HttpPost]
        [Route("UnbookCamp/{id}")]
        public IActionResult UnbookCamp(int id)
        {
            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var camp = dbContext.Camps.FirstOrDefault(x => x.CampId == id && x.IsBooked && x.CreatedByEmail == userEmail);
            if (camp == null)
            {
                return NotFound("Camp not found, not booked, or you do not have permission to unbook this camp");
            }

            camp.IsBooked = false;
            dbContext.SaveChanges();
            return Ok("Camp unbooked successfully");
        }
    }
}

using FailTrack.Models;
using FailTrack.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FailTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ToolingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] ToolingDto request)
        {
            if(request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede enviar datos vacios"
                });
            }

            var newItem = new Tooling
            {
                ApplicantName = request.ApplicantName,
                FaultDescription = request.FaultDescription,
                IdLine = request.IdLine,
                IdMachine = request.IdMachine,
                UpdatedAt = null,
                IdStatus = 1
            };

            _context.Tooling.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro exitoso"
            });
        }
    }
}
using FailTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FailTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralListController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GeneralListController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetLines")]
        public async Task<IActionResult> GetLines()
        {
            var lines = await _context.Lines
                .AsNoTracking()
                .ToListAsync();

            if (lines == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sin datos"
                });
            }

            return Ok(lines);
        }

        [HttpGet]
        [Route("GetMachineByLine/{lineId}")]
        public async Task<IActionResult> GetMachineByLine(int lineId)
        {
            var machines = await _context.Machines
                                .Where(m => m.IdLine == lineId)
                                .Select(m => new
                                {
                                    m.Id,
                                    Machine = m.MachineName,
                                    Line = m.IdLineNavigation.LineName
                                })
                                .AsNoTracking()
                                .ToListAsync();

            if (machines == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sin datos"
                });
            }

            return Ok(machines);
        }

        [HttpGet]
        [Route("GetStatus")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _context.Status
                                .AsNoTracking()
                                .Select(s => new
                                {
                                    s.Id,
                                    Status = s.StatusName
                                })
                                .ToListAsync();

            if(status == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sin datos"
                });
            }

            return Ok(status);
        }
    }
}
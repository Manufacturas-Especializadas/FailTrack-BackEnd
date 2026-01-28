using FailTrack.Dtos;
using FailTrack.Hubs;
using FailTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FailTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<FailTrackHub> _hubContext;

        public MaintenanceController(AppDbContext context, IHubContext<FailTrackHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Route("GetMaintenanceList")]
        public async Task<IActionResult> GetMaintenanceList()
        {
            var list = await _context.Maintenance
                            .AsNoTracking()
                            .Select(m => new
                            {
                                m.Id,
                                m.ApplicantName,
                                LineName = m.IdLineNavigation.LineName,
                                MachineName = m.IdMachineNavigation.MachineName,
                                Description = m.FaultDescription ?? "Sin descripción",
                                Status = m.IdStatusNavigation.StatusName,
                                Date = m.CreatedAt
                            })
                            .ToListAsync();

            if(list == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sin datos"
                });
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("GetMaintenanceById/{id}")]
        public async Task<IActionResult> GetMaintenanceById(int id)
        {
            var maintenanceId = await _context.Maintenance
                                .FirstOrDefaultAsync(m => m.Id == id);

            if(maintenanceId == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Id no encontrado"
                });
            }

            return Ok(maintenanceId);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] MaintenanceDto request)
        {
            if(request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede enviar datos vacios"
                });
            }

            var newItem = new Maintenance
            {
                ApplicantName = request.ApplicantName,
                FaultDescription = request.FaultDescription,
                IdLine = request.IdLine,
                IdMachine = request.IdMachine,
                UpdatedAt = null,
                IdStatus = 1
            };

            _context.Maintenance.Add(newItem);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");

            return Ok(new
            {
                success = true,
                message = "Registro existoso"
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update([FromBody] MaintenanceDto request, int id)
        {
            var existingMaintenance = await _context.Maintenance.FindAsync(id);

            if(existingMaintenance == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Id no encontrado"
                });
            }

            existingMaintenance.ApplicantName = request.ApplicantName;
            existingMaintenance.FaultDescription = request.FaultDescription;
            existingMaintenance.IdLine = request.IdLine;
            existingMaintenance.IdMachine = request.IdMachine;
            existingMaintenance.UpdatedAt = DateTime.UtcNow;
            existingMaintenance.IdStatus = request.IdStatus;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");

            return Ok(new
            {
                success = true,
                message = "Registro actualizado"
            });
        }
    }
}
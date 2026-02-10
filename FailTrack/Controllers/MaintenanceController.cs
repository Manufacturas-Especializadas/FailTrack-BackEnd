using ClosedXML.Excel;
using FailTrack.Dtos;
using FailTrack.Hubs;
using FailTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                                Date = m.CreatedAt,
                                ClosingDate = m.ClosingDate
                            })
                            .OrderByDescending(m => m.Id)
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

        [HttpGet]
        [Route("GetAvailableMonthlyReports")]
        public async Task<IActionResult> GetAvailableMonthlyReports()
        {
            try
            {
                var reportGroups = await _context.Maintenance
                        .Where(m => m.CreatedAt.HasValue)
                        .GroupBy(m => new { m.CreatedAt!.Value.Year, m.CreatedAt.Value.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Year)
                        .ThenByDescending(x => x.Month)
                        .ToListAsync();

                var result = reportGroups.Select(r => new
                {
                    r.Year,
                    r.Month,
                    MonthName = CultureInfo.CreateSpecificCulture("ex-Es").DateTimeFormat.GetMonthName(r.Month),
                    RecordCount = r.Count
                });

                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("DownloadMonthlyReport")]
        public async Task<IActionResult> DownloadMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var data = await _context.Maintenance
                            .Include(m => m.IdLineNavigation)
                            .Include(m => m.IdMachineNavigation)
                            .Include(m => m.IdStatusNavigation)
                            .Where(m => m.CreatedAt.HasValue &&
                                        m.CreatedAt.Value.Year == year &&
                                        m.CreatedAt.Value.Month == month)
                            .OrderBy(m => m.CreatedAt)
                            .ToListAsync();

                if (!data.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No hay datos para el periodo seleccionado"
                    });
                }

                using(var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Mantenimiento");

                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Solicitante";
                    worksheet.Cell(1, 3).Value = "Línea";
                    worksheet.Cell(1, 4).Value = "Máquina";
                    worksheet.Cell(1, 5).Value = "Descripción Falla";
                    worksheet.Cell(1, 6).Value = "Estatus";
                    worksheet.Cell(1, 7).Value = "Fecha Creación";
                    worksheet.Cell(1, 8).Value = "Fecha Solución";

                    var headerRange = worksheet.Range(1, 1, 1, 8);

                    headerRange.Style.Font.SetBold();
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0099cc");
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int row = 2;
                    foreach(var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.ApplicantName;
                        worksheet.Cell(row, 3).Value = item.IdLineNavigation?.LineName ?? "N/A";
                        worksheet.Cell(row, 4).Value = item.IdMachineNavigation?.MachineName ?? "N/A";
                        worksheet.Cell(row, 5).Value = item.FaultDescription;

                        var statusCell = worksheet.Cell(row, 6);
                        statusCell.Value = item.IdStatusNavigation?.StatusName ?? "N/A";

                        worksheet.Cell(row, 7).Value = item.CreatedAt;
                        worksheet.Cell(row, 8).Value = item.UpdatedAt.UtcDateTime;

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using(var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        string fileName = $"Reporte_Mantenimiento_{year}_{month:00}.xlsx";

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName
                        );
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al generar excel",
                    error = ex.Message
                });
            }
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

            var now = DateTimeOffset.UtcNow;

            var newItem = new Maintenance
            {
                ApplicantName = request.ApplicantName,
                FaultDescription = request.FaultDescription,
                IdLine = request.IdLine,
                IdMachine = request.IdMachine,
                UpdatedAt = now,
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
            existingMaintenance.Responsible = request.Responsible;
            existingMaintenance.FailureSolution = request.FailureSolution;
            existingMaintenance.IdLine = request.IdLine;
            existingMaintenance.IdMachine = request.IdMachine;
            existingMaintenance.UpdatedAt = DateTime.UtcNow;
            existingMaintenance.IdStatus = request.IdStatus;

            if(existingMaintenance.ClosingDate == null && request.IdStatus == 3)
            {
                existingMaintenance.ClosingDate = DateTime.UtcNow;
            }

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
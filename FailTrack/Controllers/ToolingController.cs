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
    public class ToolingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<FailTrackHub> _hubContext;

        public ToolingController(AppDbContext context, IHubContext<FailTrackHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Route("GetTooling/{id}")]
        public async Task<IActionResult> GetTooling(int id)
        {
            var toolingId = await _context.Tooling.FirstOrDefaultAsync(t => t.Id == id);

            if(toolingId == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Id no encontrado"
                });
            }

            return Ok(toolingId);
        }

        [HttpGet]
        [Route("GetAvailableMonthlyReports")]
        public async Task<IActionResult> GetAvailableMonthlyReports()
        {
            try
            {
                var reportGroups = await _context.Tooling
                            .Where(t => t.CreatedAt.HasValue)
                            .GroupBy(t => new { t.CreatedAt!.Value.Year, t.CreatedAt!.Value.Month })
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
                    MonthName = CultureInfo.CreateSpecificCulture("es-Mx").DateTimeFormat.GetMonthName(r.Month),
                    RecordCount = r.Count
                });

                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DownloadMonthlyReport")]
        public async Task<IActionResult> DownloadMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var data = await _context.Tooling
                                .Include(t => t.IdLineNavigation)
                                .Include(t => t.IdMachineNavigation)
                                .Include(t => t.IdStatusNavigation)
                                .Where(t => t.CreatedAt.HasValue &&
                                                t.CreatedAt.Value.Year == year &&
                                                t.CreatedAt.Value.Month == month)
                                .OrderBy(t => t.CreatedAt)
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
                    var worksheet = workbook.Worksheets.Add("Herramentales");

                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Solicitante";
                    worksheet.Cell(1, 3).Value = "Línea";
                    worksheet.Cell(1, 4).Value = "Máquina";
                    worksheet.Cell(1, 5).Value = "Descripción falla";
                    worksheet.Cell(1, 6).Value = "Estatus";
                    worksheet.Cell(1, 7).Value = "Fecha creación";
                    worksheet.Cell(1, 8).Value = "Fecha de solución";

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
                        worksheet.Cell(row, 8).Value = item.ClosingDate?.LocalDateTime;

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using(var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        string fileName = $"Reporte_Herramentales_{year}_{month:00}.xlsx";

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

        [HttpGet]
        [Route("GetToolingList")]
        public async Task<IActionResult> GetToolingList()
        {
            var list = await _context.Tooling
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

            if (list == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sin datos"
                });
            }

            return Ok(list);
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

            var now = DateTimeOffset.UtcNow;

            var newItem = new Tooling
            {
                ApplicantName = request.ApplicantName,
                FaultDescription = request.FaultDescription,
                IdLine = request.IdLine,
                IdMachine = request.IdMachine,
                UpdatedAt = now,
                IdStatus = 1
            };

            _context.Tooling.Add(newItem);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");

            return Ok(new
            {
                success = true,
                message = "Registro exitoso"
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update([FromBody] ToolingDto request, int id)
        {
            var existingTooling = await _context.Tooling.FindAsync(id);

            if(existingTooling == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Id no encontrado"
                });
            }

            existingTooling.ApplicantName = request.ApplicantName;
            existingTooling.FaultDescription = request.FaultDescription;
            existingTooling.Responsible = request.Responsible;
            existingTooling.FailureSolution = request.FailureSolution;
            existingTooling.IdLine = request.IdLine;
            existingTooling.IdMachine = request.IdMachine;
            existingTooling.UpdatedAt = DateTime.UtcNow;
            existingTooling.IdStatus = request.IdStatus;

            if(existingTooling.ClosingDate == null && request.IdStatus == 3)
            {
                existingTooling.ClosingDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");

            return Ok(new
            {
                success = true,
                message = "Registro actualizado"
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tooling = await _context.Tooling.FindAsync(id);

            if(tooling == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Id no encontrado"
                });
            }

            _context.Tooling.Remove(tooling);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro eliminado"
            });
        }
    }
}
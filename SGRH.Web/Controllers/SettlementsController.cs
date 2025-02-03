using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using SGRH.Web.Services;
using System.Security.Claims;

namespace SGRH.Web.Controllers
{
    [Authorize (Roles = "SupervisorRH, Administrador")]
    public class SettlementsController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly ISettlementService _settlementService;
        private readonly ILayoffsService _layoffsService;
        private readonly IPayrollService _payrollService;

        public SettlementsController(IPersonalActionService personalActionService, ISettlementService settlementService, ILayoffsService layoffsService,IPayrollService payrollService)
        {
            _personalActionService = personalActionService;
            _settlementService = settlementService;
            _layoffsService = layoffsService;
            _payrollService = payrollService;
        }

        [Authorize]
        public async Task<IActionResult>Index()
        {
            ViewBag.Titulo = "Gestión de Liquidaciones";
            ViewBag.NombreUbicacion = "Lista de Liquidaciones";
            var settlements = await _settlementService.GetSettlements();
            return View(settlements);
        }


        public async Task<IActionResult> CreateSettlement(int layoffId)
        {

            var layoff = await _layoffsService.GetLayoffById(layoffId);

            if (layoff == null || layoff.PersonalAction == null || layoff.PersonalAction.User == null)
            {
                TempData["ErrorMessage"] = "No se logró encontrar el usuario para crear la liquidación";
                return View("Index");
            }

            var model = new CreateSettlementViewModel
            {
                LayoffId = layoffId,
                Layoff = layoff,
                UnenjoyedVacation = layoff.PersonalAction.User.VacationDays
            };

            model.ShowCalculatedlEntry = await _payrollService.HasEnoughDataForAvgSalaryCalculation(layoff.PersonalAction.User.Id);

            if (!model.ShowCalculatedlEntry)
            {

                // Calcular el promedio de los últimos 6 meses
                var (avgSalary, dailyAvgSalary) = await _payrollService.CalculateAvgLast6MonthsSalary(layoff.PersonalAction.User.Id);
                model.AvgLast6MonthsSalary = avgSalary;
                model.DailyAvgLast6Months = dailyAvgSalary;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSettlement(CreateSettlementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                model.currentUserId = currentUserId;

                var (success, message) = await _settlementService.CreateSettlement(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Liquidación registrada de manera exitosa.";
                    return RedirectToAction("Index", "Layoffs");
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    TempData["ErrorMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al registrar la liquidación del empleado.";
                }
            }

            return View("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSettlement(int id)
        {
            var result = await _settlementService.DeleteSettlement(id);
            if (result.success)
            {
                TempData["SuccessMessage"] = result.message;
            }
            else
            {
                TempData["ErrorMessage"] = result.message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet] 
        public async Task<IActionResult> SettlementDetails(int Settlementid)
        {
            ViewBag.Titulo = "Gestión de Liquidaciones";
            ViewBag.NombreUbicacion = "Detalles liquidacion";

            var settlement = await _settlementService.GetSettlementById(Settlementid);
            if (settlement == null)
            {
                TempData["ErrorMessage"] = "No se encontró la liquidación.";
                return RedirectToAction("Index");
            }

            var layoff = await _layoffsService.GetLayoffById(settlement.LayoffId);
            if (layoff == null)
            {
                TempData["ErrorMessage"] = "No se encontró el despido asociado a la liquidación.";
                return RedirectToAction("Index");
            }

            var model = new SettlementDetailsViewModel
            {
                Settlement = settlement,
                Layoff = layoff
            };

            return View(model);
        }

        public async Task<JsonResult> CountSettlements()
        {
            var settlements = await _settlementService.GetSettlementsCount();
            return Json(settlements);
        }

        public async Task<JsonResult> TotalAmountSettlements()
        {
            var totalSettlements = await _settlementService.GetTotalSettlementAmount();
            return Json(totalSettlements);
        }

        public async Task<IActionResult> GetMonthlySettlementAmount()
        {
            try
            {
                var settlementAmounts = await _settlementService.GetMonthlySettlementAmount();
                var formattedData = settlementAmounts.Select(a => new { Month = a.Month, Amount = a.Amount }).ToList();
                return Json(formattedData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener el total acumulado por mes en liquidaciones: {ex.Message}");
            }
        }



    }
}

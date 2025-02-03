using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SendGrid.Helpers.Mail;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using SGRH.Web.Services;
using System.Security.Claims;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IPayrollPeriodService _payrollPeriodService;
        private readonly IOvertimeService _overtimeService;
        private readonly EmailService _emailService;
        public PayrollController(IPayrollService payrollService, IPayrollPeriodService payrollPeriodService, IOvertimeService overtimeService, EmailService emailService)
        {
            _payrollService = payrollService;
            _payrollPeriodService = payrollPeriodService;
            _overtimeService = overtimeService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Listar Nóminas";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payrolls =  await _payrollService.GetAllPayrolls(User);
            return View(payrolls);
        }
        public async Task<IActionResult> PersonalPayrolls()
        {
            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Listar Nóminas";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payrolls = await _payrollService.GetPersonalPayrolls(userId);
            return View(payrolls);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalOvertimeForEmployee(string userId)
        {
            var currentPayrollPeriod = await _payrollPeriodService.GetCurrentPayrollPeriodAsync();
            var (totalOtHours, totalOtAmount) = await _overtimeService.GetTotalOvertimeForPayrollPeriod(userId, currentPayrollPeriod.Id);

            return Json(new { totalOtHours, totalOtAmount });
        }

        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> CreatePayroll()
        {
            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Registrar Nómina";

            var model = new CreatePayrollViewModel
            {
                PayrollPeriods = (await _payrollPeriodService.GetAllPayrollPeriodsAsync())
                    .Select(p => new PayrollPeriodViewModel
                    {
                        Id = p.Id,
                        PeriodName = p.PeriodName
                    })
                    .ToList()
            };

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> CreatePayroll(CreatePayrollViewModel model)    
        {
            if (ModelState.IsValid)
            {
                var (success,errorMessage) = await _payrollService.CreatePayroll(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Nómina registrada correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al registrar la nómina del empleado.";
                }
                    
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtener el ID del usuario actualmente autenticado

                // Obtener el periodo de nómina actual
                var currentPayrollPeriod = await _payrollPeriodService.GetCurrentPayrollPeriodAsync();

                // Obtener las sumatorias de horas extras y montos para el usuario y el periodo de nómina actual
                var (totalOtHours, totalOtAmount) = await _overtimeService.GetTotalOvertimeForPayrollPeriod(userId, currentPayrollPeriod.Id);

                model.PayrollPeriods = (await _payrollPeriodService.GetAllPayrollPeriodsAsync())
                     .Select(p => new PayrollPeriodViewModel
                     {
                         Id = p.Id,
                         PeriodName = p.PeriodName
                     })
                     .ToList();
                model.OtHours = totalOtHours;
                model.OtHoursAmount = totalOtAmount;

                TempData["ErrorMessage"] = "No es posible procesar el formulario sin los campos requeridos.";

                return View(model);
            }

            return View(model);

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PayrollDetails(int id) 
        {
            var payroll = await _payrollService.GetPayrollById(id);
            if (payroll == null)
            {
                return NotFound();
            }

            var model = new DetailsPayrollViewModel
            {
                FullName = payroll.User.FullName,
                Department = payroll.User.Department.Department_Name,
                BaseSalary = payroll.User.BaseSalary,
                PeriodName = payroll.User.workPeriod.PeriodName,
                Id_Payroll = payroll.Id_Payroll,
                UserId = payroll.User.Id,
                PayrollPeriodId = payroll.PayrollPeriodId,
                PayrollPeriod = payroll.PayrollPeriod,
                PayrollFrequency = payroll.PayrollFrequency,
                OrdinarySalary = payroll.OrdinarySalary,
                OtHours = payroll.OtHours,
                OtHoursAmount = payroll.OtHoursAmount,
                BancoPopular = payroll.BancoPopular,
                EnfermedadMaternidad = payroll.EnfermedadMaternidad,
                IVM = payroll.IVM,
                TotalDeductions = payroll.TotalDeductions,
                GrossSalary = payroll.GrossSalary,
                NetSalary = payroll.NetSalary
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> UpdatePayroll(int id)
        {
            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Actualizar Nómina";

            var payroll = await _payrollService.GetPayrollById(id);
            if (payroll == null)
            {
                return NotFound();
            }

            var model = new UpdatePayrollViewModel
            {
                FullName = payroll.User.FullName,
                Department = payroll.User.Department.Department_Name,
                BaseSalary = payroll.User.BaseSalary,
                PeriodName = payroll.User.workPeriod.PeriodName,
                Id_Payroll = payroll.Id_Payroll,
                UserId = payroll.User.Id,
                PayrollPeriodId = payroll.PayrollPeriodId,
                PayrollFrequency = payroll.PayrollFrequency,
                OrdinarySalary = payroll.OrdinarySalary,
                OtHours = payroll.OtHours,
                OtHoursAmount = payroll.OtHoursAmount,
                BancoPopular = payroll.BancoPopular,
                EnfermedadMaternidad = payroll.EnfermedadMaternidad,
                IVM = payroll.IVM,
                TotalDeductions = payroll.TotalDeductions,
                GrossSalary = payroll.GrossSalary,
                NetSalary = payroll.NetSalary,
                PayrollPeriods = (await _payrollPeriodService.GetAllPayrollPeriodsAsync())
                     .Select(p => new PayrollPeriodViewModel
                     {
                         Id = p.Id,
                         PeriodName = p.PeriodName
                     })
                     .ToList()
        };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> UpdatePayroll(UpdatePayrollViewModel model)
        {
            if (ModelState.IsValid)
            {

                var (success, errorMessage) = await _payrollService.UpdatePayroll(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Nómina actualizada correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al actualizar la nómina del empleado.";
                }
            }


            TempData["ErrorMessage"] = "No es posible procesar el formulario sin los campos requeridos.";

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> DeletePayroll(int id)
        {
            var (success, errorMessage) = await _payrollService.DeletePayroll(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Nómina eliminada correctamente.";
                return RedirectToAction("Index");
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al eliminar la nómina.";
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "SupervisorRH , Administrador")]
        public IActionResult GeneratePeriods()
        {
            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Generar periodos de pago";
            return View( new GeneratePeriodsViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> GeneratePeriods(GeneratePeriodsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _payrollPeriodService.GeneratePayrollPeriodsAsync(model.Year);

                TempData["SuccessMessage"] = "Se generaron los periodos para el año indicado satisfactoriamente.";
                return RedirectToAction("GeneratePeriods");
            }

            ViewBag.Titulo = "Gestión de Nóminas";
            ViewBag.NombreUbicacion = "Generar periodos de pago";
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> GeneratePayrollPdf(int id)
        {
            var payroll = await _payrollService.GetPayrollById(id);
            if (payroll == null)
            {
                return NotFound();
            }
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics graphics = XGraphics.FromPdfPage(page);
            XFont fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            XFont fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 12, XFontStyle.Regular);
            int yPosition = 50;
            graphics.DrawString($"Información de Nómina para: {payroll.User.FullName}", fontTitle, XBrushes.Black,
                new XRect(50, yPosition, page.Width - 100, page.Height), XStringFormats.TopLeft);
            yPosition += 40;
            XPen pen = new XPen(XColors.Black, 2);
            graphics.DrawLine(pen, 40, yPosition, page.Width - 40, yPosition);
            yPosition += 10;
            DrawHeader(graphics, "Detalles de la Nómina", fontHeader, ref yPosition);
            DrawText(graphics, $"Departamento: {payroll.User.Department.Department_Name}", fontNormal, ref yPosition);
            DrawText(graphics, $"Salario Base: ₡{payroll.User.BaseSalary:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"Jornada Laboral: {payroll.User.workPeriod.PeriodName}", fontNormal, ref yPosition);
            DrawText(graphics, $"Periodo de Pago: {payroll.PayrollPeriod.PeriodName}", fontNormal, ref yPosition);
            DrawText(graphics, $"Frecuencia de Nómina: {payroll.PayrollFrequency}", fontNormal, ref yPosition);
            yPosition += 20;
            DrawHeader(graphics, "Detalles Salariales", fontHeader, ref yPosition);
            DrawText(graphics, $"Salario Ordinario: ₡{payroll.OrdinarySalary:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"Cantidad de Horas Extra: {payroll.OtHours}", fontNormal, ref yPosition);
            DrawText(graphics, $"Monto Total por Horas Extra: ₡{payroll.OtHoursAmount:n}", fontNormal, ref yPosition);
            yPosition += 20;
            DrawHeader(graphics, "Deducciones y Otros", fontHeader, ref yPosition);
            DrawText(graphics, $"CCSS S.E.M.: ₡{payroll.EnfermedadMaternidad:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"CCSS I.V.M.: ₡{payroll.IVM:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"Banco Popular: ₡{payroll.BancoPopular:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"Monto Total Deducciones: ₡{payroll.TotalDeductions:n}", fontNormal, ref yPosition);
            yPosition += 20;
            DrawHeader(graphics, "Resumen", fontHeader, ref yPosition);
            DrawText(graphics, $"Salario Bruto: ₡{payroll.GrossSalary:n}", fontNormal, ref yPosition);
            DrawText(graphics, $"Salario Neto: ₡{payroll.NetSalary:n}", fontNormal, ref yPosition);
            MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;
            return File(stream, "application/pdf", $"Nomina_{payroll.User.FullName}.pdf");
        }

        private void DrawHeader(XGraphics graphics, string text, XFont font, ref int yPosition)
        {
            graphics.DrawString(text, font, XBrushes.DarkBlue,
                new XRect(50, yPosition, graphics.PageSize.Width - 100, graphics.PageSize.Height), XStringFormats.TopLeft);
            yPosition += 20;
        }
        private void DrawText(XGraphics graphics, string text, XFont font, ref int yPosition)
        {
            graphics.DrawString(text, font, XBrushes.Black,
                new XRect(70, yPosition, graphics.PageSize.Width - 100, graphics.PageSize.Height), XStringFormats.TopLeft);
            yPosition += 20;
        }

        [HttpPost]
        [Authorize(Roles = "SupervisorRH, Administrador")]
        public async Task<IActionResult> SendPayrollEmail(int id)
        {
            var payroll = await _payrollService.GetPayrollById(id);
            if (payroll == null)
            {
                return NotFound();
            }

            try
            {
                // Generar el PDF para esta nómina
                var pdfFileBytes = GeneratePayrollPdfBytes(payroll);

                // Obtener la dirección de correo electrónico del usuario asociado a esta nómina
                var toEmail = payroll.User.Email;
                var username = payroll.User.FullName;

                // Crear el mensaje de correo electrónico
                var subject = $"Información de Nómina para {username} - "+ payroll.PayrollPeriod.PeriodName;
                var message = $"Estimado {username}. " +
                    $"\n" +
                    $"Adjunto encontrarás la información de la nómina correspondiente al periodo "+payroll.PayrollPeriod.PeriodName+".";

                // Enviar el correo electrónico con el PDF adjunto
                await _emailService.SendEmailWithAttachment(subject, toEmail, username, message, pdfFileBytes, "Nómina " + payroll.PayrollPeriod.PeriodName +".pdf");

                TempData["SuccessMessage"] = $"Correo electrónico con la información de la nómina enviado a {toEmail}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al enviar el correo electrónico: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        public byte[] GeneratePayrollPdfBytes(Payroll payroll)
        {
            // Crear un nuevo documento PDF
            PdfDocument document = new PdfDocument();

            // Agregar una nueva página al documento
            PdfPage page = document.AddPage();

            // Obtener el objeto XGraphics para dibujar en la página
            XGraphics graphics = XGraphics.FromPdfPage(page);

            // Definir fuentes para el texto
            XFont fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            XFont fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 12, XFontStyle.Regular);

            // Posición inicial de dibujo en la página
            int yPosition = 50;

            // Dibujar título con el nombre del empleado
            graphics.DrawString($"Información de Nómina para: {payroll.User.FullName}", fontTitle, XBrushes.Black,
                new XRect(50, yPosition, page.Width - 100, page.Height), XStringFormats.TopLeft);
            yPosition += 40;

            // Dibujar línea separadora
            XPen pen = new XPen(XColors.Black, 2);
            graphics.DrawLine(pen, 40, yPosition, page.Width - 40, yPosition);
            yPosition += 10;

            // Dibujar secciones con la información de la nómina
            DrawSectionHeader(graphics, "Detalles de la Nómina", fontHeader, ref yPosition);
            DrawTextInternal(graphics, $"Departamento: {payroll.User.Department.Department_Name}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Salario Base: ₡{payroll.User.BaseSalary:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Jornada Laboral: {payroll.User.workPeriod.PeriodName}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Periodo de Pago: {payroll.PayrollPeriod.PeriodName}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Frecuencia de Nómina: {payroll.PayrollFrequency}", fontNormal, ref yPosition);

            yPosition += 20;

            DrawSectionHeader(graphics, "Detalles Salariales", fontHeader, ref yPosition);
            DrawTextInternal(graphics, $"Salario Ordinario: ₡{payroll.OrdinarySalary:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Cantidad de Horas Extra: {payroll.OtHours}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Monto Total por Horas Extra: ₡{payroll.OtHoursAmount:n}", fontNormal, ref yPosition);

            yPosition += 20;

            DrawSectionHeader(graphics, "Deducciones y Otros", fontHeader, ref yPosition);
            DrawTextInternal(graphics, $"CCSS S.E.M.: ₡{payroll.EnfermedadMaternidad:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"CCSS I.V.M.: ₡{payroll.IVM:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Banco Popular: ₡{payroll.BancoPopular:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Monto Total Deducciones: ₡{payroll.TotalDeductions:n}", fontNormal, ref yPosition);

            yPosition += 20;

            DrawSectionHeader(graphics, "Resumen", fontHeader, ref yPosition);
            DrawTextInternal(graphics, $"Salario Bruto: ₡{payroll.GrossSalary:n}", fontNormal, ref yPosition);
            DrawTextInternal(graphics, $"Salario Neto: ₡{payroll.NetSalary:n}", fontNormal, ref yPosition);

            // Guardar el documento PDF en una memoria
            MemoryStream stream = new MemoryStream();
            document.Save(stream, false);

            // Convertir el contenido de la memoria en un arreglo de bytes
            byte[] pdfBytes = stream.ToArray();

            // Retornar el arreglo de bytes del PDF
            return pdfBytes;
        }

        private void DrawSectionHeader(XGraphics graphics, string text, XFont font, ref int yPosition)
        {
            // Dibujar texto como encabezado de sección
            graphics.DrawString(text, font, XBrushes.DarkBlue,
                new XRect(50, yPosition, graphics.PageSize.Width - 100, graphics.PageSize.Height), XStringFormats.TopLeft);

            // Incrementar posición Y para el siguiente elemento
            yPosition += 20;
        }

        private void DrawTextInternal(XGraphics graphics, string text, XFont font, ref int yPosition)
        {
            // Dibujar texto normal en la página
            graphics.DrawString(text, font, XBrushes.Black,
                new XRect(70, yPosition, graphics.PageSize.Width - 100, graphics.PageSize.Height), XStringFormats.TopLeft);

            // Incrementar posición Y para el siguiente elemento
            yPosition += 20;
        }

        public async Task<JsonResult> GetPayrollCount(string userId)
        {
            var payrollCount = await _payrollService.GetPayrollCount(userId);

            return Json(payrollCount);
        }

        public async Task<JsonResult> GetPayrollAmount(string userId)
        {
            var payrollAmount = await _payrollService.GetTotalPayrollAmount(userId);

            return Json(payrollAmount);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyPayrollAmount(string userId)
        {
            var monthlyPayrollAmounts = await _payrollService.GetMonthlyPayrollAmount(userId);
            var formattedData = monthlyPayrollAmounts.Select(a => new { Month = a.Month, Amount = a.Amount }).ToList();
            return Json(formattedData);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AbsenceCategoriesController : Controller
    {
        private readonly SgrhContext _context;

        public AbsenceCategoriesController(SgrhContext context)
        {
            _context = context;
        }

        // GET: AbsenceCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.AbsenceCategories.ToListAsync());
        }

        // GET: AbsenceCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var absenceCategory = await _context.AbsenceCategories
                .FirstOrDefaultAsync(m => m.Id_Absence_Category == id);
            if (absenceCategory == null)
            {
                return NotFound();
            }

            return View(absenceCategory);
        }

        // GET: AbsenceCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AbsenceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Absence_Category,Category_Name")] AbsenceCategory absenceCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(absenceCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(absenceCategory);
        }

        // GET: AbsenceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var absenceCategory = await _context.AbsenceCategories.FindAsync(id);
            if (absenceCategory == null)
            {
                return NotFound();
            }
            return View(absenceCategory);
        }

        // POST: AbsenceCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Absence_Category,Category_Name")] AbsenceCategory absenceCategory)
        {
            if (id != absenceCategory.Id_Absence_Category)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(absenceCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AbsenceCategoryExists(absenceCategory.Id_Absence_Category))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(absenceCategory);
        }

        // GET: AbsenceCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var absenceCategory = await _context.AbsenceCategories
                .FirstOrDefaultAsync(m => m.Id_Absence_Category == id);
            if (absenceCategory == null)
            {
                return NotFound();
            }

            return View(absenceCategory);
        }

        // POST: AbsenceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var absenceCategory = await _context.AbsenceCategories.FindAsync(id);
            if (absenceCategory != null)
            {
                _context.AbsenceCategories.Remove(absenceCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AbsenceCategoryExists(int id)
        {
            return _context.AbsenceCategories.Any(e => e.Id_Absence_Category == id);
        }
    }
}

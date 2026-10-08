using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    public class DanhMucMonController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public DanhMucMonController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // GET: DanhMucMon
        public async Task<IActionResult> Index()
        {
            return View(await _context.DanhMucMons.ToListAsync());
        }

        // GET: DanhMucMon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucMon = await _context.DanhMucMons
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);
            if (danhMucMon == null)
            {
                return NotFound();
            }

            return View(danhMucMon);
        }

        // GET: DanhMucMon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DanhMucMon/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhMuc,TenDanhMuc,MoTa")] DanhMucMon danhMucMon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMucMon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhMucMon);
        }

        // GET: DanhMucMon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucMon = await _context.DanhMucMons.FindAsync(id);
            if (danhMucMon == null)
            {
                return NotFound();
            }
            return View(danhMucMon);
        }

        // POST: DanhMucMon/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDanhMuc,TenDanhMuc,MoTa")] DanhMucMon danhMucMon)
        {
            if (id != danhMucMon.MaDanhMuc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMucMon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucMonExists(danhMucMon.MaDanhMuc))
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
            return View(danhMucMon);
        }

        // GET: DanhMucMon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucMon = await _context.DanhMucMons
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);
            if (danhMucMon == null)
            {
                return NotFound();
            }

            return View(danhMucMon);
        }

        // POST: DanhMucMon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMucMon = await _context.DanhMucMons.FindAsync(id);
            if (danhMucMon != null)
            {
                _context.DanhMucMons.Remove(danhMucMon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucMonExists(int id)
        {
            return _context.DanhMucMons.Any(e => e.MaDanhMuc == id);
        }
    }
}

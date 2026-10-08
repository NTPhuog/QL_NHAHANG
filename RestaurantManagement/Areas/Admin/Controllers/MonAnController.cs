using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
public class MonAnController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public MonAnController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // GET: MonAn
        public async Task<IActionResult> Index()
        {
            var quanLyNhaHangContext = _context.MonAns.Include(m => m.MaDanhMucNavigation);
            return View(await quanLyNhaHangContext.Where(m => HttpContext.Session.GetString("Role") == "Admin" || m.TrangThai == "Hiển thị").ToListAsync());
        }

        // GET: MonAn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.MaDanhMucNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null || (HttpContext.Session.GetString("Role") != "Admin" && monAn.TrangThai != "Hiển thị"))
            {
                return NotFound();
            }

            return View(monAn);
        }

        // GET: MonAn/Create
        public IActionResult Create()
        {
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMons, "MaDanhMuc", "TenDanhMuc");
            return View();
        }

        // POST: MonAn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaMon,MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai")] MonAn monAn)
        {
            ModelState.Remove("MaDanhMucNavigation");
            if (!await _context.DanhMucMons.AnyAsync(x => x.MaDanhMuc == monAn.MaDanhMuc)) ModelState.AddModelError("MaDanhMuc", "Danh mục không tồn tại.");
            if (ModelState.IsValid)
            {
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMons, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

        // GET: MonAn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null || (HttpContext.Session.GetString("Role") != "Admin" && monAn.TrangThai != "Hiển thị"))
            {
                return NotFound();
            }
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMons, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

        // POST: MonAn/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaMon,MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai")] MonAn monAn)
        {
            if (id != monAn.MaMon)
            {
                return NotFound();
            }

            ModelState.Remove("MaDanhMucNavigation");
            if (!await _context.DanhMucMons.AnyAsync(x => x.MaDanhMuc == monAn.MaDanhMuc)) ModelState.AddModelError("MaDanhMuc", "Danh mục không tồn tại.");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonAnExists(monAn.MaMon))
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
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMons, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

        // GET: MonAn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.MaDanhMucNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null || (HttpContext.Session.GetString("Role") != "Admin" && monAn.TrangThai != "Hiển thị"))
            {
                return NotFound();
            }

            return View(monAn);
        }

        // POST: MonAn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {
                _context.MonAns.Remove(monAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonAnExists(int id)
        {
            return _context.MonAns.Any(e => e.MaMon == id);
        }
    }
}



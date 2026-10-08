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
    public class BanAnController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public BanAnController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // GET: BanAn
        public async Task<IActionResult> Index()
        {
            return View(await _context.BanAns.ToListAsync());
        }

        // GET: BanAn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banAn = await _context.BanAns
                .FirstOrDefaultAsync(m => m.MaBan == id);
            if (banAn == null)
            {
                return NotFound();
            }

            return View(banAn);
        }

        // GET: BanAn/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BanAn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaBan,TenBan,SoChoNgoi,TrangThai")] BanAn banAn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(banAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(banAn);
        }

        // GET: BanAn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banAn = await _context.BanAns.FindAsync(id);
            if (banAn == null)
            {
                return NotFound();
            }
            return View(banAn);
        }

        // POST: BanAn/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBan,TenBan,SoChoNgoi,TrangThai")] BanAn banAn)
        {
            if (id != banAn.MaBan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(banAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanAnExists(banAn.MaBan))
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
            return View(banAn);
        }

        // GET: BanAn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banAn = await _context.BanAns
                .FirstOrDefaultAsync(m => m.MaBan == id);
            if (banAn == null)
            {
                return NotFound();
            }

            return View(banAn);
        }

        // POST: BanAn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banAn = await _context.BanAns.FindAsync(id);
            if (banAn != null)
            {
                _context.BanAns.Remove(banAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BanAnExists(int id)
        {
            return _context.BanAns.Any(e => e.MaBan == id);
        }
    }
}

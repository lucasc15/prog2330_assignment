using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LCBusService.Models;

namespace LCBusService.Controllers
{
    public class DriversController : Controller
    {
        private BusServiceContext _context;

        public DriversController(BusServiceContext context)
        {
            _context = context;    
        }

        // GET: Drivers
        public async Task<IActionResult> Index()
        {
            var busServiceContext = _context.Driver.Include(d => d.ProvinceCodeNavigation);
            return View(await busServiceContext.ToListAsync());
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Driver.SingleOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DriverId,City,DateHired,FirstName,FullName,HomePhone,LastName,PostalCode,ProvinceCode,Street,WorkPhone")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                ViewData["message"] = "Successfully made driver";
                return RedirectToAction("Index");
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", driver.ProvinceCode);
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Driver.SingleOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p => p.ProvinceCode), "ProvinceCode", "ProvinceCode");
            return View(driver);
        }

        // POST: Drivers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DriverId,City,DateHired,FirstName,FullName,HomePhone,LastName,PostalCode,ProvinceCode,Street,WorkPhone")] Driver driver)
        {
            if (id != driver.DriverId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.DriverId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                ViewData["message"] = "Successfully editied driver";
                return RedirectToAction("Index");
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p => p.ProvinceCode), "ProvinceCode", "ProvinceCode");
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Driver.SingleOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Driver driver = null;
            try
            {
                driver = await _context.Driver.SingleOrDefaultAsync(m => m.DriverId == id);
                _context.Driver.Remove(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            } catch (Exception e)
            {
                if (driver != null)
                {
                    ViewData["message"] = "Error removing the driver as the are still assigned routes, please remove routes and try again";
                    return View(driver);
                }
                ViewData["message"] = string.Format("Unknown error, could not delete driver. Please try again later. Error: {0}", e.Message);
                return RedirectToAction("Index");
            }
        }

        public JsonResult ProvinceCodeRemoteValidator(string provinceCode)
        {
            Driver driver = new Models.Driver(_context);
            if (driver.ProvinceCodeExists(provinceCode))
            {
                return Json(true);
            }
            return Json("Province code is invalid, ensure it is at least two letter");
        }

        private bool DriverExists(int id)
        {
            return _context.Driver.Any(e => e.DriverId == id);
        }
    }
}

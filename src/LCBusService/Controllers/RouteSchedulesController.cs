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
    public class RouteSchedulesController : Controller
    {
        private readonly BusServiceContext _context;

        public RouteSchedulesController(BusServiceContext context)
        {
            _context = context;    
        }

        // GET: RouteSchedules
        public async Task<IActionResult> Index(string id)
        {
            // Get the busRouteCode from cookie or query string or url param
            string busRouteCode = getBusRouteCode(id); 
            if (string.IsNullOrEmpty(busRouteCode))
            {
                return RedirectToAction("Index", "BusRoutes");
            }
            var busServiceContext = await  _context.RouteSchedule.
                                        Where(rs => rs.BusRouteCode == busRouteCode).
                                        OrderBy(rs => rs.StartTime).
                                        Include(rs => rs.BusRouteCodeNavigation).ToListAsync();
            if (busServiceContext.Count() > 0)
            {
                ViewData["header"] = ViewData["title"] = string.Format("Schedule for: {0} ({1})", 
                    busServiceContext[0].BusRouteCodeNavigation.RouteName, busRouteCode);
                return View(busServiceContext);
            }
            ViewData["message"] = string.Format("Sorry, there are no stops scheduled for this route code: {0}", busRouteCode);
            return View(new List<RouteSchedule>());
        }

        // find busRouteCode passed to function. Helper function to search cookie or query string for the id if not in url ../index/id
        public string getBusRouteCode(string busRouteCode)
        {
            if (string.IsNullOrEmpty(busRouteCode))
            {
                busRouteCode = string.IsNullOrEmpty(Request.Cookies["busroutecode"]) ?
                    Request.Cookies["busroutecode"] : Request.Query["busroutecode"].ToString();
            }
            return busRouteCode;
        }
        // GET: returns the times that a particular route stops at a given stop
        public async Task<IActionResult> RouteStopSchedule(int? routeStopId) {
            // First step is to check either form or query string for the current values
            if (routeStopId == null)
            {
                try
                {
                    routeStopId = (string.IsNullOrEmpty(Request.Query["routestopid"])) ?
                        int.Parse(Request.Query["routestopid"]) : int.Parse(Request.Form["routestopid"]);
                } catch
                {
                    return RedirectToAction("Index", "BusStops",
                        new { message = "Could not find schedule data for the specified stop, please select the stop again." });
                }
            }
            var routeStop = await _context.RouteStop.
                                Include(rs => rs.BusRouteCodeNavigation).
                                Include(rs => rs.BusStopNumberNavigation).
                                Where(rs => rs.RouteStopId == routeStopId).FirstAsync();
            var routeSchedule = await _context.RouteSchedule.
                                    Where(rs => rs.BusRouteCode == rs.BusRouteCode).
                                    OrderBy(rs => rs.StartTime).
                                    ToListAsync();
            foreach (RouteSchedule schedule in routeSchedule)
            {
                schedule.StartTime.Add(TimeSpan.FromMinutes((double)routeStop.OffsetMinutes)); 
            }
            ViewData["stop"] = string.Format("Stop: {0} - {1}", routeStop.BusStopNumber, routeStop.BusStopNumberNavigation.Location);
            ViewData["route"] = string.Format("Route: {0} - {1}", routeStop.BusRouteCode, routeStop.BusRouteCodeNavigation.RouteName);
            ViewData["title"] = ViewData["stop"] + " " + ViewData["route"];
            return View("Index", routeSchedule);
        }
        // GET: RouteSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeSchedule = await _context.RouteSchedule.SingleOrDefaultAsync(m => m.RouteScheduleId == id);
            if (routeSchedule == null)
            {
                return NotFound();
            }

            return View(routeSchedule);
        }

        // GET: RouteSchedules/Create
        public IActionResult Create()
        {
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode");
            return View();
        }

        // POST: RouteSchedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteScheduleId,BusRouteCode,Comments,IsWeekDay,StartTime")] RouteSchedule routeSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(routeSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeSchedule.BusRouteCode);
            return View(routeSchedule);
        }

        // GET: RouteSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeSchedule = await _context.RouteSchedule.SingleOrDefaultAsync(m => m.RouteScheduleId == id);
            if (routeSchedule == null)
            {
                return NotFound();
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeSchedule.BusRouteCode);
            return View(routeSchedule);
        }

        // POST: RouteSchedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteScheduleId,BusRouteCode,Comments,IsWeekDay,StartTime")] RouteSchedule routeSchedule)
        {
            if (id != routeSchedule.RouteScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routeSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteScheduleExists(routeSchedule.RouteScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeSchedule.BusRouteCode);
            return View(routeSchedule);
        }

        // GET: RouteSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeSchedule = await _context.RouteSchedule.SingleOrDefaultAsync(m => m.RouteScheduleId == id);
            if (routeSchedule == null)
            {
                return NotFound();
            }

            return View(routeSchedule);
        }

        // POST: RouteSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routeSchedule = await _context.RouteSchedule.SingleOrDefaultAsync(m => m.RouteScheduleId == id);
            _context.RouteSchedule.Remove(routeSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool RouteScheduleExists(int id)
        {
            return _context.RouteSchedule.Any(e => e.RouteScheduleId == id);
        }
    }
}

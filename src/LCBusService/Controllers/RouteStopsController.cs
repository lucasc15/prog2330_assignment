using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using LCBusService.Models;

namespace LCBusService.Controllers
{
    public class RouteStopsController : Controller
    {
        private readonly BusServiceContext _context;
        private string cookieBusRouteCode = "busroutecode";
        private string cookieBusRouteName = "busroutename";
        private string queryBusRouteCode = "busroutecode";
        public RouteStopsController(BusServiceContext context)
        {
            _context = context;    
        }

        // GET: RouteStops/?id || RouteStops/?busRouteCode=id
        public async Task<IActionResult> Index(string busRouteCode)
        {
            busRouteCode = (busRouteCode != null) ? busRouteCode :
                (new List<string> { Request.Query[queryBusRouteCode].FirstOrDefault(), Request.Cookies[cookieBusRouteCode]}).
                FirstOrDefault(s => !string.IsNullOrEmpty(s));
            if (busRouteCode == null)
            {
                await redirectIndex();
            }
            var busServiceContext = _context.RouteStop.
                Include(r => r.BusRouteCodeNavigation).
                Include(r => r.BusStopNumberNavigation).
                Where(r => r.BusRouteCode == busRouteCode).
                OrderBy(r => r.OffsetMinutes);
            string routeName = (await _context.BusRoute.Where(r => r.BusRouteCode == busRouteCode).FirstOrDefaultAsync()).RouteName;
            ViewData["Title"] = string.Format("Route Stops ({0})", routeName);
            Response.Cookies.Append(cookieBusRouteName, routeName);
            Response.Cookies.Append(cookieBusRouteCode, busRouteCode);
            return View(await busServiceContext.ToListAsync());
        }

        // Redirect to index in the case where there is not a valid route/id selected. Used in multiple functions as a helper function.
        private async Task<IActionResult> redirectIndex()
        {
            ViewData["message"] = "Please select a bus route before viewing bus stops";
            var routes = await _context.BusRoute.OrderBy(r => int.Parse(r.BusRouteCode)).ToListAsync();
            return View("~/Views/BusRoutes/Index.cshtml", routes);
        }

        // GET: RouteStops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.SingleOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // GET: RouteStops/Create (relies on cookie data) || RouteStops/Create/busRouteCode (will set busRouteCode and routeName in cookie)
        public async Task<IActionResult> Create(string busRouteCode)
        {
            busRouteCode = (busRouteCode != null) ? busRouteCode : Request.Cookies[cookieBusRouteCode];
            if (busRouteCode == null)
            {
                return await redirectIndex();
            }
            string routeName = (await _context.BusRoute.Where(r => r.BusRouteCode == busRouteCode).FirstOrDefaultAsync()).RouteName;
            ViewData["BusRouteCode"] = busRouteCode;
            ViewData["Title"] = string.Format("Create stop for Route: {0} - {1}", busRouteCode, routeName);
            ViewData["BusStopLocations"] = new SelectList(
                _context.BusStop.OrderBy(bs => bs.Location).OrderBy(bs => bs.GoingDowntown),
                "BusStopNumber",
                "Location");
            ViewData["BusRouteCode"] = Request.Cookies[cookieBusRouteCode];
            Response.Cookies.Append(cookieBusRouteCode, busRouteCode);
            Response.Cookies.Append(cookieBusRouteName, routeName);
            return View();
        }

        // POST: RouteStops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(routeStop);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return await Create(Request.Cookies[cookieBusRouteCode]);
        }

        // GET: RouteStops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.SingleOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }
            string busRouteCode = Request.Cookies[cookieBusRouteCode];
            string busRouteName = (await _context.BusRoute.Where(r => r.BusRouteCode == busRouteCode).FirstOrDefaultAsync()).RouteName;
            ViewData["BusRouteCode"] = busRouteCode;
            ViewData["Title"] = string.Format("Edit stop for Route: {0} - {1}", busRouteCode, busRouteName);
            ViewData["BusStopLocations"] = new SelectList(
                _context.BusStop.OrderBy(bs => bs.Location).OrderBy(bs => bs.GoingDowntown), 
                "BusStopNumber", 
                "Location");
            Response.Cookies.Append(cookieBusRouteName, busRouteName);
            return View(routeStop);
        }

        // POST: RouteStops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            if (id != routeStop.RouteStopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routeStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteStopExists(routeStop.RouteStopId))
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
            return await Edit(routeStop.RouteStopId);
        }

        // GET: RouteStops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.SingleOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // POST: RouteStops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routeStop = await _context.RouteStop.SingleOrDefaultAsync(m => m.RouteStopId == id);
            _context.RouteStop.Remove(routeStop);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool RouteStopExists(int id)
        {
            return _context.RouteStop.Any(e => e.RouteStopId == id);
        }
    }
}

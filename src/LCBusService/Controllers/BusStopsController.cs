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
    public class BusStopsController : Controller
    {
        private readonly BusServiceContext _context;

        public BusStopsController(BusServiceContext context)
        {
            _context = context;    
        }

        // GET: BusStops || BusStops?orderby=value to order stops by specific field
        public async Task<IActionResult> Index(string message)
        {
            ViewData["Header"] = "Bus Stops";
            List<BusStop> stops;
            switch (Request.Query["orderby"])
            {
                case "location":
                    stops = await _context.BusStop.OrderBy(bs => bs.Location).ToListAsync();
                    break;
                default:
                    stops = await _context.BusStop.OrderBy(bs => bs.BusStopNumber).ToListAsync();
                    break;
            }
            // could not get sessions going with TempData so passed internal message with cookie that is deleted from RouteSelector
            ViewData["message"] = message;
            return View(stops);
        }

        // GET: BusStops/RouteSelector/{id}
        // Returns the routes for a given stop {id}; returns to BusStop index if there are no routes or an invalid ID is passes (i.e. null)
        public async Task<IActionResult> RouteSelector(int? id)
        {
            // Redirect is there is no busStopNumber given as id
            if (id == null)
            {
                ViewData["message"] = "Please select a valid bus stop number.";
                return View("Index", await _context.BusStop.OrderBy(bs => bs.BusStopNumber).ToListAsync());
            }
            // Find the routes for the given stop using the routeStops table
            var routeStops = await (_context.RouteStop.
                Where(rs => rs.BusStopNumber == id).
                Include(rs => rs.BusRouteCodeNavigation)).ToListAsync();
            string busStopName;
            try
            {
                busStopName = (await _context.BusStop.Where(bs => bs.BusStopNumber == id).FirstOrDefaultAsync()).Location;
            } catch
            {
                // Case where there was no valid record returned
                return RedirectToAction("Index", new { message = "Invalid stop id please select via hyperlink" });
            }
            // If there is only one route, go directly to schedule, otherwise redirect to page to choose route. Redirect to index if no routes exist
            if (routeStops.Count() < 1)
            {
                ViewData["message"] = string.Format("Sorry, no routes for this stop: {0} - {1}", id, busStopName);
                return View("Index", await _context.BusStop.OrderBy(bs => bs.BusStopNumber).ToListAsync());
            } else if (routeStops.Count() == 1)
            {
                return RedirectToAction("RouteStopSchedule", "RouteSchedules", 
                    new { routeStopid = routeStops[0].RouteStopId });
            } else
            {
                ViewData["busstopnumber"] = id;
                return View("RouteSelector", routeStops);
            }
        }
 
        // GET: BusStops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busStop = await _context.BusStop.SingleOrDefaultAsync(m => m.BusStopNumber == id);
            if (busStop == null)
            {
                return NotFound();
            }

            return View(busStop);
        }

        // GET: BusStops/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusStops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusStopNumber,GoingDowntown,Location")] BusStop busStop)
        {
            busStop.LocationHash = hashBusStopName(busStop.Location);
            if (ModelState.IsValid)
            {
                _context.Add(busStop);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(busStop);
        }

        // GET: BusStops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busStop = await _context.BusStop.SingleOrDefaultAsync(m => m.BusStopNumber == id);
            if (busStop == null)
            {
                return NotFound();
            }
            return View(busStop);
        }

        // POST: BusStops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GoingDowntown,Location")] BusStop busStop)
        {
            if (id != busStop.BusStopNumber)
            {
                return NotFound();
            }
            busStop.LocationHash = hashBusStopName(busStop.Location);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(busStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusStopExists(busStop.BusStopNumber))
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
            return View(busStop);
        }

        // GET: BusStops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busStop = await _context.BusStop.SingleOrDefaultAsync(m => m.BusStopNumber == id);
            if (busStop == null)
            {
                return NotFound();
            }

            return View(busStop);
        }

        // POST: BusStops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var busStop = await _context.BusStop.SingleOrDefaultAsync(m => m.BusStopNumber == id);
            _context.RouteStop.RemoveRange(_context.RouteStop.Where(x => x.BusStopNumber == id));
            _context.BusStop.Remove(busStop);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BusStopExists(int id)
        {
            return _context.BusStop.Any(e => e.BusStopNumber == id);
        }

        // Returns a hash which is the sum of the byte value of all the letters in the stop name
        private int hashBusStopName(string name)
        {
            int hashValue = 0;
            if (name != null || name != "")
            {
                foreach (char c in name)
                {
                    hashValue += Convert.ToByte(c);
                }
            }
            return hashValue;
        }
    }
}

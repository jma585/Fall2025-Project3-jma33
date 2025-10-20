using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_jma33.Data;
using Fall2025_Project3_jma33.Models;

namespace Fall2025_Project3_jma33.Controllers
{
    public class MovieActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MovieActors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MovieActors.Include(m => m.Actor).Include(m => m.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MovieActors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActors = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movieActors == null)
            {
                return NotFound();
            }

            return View(movieActors);
        }

        // GET: MovieActors/Create
        public IActionResult Create()
        {
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Title");
            return View();
        }

        // POST: MovieActors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,ActorId")] MovieActors movieActors)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movieActors);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Name", movieActors.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Title", movieActors.MovieId);
            return View(movieActors);
        }

        // GET: MovieActors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActors = await _context.MovieActors.FindAsync(id);
            if (movieActors == null)
            {
                return NotFound();
            }
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Name", movieActors.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Title", movieActors.MovieId);
            return View(movieActors);
        }

        // POST: MovieActors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,ActorId")] MovieActors movieActors)
        {
            if (id != movieActors.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieActors);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieActorsExists(movieActors.Id))
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
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Name", movieActors.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Title", movieActors.MovieId);
            return View(movieActors);
        }

        // GET: MovieActors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActors = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movieActors == null)
            {
                return NotFound();
            }

            return View(movieActors);
        }

        // POST: MovieActors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movieActors = await _context.MovieActors.FindAsync(id);
            if (movieActors != null)
            {
                _context.MovieActors.Remove(movieActors);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieActorsExists(int id)
        {
            return _context.MovieActors.Any(e => e.Id == id);
        }
    }
}

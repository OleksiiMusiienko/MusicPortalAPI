using Microsoft.AspNetCore.Mvc;
using MusicPortal.Models;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;
using Portal.DAL.Entities;

namespace MusicPortal.Controllers
{
    public class GenreController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IGenreService _context;
        private readonly ISongService _songService;
        public GenreController(IGenreService context, ISongService songService)
        {
            _context = context;
            _songService = songService;
        }
        public async Task <IActionResult> Index()
        {
            return View(await _context.GetAllGenres());
        }
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("Admin") == null)
            {
                HttpContext.Session.Clear();
                ViewBag.UserId = null;
            }
            return View();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] GenreDTO gen)
        {
            GenreDTO genDto = await _context.GetGenreByName(gen.Name);
            if (genDto != null)
            {
                ModelState.AddModelError("", "Такой жанр есть!");
                return View(genDto);
            }
            genDto = new GenreDTO();
            if (ModelState.IsValid)
            {
                genDto.Name = gen.Name;
                await _context.CreateGenre(genDto);                
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gen = await _context.GetGenreById((int)id);
            if (gen == null)
            {
                return NotFound();
            }

            return View(gen);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {           
            GenreDTO gen = await _context.GetGenreById((int)id);
            var songs = await _songService.GetSongsByGenre(gen);
            if (songs != null)
            {
                foreach (var song in songs)
                {
                    await _songService.Delete(song.Id);
                }
            }
            if (gen != null)
            {
                await _context.DeleteGenre(id);
            }
            return RedirectToAction(nameof(Index));
        }

    }
}

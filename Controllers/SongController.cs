using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Models;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;

namespace MusicPortal.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongService _context;
        private readonly IGenreService _genreService;
        private readonly IWebHostEnvironment _appEnvironment; //рабочее окружение приложения

        public SongController(ISongService context, IGenreService genreService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _genreService = genreService;
            _appEnvironment = webHostEnvironment;
        }
        public async Task <IActionResult> Index(string author, int genre = 0, int page = 1, SortState sortOrder = SortState.NameAsc)
        {
            int pageSize = 5;
            IEnumerable<SongDTO> songs = await _context.GetAllSongs();
            //фильтрация
            if (genre != 0)
            {
                songs = songs.Where(s => s.GenreId == genre);
            }
            if (!string.IsNullOrEmpty(author))
            {
                songs =songs.Where(s => s.Author == author);
            }
            //сортировка

            songs = sortOrder switch
            {
                SortState.NameDesc => songs.OrderByDescending(s => s.Name),
                SortState.AuthorAsc => songs.OrderBy(s => s.Author),
                SortState.AuthorDesc => songs.OrderByDescending(s => s.Author),
                SortState.GenreAsc => songs.OrderBy(s => s.Genre),
                SortState.GenreDesc => songs.OrderByDescending(s => s.Genre),
                _ => songs.OrderBy(s => s.Name),
            };

            //пагинация
            var count = songs.Count();
            var items = songs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // формируем модель представления
            var gen =await _genreService.GetAllGenres();
            List<GenreDTO> genres= gen.ToList();
            IndexViewModel viewModel = new IndexViewModel(
              items,
              new PageViewModel(count, page, pageSize),
              new SortViewModel(sortOrder),
              new FilterViewModel(genres, genre, author)              
          );
            return View(viewModel);
        }
        public async Task <IActionResult> Create()
        {
            ViewBag.Genres = new SelectList(await _genreService.GetAllGenres(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1000000000)]
        public async Task<IActionResult> Create(SongDTO songDTO, IFormFile uploadedFile)
        {
            var songsByAuthor = await _context.GetSongsByAuthor(songDTO.Author!);
            SongDTO sdto = songsByAuthor.Where(s=>s.Name == songDTO.Name).FirstOrDefault();

            if (sdto != null)
            {
                ModelState.AddModelError("", "Такая песня есть!");
                return View(songDTO);
            }
            else
            {
                sdto = new SongDTO();
                if(ModelState.IsValid && uploadedFile != null)
                {
                        await UploadSong(songDTO, uploadedFile);
                        await _context.Create(songDTO);
                        return RedirectToAction("Index");

                }
            }
            ViewBag.Genres = new SelectList(await _genreService.GetAllGenres(), "Id", "Name", songDTO.GenreId);
           
            return RedirectToAction("Index");
        }
        private async Task UploadSong(SongDTO songDTO, IFormFile uploadedFile)
        {
                // Путь к папке 
                string path = "/data/" + uploadedFile.FileName; // имя файла
                songDTO.Path = path;
               
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
          
            var song = await _context.GetSongById((int)id);
            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.GetSongById(id);
            if (song != null)
            {
                await _context.Delete(id);
            }
            TempData["Message"] = "Песня удалена!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SongDTO song = await _context.GetSongById((int)id);
            if (song == null)
            {
                return NotFound();
            }
            ViewBag.ListGenres = new SelectList(await _genreService.GetAllGenres(), "Id", "Name", song.GenreId);
            return View(song);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SongDTO songDto)
        {
            if (id != songDto.Id)
            {
                return NotFound();
            }          
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Update(songDto);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction("Index");
            }
            return View(songDto);
        }
    }
}

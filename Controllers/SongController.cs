using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;

namespace MusicPortal.Controllers
{
    [ApiController]
    [Route("api/Songs")]
    public class SongController : ControllerBase
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SongDTO>>> GetSongs()
        {
            var songs = await _context.GetAllSongs();
            if (songs == null)
            {
                return NotFound();
            }
            return Ok(songs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SongDTO>>> GetSong(int id)
        {
            var song = await _context.GetSongById(id);
            if (song == null)
            {
                return NotFound();
            }
            return Ok(song);
        }

        [HttpPost]
        public async Task <ActionResult<SongDTO>> PostSong(SongDTO songDTO,  IFormFile uploadedFile)
        {
            var songsByAuthor = await _context.GetSongsByAuthor(songDTO.Author!);
            SongDTO sdto = songsByAuthor.Where(s => s.Name == songDTO.Name).FirstOrDefault();

            if (sdto != null)
            {
                ModelState.AddModelError("", "Такая песня есть!");
                return BadRequest(ModelState);                
            }
            else
            {
                if (ModelState.IsValid && uploadedFile != null)
                {
                    await UploadSong(songDTO, uploadedFile);
                    await _context.Create(songDTO);
                }
            }
            //ViewBag.Genres = new SelectList(await _genreService.GetAllGenres(), "Id", "Name", songDTO.GenreId);

            return new ObjectResult(songDTO);
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

        [HttpPut]
        public async Task<ActionResult<SongDTO>> PutSong(SongDTO songDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Update(songDTO);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            return Ok("Песня обновлена.");
        }

        [HttpDelete]
        public async Task<ActionResult<SongDTO>> Delete(int id)
        {
            var song = await _context.GetSongById(id);
            if (song != null)
            {
                await _context.Delete(id);
            }
            return Ok("Песня удалена.");
        }
       
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;
using MusicPortal.Models;

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
        public async Task <ActionResult<SongDTO>> PostSong([FromForm] string name, [FromForm] string author, [FromForm] int genreId, [FromForm] string genre, [FromForm] IFormFile file)
        {
            if (file == null)
                return BadRequest("Файл не загружен");

            var songsByAuthor = await _context.GetSongsByAuthor(author);
            SongDTO songDTO = songsByAuthor.Where(s => s.Name == name).FirstOrDefault();

            if (songDTO != null)
            {
                return BadRequest("Такая песня есть");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    songDTO = new SongDTO();
                    songDTO.Name = name;
                    songDTO.Author = author; 
                    songDTO.GenreId = genreId;
                    songDTO.Genre = genre;
                    await UploadSong(songDTO, file);
                    await _context.Create(songDTO);
                }
            }          
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
            return new ObjectResult(songDTO);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SongDTO>> DeleteSong(int id)
        {
            var song = await _context.GetSongById(id);
            if (song != null)
            {
                await _context.Delete(id);
            }
            return Ok(id);
        }
       
    }
}

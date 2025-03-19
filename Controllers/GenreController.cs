using Microsoft.AspNetCore.Mvc;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;

namespace MusicPortal.Controllers
{
    [ApiController]
    [Route("api/Genres")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _context;
        private readonly ISongService _songService;
        public GenreController(IGenreService context, ISongService songService)
        {
            _context = context;
            _songService = songService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDTO>>> GetGenres()
        {
            var genres = await _context.GetAllGenres();
            if(genres == null)
            {
                return NotFound();
            }
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenreDTO>> GetGenre(int id)
        {
            var genre = await _context.GetGenreById(id);
            if (genre == null)
            {
                return NotFound();
            }
            return new ObjectResult(genre);
        }

        [HttpPost]
        public async Task<ActionResult<GenreDTO>> PostGenre(GenreDTO genre)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _context.CreateGenre(genre);
            return Ok(genre); 
        }

        [HttpDelete]
        public async Task<ActionResult<GenreDTO>> DeleteGenre(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var genre =await _context.GetGenreById(id);
            if (genre == null)
            {
                return NotFound();
            }
            await _context.DeleteGenre(genre.Id);
            return Ok("Жанр удален.");
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MusicPortal.Models
{
    public class UploadModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Path { get; set; }  //путь к папке с песней

        public int? GenreId { get; set; } //для реализации комбобокса
              
        public string? Genre { get; set; } //отображение жанра
        public IFormFile File { get; set; }

    }
}

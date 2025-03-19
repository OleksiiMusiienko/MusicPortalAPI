using Microsoft.EntityFrameworkCore;
using Portal.BLL.Infrastructure;
using Portal.BLL.Interfaces;
using Portal.BLL.Services;
namespace MusicPortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddPortalContext(connection);
            builder.Services.AddControllers();
            builder.Services.AddUnitOfWorkService();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IGenreService, GenreService>();
            builder.Services.AddTransient<ISongService, SongService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

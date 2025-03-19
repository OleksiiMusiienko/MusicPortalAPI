using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Models;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;


namespace MusicPortal.Controllers
{
    [ApiController]
    [Route("api/Users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _context;
        public UsersController(IUserService context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.GetAllUsers();
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }

        [HttpGet("id")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUser( int id)
        {
            var user = await _context.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return new ObjectResult(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(RegisterModel user)
        {
            UserDTO userDto = await _context.GetUserByLog(user.LoginMail);
            if (userDto != null)
            {
                return BadRequest("Такой логин занят!");
                
            }
            userDto = new UserDTO();
            if (ModelState.IsValid)
            {
                if (user.Name == "Admin" || user.Name == "admin" || user.Name == "Administrator" || user.Name == "administrator"
                    || user.Name == "Админ" || user.Name == "Администратор")
                {
                    if (await _context.GetAdmin())
                    {
                        return BadRequest("Имя администратора использовать запрещено!");                        
                    }
                    else
                    {
                        userDto.Name = user.Name;
                        userDto.LoginMail = user.LoginMail;
                        userDto.Password = user.Password;
                        userDto.StatusAdmin = true;
                        userDto.Register = true;
                        await _context.CreateUser(userDto);
                        return new ObjectResult(userDto);
                    }
                }
                userDto.Name = user.Name;
                userDto.LoginMail = user.LoginMail;
                userDto.Password = user.Password;
                userDto.StatusAdmin = false;
                userDto.Register = user.Register;
                await _context.CreateUser(userDto);                
            }
            return new ObjectResult(userDto);
        }


        [HttpPut]
        public async Task<ActionResult<UserDTO>> PutUser(RegisterModel us)
        {
            UserDTO user = new UserDTO();
            if (us != null)
            {
                user = new UserDTO
                {
                    Id = us.Id,
                    Name = us.Name,
                    LoginMail = us.LoginMail,
                    Password = us.Password,
                    Register = true,
                    DateReg = us.DateReg,
                    StatusAdmin = us.StatusAdmin,
                };

                if (HttpContext.Session.GetInt32("Admin") == 1)
                {
                    user.StatusAdmin = true;
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        await _context.UpdateUser(user, false);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return NotFound();
                    }
                }
            }
            return new ObjectResult(user);
        }

       [HttpDelete]
        public async Task<ActionResult<UserDTO>> Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null)
            {
                return NotFound();
            }           
            var user = await _context.GetUserById((int)id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
               await _context.DeleteUser((int)id);
            }
            return Ok("Пользователь удален");
        }         
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Models;
using Portal.BLL.DTO;
using Portal.BLL.Interfaces;
using System.Security.Cryptography;
using System.Text;


namespace MusicPortal.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _context;
        public UsersController(IUserService context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.GetAllUsers());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

           UserDTO us = await _context.GetUserById((int)id);
            if (us == null)
            {
                return NotFound();
            }

            return View(us);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("Admin") == null)
            {
                HttpContext.Session.Clear();
                ViewBag.UserId = null;
            }
			return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,LoginMail,Password,PasswordConfirm,Register,DateReg")] RegisterModel user)
        {
            UserDTO userDto = await _context.GetUserByLog(user.LoginMail);
            if (userDto != null)
            {
                ModelState.AddModelError("LoginMail", "Такой логин занят!");
                return View(user);
            }
            userDto = new UserDTO();
            if (ModelState.IsValid)
            {
                if (user.Name == "Admin" || user.Name == "admin" || user.Name == "Administrator" || user.Name == "administrator"
                    || user.Name == "Админ" || user.Name == "Администратор")
                {
                    if (await _context.GetAdmin())
                    {
                        ModelState.AddModelError("Name", "Имя администратора использовать запрещено!");
                        return View(user);
                    }
                    else
                    {
                        userDto.Name = user.Name;
                        userDto.LoginMail = user.LoginMail;
                        userDto.Password = user.Password;
                        userDto.StatusAdmin = true;
                        userDto.Register = true;
                        await _context.CreateUser(userDto);
                        return RedirectToAction("Logon", "Users");
                    }
                }

                userDto.Name = user.Name;
                userDto.LoginMail = user.LoginMail;
                userDto.Password = user.Password;
                userDto.StatusAdmin = false;
                userDto.Register = user.Register;
                await _context.CreateUser(userDto);
                if (HttpContext.Session.GetInt32("Admin") == 1)
                {
                    return RedirectToAction("Index", "Users");
                }
                return RedirectToAction("Index", "Song");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userBase = await _context.GetUserById((int)id);
            if (userBase == null)
            {
                return NotFound();
            }
            RegisterModel model = new RegisterModel();
            model.Id = userBase.Id;
            model.Name = userBase.Name;
            model.LoginMail = userBase.LoginMail;
            model.Register = userBase.Register;
            model.DateReg = userBase.DateReg;
            model.StatusAdmin = userBase.StatusAdmin;
            return View(model);         
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("Id,Name,LoginMail,Password,PasswordConfirm,Register,StatusAdmin,DateReg")] RegisterModel us)
        {
            if (id != us.Id)
            {
                return NotFound();
            }
            UserDTO user = new UserDTO
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
                return RedirectToAction("Logon");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
           
            var user = await _context.GetUserById((int)id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.GetUserById((int)id);
            if (user != null)
            {
                await _context.DeleteUser((int)id);
            }
            TempData["Message"] = "Пользователь удален!";
            return RedirectToAction(nameof(Index));
        }

        //авторизация пользователя
        public IActionResult Logon()
        {
            if (HttpContext.Session.GetString("Name")!= null)
            {
                HttpContext.Session.Clear();
                ViewBag.UserId = null;
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logon(LoginModel logon)
        {
            if (ModelState.IsValid)
            {
                UserDTO udto = await _context.GetUserByLog(logon.LoginMail);
                if (udto == null)
                {
                    ModelState.AddModelError("", "Ошибка в логине или пароле!");
                    return View(logon);
                }
                else if(!udto.Register)
                {
                    ModelState.AddModelError("", "Ожидается подтверждение регистрации!");
                    return View(logon);
                }
                
                string? salt = udto.Salt;

                //переводим пароль в байт-массив  
                byte[] password = Encoding.Unicode.GetBytes(salt + logon.Password);

                //вычисляем хеш-представление в байтах  
                byte[] byteHash = SHA256.HashData(password);

                StringBuilder hash = new StringBuilder(byteHash.Length);
                for (int i = 0; i < byteHash.Length; i++)
                    hash.Append(string.Format("{0:X2}", byteHash[i]));

                if (udto.Password != hash.ToString())
                {
                    ModelState.AddModelError("", "Ошибка в логине или пароле!");
                    return View(logon);
                }
                HttpContext.Session.SetString("Name", udto.Name);
                HttpContext.Session.SetString("Login", udto.LoginMail);
                HttpContext.Session.SetInt32("Ident", udto.Id);
                if (udto.StatusAdmin)
                {
                    HttpContext.Session.SetInt32("Admin", 1);
                }
                return RedirectToAction("Index", "Song");
            }
            return View(logon);
        }

        public async Task<IActionResult> Confirm()
        {
            return View(await _context.GetUsersRegister());
        }
        public async Task<IActionResult> RegisterConfirm(UserDTO user)
        {
            if (user == null)
            {
                return NotFound();
            }
            user.Register = true;
                try
                {
                    await _context.UpdateUser(user, true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }                     
            return RedirectToAction("Confirm");
        }
        public async Task<IActionResult> EditAdmin(int? id)
        {           
            if (id == null)
            {
                return NotFound();
            }
            var userBase = await _context.GetUserById((int)id);            
            if(userBase == null)
            {
                return NotFound();
            }
            EditUserAdminModel model = new EditUserAdminModel();
            model.Id = userBase.Id;
            model.Name = userBase.Name;
            model.Register = userBase.Register;
            model.StatusAdmin = userBase.StatusAdmin;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(int id,EditUserAdminModel  user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                UserDTO userDTO = await _context.GetUserById(user.Id);
                userDTO.StatusAdmin = user.StatusAdmin;
                userDTO.Register = user.Register;
                userDTO.Name = user.Name;
                try
                {             
                    await _context.UpdateUser(userDTO);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            return RedirectToAction("Index");
        }
    }
}

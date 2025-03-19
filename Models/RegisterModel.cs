using System.ComponentModel.DataAnnotations;

namespace MusicPortal.Models
{
    public class RegisterModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Имя")]        
        public string? Name { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Логин(E-mail)")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес электронной почты")]
        public string? LoginMail { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])\S{6,16}$", ErrorMessage = "Не менее 6 символов, A, a, спецсимволы")]
        public string? Password { get; set; } //при доступе админа в пароль присваивать null for  - public async Task UpdateUser(UserDTO userDTO), там проверка

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        public string? PasswordConfirm { get; set; }

        [Display(Name = "Зарегистриван")]
        public bool Register {  get; set; }
        public string? DateReg { get; set; }
        public bool StatusAdmin { get; set; }

    }
}

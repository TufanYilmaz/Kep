using System.ComponentModel.DataAnnotations;


namespace KepNotificationDev.Models
{
    public class User
    {
        //public int Id { get; set; }
        [Display(Name = "Kullanıcı")]
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string Username { get; set; }
        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola gereklidir.")]
        public string Password { get; set; }
        [Display(Name = "Kullanıcı Sözleşmesini okudum ve kabul ediyorum.")]
        [Required(ErrorMessage = "Kullanıcı Sözleşmesini kabul edilmedi.")]
        public bool Checked { get; set; }
    }
}
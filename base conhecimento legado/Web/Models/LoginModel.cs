using System.ComponentModel.DataAnnotations;
using Domain;

namespace Web.Models
{
    public class LoginModel
    {
        [UpperCaseAttribute]
        [Required(ErrorMessage = "Login obrigatório")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Senha obrigatório")]
        [DataType(DataType.Password, ErrorMessage = "Senha obrigatório!")]
        public string Senha { get; set; }
        public string Empresa { get; set; }
    }
}
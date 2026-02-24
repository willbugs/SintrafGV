using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Retorno
    {
        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,80}$", ErrorMessage = "Existem caracteres inválidos!")]
        public string Redirecao { get; set; }
    }
}
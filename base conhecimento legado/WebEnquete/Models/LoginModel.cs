using System;
using System.ComponentModel.DataAnnotations;
using Domain;

namespace Web.Models
{
    public class LoginModel
    {
        [UpperCaseAttribute]
        [Required(ErrorMessage = "CPF obrigatório")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "Matrícula obrigatória")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "Data nascimento obrigatória")]
        public DateTime? DataNasc { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class CLogEnqueteLogin : CBase
    {
        [Required(ErrorMessage = "Data obrigatória")]
        public virtual DateTime DATA { get; set; }
        public virtual CPessoas PESSOA { get; set; }
        [Required(ErrorMessage = "CPF obrigatório")]
        public virtual string CPF { get; set; }
        [Required(ErrorMessage = "Matrícula obrigatório")]
        public virtual string MATRICULA { get; set; }
        [Required(ErrorMessage = "Data nascimento obrigatória")]
        public virtual DateTime? DATANASCIMENTO { get; set; }
    }
}

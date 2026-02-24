using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class CParametros : CBase
    {
        [Display(Name = "Endereço servidor email")]
        [Required(ErrorMessage = "Numero da porta requerido.")]
        public virtual string SMTP { get; set; }
        
        [Display(Name = "Login do servidor")]
        [Required(ErrorMessage = "Email de login obrigatório.")]
        public virtual string LOGIN { get; set; }
        
        [Display(Name = "Senha servidor email")]
        [Required(ErrorMessage = "Senha obrigatória.")]
        public virtual string SENHA { get; set; }
        
        [Required(ErrorMessage = "Porta servidor obrigatória..")]
        [Display(Name = "Porta do servidor")]
        public virtual string PORTA { get; set; }
        
        [Display(Name = "Email para notificação de novos cadastros")]
        [Required(ErrorMessage = "Email ativação obrigatório.")]
        public virtual string EMAILATIVACOES { get; set; }
    }
}

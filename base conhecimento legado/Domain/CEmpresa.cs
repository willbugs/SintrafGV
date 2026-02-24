using System.ComponentModel.DataAnnotations;


namespace Domain
{
    public class CEmpresa : CBase
    {
        [Required(ErrorMessage = "Nome reduzido obrigatório!")]
        public virtual string NOMEREDUZIDO { get; set; }
        [Required(ErrorMessage = "CNPJ obrigatório")]
        public virtual string EMITENTE_CNPJ { get; set; }
        public virtual string EMITENTE_IE { get; set; }
        [Required(ErrorMessage = "Razão Social obrigatório")]
        public virtual string EMITENTE_RAZAOSOCIAL { get; set; }
        public virtual string EMITENTE_FANTASIA { get; set; }
        [Required(ErrorMessage = "UF obrigatória!")]
        public virtual string EMITENTE_UF { get; set; }
    }
}

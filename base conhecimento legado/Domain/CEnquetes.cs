using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Domain
{
    public class CEnquetes : CBase
    {
        private bool _ativo;

        [Display(Name = "Data inicio da votação")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required(ErrorMessage = "Data de inicio da votação obrigatória.")]
        public virtual DateTime DATA { get; set; }

        [Required(ErrorMessage = "Hora de inicio da votação obrigatória.")]
        public virtual string HORAINICIO { get; set; }

        [Required(ErrorMessage = "Data encerramento de votação obrigatória.")]
        public virtual DateTime? DATARESULTADO { get; set; }

        [Required(ErrorMessage = "Hora encerramento de votação obrigatória.")]
        public virtual string HORAFINAL { get; set; }

        [Display(Name = "Título enquete")]
        [Required(ErrorMessage = "Título da enquete obrigatorio.")]
        public virtual string TITULO { get; set; }
        [Display(Name = "Descrição enquete")]
        [Required(ErrorMessage = "Descrição da enquete obrigatorio.")]
        public virtual string DESCRICAO { get; set; }

        [Display(Name = "Arquivo anexo")]
        public virtual string ARQUIVOANEXO { get; set; }
        public virtual HttpPostedFileBase ARQUIVO { get; set; }
        [Display(Name = "Banco")]
        public virtual CBancos BANCO { get; set; }
        [Display(Name = "Enquete ativa")]
        [Required(ErrorMessage = "Enquete ativa obrigatória")]
        public virtual bool ATIVO { get; set; }
        [Required(ErrorMessage = "Enquete para associados obrigatório")]
        public virtual bool ASSOCIADO { get; set; }

        [Required(ErrorMessage = "Pergunta obrigatoria.")]
        public virtual string PERGUNTA { get; set; }
        [Required(ErrorMessage = "Opção de resposta 01 obrigatoria.")]
        public virtual string RESPOSTA01 { get; set; }
        [Required(ErrorMessage = "Opção de resposta 02 obrigatoria.")]
        public virtual string RESPOSTA02 { get; set; }

        public virtual List<SelectListItem> DropBANCO { get; set; }
        public virtual List<SelectListItem> DropASSOCIADO { get; set; }
        public virtual List<SelectListItem> DropATIVO { get; set; }
        public virtual string TempATIVO
        {
            get => ATIVO ? "Sim" : "Não";
            set { }
        }
        public virtual string TempBANCO
        {
            get => BANCO == null ? "" : BANCO.NOME;
            set { }
        }
        public CEnquetes()
        {
            RESPOSTA01 = "SIM";
            RESPOSTA02 = "NÃO";
            DATA = DateTime.Now;
            ATIVO = true;
            DropBANCO = new List<SelectListItem>();
            DropASSOCIADO = new List<SelectListItem>
            {
                new SelectListItem{Value = "True", Text = "Sim"},
                new SelectListItem{Value = "False", Text = "Não"},
            };
            DropATIVO = new List<SelectListItem>
            {
                new SelectListItem{Value = "True", Text = "Sim"},
                new SelectListItem{Value = "False", Text = "Não"},
            };
        }
    }
}

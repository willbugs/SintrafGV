using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Web.Models
{
    public class RelatorioModel
    {
        [Required(ErrorMessage = "Selecione um relatório")]
        public virtual string Relatorio { get; set; }
        public virtual List<SelectListItem> DropRelatorios { get; set; }

        public RelatorioModel()
        {
            DropRelatorios = new List<SelectListItem> { new SelectListItem { Text = "", Value = "" } };
        }

    }
}
using System.Collections.Generic;
using System.Web.Mvc;

namespace Web.Models
{
    public class FiltroModel
    {
        public virtual int Registros { get; set; }
        public virtual string Chavebusca { get; set; }
        public virtual string Ordem { get; set; }
        public virtual List<SelectListItem> Ordens { get; set; }
        public FiltroModel()
        {
            Registros = 0;
            Ordens = new List<SelectListItem>();
        }
    }
}
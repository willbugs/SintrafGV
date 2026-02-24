using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Domain
{
    public class CUsuariosEmpresas : CBase
    {
        public virtual CUsuarios PUSUARIO { get; set; }
        [Required(ErrorMessage = "Empresa obrigatório")]
        public virtual CEmpresa EMPRESA { get; set; }
        public virtual List<SelectListItem> DropEMPRESA { get; set; }

        public virtual string TempPUSUARIO
        {
            get
            {
                return PUSUARIO.NOMEUSUARIO;
            }

        }
        public virtual string TempEMPRESA
        {
            get
            {
                return EMPRESA.NOMEREDUZIDO;
            }

        }

        public CUsuariosEmpresas()
        {
            DropEMPRESA = new List<SelectListItem> { };
        }

    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Domain
{
    public class CGrupoUsuariosPermissoes: CBase
    {
        public virtual CGrupoUsuarios Proprietario { get; set; }

        private CSubMenus _submenu;
        public virtual CSubMenus Submenus
        {
            get
            {
                if (_submenu != null)
                    Atela = string.Format("{0:n} - {1}", Ativo == 0 ? " " : "A", _submenu.NomeTela);
                return _submenu;
            }
            set
            {
                _submenu = value;
                if (_submenu != null)
                    Atela = string.Format("{0:n} - {1}", Ativo == 0 ? " " : "A", _submenu.NomeTela);
            }
        }
        public virtual List<SelectListItem> DropSubmenus { get; set; }

        [Required(ErrorMessage = "* obrigatório!")]
        public virtual string Atela { get; set; }

        public virtual int Ativo { get; set; }

        public CGrupoUsuariosPermissoes()
        {
            DropSubmenus = new List<SelectListItem>();
        }

    }
}

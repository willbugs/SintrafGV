using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Domain
{
    public class CUsuariosPermissoes : CBase
    {
        public virtual CUsuarios Proprietario { get; set; }
        private CSubMenus _submenu;
        public virtual CSubMenus Submenus
        {
            get
            {
                if (_submenu != null)
                    Atela = $"{(Ativo == 0 ? " " : "A"):n} - {_submenu.Descricao}";
                return _submenu;
            }
            set
            {
                _submenu = value;
                if (_submenu != null)
                    Atela = $"{(Ativo == 0 ? " " : "A"):n} - {_submenu.Descricao}";
            }
        }
        public virtual List<SelectListItem> DropSubmenus { get; set; }
        [Required(ErrorMessage = "* obrigatório!")]
        public virtual string Atela { get; set; }
        public virtual int Ativo { get; set; }
        
        public CUsuariosPermissoes()
        {
            DropSubmenus = new List<SelectListItem>();
        }

    }
}

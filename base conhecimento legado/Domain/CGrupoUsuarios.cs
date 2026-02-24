using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Domain
{
    public class CGrupoUsuarios : CBase
    {
        [Required(ErrorMessage = "Campo obrigatório!")]
        public virtual string Nomegrupo { get; set; }
        public virtual IList<CGrupoUsuariosPermissoes> GRUPOUSUARIOSPERMISSOES { get; set; }

        public CGrupoUsuarios()
        {
            GRUPOUSUARIOSPERMISSOES = new List<CGrupoUsuariosPermissoes>();
        }
        public virtual void LimparListas()
        {
            GRUPOUSUARIOSPERMISSOES.Clear();
        }

        public virtual void AddGrupoUsuariosPermissoes(CGrupoUsuariosPermissoes entidade)
        {
            if (GRUPOUSUARIOSPERMISSOES.Any(e => e.Submenus.Sguid.Equals(entidade.Submenus.Sguid))) return;
            GRUPOUSUARIOSPERMISSOES.Add(entidade);
        }
    }
}

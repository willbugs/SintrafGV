using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Domain
{
    public class CUsuarios : CBase
    {
        private string _tempfoto;
        
        [Required(ErrorMessage = "Campo obrigatório")]
        [MaxLength(50, ErrorMessage = "Tamanho maximo 50 caracteres!")]
        public virtual string LOGINUSUARIO { get; set; }
        [Required(ErrorMessage = "Campo obrigatório")]
        [MaxLength(30, ErrorMessage = "Tamanho maximo 30 caracteres!")]
        [DataType(DataType.Password, ErrorMessage = "PASSWORD Invalido!")]
        public virtual string SENHA { get; set; }
        public virtual IList<CUsuariosEmpresas> USUARIOSEMPRESAS { get; set; }
        [Required(ErrorMessage = "Campo obrigatório")]
        [MaxLength(50, ErrorMessage = "Tamanho maximo 50 caracteres!")]
        public virtual string NOMEUSUARIO { get; set; }
        [MaxLength(1, ErrorMessage = "Tamanho maximo 1 caracter!")]
        public virtual string TIPOUSUARIO { get; set; }
        public virtual DateTime? Ultimadataconexao { get; set; }
        [RegularExpression(@"^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})$", ErrorMessage = "Existem caracteres inválidos!")]
        public virtual string Ultimoipconecao { get; set; }
        public virtual string Sessao { get; set; }
        public virtual byte[] FOTO { get; set; }
        public virtual string TempFoto
        {
            get
            {
                if (FOTO != null) TempFoto = $"data:image/jpeg;base64,{Convert.ToBase64String(FOTO)}";
                return _tempfoto;
            }
            set
            {
                if (value != null)
                {
                    var posicao = (value.IndexOf(",", StringComparison.Ordinal) + 1);
                    FOTO = Convert.FromBase64String(value.Substring(posicao, value.Length - posicao));
                }

                _tempfoto = value;
            }
        }
        public virtual List<SelectListItem> DropTIPOUSUARIO { get; set; }
        public virtual List<SelectListItem> DropUSUARIOSEMPRESAS { get; set; }
        public virtual int Logado { get; set; }
        public virtual IList<CUsuariosPermissoes> USUARIOSPERMISSOES { get; set; }
        public virtual CGrupoUsuarios GrupoUsuarios { get; set; }
        public virtual List<SelectListItem> DropGrupoUsuarios { get; set; }
        public CUsuarios()
        {
            Logado = 0;
            Sessao = "";
            USUARIOSEMPRESAS = new List<CUsuariosEmpresas>();
            USUARIOSPERMISSOES = new List<CUsuariosPermissoes>();
            DropGrupoUsuarios = new List<SelectListItem>();
            DropTIPOUSUARIO = new List<SelectListItem>
            {
                new SelectListItem {Text = "ADMINISTRATIVO", Value = "A"},
                new SelectListItem {Text = "ECOMMERCE", Value = "E"},
                new SelectListItem {Text = "PROVEDOR", Value = "P"}
            };
        }

        public virtual void LimparPermissoes()
        {
            USUARIOSPERMISSOES.Clear();
        }

        public virtual void LimparListas()
        {
            USUARIOSEMPRESAS.Clear();
            USUARIOSPERMISSOES.Clear();
        }

        public virtual void AddUsuariosEmpresas(CUsuariosEmpresas entidade)
        {
            entidade.PUSUARIO = this;
            USUARIOSEMPRESAS.Add(entidade);
        }

        public virtual void AddUsuariosPermissoes(CUsuariosPermissoes entidade)
        {
            if (USUARIOSPERMISSOES.Any(e => e.Submenus.Sguid.Equals(entidade.Submenus.Sguid))) return;
            entidade.Proprietario = this;
            USUARIOSPERMISSOES.Add(entidade);
        }
    }
}

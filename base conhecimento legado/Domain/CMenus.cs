using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class CMenus : CBase1
    {
        [Required(ErrorMessage = "Campo requerido!")]
        public virtual string Menu { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int Acesso { get; set; }

        public virtual IList<CSubMenus> SubMenu { get; set; }

        public virtual CUsuarios Usuario { get; set; }

        public virtual int Sequencia { get; set; }

        public virtual string Icone { get; set; }

        public CMenus()
        {
            SubMenu = new List<CSubMenus>();
        }
    }
    public class CSubMenus : CBase1
    {
        public virtual CMenus Menu { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual string Descricao { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int Acesso { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int Tipo { get; set; } // refere-se a: 0 submenu, 1 tela, 2 divisao

        public virtual IList<CTelas> Tela { get; set; }

        public virtual string NomeTela { get; set; }

        public virtual int Sequencia { get; set; }

        public virtual string Atela { get; set; }

        public virtual IList<CUsuariosPermissoes> UsuariosParmissoes { get; set; }

        public CSubMenus()
        {
            Tela = new List<CTelas>();
            Tipo = 0;
            Acesso = 0;
            UsuariosParmissoes = new List<CUsuariosPermissoes>();
        }

    }
    public class CTelas : CBase1
    {
        public virtual int Sequencia { get; set; }

        public virtual CSubMenus SubMenu { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual string Help { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual string NomeTela { get; set; }

        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int Acesso { get; set; }

        public virtual string Parent { get; set; }

        public virtual IList<CCampos> Campos { get; set; }

        public virtual int Lista { get; set; }

        public virtual int Novo { get; set; }

        public virtual int Apagar { get; set; }

        public virtual int Seleciona { get; set; }

        public virtual int Importar { get; set; }

        public virtual IList<CRelatorios>  Relatorios { get; set; }

        public CTelas()
        {
            Campos = new List<CCampos>();
            Acesso = 0;
        }
    }
    public class CCampos : CBase1
    {
        public virtual int Sequencia { get; set; }
        public virtual CTelas Tela { get; set; }
        public virtual string Descricao { get; set; }
        [Required(ErrorMessage = "Campo requerido!")]
        public virtual string Campo { get; set; }
        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int Visivel { get; set; }
        [Required(ErrorMessage = "Campo requerido!")]
        public virtual int ReadOnly { get; set; }
        public virtual string Tipo { get; set; } // 0- campo 1- Drop 2- Data 3- Valor 4- Memo - 5 drop de outra tabela
        public virtual int Lista { get; set; }
        public virtual string Listar { get; set; }
        public virtual string Source { get; set; }
        public virtual string Mascara { get; set; }
        public virtual string CampoSource { get; set; }
        public virtual string IdCampo { get; set; }
        public virtual int Grid { get; set; }
        public virtual int Chave { get; set; }
        public virtual string FuncaoBusca { get; set; }
        public virtual int Adicionar { get; set; }
        public virtual string Collapse { get; set; }
        public virtual int Unico { get; set; }
        public CCampos()
        {
            ReadOnly = 0;
            Visivel = 0;
        }
    }

    public class CRelatorios : CBase1
    {
        public virtual CTelas Tela { get; set; }
        public virtual string ReportName { get; set; }
        public virtual IList<CRelatoriosDataSet> DataSets { get; set; }
        public virtual IList<CRelatoriosFiltros> Filtros { get; set; }
        public virtual string Titulo { get; set; }
        public virtual string Extensao { get; set; }
    }

    public class CRelatoriosDataSet : CBase1
    {
        public virtual CRelatorios Relatorio { get; set; }
        public virtual string DataSetName { get; set; }
        public virtual string MetodoName { get; set; }
    }

    public class CRelatoriosFiltros : CBase1
    {
        public virtual CRelatorios Relatorio { get; set; }
        public virtual string TituloCampo { get; set; }
        public virtual string NomeCampo { get; set; }
        public virtual int TipoCampo { get; set; }
        public virtual string Source { get; set; }
        public virtual string SourceField { get; set; }
        public virtual bool Requerido { get; set; }
    }

}
using FluentNHibernate.Mapping;
using Domain;

namespace Persistencia
{
    public class MMenus : ClassMap<CMenus>
    {
        public MMenus()
        {
            Map(e => e.Menu).Length(40).Not.Nullable();
            Map(e => e.Acesso).Not.Nullable();
            Map(e => e.Sequencia);
            Map(e => e.Icone).Length(20);
            HasMany(e => e.SubMenu).KeyNullable().Inverse().KeyColumn("Menu").ForeignKeyConstraintName("FK_MENUS_SUBMENUS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("MENUS");
        }

    }
    public class MSubMenus : ClassMap<CSubMenus>
    {
        public MSubMenus()
        {
            Map(e => e.Sequencia);
            Map(e => e.Descricao).Length(40).Not.Nullable();
            Map(e => e.NomeTela).Length(40).Not.Nullable();
            Map(e => e.Acesso).Not.Nullable();
            Map(e => e.Tipo).Not.Nullable();
            References(e => e.Menu).Column("Menu").ForeignKey("FK_MENUS_SUBMENUS").LazyLoad();
            HasMany(e => e.Tela).KeyNullable().Inverse().KeyColumn("SubMenu").ForeignKeyConstraintName("FK_SUBMENUS_TELAS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            HasMany(e => e.UsuariosParmissoes).KeyNullable().Inverse().KeyColumn("Submenus").ForeignKeyConstraintName("FK_USUARIOSPERMISSOES_SUBMENUS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("SUBMENUS");
        }

    }
    public class MTelas : ClassMap<CTelas>
    {
        public MTelas()
        {
            Map(e => e.Sequencia);
            Map(e => e.Novo);
            Map(e => e.Apagar);
            Map(e => e.Seleciona);
            Map(e => e.Importar);
            Map(e => e.Help).Length(80).Not.Nullable();
            Map(e => e.Acesso).Not.Nullable();
            Map(e => e.NomeTela).Length(40).Not.Nullable();
            Map(e => e.Parent).Length(40).Not.Nullable();
            Map(e => e.Lista).Not.Nullable();
            References(e => e.SubMenu).Column("SubMenu").ForeignKey("FK_SUBMENUS_TELAS").LazyLoad();
            HasMany(e => e.Campos).KeyNullable().Inverse().KeyColumn("Tela").ForeignKeyConstraintName("FK_TELAS_CAMPOS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            HasMany(e => e.Relatorios).KeyNullable().Inverse().KeyColumn("Tela").ForeignKeyConstraintName("FK_TELAS_RELATORIOS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("TELAS");
        }

    }
    public class MCampos : ClassMap<CCampos>
    {
        public MCampos()
        {
            Map(e => e.Sequencia);
            Map(e => e.Adicionar);
            Map(e => e.Campo).Length(20).Not.Nullable();
            Map(e => e.Descricao).Length(40);
            Map(e => e.Tipo).Length(1).Not.Nullable();
            Map(e => e.Visivel).Not.Nullable();
            Map(e => e.ReadOnly).Not.Nullable();
            Map(e => e.Source).Length(40);
            Map(e => e.CampoSource).Length(40);
            Map(e => e.Mascara).Length(40);
            Map(e => e.IdCampo).Length(40);
            Map(e => e.Lista).Not.Nullable();
            Map(e => e.Listar).Length(40);
            Map(e => e.Grid).Not.Nullable();
            Map(e => e.Chave).Not.Nullable();
            Map(e => e.FuncaoBusca).Length(40);
            Map(e => e.Collapse).Length(40);
            Map(e => e.Unico);
            References(e => e.Tela).Column("Tela").ForeignKey("FK_TELAS_CAMPOS").LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("CAMPOS");
        }

    }

    public class MRelatorios : ClassMap<CRelatorios>
    {
        public MRelatorios()
        {
            Map(e => e.ReportName).Not.Nullable().Index("ReportName");
            Map(e => e.Titulo).Not.Nullable();
            Map(e => e.Extensao);
            HasMany(e => e.Filtros).KeyNullable().Inverse().KeyColumn("Relatorio").ForeignKeyConstraintName("FK_RELATORIOSFILTROS_RELATORIOS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            HasMany(e => e.DataSets).KeyNullable().Inverse().KeyColumn("Relatorio").ForeignKeyConstraintName("FK_RELATORIOSDATASETS_RELATORIOS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            References(e => e.Tela).Column("Tela").ForeignKey("FK_TELAS_RELATORIOS").LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("RELATORIOS");
        }

    }

    public class MRelatoriosDataSet : ClassMap<CRelatoriosDataSet>
    {
        public MRelatoriosDataSet()
        {
            Map(e => e.DataSetName).Not.Nullable();
            Map(e => e.MetodoName).Not.Nullable();
            References(e => e.Relatorio).Column("Relatorio").ForeignKey("FK_RELATORIOSDATASETS_RELATORIOS").LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("RELATORIOS_DATASETS");
        }

    }

    public class MRelatoriosFiltros : ClassMap<CRelatoriosFiltros>
    {
        public MRelatoriosFiltros()
        {
            Map(e => e.NomeCampo).Not.Nullable();
            Map(e => e.TituloCampo).Not.Nullable();
            Map(e => e.TipoCampo).Not.Nullable();
            Map(e => e.Source).Not.Nullable();
            Map(e => e.SourceField).Not.Nullable();
            Map(e => e.Requerido).Not.Nullable();
            References(e => e.Relatorio).Column("Relatorio").ForeignKey("FK_RELATORIOSFILTROS_RELATORIOS").LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("RELATORIOS_FILTROS");
        }

    }

}


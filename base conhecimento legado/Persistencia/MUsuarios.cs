using FluentNHibernate.Mapping;
using Domain;

namespace Persistencia
{
    public class MUsuarios : ClassMap<CUsuarios>
    {
        public MUsuarios()
        {
            Map(e => e.TIPOUSUARIO).Length(1);
            Map(e => e.Sessao).Length(40).Not.Nullable();
            Map(e => e.LOGINUSUARIO).Not.Nullable().Length(50);
            Map(e => e.SENHA).Not.Nullable().Length(30);
            Map(e => e.NOMEUSUARIO).Not.Nullable().Length(50);
            Map(e => e.Ultimadataconexao).Nullable();
            Map(e => e.Ultimoipconecao).Length(15).Nullable();
            Map(e => e.Logado).Nullable();
            Map(e => e.FOTO).CustomSqlType("VARBINARY (MAX)").Length(2147483647).Nullable();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_USUARIOS").LazyLoad();
            References(e => e.GrupoUsuarios).Column("GRUPOUSUARIOS").ForeignKey("FK_USUARIOS_GRUPOUSUARIOS").Nullable().LazyLoad();
            HasMany(e => e.USUARIOSPERMISSOES).Inverse().KeyColumn("PROPRIETARIO").ForeignKeyConstraintName("FK_USUARIOS_USUARIOSPERMISSOES").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            HasMany(e => e.USUARIOSEMPRESAS).Inverse().KeyColumn("PUSUARIO").ForeignKeyConstraintName("FK_USUARIOS_USUARIOS_EMPRESAS").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            Table("USUARIOS");
        }
    }
}

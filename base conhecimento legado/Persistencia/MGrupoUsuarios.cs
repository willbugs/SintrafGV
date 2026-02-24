using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MGrupoUsuarios : ClassMap<CGrupoUsuarios>
    {
        public MGrupoUsuarios()
        {
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Map(e => e.Nomegrupo).Length(60).Not.Nullable();
            HasMany(e => e.GRUPOUSUARIOSPERMISSOES).Inverse().KeyColumn("PROPRIETARIO").ForeignKeyConstraintName("FK_GRUPOUSUARIOS_GRUPOUSUARIOSPERMISSAO").ForeignKeyCascadeOnDelete().Cascade.AllDeleteOrphan().LazyLoad();
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_GRUPOUSUARIOS_GRUPOUSUARIOS").LazyLoad();
            Table("GRUPO_USUARIOS");
        }
    }
}

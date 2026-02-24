using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MGrupoUsuariosPermissoes : ClassMap<CGrupoUsuariosPermissoes>
    {
        public MGrupoUsuariosPermissoes()
        {
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Map(e => e.Ativo).Not.Nullable().Default("0");
            References(e => e.Proprietario).Column("PROPRIETARIO").ForeignKey("FK_GRUPOUSUARIOS_GRUPOUSUARIOSPERMISSAO").LazyLoad();
            References(e => e.Submenus).Column("SUBMENUS").ForeignKey("FK_GRUPOUSUARIOSPERMISSOES_SUBMENUS").LazyLoad();
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_GRUPOUSUARIOSPERMISSOES_USUARIOS").LazyLoad();
            Table("GRUPO_USUARIOS_PERMISSOES");

        }
    }
}

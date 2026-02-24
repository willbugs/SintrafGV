using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MUsuariosPermissoes: ClassMap<CUsuariosPermissoes>
    {
        public MUsuariosPermissoes()
        {
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Map(e => e.Ativo).Not.Nullable().Default("0");
            References(e => e.Proprietario).Column("PROPRIETARIO").ForeignKey("FK_USUARIOS_USUARIOSPERMISSOES").LazyLoad();
            References(e => e.Submenus).Column("SUBMENUS").ForeignKey("FK_USUARIOSPERMISSOES_SUBMENUS").LazyLoad();
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOSPERMISSOES_USUARIOS").LazyLoad();
            Table("USUARIOS_PERMISSOES");
        }
    }
}

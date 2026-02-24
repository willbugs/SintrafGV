using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MUsuariosEmpresas : ClassMap<CUsuariosEmpresas>
    {
        public MUsuariosEmpresas()
        {
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.PUSUARIO).Column("PUSUARIO").ForeignKey("FK_USUARIOS_USUARIOS_EMPRESAS").LazyLoad();
            References(e => e.EMPRESA).Column("EMPRESA").ForeignKey("FK_USUARIOS_EMPRESAS_EMPRESAS").LazyLoad();
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_EMPRESAS_USUARIO").LazyLoad();
            Table("USUARIOS_EMPRESAS");
        }
    }
}

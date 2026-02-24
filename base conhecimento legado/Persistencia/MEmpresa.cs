using FluentNHibernate.Mapping;
using Domain;


namespace Persistencia
{
    public class MEmpresa : ClassMap<CEmpresa>
    {
        public MEmpresa()
        {
            Map(e => e.NOMEREDUZIDO).Not.Nullable().LazyLoad();
            Map(e => e.EMITENTE_CNPJ).Not.Nullable().LazyLoad();
            Map(e => e.EMITENTE_IE).LazyLoad();
            Map(e => e.EMITENTE_RAZAOSOCIAL).Not.Nullable().LazyLoad();
            Map(e => e.EMITENTE_FANTASIA).LazyLoad();
            Map(e => e.EMITENTE_UF).LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_EMPRESAS").LazyLoad();
            Table("EMPRESAS");
        }
    }
}

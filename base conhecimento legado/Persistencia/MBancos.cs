using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MBancos : ClassMap<CBancos>
    {
        public MBancos()
        {
            Map(e => e.NOME).Not.Nullable().Index("BANCOSBANCO").LazyLoad();
            Map(e => e.NUMERO).Not.Nullable().Index("BANCOSNUMEROBANCO").LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_BANCOS").LazyLoad();
            Table("BANCOS");
        }
    }
}

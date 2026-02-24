using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MParametros : ClassMap<CParametros>
    {
        public MParametros()
        {
            Map(e => e.SMTP);
            Map(e => e.LOGIN);
            Map(e => e.SENHA);
            Map(e => e.PORTA);
            Map(e => e.EMAILATIVACOES);
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_PARAMETROS_USUARIOS").LazyLoad();
            Table("PARAMETROS");
        }
    }
}

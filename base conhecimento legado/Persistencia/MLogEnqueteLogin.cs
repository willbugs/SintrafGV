using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MLogEnqueteLogin : ClassMap<CLogEnqueteLogin>
    {
        public MLogEnqueteLogin()
        {
            Map(e => e.DATA).Not.Nullable().LazyLoad().Index("LogEnqueteLoginData");
            Map(e => e.DATANASCIMENTO).Not.Nullable().LazyLoad();
            Map(e => e.CPF).Not.Nullable().LazyLoad();
            Map(e => e.MATRICULA).Not.Nullable().LazyLoad();
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_LOGENQUETELOGIN").LazyLoad();
            References(e => e.PESSOA).Column("PESSOA").ForeignKey("FK_PESSOA_LOGENQUETELOGIN").LazyLoad();
            Table("LOGENQUETELOGIN");
        }

    }
}

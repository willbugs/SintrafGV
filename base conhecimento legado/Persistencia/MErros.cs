using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MErros : ClassMap<CErros>
    {
        public MErros()
        {
            Map(e => e.Data).Index("INDEXDATAIP");
            Map(e => e.IpServidor).Index("INDEXDATAIP");
            Map(e => e.Mensagem);
            Map(e => e.Source);
            Map(e => e.StackTrace);
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_ERROS_USUARIOS").LazyLoad();
            Table("ERROS");
        }
    }
}

using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MPessoasEnquetes : ClassMap<CPessoasEnquetes>
    {
        public MPessoasEnquetes()
        {
            Map(e => e.PERGUNTA).Index("PESSOASENQUETESPERGUNTAS");
            Map(e => e.RESPOSTA01);
            Map(e => e.RESPOSTA02);
            Id(e => e.Sguid).Not.Nullable().Length(40);
            References(e => e.PESSOA).Column("PESSOA").LazyLoad();
            References(e => e.ENQUETE).Column("ENQUETE").LazyLoad();
            References(e => e.Usuario).Column("USUARIO").LazyLoad();
            Table("PESSOASENQUETES");
        }
    }
}

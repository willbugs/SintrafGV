using Domain;
using FluentNHibernate.Mapping;

namespace Persistencia
{
    public class MEnquetes : ClassMap<CEnquetes>
    {
        public MEnquetes()
        {
            Map(e => e.DATA).Not.Nullable().Index("ENQUETES_DATA");
            Map(e => e.HORAINICIO).Not.Nullable();
            Map(e => e.HORAFINAL).Not.Nullable();
            Map(e => e.DATARESULTADO).Index("ENQUETES_DATARESULTADO");
            Map(e => e.DESCRICAO).Not.Nullable().CustomSqlType("varchar(max)");
            Map(e => e.TITULO).Not.Nullable().Index("ENQUETES_TITULO");
            Map(e => e.ARQUIVOANEXO);
            Map(e => e.ATIVO).Not.Nullable();
            Map(e => e.ASSOCIADO).Not.Nullable();
            Map(e => e.PERGUNTA).Not.Nullable();
            Map(e => e.RESPOSTA01).Not.Nullable();
            Map(e => e.RESPOSTA02).Not.Nullable();
            References(e => e.BANCO).Column("BANCO").ForeignKey("FK_BANCO_ENQUETES").LazyLoad();
            References(e => e.Usuario).Column("USUARIO").ForeignKey("FK_USUARIOS_ENQUETES").LazyLoad();
            //pessoas enquete para apagar em cascata - ou procedure?!
            Id(e => e.Sguid).Not.Nullable().Length(40);
            Table("ENQUETES");
        }
    }
}

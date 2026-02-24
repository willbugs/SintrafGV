using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;

namespace Negocios
{
    public class NPessoasEnquetes : NBase<CPessoasEnquetes>
    {
        public NPessoasEnquetes(ISession secaoBanco) : base(secaoBanco) { }

        public virtual IList<CPessoasEnquetes> RetornarListaTitulos(string enquete)
        {
            var uniqueVotes = new HashSet<string>();
            var resultList = new List<CPessoasEnquetes>();

            var allVotes = SecaoBanco.Query<CPessoasEnquetes>()
                .Where(e => e.ENQUETE.Sguid.Equals(enquete))
                .ToList();

            foreach (var vote in allVotes)
            {
                if (!uniqueVotes.Contains(vote.PESSOA.Sguid))
                {
                    uniqueVotes.Add(vote.PESSOA.Sguid);
           //         SecaoBanco.Refresh(vote);
                    resultList.Add(vote);
                }
            }

            return resultList;
        }
        public virtual CPessoasEnquetes RetornarVotoPessoaEnquete(CEnquetes enquete, CPessoas pessoa)
        {
            var lobjects = (from e in SecaoBanco.Query<CPessoasEnquetes>()
                            where e.ENQUETE.Sguid.Equals(enquete.Sguid) && e.PESSOA.Sguid.Equals(pessoa.Sguid)
                            select e).FirstOrDefault(e => true);
         //   if (lobjects != null) { SecaoBanco.Refresh(lobjects); }
            return lobjects;
        }
    }
}

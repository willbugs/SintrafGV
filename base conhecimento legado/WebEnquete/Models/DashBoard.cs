using System.Collections.Generic;
using Domain;

namespace Webcliente.Models
{
    public class DashBoard
    {
        public IList<CVendas> Vendas { get; set; }

        public DashBoard()
        {
            Vendas = new List<CVendas>();
        }
    }
}
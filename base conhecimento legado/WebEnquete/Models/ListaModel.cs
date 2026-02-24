using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain;

namespace Web.Models
{
    public class ListaModel
    {
        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public string Classe { get; set; }

        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public string Campo { get; set; }

        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public string IdCampo { get; set; }

        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public IList<CBase> Lista { get; set; }

        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public string CampoClasse { get; set; }

        [RegularExpression(@"^[0-9|a-zA-Z'., /\\-s]{1,30}$", ErrorMessage = @"Existem caracteres inválidos!")]
        public string CampoSource { get; set; }

        public string Descricao { get; set; }

        public string Collapse { get; set; }

        public int Unico { get; set; }

        public string Secao { get; set; }

        public ListaModel()
        {
            Lista = new List<CBase>();
        }
    }
}
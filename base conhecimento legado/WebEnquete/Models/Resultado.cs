using System;

namespace Web.Models
{
    public class Resultado
    {
        public virtual string Titulo { get; set; }
        public virtual DateTime? DataResultado { get; set; }
        public virtual string JsonGrafico { get; set; }
    }
}
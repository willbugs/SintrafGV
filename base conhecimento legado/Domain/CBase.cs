using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public abstract class CBase
    {
        [Required(ErrorMessage = "Identificado inválido!")]
        public virtual string Sguid { get; set; }
        public virtual CUsuarios Usuario { get; set; }
        protected CBase()
        {
            Sguid = Guid.NewGuid().ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return Sguid == ((CBase)obj).Sguid;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
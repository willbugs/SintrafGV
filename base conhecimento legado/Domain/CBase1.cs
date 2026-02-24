using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public abstract class CBase1
    {
        [Required]
        public virtual string Sguid { get; set; }
        protected CBase1()
        {
            Sguid = Guid.NewGuid().ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return this.Sguid == ((CBase)obj).Sguid;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
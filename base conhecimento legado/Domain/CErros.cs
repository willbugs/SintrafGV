using System;

namespace Domain
{
    public class CErros : CBase
    {
        public virtual string IpServidor { get; set; }
        public virtual DateTime Data { get; set; }
        public virtual string Mensagem { get; set; }
        public virtual string Source { get; set; }
        public virtual string StackTrace { get; set; }

        public CErros()
        {
            Data = DateTime.Now;
        }
    }
}

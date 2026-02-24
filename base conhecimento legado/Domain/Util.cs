using System.Globalization;

namespace Domain
{

    public class GoDaddyGodaddy
    {
        public virtual string data { get; set; }
        public virtual string name { get; set; }
        public virtual int port { get; set; }
        public virtual int priority { get; set; }
        public virtual int ttl { get; set; }
        public virtual string type { get; set; }
        public virtual int weight { get; set; }

    }

    public static class Valor
    {
        public static decimal Arredondar(decimal valor, int casasDecimais)
        {
            var valorNovo = decimal.Round(valor, casasDecimais);
            var valorNovoStr = valorNovo.ToString("F" + casasDecimais, CultureInfo.CurrentCulture);
            return decimal.Parse(valorNovoStr);
        }

        public static decimal? Arredondar(decimal? valor, int casasDecimais)
        {
            if (valor == null) return null;
            return Arredondar(valor.Value, casasDecimais);
        }
    }

    public enum Banco
    {
        SQLServer,
        PostgreSQL,
        FirebirdSql,
        MySQL,
        Memory
    }

}

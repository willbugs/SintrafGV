using System;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class CepController : Controller
    {
        public JsonResult Cep(string cep)
        {
            var sEndereco = "https://viacep.com.br/ws/" + cep + "/piped/";
            var requisicao = (HttpWebRequest)WebRequest.Create(sEndereco);
            HttpWebResponse resposta;
            object result;
            try
            {
                resposta = (HttpWebResponse)requisicao.GetResponse();    
            }
            catch (Exception)
            {
                result = new { status = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int cont;
            var buffer = new byte[1000];
            var sb = new StringBuilder();
            var stream = resposta.GetResponseStream();
            var w1252 = Encoding.GetEncoding(1252);
            var utf8 = Encoding.UTF8;
            do
            {
                cont = stream.Read(buffer, 0, buffer.Length);
                var temp = Encoding.Default.GetString(buffer, 0, cont).Trim();
                sb.Append(utf8.GetString(w1252.GetBytes(@temp)));

            } while (cont > 0);
            var pagina = sb.ToString();


            var logradouro = "";
            var bairro = "";
            var cidade = "";
            var estado = "";
            const char delimiter = '|';
            var substrings = pagina.Split(delimiter);
            try
            {
                var temp = substrings[1];
                logradouro = (temp.Substring(temp.IndexOf(":", StringComparison.Ordinal), temp.Length - temp.IndexOf(":", StringComparison.Ordinal))).Replace(":", "");
                temp = substrings[3];
                bairro = (temp.Substring(temp.IndexOf(":", StringComparison.Ordinal), temp.Length - temp.IndexOf(":", StringComparison.Ordinal))).Replace(":", "");
                temp = substrings[4];
                cidade = (temp.Substring(temp.IndexOf(":", StringComparison.Ordinal), temp.Length - temp.IndexOf(":", StringComparison.Ordinal))).Replace(":", "");
                temp = substrings[5];
                estado = (temp.Substring(temp.IndexOf(":", StringComparison.Ordinal), temp.Length - temp.IndexOf(":", StringComparison.Ordinal))).Replace(":", "");
            }
            catch (Exception)
            {
                logradouro = "";
                bairro = "";
                cidade = "";
                estado = "";
            }
            finally
            {
                result = new { state = estado.ToUpper(), city = cidade.ToUpper(), district = bairro.ToUpper(), address = logradouro.ToUpper(), status = 1 };
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
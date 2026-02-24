using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocios;
using Newtonsoft.Json;
using NHibernate;
using Ninject;
using Persistencia;

namespace Teste
{

    [TestClass]
    public class Teste
    {
        private IKernel Kernel { get; set; }
        private ISession _session;

        [TestMethod]
        private void InjetarDependencia()
        {
            Kernel = new StandardKernel();
            Kernel.Bind<ISession>().ToMethod(c => FluentSessionFactory.AbrirSession("WILLBUGS")).InSingletonScope().Named("Default");
            Kernel.Inject(this);
            _session = Kernel.Get<ISession>("Default");
        }

        [TestMethod]
        public void Testar_Pastas()
        {
            decimal valor = 0;
             var valorc = valor.ToString().Replace(',', '.');



            var root = @"D:\Sites\BigCard\Arquivos_para_download\Representantes";
            var subdirectoryEntries = Directory.GetDirectories(root);
            var xxxxx= subdirectoryEntries.Select(item => item.Replace(root, "").Replace("\\", "")).ToList();

            foreach (var ptt in xxxxx)
            {
                var rootx = $@"D:\Sites\BigCard\Arquivos_para_download\Representantes\{ptt}";
                var filePaths = Directory.GetDirectories(rootx);

                

            }


        }


        [TestMethod]
        public void GerarBanco()
        {
            FluentSessionFactory.AbrirSession("TESTE", Banco.SQLServer, true);
        }

        [TestMethod]
        public void Teste_Consulta_Direto()
        {
            InjetarDependencia();

            

            var query = _session.CreateSQLQuery("select * from pessoas where pessoas.cpf = '24897903823'");
            query.AddEntity("CPessoas", typeof(CPessoas));
            var entities = query.List();
            var lpessoas = entities.Cast<CPessoas>().ToList();
        }


        [TestMethod]
        public void Teste_Recarga()
        {
            WebRequest req = null;
            WebResponse rsp = null;
            var responsefromserver = "";

            var webClient = new WebClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                
            var formData = new NameValueCollection { ["codigo_transacao"] = "5" };

            
            formData["loja_primaria"] = "bigcard";
            formData["nome_primario"] = "bigcardweb";
            formData["senha_primaria"] = "260815";

            formData["versao"] = "3.92";
            formData["compra"] = "111";
            formData["produto"] = "11";
            formData["ddd"] = "33";
            formData["fone"] =  "999999999";
            formData["usuario_local"] = "111111111111111";

            var responseBytes = webClient.UploadValues("https://teste.cellcard.com.br/integracao_xml.php", "POST", formData);
            responsefromserver = Encoding.UTF8.GetString(responseBytes);
            webClient.Dispose();


        }

        [TestMethod]
        public void Teste_Corrigir_Bancos()
        {

            InjetarDependencia();
            var nBancos = new NBancos(_session);
            var lbancos = nBancos.RetornarTodos();
            var lnbancos = new List<CBancos>();

            var linhasArquivo = File.ReadAllLines(@"D:\Downloads\Codigo de bancos.txt", Encoding.ASCII);

            foreach (var banco in linhasArquivo.Skip(1))
            {
                var dados = banco.Split(';');
                var numero = dados[0].Substring(1, 3);
                if (!lbancos.Any(e => e.NUMERO.Equals(numero)))
                {
                    lnbancos.Add(new CBancos
                    {
                        NOME = dados[1].TrimEnd().ToUpper(),
                        NUMERO = numero
                    });
                }
            }


            nBancos.Gravar(lnbancos);

        }


        [TestMethod]
        public void Teste_Corrigir_Importacao()
        {
            InjetarDependencia();
            var nBancos = new NBancos(_session);
            var nPessoas = new NPessoas(_session);
            var lpessoas = nPessoas.RetornarTodos();
            var lbancos = nBancos.RetornarTodos();
            var linhasArquivo = File.ReadAllLines(@"D:\progs\Sintrafgv\associados.csv", Encoding.ASCII).Skip(1);

            foreach (var linha in linhasArquivo)
            {
                var campos = linha.Split(';');
                var nome = campos[2].ToUpper().Trim();
                var matricula = campos[1];
                var banco = campos[13];
                var nomeBanco = banco.ToUpper();
                var cBanco = lbancos.FirstOrDefault(e => e.NOME.Contains(nomeBanco));
                var cPessoas = lpessoas.FirstOrDefault(e => e.NOME.ToUpper().Equals(nome));
                
                if (cBanco == null || cPessoas == null) continue;

                if (cPessoas.BANCO != null) continue;
                
                cPessoas.BANCO = cBanco;
                nPessoas.Gravar(cPessoas);
            }

        }


        [TestMethod]
        public void Teste_Importacao()
        {
            var linhasArquivo = File.ReadAllLines(@"D:\progs\Sintrafgv\associados.csv", Encoding.ASCII);
            // 0           1              2              3      4   5         6       7          8       9          10        11             12      
            // ID;Matrícula Sindical;Nome do Associado;CODFUNC;CPF;Telefone;CELULAR;RESIDENCIA;BAIRRO;NATURALIDADE;ESTADO;DATNASCIMENTO;ESTADO CIVIL;
            // 13       14        15      16       17            18       19       20   21      22        23         24         25     26        27    28
            // BANCO;CODAGENCIA;AGENCIA;CIDADE;CONTA MATRICULA;FUNCAO;DATADMISSAO;CTPS;SERIE;DATFILIACAO;MOTIVO;DATDESLIGAMENTO;SEXO;CARTEIRINHA;BASE;Campo1

            InjetarDependencia();
            var nBancos = new NBancos(_session);
            var nPessoas = new NPessoas(_session);
            var listapessoas = new List<CPessoas>();
            DateTime dtn;

            foreach (var linha in linhasArquivo.Skip(1))
            {
                var campos = linha.Split(';');
                var cPessoas = new CPessoas();
                cPessoas.MATRICULASINDICATO = campos[1].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.NOME = campos[2].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "").Replace(",","");
                cPessoas.MATRICULABANCARIA = campos[3].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "").Replace(",","");
                cPessoas.CPF = campos[4].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "").Replace(",","");
                cPessoas.TELEFONE = campos[5].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.CELULAR = campos[6].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");

                if (campos[7].ToUpper().Contains("@"))
                {
                    cPessoas.EMAIL = campos[7].ToLower();
                }
                else
                {
                    cPessoas.ENDERECO = campos[7].ToUpper();    
                }
                



                cPessoas.BAIRRO = campos[8].ToUpper();
                cPessoas.NATURALIDADE = campos[9].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.ESTADO = campos[10].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                if (!string.IsNullOrEmpty(campos[11]))
                {
                    cPessoas.DATNASCIMENTO = Convert.ToDateTime(campos[11]);
                }

                cPessoas.ESTADOCIVIL = campos[12].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");

                if (cPessoas.ESTADOCIVIL.Contains("CAS")) cPessoas.ESTADOCIVIL = "CASADO";
                if (cPessoas.ESTADOCIVIL.Contains("SEPA")) cPessoas.ESTADOCIVIL = "SEPARADO";
                if (cPessoas.ESTADOCIVIL.Contains("SOLT")) cPessoas.ESTADOCIVIL = "SOLTEIRO";

                var nomebanco = campos[13].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                var cBancos = nBancos.RetornarNomeBanco(nomebanco);
                if (cBancos != null)
                {
                    cPessoas.BANCO = cBancos;
                }
                cPessoas.CODAGENCIA = campos[14].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.AGENCIA = campos[15].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.CIDADE = campos[16].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.CONTA = campos[17].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.FUNCAO = campos[18].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                if (!string.IsNullOrEmpty(campos[19]))
                {
                    cPessoas.DATADMISSAO = Convert.ToDateTime(campos[19]);
                }
                cPessoas.CTPS = campos[20].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.SERIE = campos[21].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                if (!string.IsNullOrEmpty(campos[22]))
                {
                    cPessoas.DATFILIACAO = Convert.ToDateTime(campos[22]);
                }
                cPessoas.MOTIVO = campos[23].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                if (!string.IsNullOrEmpty(campos[24]))
                {
                    cPessoas.DATDESLIGAMENTO = Convert.ToDateTime(campos[24]);
                }
                cPessoas.SEXO = campos[25].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.CARTEIRINHA = campos[26].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.BASE = campos[27].ToUpper().Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("/", "").Replace("-", "");
                cPessoas.APOSENTADO = cPessoas.MOTIVO.Contains("Aposentad");
                if (cPessoas.SEXO.ToUpper().Equals("M")) cPessoas.SEXO = "MASCULINO";
                if (cPessoas.SEXO.ToUpper().Equals("F")) cPessoas.SEXO = "FEMININO";
                if (string.IsNullOrEmpty(cPessoas.SEXO)) cPessoas.SEXO = "OUTROS";
                listapessoas.Add(cPessoas);
            }

            nPessoas.Gravar(listapessoas);
        }

        [TestMethod]
        public void Teste_Grafico()
        {

            var json = "{\"type\":\"horizontalBar\",\"data\":{\"labels\":[\"Sim\",\"Não\",\"Prefiro não votar\",\"Prefiro não 1\",\"Prefiro não 2\",\"Prefiro não 3\"],\"datasets\":[{\"label\":\"Revenue\",\"backgroundColor\":\"#4e73df\",\"borderColor\":\"#4e73df\",\"data\":[\"4500\",\"5300\",\"6250\",\"7800\",\"9800\",\"15000\"]}]},\"options\":{\"maintainAspectRatio\":true,\"legend\":{\"display\":false,\"position\":\"top\"},\"title\":{}}}";
            var grafico = JsonConvert.DeserializeObject<Grafico>(json);

            var grafic = new Grafico();

            grafic.data.labels.Add("Pergunta 1");
            grafic.data.labels.Add("Pergunta 2");
            grafic.data.labels.Add("Pergunta 3");
            grafic.data.labels.Add("Pergunta 4");
            grafic.data.labels.Add("Pergunta 5");

            var percentuais = new List<string>();
            percentuais.Add("4500");
            percentuais.Add("5300");
            percentuais.Add("6250");
            percentuais.Add("7800");
            percentuais.Add("9800");


            grafic.data.datasets.Add(new Dataset
            {
                backgroundColor = "#4e73df",
                label = "Resultado",
                borderColor = "#4e73df",
                data = percentuais

            });

            grafic.type = "horizontalBar";
            grafic.options = new Options
            {
                maintainAspectRatio = true,
                legend = new Legend { display = true, position = "top" }
            };


            var jsonnovo = JsonConvert.SerializeObject(grafic);







        }
    }
    public class Dataset
    {
        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public IList<string> data { get; set; }

        public Dataset()
        {
            data = new List<string>();
        }
    }
    public class Data
    {
        public IList<string> labels { get; set; }
        public IList<Dataset> datasets { get; set; }

        public Data()
        {
            labels = new List<string>();
            datasets = new List<Dataset>();
        }
    }
    public class Legend
    {
        public bool display { get; set; }
        public string position { get; set; }
    }
    public class Title
    {
    }
    public class Options
    {
        public bool maintainAspectRatio { get; set; }
        public Legend legend { get; set; }
        public Title title { get; set; }

        public Options()
        {
            legend = new Legend();
            title = new Title();
        }
    }
    public class Grafico
    {
        public string type { get; set; }
        public Data data { get; set; }
        public Options options { get; set; }

        public Grafico()
        {
            data = new Data();
            options = new Options();
        }
    }

}
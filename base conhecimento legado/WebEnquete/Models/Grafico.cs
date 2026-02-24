using System.Collections.Generic;

namespace Web.Models
{
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagMaui.Models
{
    public class Source
    {
        public string id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public double score { get; set; }
    }
}

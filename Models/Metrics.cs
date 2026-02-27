using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagMaui.Models
{
    public class Metrics
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
        public double duration { get; set; }
        public string model { get; set; }
    }
}

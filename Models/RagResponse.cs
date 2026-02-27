using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagMaui.Models
{
    public class RagResponse
    {
        public string id { get; set; }
        public string type { get; set; }
        public bool close { get; set; }
        public string error { get; set; }
        public int chatId { get; set; }
        public string textResponse { get; set; }
        public List<Source> sources { get; set; }
        public Metrics metrics { get; set; }
    }
}

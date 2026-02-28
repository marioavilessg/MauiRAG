using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace RagMaui.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string DocId { get; set; }
        public string Filename { get; set; }
        public string Docpath { get; set; }
    }
}

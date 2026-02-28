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
        public string text { get; set; }
        public string content { get; set; }
        public string answer { get; set; }
        public List<Source> sources { get; set; }
        public Metrics metrics { get; set; }

        public string RawJson { get; set; }

        public string GetText()
        {
            if (!string.IsNullOrWhiteSpace(textResponse)) return textResponse;
            if (!string.IsNullOrWhiteSpace(text)) return text;
            if (!string.IsNullOrWhiteSpace(content)) return content;
            if (!string.IsNullOrWhiteSpace(answer)) return answer;
            return string.Empty;
        }
    }
}

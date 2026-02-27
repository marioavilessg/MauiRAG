using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagMaui.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } // "user" o "assistant"
        public string Content { get; set; }
    }
}

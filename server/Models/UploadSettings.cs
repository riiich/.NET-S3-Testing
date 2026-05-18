using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class UploadSettings
    {
        public string[] AcceptedTypes { get; set; } = ["application/pdf", "application/rtf", "text/plain", "image/png", "image/jpeg", "image/gif"];
    }
}
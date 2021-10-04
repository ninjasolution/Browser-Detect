using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserDetect.Models
{
    public class BrowserURL
    {
        public BrowserURL(int id, string browserName, string url) 
        {
            Id = id;
            BrowserName = browserName;
            URL = url;
            DateTime = DateTime.Now;
        }
        public int Id { get; set; }
        public string BrowserName { get; set; }
        public string URL { get; set; }
        public DateTime DateTime { get; set; }
    }
}

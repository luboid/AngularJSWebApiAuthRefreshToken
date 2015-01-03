using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Browser
{
    public class BrowserContext
    {
        public int? Page { get; set; }
        public int? LastPage { get; set; }
        public int? PageSize { get; set; }
        public string Sort { get; set; }
        public SortDir? Sortdir { get; set; }

        public string SearchColumn { get; set; }
        public string[] SearchValue { get; set; }
    }
}

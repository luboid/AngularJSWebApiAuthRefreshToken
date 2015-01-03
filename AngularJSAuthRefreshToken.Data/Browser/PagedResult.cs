using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Browser
{
    public class PagedResult<T> 
        where T : class
    {
        public ICollection<T> Collection { get; set; }
        public int Count { get; set; }
        public string Sort { get; set; }
        public SortDir SortDir { get; set; }
    }
}

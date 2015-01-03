using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Browser
{
    public class Column
    {
        public Column()
        {
            Searchable = true;
            Sortable = true;
        }

        public string Name { get; set; }
        public DataType DataType { get; set; }
        public ValueCase Case { get; set; }
        public bool Searchable { get; set; }
        public bool Sortable { get; set; }
    }
}

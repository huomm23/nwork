using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App;

namespace Sk.InspectorAop
{
    class B
    {
        private string str { get; set; }

        public void Ex(B b)
        {
            Dictionary<string, shorder> dic = new Dictionary<string, shorder>();
            dic=  dic.OrderBy(p => p.Value.CREATED_AT).ThenByDescending(p => p.Value.WHO).ToDictionary(p => p.Key, p => p.Value);
        }
    }
    public class shorder
    {
        public shorder()
        {
           // this.shorderdetails = new HashSet<shorderdetails>();
        }

        public string WHO { get; set; }
        public Nullable<decimal> CREATED_AT { get; set; }
        public string PRIORITY { get; set; }
        public Nullable<int> SysFlag { get; set; }

       // public virtual ICollection<shorderdetails> shorderdetails { get; set; }
    }

}

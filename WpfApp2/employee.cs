using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class employee
    {
        public int id { get; set; }
        public int idName { get; set; }
        public int idSername { get; set; }
        public int idPosition { get; set; }
        public int idCompany { get; set; }

        public employee() { }
        public employee(int id, int idName, int idSername, int idPosition, int idCompany )
        {
            this.id = id;
            this.idName = idName;
            this.idSername = idSername;
            this.idPosition = idPosition;
            this.idCompany = idCompany;

        }
    }
}

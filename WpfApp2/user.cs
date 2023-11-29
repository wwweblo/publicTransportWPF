using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class user
    {
        public int id;
        public string login { get; set; }
        public string password { get; set; }
        public int idCompany { get; set; }
        public int idRole { get; set; }
        public user(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
        public user(int id, string login, string password, int idCompany, int idRole)
        {
            this.id = id;
            this.login = login;
            this.password = password;
            this.idCompany = idCompany;
            this.idRole = idRole;
        }
    }
}

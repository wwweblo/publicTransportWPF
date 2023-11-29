using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainMenue.xaml
    /// </summary>
    public partial class MainMenue : Window
    {
        private user currentUser;
        private SqlConnection connection;
        public MainMenue(user currentUser, SqlConnection connection)
        {
            InitializeComponent();

            this.currentUser = currentUser;
            this.connection = connection;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            userWindow uw = new userWindow(currentUser);
            uw.Show();
        }
    }
}

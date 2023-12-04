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

        //Пользователи
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.idRole == 2 || currentUser.idRole == 1)
            {
                userWindow uw = new userWindow(currentUser);
                uw.Show();
            }
        }
        //Маршруты
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (currentUser.idRole == 4 || currentUser.idRole == 2 || currentUser.idRole == 1)
            {
                routeWindow routeWindow = new routeWindow(currentUser);
                routeWindow.Show();
            }
        }

        //Компании
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (currentUser.idRole == 1)
            {
                companyWindow companyWindow = new companyWindow(currentUser);
                companyWindow.Show();
            }
        }
        //аккаунты
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (currentUser.idRole == 1)
            {
                accountWindow accountWindow = new accountWindow(currentUser);
                accountWindow.Show();
            }
        }
    }
}

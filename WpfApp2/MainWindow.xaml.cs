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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        String server_name = "meoww";
        String data_base = "public_transport_2";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void signInBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получить данные из формы
            string login = loginInput.Text;
            string password = passwordInput.Text;

            // Создать объект подключения к базе данных
            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            connection.Open();

            // Создать запрос
            string query = "SELECT id, id_company, id_role FROM account WHERE login = @login AND password = @password";

            // Создать объект подготовки запроса
            SqlCommand command = new SqlCommand(query, connection);

            // Заполнить параметры запроса
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            // Выполнить запрос
            SqlDataReader reader = command.ExecuteReader();

            // Если в базе данных найден пользователь с такими данными
            if (reader.HasRows)
            {
                // Получить id компании и роль пользователя
                reader.Read();
                int id = Convert.ToInt32(reader["id"]);
                int idRole = Convert.ToInt32(reader["id_role"]);
                int idCompany = Convert.ToInt32(reader["id_company"]);

                // Показать сообщение с id компании и ролью пользователя
                MessageBox.Show("Успешно авторизован", "Компания: " + idCompany + ", роль: " + idRole);

                user currentUser = new user(id, login, password, idCompany, idRole);
                Debug.WriteLine($"currentUser: {currentUser}");

                MainMenue menue = new MainMenue(currentUser, connection);
                menue.Show();
                this.Close();

            }
            else
            {
                // Показать сообщение об ошибке
                MessageBox.Show("Неверный логин или пароль", "Пожалуйста, попробуйте ещё раз");
            }

            // Закрыть соединение
            connection.Close();
        }


    }
}

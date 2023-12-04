using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Логика взаимодействия для companyWindow.xaml
    /// </summary>
    public partial class companyWindow : Window
    {
        string server_name = "meoww";
        string data_base = "public_transport_2";
        public user currentUser;

        public companyWindow(user currentUser)
        {
            InitializeComponent();

            this.currentUser = currentUser;

            RefreshDataGrid();
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = searchInput.Text;
            string connectionString = $"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT id, name FROM company WHERE id LIKE '%{input}%'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            string id = Convert.ToString(reader["id"]);
                            string name = Convert.ToString(reader["name"]);

                            idBox.Text = id;
                            nameBox.Text = name;
                        }
                        else
                        {
                            MessageBox.Show("Нет компаний для указанного названия");
                        }
                    }
                }
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string id = idBox.Text;
            string name = nameBox.Text;

            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            connection.Open();

            string updateQuery = "";
            string insertQuery = "";

            if (!string.IsNullOrEmpty(id))
            {
                updateQuery = $"UPDATE company SET name = '{name}' WHERE id = {id}";
            }
            else
            {
                insertQuery = $"INSERT INTO company (name) VALUES ('{name}')";
            }

            SqlCommand command;

            if (!string.IsNullOrEmpty(updateQuery))
            {
                command = new SqlCommand(updateQuery, connection);
            }
            else
            {
                command = new SqlCommand(insertQuery, connection);
            }

            command.ExecuteNonQuery();

            RefreshDataGrid();
            connection.Close();
        }

        private void RefreshDataGrid()
        {
            if (currentUser.idRole == 1)
            {
                using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
                {
                    connection.Open();

                    string query = "SELECT id, name FROM company";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        companyDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            string idToDelete = idBox.Text;

            if (!string.IsNullOrEmpty(idToDelete))
            {
                using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
                {
                    connection.Open();

                    string deleteQuery = $"DELETE FROM company WHERE id = {idToDelete}";
                    SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                    deleteCommand.ExecuteNonQuery();

                    RefreshDataGrid();
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Введите ID для удаления записи.");
            }
        }
    }
}

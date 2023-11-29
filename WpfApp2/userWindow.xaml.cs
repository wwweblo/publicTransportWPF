using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для userWindow.xaml
    /// </summary>
    public partial class userWindow : Window
    {
        private user currentUser;
        string server_name = "meoww";
        string data_base = "public_transport_2";

        public userWindow(user currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;

            RefreshDataGrid();

        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = searchInput.Text;
            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            connection.Open();

            string query = $"SELECT employee.id, name.name AS name, sername.name AS sername, middlename.name AS middlename, position.name AS position, company.name AS company" +
                $" FROM employee" +
                $" JOIN name ON employee.name = name.id" +
                $" JOIN sername ON employee.sername = sername.id" +
                $" JOIN middlename ON employee.middlename = middlename.id" +
                $" JOIN position ON employee.id_position = position.id" +
                $" JOIN company ON employee.id_company = company.id" +
                $" WHERE employee.id = {input}";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                string id = Convert.ToString(reader["id"]);
                string name = Convert.ToString(reader["name"]);
                string sername = Convert.ToString(reader["sername"]);
                string middlename = Convert.ToString(reader["middlename"]);
                string position = Convert.ToString(reader["position"]);
                string company = Convert.ToString(reader["company"]);

                idBox.Text = id;
                nameBox.Text = name;
                sernameBox.Text = sername;
                middlenameBox.Text = middlename;
                positionBox.Text = position;
                companyBox.Text = company;

            }

            connection.Close();

        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получение данных из TextBox
            string id = idBox.Text;
            string name = nameBox.Text;
            string sername = sernameBox.Text;
            string middlename = middlenameBox.Text;
            string position = positionBox.Text;
            string company = companyBox.Text;

            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            // Открываем соединение
            connection.Open();

            // Проверяем, существуют ли записи в связанных таблицах, и добавляем их, если необходимо
            AddToTableIfNotExists("name", name, connection);
            AddToTableIfNotExists("sername", sername, connection);
            AddToTableIfNotExists("middlename", middlename, connection);
            AddToTableIfNotExists("position", position, connection);
            AddToTableIfNotExists("company", company, connection);

            // Проверяем, существует ли запись в таблице employee с указанным id
            string checkIdExistenceQuery = $"SELECT COUNT(*) FROM employee WHERE id = {id}";
            SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

            int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

            if (idExistence > 0)
            {
                // Запись существует, выполняем обновление
                string updateQuery = $"UPDATE employee SET name = (SELECT id FROM name WHERE name = '{name}'), " +
                                    $"sername = (SELECT id FROM sername WHERE name = '{sername}'), " +
                                    $"middlename = (SELECT id FROM middlename WHERE name = '{middlename}'), " +
                                    $"id_position = (SELECT id FROM position WHERE name = '{position}'), " +
                                    $"id_company = (SELECT id FROM company WHERE name = '{company}') " +
                                    $"WHERE id = {id}";

                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.ExecuteNonQuery();
            }
            else
            {
                // Запись не существует, выполняем вставку
                string insertQuery = $"INSERT INTO employee (name, sername, middlename, id_position, id_company) " +
                                    $"VALUES ((SELECT id FROM name WHERE name = '{name}'), " +
                                    $"(SELECT id FROM sername WHERE name = '{sername}'), " +
                                    $"(SELECT id FROM middlename WHERE name = '{middlename}'), " +
                                    $"(SELECT id FROM position WHERE name = '{position}'), " +
                                    $"(SELECT id FROM company WHERE name = '{company}'))";

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.ExecuteNonQuery();
            }

            RefreshDataGrid();
            // Закрываем соединение
            connection.Close();
        }

        private void RefreshDataGrid()
        {
            using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
            {
                connection.Open();

                string query = "";
                string companyQuery = "";

                switch (currentUser.idRole)
                {
                    // Суперадмин
                    case 1:
                        query = "SELECT employee.id, name.name, sername.name AS sername, middlename.name AS middlename, position.name AS position, company.name AS company FROM employee" +
                                " JOIN name ON employee.name = name.id" +
                                " JOIN sername ON employee.sername =  sername.id" +
                                " JOIN middlename ON employee.middlename =  middlename.id" +
                                " JOIN position ON employee.id_position = position.id" +
                                " JOIN company ON employee.id_company = company.id";
                        break;

                    // Оператор
                    case 4:
                        query = "SELECT employee.id, name.name, sername.name AS sername, middlename.name AS middlename, position.name AS position, company.name AS company FROM employee" +
                                " JOIN name ON employee.name = name.id" +
                                " JOIN sername ON employee.sername =  sername.id" +
                                " JOIN middlename ON employee.middlename =  middlename.id" +
                                " JOIN position ON employee.id_position = position.id" +
                                $" JOIN company ON employee.id_company = company.id WHERE employee.id_company = {currentUser.idCompany}";

                        // Добавлен запрос к таблице company для получения названия компании по idCompany
                        companyQuery = $"SELECT name FROM company WHERE id = {currentUser.idCompany}";

                        break;
                }

                using (SqlCommand companyCommand = new SqlCommand(companyQuery, connection))
                {
                    if (currentUser.idRole == 4)
                    {
                        connection.Open();

                        // Отключаем редактирование companyBox
                        companyBox.IsReadOnly = true;

                        // Устанавливаем текст в companyBox равным названию компании
                        companyBox.Text = Convert.ToString(companyCommand.ExecuteScalar());

                        connection.Close();
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    employeeDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
        }

        void AddToTableIfNotExists(string tableName, string value, SqlConnection conn)
        {
            string checkExistenceQuery = $"SELECT id FROM {tableName} WHERE name = '{value}'";
            SqlCommand checkExistenceCommand = new SqlCommand(checkExistenceQuery, conn);

            object result = checkExistenceCommand.ExecuteScalar();

            if (result == null)
            {
                // Значение отсутствует в таблице, добавляем его
                string insertQuery = $"INSERT INTO {tableName} (name) VALUES ('{value}')";
                SqlCommand insertCommand = new SqlCommand(insertQuery, conn);
                insertCommand.ExecuteNonQuery();
            }
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получение id для удаления
            string idToDelete = idBox.Text;

            if (!string.IsNullOrEmpty(idToDelete))
            {
                // Открываем соединение
                using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
                {
                    connection.Open();

                    // Проверяем, существует ли запись в таблице employee с указанным id
                    string checkIdExistenceQuery = $"SELECT COUNT(*) FROM employee WHERE id = {idToDelete}";
                    SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

                    int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

                    if (idExistence > 0)
                    {
                        // Запись существует, выполняем удаление
                        string deleteQuery = $"DELETE FROM employee WHERE id = {idToDelete}";
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.ExecuteNonQuery();

                        // Опционально: Очищаем текстовые поля после удаления
                        idBox.Text = "";
                        nameBox.Text = "";
                        sernameBox.Text = "";
                        middlenameBox.Text = "";
                        positionBox.Text = "";
                        companyBox.Text = "";

                        // Обновляем DataGrid после удаления
                        RefreshDataGrid();
                    }
                    else { MessageBox.Show("Запись с указанным ID не существует.");}

                    // Закрываем соединение
                    connection.Close();
                }
            }
            else{ MessageBox.Show("Введите ID для удаления записи.");}
        }

    }
}

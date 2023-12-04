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
using System.Net;
using System.Windows.Controls.Primitives;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для routeWindow.xaml
    /// </summary>
    public partial class routeWindow : Window
    {
        private user currentUser;
        string server_name = "meoww";
        string data_base = "public_transport_2";
        public routeWindow(user currentUser)
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

                string query = $"SELECT route.id, route.number, start_stop.name AS start_area, end_stop.name AS end_area, route.total_travel_time, company.name AS company, transportType.name AS transport_type, route.distanse" +
                    $" FROM route" +
                    $" JOIN stop AS start_stop ON route.start_area = start_stop.id" +
                    $" JOIN stop AS end_stop ON route.end_area = end_stop.id" +
                    $" JOIN company ON route.id_company = company.id" +
                    $" JOIN transportType ON route.id_transport_type = transportType.id" +
                    $" WHERE route.id = {input}";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            string id = Convert.ToString(reader["id"]);
                            string number = Convert.ToString(reader["number"]);
                            string startArea = Convert.ToString(reader["start_area"]);
                            string endArea = Convert.ToString(reader["end_area"]);
                            string totalTravelTime = Convert.ToString(reader["total_travel_time"]);
                            string company = Convert.ToString(reader["company"]);
                            string transportType = Convert.ToString(reader["transport_type"]);
                            string distance = Convert.ToString(reader["distanse"]);

                            // Предполагается, что у вас есть соответствующие TextBox'ы для отображения данных
                            idBox.Text = id;
                            numberBox.Text = number;
                            startAreaBox.Text = startArea;
                            endAreaBox.Text = endArea;
                            totalTravelTimeBox.Text = totalTravelTime;
                            companyBox.Text = company;
                            transportTypeBox.Text = transportType;
                            distanceBox.Text = distance;
                        }
                        else
                        {
                            // Обработка случая, когда нет данных для отображения
                            MessageBox.Show("Нет данных для указанного маршрута");
                        }
                    }
                }
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получение данных из TextBox
            string id = idBox.Text;
            string number = numberBox.Text;
            string startArea = startAreaBox.Text;
            string endArea = endAreaBox.Text;
            string totalTravelTime = totalTravelTimeBox.Text;
            string company = companyBox.Text;
            string transportType = transportTypeBox.Text;
            string distance = distanceBox.Text;

            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            // Открываем соединение
            connection.Open();

            // Проверяем, существуют ли записи в связанных таблицах, и добавляем их, если необходимо
            AddToTableIfNotExists("name", startArea, connection);
            AddToTableIfNotExists("name", endArea, connection);
            AddToTableIfNotExists("company", company, connection);
            AddToTableIfNotExists("transportType", transportType, connection);

            // Проверяем, существует ли запись в таблице route с указанным id
            string checkIdExistenceQuery = $"SELECT COUNT(*) FROM route WHERE id = {id}";
            SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

            int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

            if (idExistence > 0)
            {
                // Запись существует, выполняем обновление
                string updateQuery = $"UPDATE route SET number = '{number}', " +
                                    $"start_area = (SELECT id FROM stop WHERE name = '{startArea}'), " +
                                    $"end_area = (SELECT id FROM stop WHERE name = '{endArea}'), " +
                                    $"total_travel_time = '{totalTravelTime}', " +
                                    $"id_company = (SELECT id FROM company WHERE name = '{company}'), " +
                                    $"id_transport_type = (SELECT id FROM transportType WHERE name = '{transportType}'), " +
                                    $"distanse = '{distance}' " +
                                    $"WHERE id = {id}";

                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.ExecuteNonQuery();
            }
            else
            {
                // Запись не существует, выполняем вставку
                string insertQuery = $"INSERT INTO route (number, start_area, end_area, total_travel_time, id_company, id_transport_type, distanse) " +
                    $"VALUES ('{number}', " +
                    $"(SELECT id FROM stop WHERE name = '{startArea}'), " +
                    $"(SELECT id FROM stop WHERE name = '{endArea}'), " +
                    $"'{totalTravelTime}', " +
                    $"(SELECT id FROM company WHERE name = '{company}'), " +
                    $"(SELECT id FROM transportType WHERE name = '{transportType}'), " +
                    $"'{distance}')";

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
                        query = $"SELECT route.id, route.number, start_stop.name AS start_area, end_stop.name AS end_area, route.total_travel_time, company.name AS company, transportType.name AS transport_type, route.distanse" +
                                $" FROM route" +
                                $" JOIN stop AS start_stop ON route.start_area = start_stop.id" +
                                $" JOIN stop AS end_stop ON route.end_area = end_stop.id" +
                                $" JOIN company ON route.id_company = company.id" +
                                $" JOIN transportType ON route.id_transport_type = transportType.id";
                        break;

                    // Админ
                    case 2:
                        query = $"SELECT route.id, route.number, start_stop.name AS start_area, end_stop.name AS end_area, route.total_travel_time, company.name AS company, transportType.name AS transport_type, route.distanse" +
                                $" FROM route" +
                                $" JOIN stop AS start_stop ON route.start_area = start_stop.id" +
                                $" JOIN stop AS end_stop ON route.end_area = end_stop.id" +
                                $" JOIN company ON route.id_company = company.id" +
                                $" JOIN transportType ON route.id_transport_type = transportType.id" +
                                $" WHERE employee.id_company = {currentUser.idCompany}";

                        // Добавлен запрос к таблице company для получения названия компании по idCompany
                        companyQuery = $"SELECT name FROM company WHERE id = {currentUser.idCompany}";

                        break;

                    //Оператор
                    case 4:
                        query = $"SELECT route.id, route.number, start_stop.name AS start_area, end_stop.name AS end_area, route.total_travel_time, company.name AS company, transportType.name AS transport_type, route.distanse" +
                                $" FROM route" +
                                $" JOIN stop AS start_stop ON route.start_area = start_stop.id" +
                                $" JOIN stop AS end_stop ON route.end_area = end_stop.id" +
                                $" JOIN company ON route.id_company = company.id" +
                                $" JOIN transportType ON route.id_transport_type = transportType.id" +
                                $" WHERE employee.id_company = {currentUser.idCompany}";

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

                    // Проверяем, существует ли запись в таблице route с указанным id
                    string checkIdExistenceQuery = $"SELECT COUNT(*) FROM route WHERE id = {idToDelete}";
                    SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

                    int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

                    if (idExistence > 0)
                    {
                        // Запись существует, выполняем удаление
                        string deleteQuery = $"DELETE FROM route WHERE id = {idToDelete}";
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.ExecuteNonQuery();

                        // Опционально: Очищаем текстовые поля после удаления
                        idBox.Text = "";
                        numberBox.Text = "";
                        startAreaBox.Text = "";
                        endAreaBox.Text = "";
                        totalTravelTimeBox.Text = "";
                        companyBox.Text = "";
                        transportTypeBox.Text = "";
                        distanceBox.Text = "";

                        // Обновляем DataGrid после удаления
                        RefreshDataGrid();
                    }
                    else { MessageBox.Show("Запись с указанным ID не существует."); }

                    // Закрываем соединение
                    connection.Close();
                }
            }
            else { MessageBox.Show("Введите ID для удаления записи."); }
        }


    }

}

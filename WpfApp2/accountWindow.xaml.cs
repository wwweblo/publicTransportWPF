using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp2
{
    public partial class accountWindow : Window
    {
        private user currentUser;
        string server_name = "meoww";
        string data_base = "public_transport_2";

        public accountWindow(user currentUser)
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

            string query = "SELECT account.id, account.login, account.password, company.name AS company, role.name AS role, employee.id AS employee" +
                    " FROM account" +
                    " JOIN company ON account.id_company = company.id" +
                    " JOIN role ON account.id_role = role.id" +
                    " JOIN employee ON account.id_employee = employee.id" +
                    $" WHERE account.id = {input}";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                string id = Convert.ToString(reader["id"]);
                string login = Convert.ToString(reader["login"]);
                string password = Convert.ToString(reader["password"]);
                string id_company = Convert.ToString(reader["company"]);
                string id_role = Convert.ToString(reader["role"]);
                string id_employee = Convert.ToString(reader["employee"]);

                idBox.Text = id;
                loginBox.Text = login;
                passwordBox.Text = password;
                idCompanyBox.Text = id_company;
                idRoleBox.Text = id_role;
                idEmployeeBox.Text = id_employee;
            }

            connection.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string id = idBox.Text;
            string login = loginBox.Text;
            string password = passwordBox.Text;
            string company = idCompanyBox.Text;
            string role = idRoleBox.Text;
            string id_employee = idEmployeeBox.Text;

            SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True");

            connection.Open();

            AddToTableIfNotExists("company", company, connection);
            AddToTableIfNotExists("role", role, connection);
            AddToTableIfNotExists("employee", id_employee, connection);

            string checkIdExistenceQuery = $"SELECT COUNT(*) FROM account WHERE id = {id}";
            SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

            int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

            if (idExistence > 0)
            {
                string updateQuery = $"UPDATE account SET login = '{login}', password = '{password}'," +
                    $" id_company = (SELECT id FROM company WHERE name = '{company}')," +
                    $" id_role = (SELECT id FROM role WHERE name = '{role}')," +
                    $" id_employee = {id_employee}" +
                    $" WHERE id = {id}";

                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.ExecuteNonQuery();
            }
            else
            {
                string insertQuery = $"INSERT INTO account (login, password, id_company, id_role, id_employee)" +
                                    $"VALUES ('{login}', '{password}'," +
                                    $" (SELECT id FROM company WHERE name = {company})," +
                                    $" (SELECT id FROM role WHERE name = {role})," +
                                    $" (SELECT id FROM employee WHERE id = {id_employee}))";
                    

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.ExecuteNonQuery();
            }

            RefreshDataGrid();
            connection.Close();
        }

        private void RefreshDataGrid()
        {
            using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
            {
                connection.Open();

                string query = "SELECT account.id, account.login, account.password, company.name AS company, role.name AS role, employee.id AS employee" +
                    " FROM account" +
                    " JOIN company ON account.id_company = company.id" +
                    " JOIN role ON account.id_role = role.id" +
                    " JOIN employee ON account.id_employee = employee.id";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    accountDataGrid.ItemsSource = dataTable.DefaultView;
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
            string idToDelete = idBox.Text;

            if (!string.IsNullOrEmpty(idToDelete))
            {
                using (SqlConnection connection = new SqlConnection($"Data Source={server_name};Initial Catalog={data_base};Integrated Security=True"))
                {
                    connection.Open();

                    string checkIdExistenceQuery = $"SELECT COUNT(*) FROM account WHERE id = {idToDelete}";
                    SqlCommand checkIdExistenceCommand = new SqlCommand(checkIdExistenceQuery, connection);

                    int idExistence = Convert.ToInt32(checkIdExistenceCommand.ExecuteScalar());

                    if (idExistence > 0)
                    {
                        string deleteQuery = $"DELETE FROM account WHERE id = {idToDelete}";
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.ExecuteNonQuery();

                        idBox.Text = "";
                        loginBox.Text = "";
                        passwordBox.Text = "";
                        idCompanyBox.Text = "";
                        idRoleBox.Text = "";
                        idEmployeeBox.Text = "";

                        RefreshDataGrid();
                    }
                    else { MessageBox.Show("Record with the specified ID does not exist."); }

                    connection.Close();
                }
            }
            else { MessageBox.Show("Enter ID to delete the record."); }
        }
    }
}

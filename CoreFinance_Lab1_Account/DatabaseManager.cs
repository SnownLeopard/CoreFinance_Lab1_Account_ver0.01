using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    public class DatabaseManager
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BankDatabase.mdf;Integrated Security=True;";

        public List<BankAccount> LoadAccounts()
        {
            List<BankAccount> accountList = new List<BankAccount>();

            string query = "SELECT AccountType, LastName, FirstName, BirthDate, Balance FROM Accounts;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader()) 
                        {
                            while (reader.Read())
                            {
                                string type = reader["AccountType"].ToString();
                                string lastName = reader["LastName"].ToString();
                                string firstName = reader["FirstName"].ToString();
                                DateTime birthDate = Convert.ToDateTime(reader["BirthDate"]);
                                double balance = Convert.ToDouble(reader["Balance"]);

                                BankAccount loadedAcc = AccountFactory.Instance.CreateAccount(
                                    type, lastName, firstName, "", birthDate, balance
                                );

                                accountList.Add(loadedAcc); 
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка чтения из SQL-базы: {ex.Message}", "БД Чтение Аврал");
                }
            }

            return accountList;
        }
    }
}

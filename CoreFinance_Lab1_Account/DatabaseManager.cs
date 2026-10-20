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

        public void CreateTablesIfNotExist()
        {
            string script = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
                BEGIN
                    CREATE TABLE Accounts (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        AccountType NVARCHAR(50) NOT NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        FirstName NVARCHAR(100) NOT NULL,
                        BirthDate DATE NOT NULL,
                        Balance DOUBLE PRECISION NOT NULL
                    )
                END;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка инициализации SQL-таблиц: {ex.Message}", "БД Аврал");
                }
            }
        }

        public void SaveAccount(string accountType, BankAccount account)
        {
            if (account == null) return;

            string query = @"
                INSERT INTO Accounts (AccountType, LastName, FirstName, BirthDate, Balance) 
                VALUES (@AccountType, @LastName, @FirstName, @BirthDate, @Balance);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountType", accountType);

                        command.Parameters.AddWithValue("@LastName", account.LastNameProperty);
                        command.Parameters.AddWithValue("@FirstName", account.FirstNameProperty);
                        command.Parameters.AddWithValue("@BirthDate", account.BirthDate);
                        command.Parameters.AddWithValue("@Balance", account.Balance);

                        command.ExecuteNonQuery(); 
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка записи в SQL-базу: {ex.Message}", "БД Аврал");
                }
            }
        }
    }
}

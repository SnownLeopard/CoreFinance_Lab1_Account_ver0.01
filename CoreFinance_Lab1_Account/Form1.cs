using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CoreFinance_Lab1_Account
{
    public partial class Form1 : Form
    {
        private BankAccount acc;
        private DateTime lastInterestDate;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            double initialBalance;
            DateTime userBirthDate;

            if (!double.TryParse(textBoxAmount.Text, out initialBalance))
            {
                MessageBox.Show("Введите корректную стартовую сумму цифрами!", "Ошибка ввода");
                return;
            }

            if (!DateTime.TryParse(maskedTextBoxBirthDay.Text, out userBirthDate))
            {
                MessageBox.Show("Введите дату рождения в формате ДД.ММ.ГГГГ)!", "Ошибка формата даты");
                maskedTextBoxBirthDay.Focus();
                return;
            }

            if (textBoxLastName.Text == "" || textBoxFirstName.Text == "")
            {
                MessageBox.Show("Пожалуйста, заполните поля Фамилия и Имя!", "Контроль ввода");
                return;
            }

            try
            {
                acc = new BankAccount(
                textBoxLastName.Text,
                textBoxFirstName.Text,
                textBoxMiddleName.Text,
                userBirthDate,
                initialBalance
                );

                lastInterestDate = DateTime.Now;

                listBoxLogs.Items.Add($"[УСПЕХ] Открыт счет. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Баланс: {acc.Balance} руб.");
                textBoxAmount.Clear();
                maskedTextBoxBirthDay.Clear();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Контроль");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void maskedTextBoxBirthDay_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBoxBirthDay.SelectionStart = 0;
        }
    }
}

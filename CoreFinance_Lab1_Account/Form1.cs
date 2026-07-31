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

            if (string.IsNullOrEmpty(textBoxAmount.Text))
            {
                initialBalance = 0;
            }

            else if (!double.TryParse(textBoxAmount.Text, out initialBalance))
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
                if (comboBoxAccountType.Text == "CreditAccount")

                {
                    acc = new CreditAccount(
                    textBoxLastName.Text,
                    textBoxFirstName.Text,
                    textBoxMiddleName.Text,
                    userBirthDate,
                    initialBalance,
                    30000
                    );

                    listBoxLogs.Items.Add($"[УСПЕХ] Открыт КРЕДИТНЫЙ счёт. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Доступный лимит: 30 000 руб.");
                }

                else
                {
                    acc = new BankAccount(
                    textBoxLastName.Text,
                    textBoxFirstName.Text,
                    textBoxMiddleName.Text,
                    userBirthDate,
                    initialBalance
                    );

                    listBoxLogs.Items.Add($"[УСПЕХ] Открыт ДЕБЕТОВЫЙ счёт. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Баланс: {acc.Balance} руб.");

                }

                lastInterestDate = DateTime.Now;

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
            comboBoxAccountType.SelectedIndex = 0;
        }

        private void maskedTextBoxBirthDay_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBoxBirthDay.SelectionStart = 0;
        }

        private void buttonCredit_Click(object sender, EventArgs e)
        {
            double amount;

            if(double.TryParse(textBoxAmount.Text, out amount))
            {
                try
                {
                    if (acc == null)
                    {
                        MessageBox.Show("Сначала необходимо открыть счет!", "Ошибка операции");
                        return;
                    }

                    acc.Credit(amount);

                    listBoxLogs.Items.Add($"[ПОПОЛНЕНИЕ] +{amount} руб. Текущий баланс: {acc.Balance} руб.");
                    textBoxAmount.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка валидации");
                }
            }
            else
            {
                MessageBox.Show("Введите корректную сумму пополнения цифрами!", "Ошибка ввода");
            }
        }

        private void buttonDebit_Click(object sender, EventArgs e)
        {
            double amount;

            if (double.TryParse(textBoxAmount.Text, out amount))
            {
                try
                {
                    if (acc == null)
                    {
                        MessageBox.Show("Сначала необходимо открыть счет!", "Ошибка операции");
                        return;
                    }

                    acc.Debit(amount);

                    if (acc.Balance < 0)
                    {
                        listBoxLogs.Items.Add($"[СНЯТИЕ] -{amount} руб. Внимание! Минус по счету (овердрафт)! Текущий баланс: {acc.Balance} руб.");
                    }
                    else
                    {
                        listBoxLogs.Items.Add($"[СНЯТИЕ] -{amount} руб. Текущий баланс: {acc.Balance} руб.");
                    }

                    textBoxAmount.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка валидации");
                }
            }
            else
            {
                MessageBox.Show("Введите корректную сумму снятия цифрами!", "Ошибка ввода");
            }
        }

        private void listBoxLogs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string itemText = listBoxLogs.Items[e.Index].ToString();

            Brush textBrush = Brushes.Blue;

            if (itemText.Contains("[ПОПОЛНЕНИЕ]") || itemText.Contains("[CREDIT]"))
            {
                textBrush = Brushes.ForestGreen;
            }    

                if (itemText.Contains("овердрафт") || itemText.Contains("Минус"))
            {
                textBrush = Brushes.Orange;
            }

            else if (itemText.Contains("[ОШИБКА]"))
            {
                textBrush = Brushes.Red;
            }

            e.DrawBackground();

            e.Graphics.DrawString(itemText, e.Font, textBrush, e.Bounds);

            e.DrawFocusRectangle();
        }
    }
}

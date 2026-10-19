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
        private IAccountFactory accountFactory = AccountFactory.Instance;
        private ILogger logger;


        private System.Windows.Forms.Timer interestTimer;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenAccount(object sender, EventArgs e)
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
                acc = accountFactory.CreateAccount(
                    comboBoxAccountType.Text,
                    textBoxLastName.Text,
                    textBoxFirstName.Text,
                    textBoxMiddleName.Text,
                    userBirthDate,
                    initialBalance
                    );

                if (comboBoxAccountType.Text == "VipCreditAccount")
                {
                    logger.Log($"[УСПЕХ] Открыт VIP КРЕДИТНЫЙ счёт. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Баланс: {acc.Balance} руб. Одобренная броня лимита: 500000 руб.");
                }

                else if (comboBoxAccountType.Text == "CreditAccount")

                {
                    logger.Log($"[УСПЕХ] Открыт КРЕДИТНЫЙ счёт. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Баланс: {acc.Balance} руб. Доступный лимит: 30000 руб.");
                }

                else
                {
                    logger.Log($"[УСПЕХ] Открыт ДЕБЕТОВЫЙ счёт. Клиент: {textBoxLastName.Text} {textBoxFirstName.Text}. Баланс: {acc.Balance} руб.");
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
            logger = new ListBoxLogger(listBoxLogs);
            AccountFactory.Instance.Initialize(logger);

            interestTimer = new System.Windows.Forms.Timer();
            interestTimer.Interval = 10000; 
            interestTimer.Tick += InterestTimer_Tick;
            interestTimer.Start();
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
                    if (!EnsureAccountExists()) return;

                    acc.Credit(amount);

                    logger.Log($"[ПОПОЛНЕНИЕ] +{amount} руб. Текущий баланс: {acc.Balance} руб.");
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
                    if (!EnsureAccountExists()) return;

                    acc.Debit(amount);

                    if (acc.Balance < 0)
                    {
                        logger.Log($"[СНЯТИЕ] -{amount} руб. Внимание! Минус по счету (овердрафт)! Текущий баланс: {acc.Balance} руб.");
                    }
                    else
                    {
                        logger.Log($"[СНЯТИЕ] -{amount} руб. Текущий баланс: {acc.Balance} руб.");
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

            if (itemText.Contains("[ПОПОЛНЕНИЕ]") || itemText.Contains("[УСПЕХ]"))
            {
                textBrush = Brushes.ForestGreen;
            }    

                if (itemText.Contains("овердрафт") || itemText.Contains("Минус"))
            {
                textBrush = Brushes.Orange;
            }

            e.DrawBackground();

            e.Graphics.DrawString(itemText, e.Font, textBrush, e.Bounds);

            e.DrawFocusRectangle();
        }
        private void InterestTimer_Tick(object sender, EventArgs e)
        {
            // Если счёт ещё не открыт оператором — таймер просто молча ждёт в памяти
            if (acc == null) return;

            try
            {
                double oldBalance = acc.Balance;

                acc.AccumulateInterest();

                double profit = acc.Balance - oldBalance;

                {
                    logger.Log($"[ДОХОД] Автоматическое начисление процентов на остаток: +{Math.Round(profit, 2)} руб. Текущий баланс: {Math.Round(acc.Balance, 2)} руб.");
                }
            }
            catch (Exception ex)
            {
                interestTimer.Stop();
                MessageBox.Show($"Сбой авто-начисления: {ex.Message}", "Ошибка системы");
            }
        }

        private bool EnsureAccountExists()
        {
            if (acc == null)
            {
                MessageBox.Show("Сначала необходимо открыть счет!", "Ошибка операции");
                return false;
            }
            return true;
        }

        private void comboBoxAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

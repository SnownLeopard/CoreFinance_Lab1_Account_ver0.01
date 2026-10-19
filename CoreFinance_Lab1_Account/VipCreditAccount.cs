using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    internal class VipCreditAccount : BankAccount
    {
        private double creditLimit;

        public VipCreditAccount(string lastName, string firstName, string middleName, DateTime berthDate, double balance)
            : base(lastName, firstName, middleName, berthDate, balance)
        {
            this.creditLimit = 500000;
        }

        public override void Debit(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Критическая ошибка ядра: Сумма снятия должна быть больше нуля!");
            }

            if (creditLimit < amount - Balance)
            {
                throw new InvalidOperationException($"Критическая ошибка: Превышен VIP-лимит овердрафта! Доступный лимит: {creditLimit} руб.");
            }

            Balance -= amount;
        }

        public override void Credit(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Критическая ошибка ядра: Сумма пополнения должна быть больше нуля!");
            }

            Balance += amount;
        }
    }
}

using System;

namespace CoreFinance_Lab1_Account
{
public class CreditAccount : BankAccount
    {
        private double creditLimit;

        public CreditAccount(string lastName, string firstName, string middleName, DateTime berthDate, double balance)
            : base(lastName, firstName, middleName, berthDate, balance)
        {
            this.creditLimit = 30000;
        }

        public override void Debit(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Критическая ошибка: Сумма снятия должна быть больше нуля!");
            }

            if (creditLimit < amount - Balance)
            {
                throw new Exception($"Критическая ошибка: Превышен лимит овердрафта! Доступный лимит: {creditLimit} руб.");
            }    

                Balance -= amount;
        }

            public override void Credit(double amount)
        {
            if (amount > 0)
            {
                Balance += amount;
            }
            else    
                throw new Exception("Критическая ошибка: Сумма пополнения должна быть больше нуля!");
        }

    }
}

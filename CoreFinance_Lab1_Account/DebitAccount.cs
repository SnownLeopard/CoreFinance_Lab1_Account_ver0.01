using System;

namespace CoreFinance_Lab1_Account
{
    public class DebitAccount : BankAccount
    {
        public DebitAccount(string lastName, string firstName, string middleName, DateTime berthDate, double balance)
            : base(lastName, firstName, middleName, berthDate, balance) { }

        public override void Debit(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Критическая ошибка: Сумма снятия должна быть больше нуля!");
            }

            if(amount <= Balance)
            {
                Balance -= amount;
            }

            else
            {
                throw new InvalidOperationException("Критическая ошибка: Недостаточно средств на счёте для совершения операции!");
            }
        }

        public override void Credit(double amount)
        {
            if (amount > 0)
            {
                Balance += amount;
            }
            else
            {
                throw new ArgumentException("Критическая ошибка: Сумма пополнения должна быть больше нуля!");
            }
        }
    }
}

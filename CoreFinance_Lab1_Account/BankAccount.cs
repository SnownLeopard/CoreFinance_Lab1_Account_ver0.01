using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    public class BankAccount
    {
        private string lastName;
        private string firstName;
        private string middleName;
        protected double balance;
        private static double interestRate = 0.01;
        private DateTime birthDate;
        private DateTime lastInterestDate;

        public double Balance
        {
            get { return balance; }
            set { balance = value; }
        }

        public DateTime BirthDate
        {
            get { return birthDate; }
            set
            {
                if(value > DateTime.Now)
                {
                    throw new ArgumentException("Дата рождения не может быть в будущем!");
                }
                int age = DateTime.Now.Year;

                if(value.Date > DateTime.Now.AddYears(-age))
                {
                    age--;
                }

                if (age < 18)
                {
                    throw new InvalidOperationException("Критическая ошибка: Клиент должен быть совершеннолетним (18+)!");
                }

                birthDate = value;
            }
        }

        public BankAccount(string lastName, string firstName, string middleName, DateTime birthDate, double initialBalance)
        {
            this.lastName = lastName;
            this.firstName = firstName;
            this.middleName = middleName;
            this.birthDate = birthDate;

            if (initialBalance >= 0)
                this.balance = initialBalance;
            else 
                this.balance = 0;
        }

        public void AccumulateInterest()
        {
            double interest = balance * interestRate;
            balance += interest;
        }

        public void Credit(double amount)
        {
            if (amount > 0)
            {
                balance += amount;
            }
            else
            {
                throw new ArgumentException("Критическая ошибка: Сумма пополнения должна быть больше нуля!");
            }
        }

        public virtual void Debit(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Критическая ошибка: Сумма снятия должна быть больше нуля!");
            }

            if (amount <= balance)
            {
                balance -= amount;
            }

            else
            {
                throw new InvalidOperationException("Критическая ошибка: Недостаточно средств на счёте для совершения операции!");
            }
        }
    }
}

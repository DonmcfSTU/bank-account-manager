/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: SampleData creates the Bank and its starting customers and
 *          accounts (made-up customers for testing).
 *          Week 4: this data is only used the FIRST time the program runs.
 *          Program saves it into the new SQLite database, and after that
 *          everything is loaded from the database instead.
 *          Week 3: uses the different constructor overloads so each one is
 *          exercised with realistic values.
 */

using BankAccountManager.Models;

namespace BankAccountManager.Data
{
    // ACCESS SPECIFIER: static class with one public method - nothing else
    // here needs to be visible.
    public static class SampleData
    {
        public static Bank CreateBank()
        {
            // CONSTRUCTOR: Bank's parameterized constructor.
            var bank = new Bank("McFadden Community Bank");

            // CONSTRUCTOR: Customer's main constructor that takes an Address
            // object (COMPOSITION).
            var maria = new Customer(1001, "Maria", "Alvarez", "(757) 555-0142",
                "maria.alvarez@example.com",
                new Address("2125 Shore Dr", "Virginia Beach", "VA", "23451"));
            // CONSTRUCTORS: full account constructors (existing accounts with a
            // known open date and terms).
            maria.AddAccount(new CheckingAccount("CHK-100101", 2450.75m,
                new DateTime(2023, 3, 14), 5.00m, 250.00m));
            maria.AddAccount(new SavingsAccount("SAV-100102", 8200.00m,
                new DateTime(2023, 3, 14), 0.0425m));
            bank.AddCustomer(maria);

            // CONSTRUCTOR: Customer's overload that takes the address pieces.
            var james = new Customer(1002, "James", "Carter", "(804) 555-0198",
                "jcarter@example.com", "4100 Monument Ave", "Richmond", "VA", "23230");
            james.AddAccount(new CheckingAccount("CHK-100201", 615.20m,
                new DateTime(2024, 8, 2), 0.00m, 100.00m));
            james.AddAccount(new MoneyMarketAccount("MMA-100202", 15000.00m,
                new DateTime(2024, 8, 2), 0.0480m, 2500.00m));
            bank.AddCustomer(james);

            var aisha = new Customer(1003, "Aisha", "Thompson", "(757) 555-0167",
                "aisha.t@example.com",
                new Address("860 Greenbrier Pkwy", "Chesapeake", "VA", "23320"));
            aisha.AddAccount(new SavingsAccount("SAV-100301", 3125.40m,
                new DateTime(2022, 11, 30), 0.0400m));
            aisha.AddAccount(new CheckingAccount("CHK-100302", 1789.05m,
                new DateTime(2025, 1, 9), 5.00m, 250.00m));
            aisha.AddAccount(new MoneyMarketAccount("MMA-100303", 6400.00m,
                new DateTime(2025, 6, 18), 0.0465m, 2500.00m));
            bank.AddCustomer(aisha);

            // Derrick's money market account is below its minimum, so month-end
            // charges a low-balance fee instead of paying interest.
            var derrick = new Customer(1004, "Derrick", "Owens", "(757) 555-0113",
                "dowens@example.com", "315 Granby St", "Norfolk", "VA", "23510");
            derrick.AddAccount(new CheckingAccount("CHK-100401", 942.18m,
                new DateTime(2021, 5, 21), 5.00m, 250.00m));
            derrick.AddAccount(new MoneyMarketAccount("MMA-100402", 1850.00m,
                new DateTime(2021, 5, 21), 0.0450m, 2500.00m));
            bank.AddCustomer(derrick);

            // A few recent transactions so the history screen has realistic
            // activity (these post with today's date).
            maria.Accounts[0].Deposit(1250.00m);   // paycheck
            maria.Accounts[0].Withdraw(84.37m);    // groceries
            aisha.Accounts[1].Withdraw(1200.00m);  // rent

            return bank;
        }
    }
}

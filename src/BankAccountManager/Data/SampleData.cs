/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: SampleData creates the starting customers and accounts for the
 *          Week 1 demo. Each customer is built with an Address object and
 *          one or more accounts of different types. (These are made-up
 *          customers for testing.) In Week 4 this data will come from an
 *          SQLite database instead.
 */

using BankAccountManager.Models;

namespace BankAccountManager.Data
{
    public static class SampleData
    {
        public static List<Customer> CreateCustomers()
        {
            var customers = new List<Customer>();

            // Customer 1 - COMPOSITION: the Address object is created and
            // handed to the Customer, which then "owns" it.
            var maria = new Customer(1001, "Maria", "Alvarez", "(757) 555-0142",
                "maria.alvarez@example.com",
                new Address("2125 Shore Dr", "Virginia Beach", "VA", "23451"));
            // INHERITANCE: each of these is a child class of Account,
            // but they can all be stored in the customer's Account list.
            maria.AddAccount(new CheckingAccount("CHK-100101", 2450.75m,
                new DateTime(2023, 3, 14), 5.00m, 250.00m));
            maria.AddAccount(new SavingsAccount("SAV-100102", 8200.00m,
                new DateTime(2023, 3, 14), 0.0425m));
            customers.Add(maria);

            // Customer 2
            var james = new Customer(1002, "James", "Carter", "(804) 555-0198",
                "jcarter@example.com",
                new Address("4100 Monument Ave", "Richmond", "VA", "23230"));
            james.AddAccount(new CheckingAccount("CHK-100201", 615.20m,
                new DateTime(2024, 8, 2), 0.00m, 100.00m));
            james.AddAccount(new MoneyMarketAccount("MMA-100202", 15000.00m,
                new DateTime(2024, 8, 2), 0.0480m, 2500.00m));
            customers.Add(james);

            // Customer 3
            var aisha = new Customer(1003, "Aisha", "Thompson", "(757) 555-0167",
                "aisha.t@example.com",
                new Address("860 Greenbrier Pkwy", "Chesapeake", "VA", "23320"));
            aisha.AddAccount(new SavingsAccount("SAV-100301", 3125.40m,
                new DateTime(2022, 11, 30), 0.0400m));
            aisha.AddAccount(new CheckingAccount("CHK-100302", 1789.05m,
                new DateTime(2025, 1, 9), 5.00m, 250.00m));
            aisha.AddAccount(new MoneyMarketAccount("MMA-100303", 6400.00m,
                new DateTime(2025, 6, 18), 0.0465m, 2500.00m));
            customers.Add(aisha);

            return customers;
        }
    }
}

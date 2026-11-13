/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: ConsoleUI holds all of the methods that write to the screen or
 *          read from the keyboard - the header, welcome message, menu,
 *          tables, and input prompts. Keeping this in one class means the
 *          Models classes don't have to know anything about the console,
 *          which should make the project easier to grow in later weeks.
 */

using BankAccountManager.Models;

namespace BankAccountManager.UI
{
    public static class ConsoleUI
    {
        private const int Width = 92;

        // Week indicator, assignment title, and my name.
        public static void ShowHeader()
        {
            Console.WriteLine(new string('=', Width));
            Console.WriteLine(Center("SDC320L Course Project - WEEK 1 (Phase 1)"));
            Console.WriteLine(Center("Inheritance, Composition, and User Interactions"));
            Console.WriteLine(Center("Bank Account Management Application"));
            Console.WriteLine(Center("Created by Donavan D. McFadden"));
            Console.WriteLine(new string('=', Width));
        }

        // Welcome message with basic instructions.
        public static void ShowWelcome()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to the McFadden Community Bank account manager!");
            Console.WriteLine("\"A penny saved is a penny earned.\" - Benjamin Franklin (probably)");
            Console.WriteLine();
            Console.WriteLine("How to use this program:");
            Console.WriteLine("  - Type the number of a menu option and press Enter.");
            Console.WriteLine("  - When asked for a customer, type the Customer ID shown in the customer list.");
            Console.WriteLine("  - When asked for an account, type the account number (example: CHK-100101).");
            Console.WriteLine("  - Type Q (or Quit) at the main menu to exit.");
        }

        public static void ShowMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------- MAIN MENU ----------------------------");
            Console.WriteLine("  1. List all customers");
            Console.WriteLine("  2. List all accounts for all customers");
            Console.WriteLine("  3. View one customer and their accounts");
            Console.WriteLine("  4. Make a deposit");
            Console.WriteLine("  5. Make a withdrawal");
            Console.WriteLine("  Q. Quit");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("  (Add, remove, and update features are coming in later phases.)");
        }

        // Table of customers. Each row uses the Customer's composed
        // Address object to show the city and state.
        public static void ShowCustomerTable(List<Customer> customers)
        {
            Console.WriteLine();
            Console.WriteLine($"{"ID",-6}{"Name",-18}{"Phone",-16}{"City, State",-22}{"Accounts",9}{"Total Balance",16}");
            Console.WriteLine(new string('-', 87));
            foreach (Customer c in customers)
            {
                string cityState = $"{c.MailingAddress.City}, {c.MailingAddress.State}";
                Console.WriteLine($"{c.CustomerId,-6}{c.FullName,-18}{c.Phone,-16}{cityState,-22}{c.Accounts.Count,9}{c.GetTotalBalance(),16:C}");
            }
        }

        // Table of accounts. Works for any mix of Checking, Savings, and
        // Money Market accounts because they all inherit from Account.
        public static void ShowAccountTable(IEnumerable<Account> accounts)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Owner",-17}{"Opened",-12}{"Balance",13}  {"Details"}");
            Console.WriteLine(new string('-', Width));
            foreach (Account a in accounts)
            {
                Console.WriteLine($"{a.AccountNumber,-12}{a.AccountType,-14}{a.OwnerName,-17}{a.DateOpened,-12:MM/dd/yyyy}{a.Balance,13:C}  {a.GetAccountDetails()}");
            }
        }

        // Full profile for one customer.
        public static void ShowCustomerDetails(Customer c)
        {
            Console.WriteLine();
            Console.WriteLine($"Customer #{c.CustomerId}: {c.FullName}");
            Console.WriteLine($"  Phone:   {c.Phone}");
            Console.WriteLine($"  Email:   {c.Email}");
            Console.WriteLine($"  Address: {c.MailingAddress}");
            ShowAccountTable(c.Accounts);
            Console.WriteLine($"{"Total balance:",-67}{c.GetTotalBalance(),13:C}");
        }

        public static string Prompt(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? string.Empty).Trim();
        }

        // Keeps asking until the user types a positive dollar amount.
        public static decimal PromptForAmount(string message)
        {
            while (true)
            {
                string input = Prompt(message).Replace("$", "").Replace(",", "");
                if (decimal.TryParse(input, out decimal amount) && amount > 0)
                {
                    return Math.Round(amount, 2);
                }
                ShowError("Please enter a dollar amount greater than zero (example: 125.50).");
            }
        }

        // Clears the console window. Console.Clear() can throw an error when
        // the output is redirected (for example, to a file), so in that case
        // the program just keeps going without clearing.
        public static void ClearScreen()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // Nothing to clear - output is not going to a real console window.
            }
        }

        public static void ShowSuccess(string message) => WriteColor(message, ConsoleColor.Green);
        public static void ShowError(string message) => WriteColor(message, ConsoleColor.Red);

        public static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to return to the main menu...");
            Console.ReadLine();
        }

        private static void WriteColor(string message, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }

        private static string Center(string text)
        {
            int padding = Math.Max(0, (Width - text.Length) / 2);
            return new string(' ', padding) + text;
        }
    }
}

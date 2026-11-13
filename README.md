# Bank Account Manager

A C# (.NET) console application for managing bank customers and their accounts, with all data stored in an SQLite database. A bank employee can add and remove customers and accounts, look up account information, make deposits and withdrawals, run month-end fees and interest, view each account's transaction history, and update customer information. Everything is saved to the database as soon as it happens.

**Author:** Donavan D. McFadden  
**School:** ECPI University, College of Technology (Advanced Object-Oriented Programming Using C#)  
**Instructor:** Dr. Wissam Hammoud  

## Features

| Option | Feature | Database |
|--------|---------|----------|
| 1 | List all customers (ID, name, phone, city/state, number of accounts, total balance) | Read |
| 2 | List all accounts for all customers | Read |
| 3 | View one customer with their address and all of their accounts | Read |
| 4 | Make a deposit | Update + transaction record |
| 5 | Make a withdrawal (each account type has its own rule) | Update + transaction record |
| 6 | Interest report for interest-earning accounts | Read |
| 7 | Month-end processing - fees, interest, and low-balance fees (Y/N confirmation) | Update |
| 8 | Add a new customer | Create |
| 9 | Open a new checking, savings, or money market account | Create |
| 10 | Close (delete) an account (Y/N confirmation; refused if overdrawn) | Delete |
| 11 | Update customer information (press Enter to keep a value) | Update |
| 12 | View the transaction history for one account | Read |
| 13 | Delete a customer (Y/N confirmation; all accounts must be closed first) | Delete |
| 14 | View the raw database tables (Customers, Accounts, Transactions, RetiredIds) | Read |
| Q | Quit (also accepts `Quit` or `Exit`) | - |

**Account types**

- **Checking** - monthly maintenance fee and an overdraft limit
- **Savings** - earns monthly interest
- **Money Market** - earns interest, but requires a minimum balance: withdrawals can't go below it, no interest is paid while under it, and a low-balance fee is charged at month end

## Object-Oriented Design

| Requirement | Where it is in the code |
|-------------|-------------------------|
| Inheritance | `Account` → `CheckingAccount`, `SavingsAccount` → `MoneyMarketAccount` |
| Interface | `IInterestBearing` (implemented by `SavingsAccount`, inherited by `MoneyMarketAccount`) |
| Abstract class | `Account` is `abstract`; `AccountType`, `GetAccountDetails()`, `ProcessMonthEnd()`, and `CanWithdraw()` are abstract |
| Composition | `Bank` has `Customer`s; `Customer` has an `Address` and `Account`s; `Account` has `Transaction`s |
| Polymorphism | `List<Account>` calls each type's own `GetAccountDetails()`, `ProcessMonthEnd()`, and withdrawal rule; the interest report works through `List<IInterestBearing>` |
| Constructors | Overloaded and chained (`: this(...)` / `: base(...)`), protected (abstract class), default (`Bank()`, `DatabaseManager()`), copy (`Address`), and internal (`Transaction`) constructors |
| Access specifiers | `private` fields and helpers, `protected` members for child classes, `internal` members for the database layer, `private set` properties changed only through validated methods |
| SQLite database | `DatabaseManager` with parameterized SQL, foreign keys, CHECK constraints, and SQL transactions |
| Runs until the user quits | Main menu loop in `Program.cs` |

Every code file has a header comment (name, date, purpose), and inline comments are labeled `INHERITANCE:`, `COMPOSITION:`, `INTERFACE:`, `POLYMORPHISM:`, `ABSTRACTION:`, `CONSTRUCTOR:`, and `ACCESS SPECIFIER(S):`.

## Database

Diagrams of the classes, database tables, and menu flow are in [docs/README.md](docs/README.md).


The program creates `bank.db` in the folder it is run from. On the first run, it creates the tables and saves the sample data. After that, it loads the saved data every time it starts.

| Table | Contents |
|-------|----------|
| `Customers` | One row per customer, including their mailing address |
| `Accounts` | One row per account of any type; `AccountType` decides which class is built, and columns that don't apply to that type are `NULL` |
| `Transactions` | Every deposit, withdrawal, fee, interest posting, and opening deposit (`ON DELETE CASCADE` with its account) |
| `RetiredIds` | Account numbers and customer IDs that were deleted, so they are never reused |

Delete `bank.db` to start over with the sample data.

## Folder Structure

```
bank-account-manager/
├── src/
│   └── BankAccountManager/
│       ├── BankAccountManager.csproj
│       ├── Program.cs                # Entry point and main menu loop
│       ├── Interfaces/
│       │   └── IInterestBearing.cs   # Interface for interest-earning accounts
│       ├── Models/
│       │   ├── Account.cs            # Abstract base class
│       │   ├── CheckingAccount.cs
│       │   ├── SavingsAccount.cs     # Implements IInterestBearing
│       │   ├── MoneyMarketAccount.cs # Derived from SavingsAccount
│       │   ├── Customer.cs           # Has an Address and Accounts
│       │   ├── Address.cs
│       │   ├── Bank.cs               # Has Customers
│       │   ├── Transaction.cs
│       │   └── TransactionType.cs
│       ├── UI/
│       │   └── ConsoleUI.cs          # All console input and output
│       └── Data/
│           ├── DatabaseManager.cs    # SQLite tables and all CRUD operations
│           └── SampleData.cs         # Starting data for a new database
├── docs/
│   └── README.md                 # Class, database, and menu-flow diagrams (Mermaid)
├── .gitignore
├── LICENSE
└── README.md
```

## Requirements

- .NET SDK 10.0. To use .NET 8 instead, change `<TargetFramework>` in `BankAccountManager.csproj` to `net8.0`.
- NuGet package `Microsoft.Data.Sqlite` 9.0.23. It is already listed in the project file, and `dotnet build` downloads it automatically.
- Visual Studio Code with the C# Dev Kit extension, or Visual Studio (optional)

## Build and Run

```bash
git clone https://github.com/DonmcfSTU/bank-account-manager.git
cd bank-account-manager/src/BankAccountManager
dotnet build
dotnet run
```

## How to Use

1. Read the welcome screen, type a menu number, and press Enter.
2. When asked for a customer or account, the program shows the list first. Type a Customer ID (example: `1003`) or an account number (example: `CHK-100101`).
3. Amounts can be typed with or without `$` and commas (example: `1,250.00`).
4. Options that change or remove data (7, 10, and 13) ask for a Y/N confirmation first.
5. When updating a customer, press Enter on any field to keep its current value.
6. Type `Q` to quit. Changes are already saved, so nothing is lost.

## Project Summary

### Project Name

Bank Account Manager

### Project Description

Bank Account Manager is a menu-driven C# console application that lets a bank employee manage customers and their checking, savings, and money market accounts. It supports adding, viewing, updating, and deleting customers and accounts, making deposits and withdrawals, running month-end fees and interest, and viewing each account's transaction history. All data is stored in an SQLite database, so it is still there the next time the program starts. The project was built in five phases, and each phase added a group of object-oriented programming concepts.

### Project Tasks

- **Task 1: Plan the user interaction and build the first classes (Phase-1)**
  - Wrote the User Interaction Document (menus, input, output formatting, and exiting)
  - Built the account inheritance hierarchy and the `Customer`/`Address` composition
- **Task 2: Design the software (Phase-2)**
  - Wrote the Software Design Document with a UML class diagram and flowchart
  - Added the `IInterestBearing` interface, the `Bank` class, and polymorphic month-end processing
- **Task 3: Add abstraction, constructors, and access specifiers (Phase-3)**
  - Made `Account` an abstract class and added constructor overloads and chaining
  - Reviewed every class and changed public setters to private setters with validated update methods
  - Added the `Transaction` class and options to add, open, close, and update records
- **Task 4: Add the SQLite database (Phase-4)**
  - Designed the `Customers`, `Accounts`, and `Transactions` tables
  - Wrote `DatabaseManager` with Create, Read, Update, and Delete operations using parameterized SQL
- **Task 5: Test, fix, and deliver (Final-Phase)**
  - Tested every menu option, invalid input, and restarting the program to confirm the data was saved
  - Fixed issues found in testing (loading overdrawn accounts, reusing deleted numbers after a restart, re-asking only the field with a bad state or ZIP code)
  - Wrote this README and the final documentation

### Project Skills Learned

- Object-oriented design: inheritance, composition, interfaces, abstract classes, and polymorphism
- Constructors (overloading, chaining, copy constructors) and access specifiers for encapsulation
- Relational database design and SQL (CREATE, INSERT, SELECT, UPDATE, DELETE, foreign keys, transactions)
- Using a NuGet package (`Microsoft.Data.Sqlite`) from C#
- Separating a program into layers (models, user interface, and data access)
- Input validation and error handling
- Version control with Git and GitHub (commits, tags, and releases)
- Writing technical documentation

### Language Used

- **C# (.NET)**: for the application, using object-oriented programming
- **SQL (SQLite)**: for storing and retrieving the data

### Development Process Used

- **Iterative development**: the application was built in five phases (one per week). Each phase built on the one before it and was tagged in GitHub (`Phase-1` through `Phase-4`, then `Final-Phase`).

### Notes

- Requires the .NET 10 SDK (or change the target framework to `net8.0` for .NET 8).
- Dependencies:
  - `Microsoft.Data.Sqlite` 9.0.23 (restored automatically by `dotnet build`)
- Run with `dotnet run` from `src/BankAccountManager`. The database file `bank.db` is created there on the first run.
- All customers in the sample data are made up for testing.

### Link to Project

[Bank Account Manager Repository](https://github.com/DonmcfSTU/bank-account-manager)

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

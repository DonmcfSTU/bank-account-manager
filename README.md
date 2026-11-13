# SDC320L Course Project – Bank Account Management Application

**Course:** SDC320L – Advanced Object-Oriented Programming Using C#  
**School:** ECPI University, College of Technology  
**Student:** Donavan D. McFadden (Donmcf8357)  
**Instructor:** Dr. Wissam Hammoud  

## About This Project

For my SDC320L course project, I chose the **Bank Account Management Application** option. It's a C# console application that lets a bank employee view customers and their accounts, make deposits and withdrawals, and (in later phases) add, remove, and update customers and accounts. I'm building it in phases over the five-week term, and each phase gets its own GitHub tag.

| Phase | Tag | Focus |
|-------|-----|-------|
| Week 1 | `Phase-1` | User interaction design, inheritance, composition |
| Week 2 | `Phase-2` | Polymorphism and an interface |
| Week 3 | `Phase-3` | Constructors, access specifiers, abstract classes |
| Week 4 | `Phase-4` | SQLite database |
| Week 5 | `Phase-5` | Final project |

## Week 3 Features (Phase 3)

- Header updated to show **Week 3 (Phase 3)**
- **Abstraction:** `Account` is now an `abstract` class. `AccountType`, `GetAccountDetails()`, `ProcessMonthEnd()`, and the protected `CanWithdraw()` rule are abstract, so every account type must provide them. `Withdraw()` is written once in the base class and asks the child's `CanWithdraw()` rule.
- **Constructors:** multiple constructors per class with `: this(...)` chaining and `: base(...)` calls, protected constructors on the abstract class, a parameterless `Bank()` constructor, a copy constructor on `Address`, and internal constructors on `Transaction`
- **Access specifiers:** public setters replaced with `private set` + validated update methods; balance can only change through the protected `PostTransaction()` method; private helpers; `internal` members used only inside the project
- New `Transaction` class and `TransactionType` enum - every account keeps its own transaction history (composition)
- New menu options: **8** add customer, **9** open account, **10** close account, **11** update customer, **12** transaction history

## Week 2 Features (Phase 2)

- Header updated to show **Week 2 (Phase 2)**
- New `IInterestBearing` interface, implemented by `SavingsAccount` (and inherited by `MoneyMarketAccount`)
- New `Bank` class that holds all customers and finds customers/accounts
- **Option 6 – Interest report:** lists only interest-earning accounts by working through the `IInterestBearing` interface
- **Option 7 – Month-end processing:** one loop calls `ProcessMonthEnd()` on every account; checking charges its fee, savings adds interest, and money market adds interest or charges a low-balance fee (polymorphism)
- Fourth sample customer added to show the money market low-balance rule

## Week 1 Features

- Header showing the project week, assignment title, and my name
- Welcome message with instructions and a short quote
- Menu-driven console interface that keeps running until the user types `Q`, `Quit`, or `Exit`
- List all customers and all accounts in aligned tables
- View one customer's profile, address, and accounts
- Deposit and withdraw, with input validation and account-specific rules
- Sample customers and accounts loaded at startup (a database comes in Week 4)

## OOP Concepts Demonstrated

**Inheritance**

```
Account                    (base class)
├── CheckingAccount        (derived – monthly fee, overdraft limit)
└── SavingsAccount         (derived – interest rate)
    └── MoneyMarketAccount (derived from SavingsAccount – minimum balance)
```

**Composition**

- `Bank` **has a** list of `Customer` objects
- `Customer` **has an** `Address` (street, city, state, ZIP)
- `Customer` **has a** list of `Account` objects

**Interface**

- `IInterestBearing` defines `InterestRate`, `CalculateMonthlyInterest()`, and `ApplyMonthlyInterest()`
- Implemented by `SavingsAccount`; `MoneyMarketAccount` inherits it and overrides the calculation

**Polymorphism**

- `Withdraw()`, `GetAccountDetails()`, and `ProcessMonthEnd()` are `virtual` in `Account` and overridden in each account type
- Accounts are stored in a `List<Account>`, and the correct override runs for each object at run time
- The interest report works with a `List<IInterestBearing>` without knowing the concrete account types

## Folder Structure

```
sdc320l-course-project/
├── src/
│   └── BankAccountManager/
│       ├── BankAccountManager.csproj
│       ├── Program.cs              # Entry point and main menu loop
│       ├── Interfaces/
│       │   └── IInterestBearing.cs # Interface for interest-earning accounts
│       ├── Models/
│       │   ├── Bank.cs             # Holds all customers (composition)
│       │   ├── Account.cs          # Abstract base class
│       │   ├── Transaction.cs      # One entry in an account's history
│       │   ├── TransactionType.cs  # Enum of transaction kinds
│       │   ├── CheckingAccount.cs  # Derived class
│       │   ├── SavingsAccount.cs   # Derived class, implements IInterestBearing
│       │   ├── MoneyMarketAccount.cs # Derived from SavingsAccount
│       │   ├── Address.cs          # Used by Customer (composition)
│       │   └── Customer.cs         # Composition: has Address + Accounts
│       ├── UI/
│       │   └── ConsoleUI.cs        # All console input/output
│       └── Data/
│           └── SampleData.cs       # Creates the starting customers
├── docs/
├── .gitignore
└── README.md
```

## Requirements

- .NET SDK 10.0 (if your machine has an older SDK, such as 8.0, change `<TargetFramework>` in `BankAccountManager.csproj` to match, e.g. `net8.0`)
- Visual Studio Code with the C# Dev Kit extension, or Visual Studio

## Build and Run

```bash
cd src/BankAccountManager
dotnet build
dotnet run
```

## How to Use

1. Read the welcome screen and type a menu number, then press Enter.
2. For option 3, type a Customer ID from the list (example: `1003`).
3. For options 4 and 5, type an account number (example: `CHK-100101`) and a dollar amount.
4. Option 6 shows the interest report; option 7 asks for confirmation (Y/N) and then runs month-end processing.
5. Options 8-11 add a customer, open an account (checking, savings, or money market), close an account (Y/N confirmation), and update a customer (press Enter to keep a value).
6. Option 12 shows the transaction history for one account.
7. Type `Q` at the main menu to exit.

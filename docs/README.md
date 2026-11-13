# Bank Account Manager - Design Diagrams

These diagrams summarize the final design. GitHub draws them from the Mermaid text below. The full User Interaction Document and Software Design Document were submitted as Word documents with the final project.

## Class Diagram

```mermaid
classDiagram
    direction LR
    class IInterestBearing {
        <<interface>>
        +InterestRate decimal
        +CalculateMonthlyInterest() decimal
        +ApplyMonthlyInterest() decimal
    }
    class Account {
        <<abstract>>
        +AccountNumber string
        +DateOpened DateTime
        +OwnerName string
        -_balance decimal
        +Balance decimal
        +AccountType string*
        +Deposit(amount)
        +Withdraw(amount)
        #CanWithdraw(amount, reason)* bool
        +GetAccountDetails()* string
        +ProcessMonthEnd()* string
        #PostTransaction(type, amount, description)
        ~RestoreHistory(balance, history)
        ~CloseOut() decimal
    }
    class CheckingAccount {
        +MonthlyFee decimal
        +OverdraftLimit decimal
        +ChangeFee(newFee)
    }
    class SavingsAccount {
        +InterestRate decimal
        +ChangeInterestRate(newRate)
    }
    class MoneyMarketAccount {
        +MinimumBalance decimal
        +LowBalanceFee decimal
    }
    class Transaction {
        +Date DateTime
        +Type TransactionType
        +Amount decimal
        +BalanceAfter decimal
        +Description string
    }
    class Customer {
        +CustomerId int
        +FirstName string
        +LastName string
        +Phone string
        +Email string
        +UpdateContactInfo(...)
        +UpdateAddress(address)
        +AddAccount(account)
        +CloseAccount(account) decimal
        +GetTotalBalance() decimal
    }
    class Address {
        +Street string
        +City string
        +State string
        +ZipCode string
    }
    class Bank {
        +Name string
        +AddCustomer(customer)
        +RemoveCustomer(customer)
        +GenerateAccountNumber(customer, prefix) string
        +FindCustomer(id) Customer
        +FindAccount(number) Account
        +GetAllAccounts() List~Account~
        +GetInterestBearingAccounts() List~IInterestBearing~
    }
    class DatabaseManager {
        +FileName string
        +InitializeDatabase() bool
        +LoadBank() Bank
        +InsertCustomer(customer)
        +InsertAccount(customerId, account)
        +UpdateCustomer(customer)
        +SaveAccountActivity(accounts)
        +DeleteAccount(number)
        +DeleteCustomer(id)
    }
    Account <|-- CheckingAccount
    Account <|-- SavingsAccount
    SavingsAccount <|-- MoneyMarketAccount
    IInterestBearing <|.. SavingsAccount
    Bank *-- "0..*" Customer
    Customer *-- "1" Address
    Customer *-- "0..*" Account
    Account *-- "1..*" Transaction
    DatabaseManager ..> Bank : loads and saves
```

## Database Tables

```mermaid
erDiagram
    Customers ||--o{ Accounts : "has (ON DELETE RESTRICT)"
    Accounts ||--|{ Transactions : "has (ON DELETE CASCADE)"
    Customers {
        INTEGER CustomerId PK
        TEXT FirstName
        TEXT LastName
        TEXT Phone
        TEXT Email
        TEXT Street
        TEXT City
        TEXT State
        TEXT ZipCode
    }
    Accounts {
        TEXT AccountNumber PK
        INTEGER CustomerId FK
        TEXT AccountType
        NUMERIC Balance
        TEXT DateOpened
        NUMERIC MonthlyFee
        NUMERIC OverdraftLimit
        NUMERIC InterestRate
        NUMERIC MinimumBalance
    }
    Transactions {
        INTEGER TransactionId PK
        TEXT AccountNumber FK
        TEXT TransDate
        TEXT TransType
        NUMERIC Amount
        NUMERIC BalanceAfter
        TEXT Description
    }
    RetiredIds {
        TEXT IdType PK
        TEXT IdValue PK
        TEXT RetiredOn
    }
```

## Main Menu Flow

```mermaid
flowchart TD
    A([Start]) --> B[Show header and welcome instructions]
    B --> C[Open bank.db and create tables if needed]
    C --> D{New database?}
    D -- Yes --> E[Save the sample data]
    D -- No --> F[LoadBank from the database]
    E --> G[Show main menu]
    F --> G
    G --> H{User choice}
    H -- 1 to 14 --> I[Run the option and save any changes]
    H -- other --> J[Show an error message]
    I --> K[Press Enter to return]
    J --> K
    K --> G
    H -- Q / Quit / Exit --> L([Goodbye])
```

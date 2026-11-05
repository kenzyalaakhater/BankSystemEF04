using BankSystemEF04.Data;
using BankSystemEF04.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
namespace BankSystemEF04;
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            using var context = new AppDbContext();
            try
            {
                context.Database.Migrate();   // make sure the database is up to date
            }
            catch (Exception ex)
            {
                Error($"Cannot connect to the database: {ex.Message}");
                return;
            }
            bool running = true;
            while (running)
            {
                ShowMenu();
                int choice = ReadChoice("  Enter choice: ", 0, 5);
                Console.WriteLine();
                try
                {
                    switch (choice)
                    {
                        case 1: AddCustomer(context); break;
                        case 2: OpenAccount(context); break;
                        case 3: UpdateAccountStatus(context); break;
                        case 4: RemoveAccountFromCustomer(context); break;
                        case 5: ListCustomers(context); break;
                        case 0: running = false; break;
                    }
                }
                catch (DbUpdateException ex)
                {
                    Error("Database error: " + (ex.InnerException?.Message ?? ex.Message));
                }
                catch (Exception ex)
                {
                    Error("Unexpected error: " + ex.Message);
                }
                context.ChangeTracker.Clear();   // forget anything left over from a failed operation
                if (running)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to return to the menu...");
                    Console.ReadKey(true);
                }
            }
            context.Database.CloseConnection();
            Console.WriteLine("Connection closed. Goodbye!");
        }
        // MENU
        static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("    National Bank — Management");
            Console.WriteLine("  1) Add a new Customer");
            Console.WriteLine("  2) Open a new Account for a Customer");
            Console.WriteLine("  3) Update Account Status (Active / Closed)");
            Console.WriteLine("  4) Remove an Account from a Customer");
            Console.WriteLine("  5) List all Customers (with accounts)");
            Console.WriteLine("  0) Exit");
        }
        // 1) ADD CUSTOMER
        static void AddCustomer(AppDbContext context)
        {
            Console.WriteLine("--- Add New Customer ---");
            var fullName = ReadText("Full Name      : ", s =>
                s.Length < 3 ? "Name must be at least 3 characters." :
                s.Length > 150 ? "Name is too long (max 150 characters)." : null);
            var nationalId = ReadText("National ID    : ", s =>
                !Regex.IsMatch(s, @"^\d{10,14}$") ? "National ID must be 10 to 14 digits." :
                context.Customers.Any(c => c.NationalId == s) ? "A customer with this National ID already exists." : null);
            var dobText = ReadText("Date of Birth  : (yyyy-MM-dd) ", s =>
            {
                if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    return "Please use the format yyyy-MM-dd (example 2000-01-25).";
                if (d > DateTime.Today || d.Year < 1900)
                    return "Date of birth must be a real date in the past.";
                return null;
            });
            var dob = DateTime.ParseExact(dobText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var email = ReadText("Email          : ", s =>
                s.Length > 200 ? "Email is too long (max 200 characters)." :
                !Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? "Please enter a valid email address." : null);
            var phone = ReadText("Phone          : ", s =>
                !Regex.IsMatch(s, @"^\+?\d{8,15}$") ? "Phone must be 8 to 15 digits (optional leading +)." : null);
            var address = ReadText("Address        : ", s =>
                s.Length == 0 ? "Address is required." :
                s.Length > 300 ? "Address is too long (max 300 characters)." : null);
            Console.WriteLine("Customer Type:");
            Console.WriteLine("    1) Individual");
            Console.WriteLine("    2) Business");
            var type = (CustomerType)(ReadChoice("  Choice: ", 1, 2) - 1);
            var customer = new Customer
            {
                FullName = fullName,
                NationalId = nationalId,
                DateOfBirth = dob,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = type
            };
            context.Customers.Add(customer);
            context.SaveChanges();
            Console.WriteLine();
            Success($"Customer created successfully. CustomerId = {customer.Id}");
        }
        // 2) OPEN ACCOUNT
        static void OpenAccount(AppDbContext context)
        {
            Console.WriteLine("--- Open New Account ---");

            var accountNumber = ReadText("Account Number : ", s =>
                !Regex.IsMatch(s, @"^[A-Za-z0-9\-]{3,20}$")
                    ? "Account number must be 3 to 20 letters, digits or '-'." : null).ToUpperInvariant();

            Console.WriteLine("Account Type:");
            Console.WriteLine("    1) Savings");
            Console.WriteLine("    2) Current");
            Console.WriteLine("    3) Business");
            var accountType = (AccountType)(ReadChoice("  Choice: ", 1, 3) - 1);

            var branchCode = ReadText("Branch Code    : ", s =>
                s.Length == 0 || s.Length > 10 ? "Branch code is required (max 10 characters)." : null).ToUpperInvariant();

            var customerId = ReadInt("Customer Id    : ");

            Console.WriteLine("Ownership Role:");
            Console.WriteLine("    1) Primary");
            Console.WriteLine("    2) CoHolder");
            var role = (OwnershipType)(ReadChoice("  Choice: ", 1, 2) - 1);

            Info($"Validating branch '{branchCode}' and customer #{customerId}...");

            // The branch and the customer must exist before anything is created
            var branch = context.Branches.Find(branchCode);
            if (branch == null)
            {
                Error($"Branch '{branchCode}' does not exist.");
                return;
            }

            var customer = context.Customers.Find(customerId);
            if (customer == null)
            {
                Error($"Customer #{customerId} does not exist.");
                return;
            }

            var account = context.Accounts.Find(accountNumber);
            bool isNewAccount = account == null;

            if (isNewAccount)
            {
                if (role == OwnershipType.CoHolder)
                {
                    Error("A new account must be opened with a Primary owner. A CoHolder can only join an existing account.");
                    return;
                }

                context.Accounts.Add(new Account
                {
                    AccountNumber = accountNumber,
                    AccountType = accountType,
                    CurrentBalance = 0m,
                    OpeningDate = DateTime.Today,
                    BranchCode = branchCode
                });
            }
            else
            {
                if (account!.BranchCode != branchCode)
                {
                    Error($"Account '{accountNumber}' belongs to branch '{account.BranchCode}', not '{branchCode}'.");
                    return;
                }

                if (context.CustomerAccounts.Any(ca => ca.AccountNumber == accountNumber && ca.CustomerId == customerId))
                {
                    Error($"Customer #{customerId} is already linked to account '{accountNumber}'.");
                    return;
                }

                if (role == OwnershipType.Primary &&
                    context.CustomerAccounts.Any(ca => ca.AccountNumber == accountNumber && ca.OwnershipType == OwnershipType.Primary))
                {
                    Error($"Account '{accountNumber}' already has a Primary owner. Add this customer as a CoHolder.");
                    return;
                }
            }

            context.CustomerAccounts.Add(new CustomerAccount
            {
                CustomerId = customerId,
                AccountNumber = accountNumber,
                OwnershipType = role,
                OwnershipStartDate = DateTime.Today,
                AccountStatus = AccountStatus.Active
            });
            context.SaveChanges();

            Success(isNewAccount
                ? $"Account '{accountNumber}' created and linked to customer {customerId} as {role} owner."
                : $"Customer {customerId} linked to existing account '{accountNumber}' as {role}.");
        }

        // 3) UPDATE ACCOUNT STATUS
        static void UpdateAccountStatus(AppDbContext context)
        {
            Console.WriteLine("--- Update Account Status ---");

            var accountNumber = ReadText("Account Number : ", s =>
                s.Length == 0 ? "Account number is required." : null).ToUpperInvariant();
            var customerId = ReadInt("Customer Id    : ");

            Console.WriteLine("New Status:");
            Console.WriteLine("    1) Active");
            Console.WriteLine("    2) Closed");
            var newStatus = (AccountStatus)(ReadChoice("  Choice: ", 1, 2) - 1);

            // Composite key order: CustomerId, AccountNumber
            var link = context.CustomerAccounts.Find(customerId, accountNumber);
            if (link == null)
            {
                Error($"Customer #{customerId} is not linked to account '{accountNumber}'.");
                return;
            }

            if (link.AccountStatus == newStatus)
            {
                Info($"The status is already {newStatus}. Nothing to update.");
                return;
            }

            link.AccountStatus = newStatus;
            context.SaveChanges();
            Success($"Status updated to {newStatus}.");
        }
        // 4) REMOVE ACCOUNT FROM CUSTOMER
        static void RemoveAccountFromCustomer(AppDbContext context)
        {
            Console.WriteLine("--- Remove Account From Customer ---");

            var accountNumber = ReadText("Account Number : ", s =>
                s.Length == 0 ? "Account number is required." : null).ToUpperInvariant();
            var customerId = ReadInt("Customer Id    : ");

            var link = context.CustomerAccounts.Find(customerId, accountNumber);
            if (link == null)
            {
                Console.WriteLine();
                Error($"Customer #{customerId} is not linked to account '{accountNumber}'.");
                return;
            }

            context.CustomerAccounts.Remove(link);

            bool hasOtherOwners = context.CustomerAccounts
                .Any(ca => ca.AccountNumber == accountNumber && ca.CustomerId != customerId);

            bool accountRemoved = false;
            bool keptBecauseOfHistory = false;

            if (!hasOtherOwners)
            {
                // That was the last owner. Remove the account too, unless it has transactions.
                if (context.Transactions.Any(t => t.AccountNumber == accountNumber))
                {
                    keptBecauseOfHistory = true;
                }
                else
                {
                    var account = context.Accounts.Find(accountNumber);
                    if (account != null)
                    {
                        context.Accounts.Remove(account);
                        accountRemoved = true;
                    }
                }
            }

            context.SaveChanges();

            Console.WriteLine();
            Success("  Ownership link deleted.");
            if (accountRemoved)
                Info($"      That was the last owner — account '{accountNumber}' was also removed.");
            else if (keptBecauseOfHistory)
                Info($"      That was the last owner, but account '{accountNumber}' was kept because it has transactions.");
        }

        // ---------------------------------------------------------------
        // 5) LIST CUSTOMERS (EAGER LOADING)
        // ---------------------------------------------------------------
        static void ListCustomers(AppDbContext context)
        {
            Console.WriteLine("--- All Customers ---");
            Console.WriteLine();

            // Eager loading: Customer -> CustomerAccounts -> Account -> Branch in ONE query
            var customers = context.Customers
                .AsNoTracking()
                .Include(c => c.CustomerAccounts)
                    .ThenInclude(ca => ca.Account)
                        .ThenInclude(a => a.Branch)
                .OrderBy(c => c.Id)
                .ToList();

            if (customers.Count == 0)
            {
                Info("  (no customers yet)");
                return;
            }

            foreach (var c in customers)
            {
                Console.WriteLine($"  #{c.Id} {c.FullName} ({c.CustomerType})");

                if (c.CustomerAccounts.Count == 0)
                {
                    Console.WriteLine("      (no accounts)");
                    continue;
                }

                foreach (var ca in c.CustomerAccounts)
                {
                    var a = ca.Account;
                    Console.WriteLine(
                        $"      {a.AccountNumber,-10} {a.AccountType,-9} Balance: {a.CurrentBalance,12:N2}   " +
                        $"{ca.OwnershipType,-8} {ca.AccountStatus,-7} @ {a.Branch.Name}");
                }
            }
        }
        // INPUT HELPERS (never crash on bad input)
        static string ReadText(string prompt, Func<string, string?> validate)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = (Console.ReadLine() ?? "").Trim();
                var error = validate(input);
                if (error == null) return input;
                Error("  " + error);
            }
        }

        static int ReadChoice(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var n) && n >= min && n <= max)
                    return n;
                Error($"  Invalid input. Please enter a number between {min} and {max}.");
            }
        }

        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var n) && n > 0)
                    return n;
                Error("  Invalid input. Please enter a positive whole number.");
            }
        }
        // COLORED OUTPUT
        static void WriteColor(string message, ConsoleColor color)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = old;
        }
        static void Success(string message) => WriteColor(message, ConsoleColor.Green);
        static void Error(string message) => WriteColor(message, ConsoleColor.Red);
        static void Info(string message) => WriteColor(message, ConsoleColor.Cyan);
    }

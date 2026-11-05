namespace BankSystemEF04.Models;
public enum CustomerType { Individual, Business }
public enum AccountType { Savings, Current, Business }
public enum OwnershipType { Primary, CoHolder }
public enum AccountStatus { Active, Closed }
public enum TransactionType { Deposit, Withdrawal, Transfer }
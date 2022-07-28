namespace EtlService.Input;

public record Transaction(
    string FirstName,
    string LastName,
    string City,
    decimal Payment,
    DateOnly Date,
    long AccountNumber,
    string Service);

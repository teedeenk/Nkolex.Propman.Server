namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccount
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Surname { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        bool AgreeToTerms { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        DateTime? DeletedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}

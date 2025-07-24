namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccount
    {
        string Name { get; set; }
        string Surname { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}

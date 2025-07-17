namespace Nkolex.Propman.Server.Abstractions
{
    public interface ICreateAccountResponse
    {
        bool Success { get; set; }
        string Message { get; set; }
        string UserId { get; set; }
    }
}

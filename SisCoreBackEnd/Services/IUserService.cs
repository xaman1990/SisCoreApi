using SisCoreBackEnd.Domain.Tenant;

namespace SisCoreBackEnd.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetUsersAsync();
        Task<User> UpdateUserAsync(int id, string? email, string? phoneNumber, string? fullName, string? employeeNumber);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignRolesAsync(int userId, List<int> roleIds, int? assignedBy);
    }
}


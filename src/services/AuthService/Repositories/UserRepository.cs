using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<User?> GetByLoginIdentifierWithRoleAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = identifier.Trim();
        var normalizedEmail = normalizedIdentifier.ToLowerInvariant();
        var normalizedPhone = NormalizePhoneNumber(normalizedIdentifier);

        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Email == normalizedEmail
                    || u.Username == normalizedIdentifier
                    || (normalizedPhone != null && u.PhoneNumber == normalizedPhone),
                cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.PhoneNumber == NormalizePhoneNumber(phoneNumber), cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }
}

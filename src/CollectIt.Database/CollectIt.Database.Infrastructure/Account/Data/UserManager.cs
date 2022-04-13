using System.Data.Common;
using CollectIt.Database.Abstractions.Account.Exceptions;
using CollectIt.Database.Entities.Account;
using CollectIt.Database.Entities.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Npgsql;

namespace CollectIt.Database.Infrastructure.Account.Data;

public class UserManager: UserManager<User>
{
    private readonly PostgresqlCollectItDbContext _context;

    public UserManager(IUserStore<User> store,
                       IOptions<IdentityOptions> optionsAccessor,
                       IPasswordHasher<User> passwordHasher,
                       IEnumerable<IUserValidator<User>> userValidators,
                       IEnumerable<IPasswordValidator<User>> passwordValidators,
                       ILookupNormalizer keyNormalizer,
                       IdentityErrorDescriber errors,
                       IServiceProvider services,
                       ILogger<UserManager> logger,
                       PostgresqlCollectItDbContext context)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
               services, logger)
    {
        _context = context;
    }

    public Task<User?> FindUserByIdAsync(int id)
    {
        return _context.Users
                       .SingleOrDefaultAsync(u => u.Id == id);
    }

    public Task<List<UserSubscription>> GetSubscriptionsForUserAsync(User user)
    {
        return GetSubscriptionsForUserByIdAsync(user.Id);
    }

    public Task<List<UserSubscription>> GetSubscriptionsForUserByIdAsync(int userId)
    {
        return _context.UsersSubscriptions
                       .Where(us => us.UserId == userId)
                       .Include(us => us.Subscription)
                       .Include(us => us.User)
                       .ToListAsync();
    }

    public Task<List<AcquiredUserResource>> GetAcquiredResourcesForUserByIdAsync(int userId)
    {
        return _context.AcquiredUserResources
                       .Where(us => us.UserId == userId)
                       .Include(us => us.Resource)
                       .Include(us => us.User)
                       .ToListAsync();
    }
    
    public Task<List<ActiveUserSubscription>> GetActiveSubscriptionsForUserAsync(User user)
    {
        return GetActiveSubscriptionsForUserByIdAsync(user.Id);
    }

    public Task<List<ActiveUserSubscription>> GetActiveSubscriptionsForUserByIdAsync(int userId)
    {
        return _context.ActiveUsersSubscriptions
                       .Where(aus => aus.UserId == userId)
                       .Include(aus => aus.Subscription)
                       .ThenInclude(s => s.Restriction)
                       .Include(aus => aus.User)
                       .ToListAsync();
    }

    public Task<List<User>> GetUsersPaged(int pageNumber, int pageSize)
    {
        return _context.Users
                       .Include(u => u.Roles)
                       .OrderBy(u => u.Id)
                       .Skip(( pageNumber - 1 ) * pageSize)
                       .Take(pageSize)
                       .ToListAsync();
    }

    public Task<List<Role>> GetRolesAsync(int userId)
    {
        return _context.Users
                       .Where(u => u.Id == userId)
                       .Include(u => u.Roles)
                       .SelectMany(u => u.Roles)
                       .ToListAsync();
    }

    public Task<bool> UserBoughtResourceAsync(int userId, int resourceId)
    {
        var resource = new Resource() {Id = resourceId};
        return _context.Users
                       .AnyAsync(u => u.AcquiredResources.Contains(resource));
    }
    
    /// <exception cref="UserNotFoundException">User with provided id does not exist</exception>
    /// <exception cref="AccountException">User with provided username already exists</exception>
    public async Task ChangeUsernameAsync(int userId, string username)
    {
        var user = await FindUserByIdAsync(userId);
        var result = await SetUserNameAsync(user, username);
        if (result.Succeeded)
        {
            return;
        }

        throw new AccountException(result.Errors.Select(err => err.Description).Aggregate((s, n) => $"{s}\n{n}"));
    }

    private static DbParameter CreateParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }
}
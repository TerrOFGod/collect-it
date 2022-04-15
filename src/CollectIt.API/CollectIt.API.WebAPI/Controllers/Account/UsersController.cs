using System.ComponentModel.DataAnnotations;
using CollectIt.API.DTO.Mappers;
using CollectIt.Database.Abstractions.Account.Exceptions;
using CollectIt.Database.Abstractions.Account.Interfaces;
using CollectIt.Database.Entities.Account;
using CollectIt.Database.Infrastructure;
using CollectIt.Database.Infrastructure.Account.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static CollectIt.API.DTO.AccountDTO;

namespace CollectIt.API.WebAPI.Controllers.Account;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager _userManager;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager userManager, 
                           ISubscriptionManager subscriptionManager,
                           ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _subscriptionManager = subscriptionManager;
        _logger = logger;
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(AccountMappers.ToReadUserDTO(user, roles.ToArray()));
    }

    [HttpGet("")]
    public async Task<IActionResult> GetUsersPaged([FromQuery(Name = "page_number")][Range(1, int.MaxValue)]int pageNumber = 1,
                                                   [FromQuery(Name = "page_size")][Range(1, int.MaxValue)] int pageSize = 10)
    {
        var users = ( await _userManager.GetUsersPaged(pageNumber, pageSize) )
           .Select(u => AccountMappers.ToReadUserDTO(u, u.Roles
                                                         .Select(r => r.Name)
                                                         .ToArray()));
        return Ok(users);
    }

    [HttpGet("{userId:int}/subscriptions")]
    public async Task<IActionResult> GetUsersSubscriptions(int userId)
    {
        var subscriptions = await _userManager.GetSubscriptionsForUserByIdAsync(userId);
        return Ok(subscriptions.Select(AccountMappers.ToReadUserSubscriptionDTO)
                               .ToArray());
    }

    [HttpGet("{userId:int}/active-subscriptions")]
    public async Task<IActionResult> GetActiveUserSubscription(int userId)
    {
        var activeSubscriptions = await _userManager.GetActiveSubscriptionsForUserByIdAsync(userId);
        return Ok(activeSubscriptions.Select(AccountMappers.ToReadUserSubscriptionDTOFromActiveUserSubscription));
    }

    [HttpGet("{userId:int}/roles")]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        var roles = await _userManager.GetRolesAsync(userId);
        return Ok(roles.Select(AccountMappers.ToReadRoleDTO)
                       .ToArray());
    }

    [HttpPost("{userId:int}/username")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangeUsername([FromForm(Name = "username")]
                                                    [Required]
                                                    string username, 
                                                    int userId)
    {
        try
        {
            var user = await _userManager.FindUserByIdAsync(userId);
            if (user is null)
            {
                return NotFound("User with provided claims not found");
            }
            if (!( user.Id == userId || await _userManager.IsInRoleAsync(user, "ADMIN") ))
            {
                
                return Unauthorized("Not authorize blyat");
            }

            await _userManager.ChangeUsernameAsync(userId, username);
            return NoContent();
        }
        catch (UserNotFoundException notFoundException)
        {
            return NotFound();
        }
        catch (AccountException accountException)
        {
            return BadRequest(accountException.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{userId:int}/email")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangeUserEmail(int userId, 
                                                     [FromForm(Name = "email")]
                                                     [Required]
                                                     string email)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        if (!(userId == user.Id || await _userManager.IsInRoleAsync(user, Role.AdminRoleName)))
        {
            return Forbid();
        }
        var result = await _userManager.SetEmailAsync(user, email);
        return result.Succeeded
                   ? NoContent()
                   : BadRequest(result.Errors
                                      .Select(e => e.Description)
                                      .ToArray());
    }

    [HttpPost("{userId:int}/roles")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AssignRoleToUser(int userId, 
                                                      [FromForm(Name = "role_name")]
                                                      [Required]
                                                      string role)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
                   ? NoContent()
                   : BadRequest(result.Errors
                                      .Select(err => err.Description)
                                      .ToArray());
    }
 
    [HttpDelete("{userId:int}/roles")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoveRoleFromUser(int userId, 
                                                        [FromForm(Name = "role_name")]string role)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded
                   ? NoContent()
                   : BadRequest(result.Errors
                                      .Select(err => err.Description)
                                      .ToArray());
    }

    [HttpPost("{userId:int}/activate")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ActivateAccount(int userId)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        var result = await _userManager.SetLockoutEnabledAsync(user, false);
        return result.Succeeded
                   ? NoContent()
                   : BadRequest(result.Errors
                                      .Select(err => err.Description)
                                      .ToArray());
    }
    
    [HttpPost("{userId:int}/deactivate")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeactivateAccount(int userId)
    {
        var user = await _userManager.FindUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        var result = await _userManager.SetLockoutEnabledAsync(user, true);
        return result.Succeeded
                   ? NoContent()
                   : BadRequest(result.Errors
                                      .Select(err => err.Description)
                                      .ToArray());
    }
}
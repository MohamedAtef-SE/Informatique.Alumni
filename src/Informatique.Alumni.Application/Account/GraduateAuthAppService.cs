using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Account;

[Authorize]
public class GraduateAuthAppService : ApplicationService, IGraduateAuthAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IEmailSender _emailSender;

    public GraduateAuthAppService(
        IdentityUserManager userManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [AllowAnonymous]
    public async Task LoginAsync(GraduateLoginDto input)
    {
        var user = await _userManager.FindByNameAsync(input.UserName);
        if (user == null)
        {
            throw new UserFriendlyException("Invalid username or password.");
        }

        // 1. Check if already locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new UserFriendlyException("Your account is locked due to too many failed attempts.");
        }

        // 2. Check Password
        if (await _userManager.CheckPasswordAsync(user, input.Password))
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            return;
        }

        // 3. Handle Failure
        // Increment Access Failed Count
        await _userManager.AccessFailedAsync(user);

        // 4. Check if Lockout Triggered JUST NOW
        if (await _userManager.IsLockedOutAsync(user))
        {
            // CRITICAL: Immediately send an Email Notification
            await _emailSender.SendAsync(
                user.Email,
                "Account Locked - Alumni System",
                "You have reached 5 failed login attempts. Please change your password."
            );
            
            throw new UserFriendlyException("Your account is locked due to too many failed attempts.");
        }

        throw new UserFriendlyException("Invalid username or password.");
    }

    [AllowAnonymous]
    public async Task ForgotPasswordAsync(string username)
    {
        // 1. Validate User exists
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        // 2. Auto-Generate Compliant Password
        var newPassword = GenerateCompliantPassword();

        // 3. Overwrite old password
        // Using ResetPasswordAsync requires a token.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            throw new UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 4. Send new password to Email
        await _emailSender.SendAsync(
            user.Email,
            "Password Recovery - Alumni System",
            $"Your new password is: {newPassword}"
        );
    }

    private string GenerateCompliantPassword()
    {
        // Rules: 8-20 chars, 1+ Upper, 1+ Lower, 1+ Digit, 1+ Symbol.
        const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowers = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string symbols = "!@#$%^&*()_+[]{}?";
        const string all = uppers + lowers + digits + symbols;

        var random = new Random();
        var password = new StringBuilder();

        // Ensure mandatory characters
        password.Append(uppers[random.Next(uppers.Length)]);
        password.Append(lowers[random.Next(lowers.Length)]);
        password.Append(digits[random.Next(digits.Length)]);
        password.Append(symbols[random.Next(symbols.Length)]);

        // Fill remaining length (Target 10 chars total is safe handling 8-20 constraint)
        // 4 chars already added, add 6 more.
        for (int i = 0; i < 6; i++)
        {
            password.Append(all[random.Next(all.Length)]);
        }

        // Shuffle
        return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
    }
}

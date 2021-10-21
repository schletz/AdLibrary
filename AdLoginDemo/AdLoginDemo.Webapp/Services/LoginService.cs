using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdLoginDemo.Application.Infrastructure;

namespace AdLoginDemo.Webapp.Services
{
    public class LoginService
    {
        // Benötigt services.AddHttpContextAccessor(); in der Startup Klasse.
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _searchuser;
        private readonly string _searchpass;

        public LoginService(IHttpContextAccessor httpContextAccessor, string searchuser, string searchpass, bool isDevelopmentMode)
        {
            _httpContextAccessor = httpContextAccessor;
            _searchuser = searchuser;
            _searchpass = searchpass;
            IsDevelopmentMode = isDevelopmentMode;
        }

        public bool IsDevelopmentMode { get; }

        public async Task<(bool success, string? message)> TryLogin(string username, string password)
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                return (false, "Das verwendete Protokoll erlaubt kein Login.");
            }
            var context = _httpContextAccessor.HttpContext;
            try
            {
                using var service = IsDevelopmentMode ? AdService.Login(_searchuser, _searchpass, username) : AdService.Login(username, password);
                var currentUser = service.CurrentUser;
                if (currentUser is null) { return (false, "Fehler beim Laden der Benutzerinformationen."); }
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, currentUser.Cn),
                        new Claim("Cn", currentUser.Cn),
                        new Claim(ClaimTypes.Role, currentUser.Role.ToString()),
                        new Claim("AdUser", currentUser.ToJson()),
                    };
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3),
                    //IsPersistent = true
                };

                await context.SignInAsync(
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return (true, null);
            }
            catch (ApplicationException e)
            {
                return (false, e.Message);
            }
        }

        public async Task Logout()
        {
            if (_httpContextAccessor.HttpContext is null) { return; }
            var context = _httpContextAccessor.HttpContext;
            await context.SignOutAsync();
        }

        public AdUser? CurrentUser
        {
            get
            {
                if (_httpContextAccessor.HttpContext is null) { return default; }
                var context = _httpContextAccessor.HttpContext;
                var adUserJson = context.User.Claims.FirstOrDefault(c => c.Type == "AdUser")?.Value;
                if (adUserJson is null) { return default; }
                return AdUser.FromJson(adUserJson);
            }
        }

        public bool IsAuthenticated => CurrentUser is not null;
    }
}
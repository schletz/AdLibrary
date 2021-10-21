using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using AdLoginDemo.Webapp.Services;

namespace AdLoginDemo.Webapp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly LoginService _loginService;

        public LoginModel(LoginService loginService)
        {
            _loginService = loginService;
        }

        public bool IsDevelopment => _loginService.IsDevelopmentMode;

        [BindProperty]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Ungültiger Benutzername")]
        public string Username { get; set; } = default!;

        [BindProperty]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Ungültiges Passwort")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [BindProperty]
        [FromQuery]
        public string? ReturnUrl { get; set; }

        [TempData]
        public string Message { get; set; } = default!;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetLogout()
        {
            await _loginService.Logout();
            return LocalRedirect("/");
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var (success, message) = await _loginService.TryLogin(Username, Password);
            if (!success)
            {
                Message = message!;
                return RedirectToPage();
            }
            return LocalRedirect(ReturnUrl ?? "/");
        }
    }
}
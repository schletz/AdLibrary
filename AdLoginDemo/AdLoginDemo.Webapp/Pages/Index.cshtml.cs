using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AdLoginDemo.Application.Infrastructure;
using AdLoginDemo.Webapp.Services;

namespace AdLoginDemo.Webapp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly LoginService loginService;
        public AdUser? CurrentUser { get; private set; } = default!;
        public IndexModel(LoginService loginService)
        {
            this.loginService = loginService;
        }

        public void OnGet()
        {
            CurrentUser = loginService.CurrentUser;
        }
    }
}

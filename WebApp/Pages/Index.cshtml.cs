using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger) => _logger = logger;

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
            _logger.LogInformation("index");
        }

        public IActionResult OnPost() => RedirectToPage("setresult", new { value = Input.SetValue });


        public class InputModel
        {
            [Display(Name = "回车设置温度")]
            [Required]
            public uint SetValue { get; set; }

        }
    }

}
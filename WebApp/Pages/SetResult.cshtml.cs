using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{

    public class GreetingModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public OutputModel? Output { get; set; }
        public GreetingModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string value)
        {
            using var client = _httpClientFactory.CreateClient();

            var greetingResponse = await client.GetAsync($"http://localhost:5002/api?temperature={ HttpUtility.UrlEncode(value)}");

            _logger.LogInformation($"greetingResponse:{greetingResponse.IsSuccessStatusCode}");

            Output = await JsonSerializer.DeserializeAsync<OutputModel>(
                await greetingResponse.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Page();
        }

        public record OutputModel(string Message);
    }
}

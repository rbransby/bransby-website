using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bransby_website.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Bransby_website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EmailSettings _emailSettings;

        [BindProperty]
        public bool HasSubmittedForm { get; set; } = false;

        public IndexModel(IOptions<EmailSettings> emailOptions)
        {
            _emailSettings = emailOptions.Value;
        }

        public void OnGet(bool hasSubmittedForm = false)
        {
            HasSubmittedForm = hasSubmittedForm;
        }

        public async Task<IActionResult> OnPostAsync(string name, string email, string subject, string message)
        {
            using (var client = new HttpClient
            {
                BaseAddress =
                                     new Uri(_emailSettings.ApiBaseUri)
            })
            {
                client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(_emailSettings.ApiKey)));

                var htmlContent = $"A message has been received on the website from {name}. Their email address is {email}. The message is:\n{message}";

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("from", "postmaster@mg.bransby.com.au"),
                    new KeyValuePair<string, string>("to", _emailSettings.From),
                    new KeyValuePair<string, string>("subject", subject),
                    new KeyValuePair<string, string>("html", htmlContent)
                });

                var response = await client.PostAsync(_emailSettings.RequestUri,
                                       content).ConfigureAwait(false);


            }
            
            return RedirectToPage("/Index",new { HasSubmittedForm = true });
        }

    }
}

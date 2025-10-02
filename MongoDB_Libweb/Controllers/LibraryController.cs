using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MongoDB_Libweb.Controllers
{
    public class LibraryController : Controller
    {

        // Home/Browse - Main book discovery page
        public IActionResult Index()
        {
            return View();
        }

        // Book Details - View single book
        public IActionResult BookDetails(string id)
        {
            return View();
        }

        // Search Results - Filtered books
        public IActionResult Search()
        {
            return View();
        }

        // Borrow History - User's borrows
        public IActionResult BorrowHistory()
        {
            return View();
        }

        // Borrow Confirmation Modal (handled via AJAX, but we need a view for the modal)
        public IActionResult BorrowConfirmation(string bookId)
        {
            return View();
        }

        // Profile - Personal info
        public IActionResult Profile()
        {
            return View();
        }

        // Categories - Browse by category
        public IActionResult Categories()
        {
            return View();
        }

        // Category Books - Books in specific category
        public IActionResult CategoryBooks(string categoryId)
        {
            return View();
        }

        // Authors - Browse by author
        public IActionResult Authors()
        {
            return View();
        }

        // Author Books - Books by specific author
        public IActionResult AuthorBooks(string authorId)
        {
            return View();
        }

        // About Us
        public IActionResult About()
        {
            return View();
        }

        // Contact/Support
        public IActionResult Contact()
        {
            return View();
        }

        // Help
        public IActionResult Help()
        {
            return View();
        }

        // Privacy Policy
        public IActionResult Privacy()
        {
            return View();
        }

        // Terms of Service
        public IActionResult Terms()
        {
            return View();
        }

        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Register Page
        public IActionResult Register()
        {
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            // Clear session data
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Library");
        }


        // Process Register
        [HttpPost]
        public async Task<IActionResult> ProcessRegister(string username, string email, string password, string fullName, string role = "User")
        {
            try
            {
                // Call register API
                var response = await CallApiAsync("user/register", HttpMethod.Post, new 
                { 
                    username, 
                    email, 
                    password, 
                    fullName, 
                    role 
                });
                
                if (ApiResponseHelper.IsSuccess(response))
                {
                    TempData["Success"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["Error"] = ApiResponseHelper.GetErrorMessage(response, "Registration failed");
                    return RedirectToAction("Register");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Registration failed: " + ex.Message;
                return RedirectToAction("Register");
            }
        }

       

        // Helper method to call API
        private async Task<JsonElement> CallApiAsync(string endpoint, HttpMethod method, object data = null)
        {
            try
            {
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(method, $"https://localhost:7124/api/v1/{endpoint}");

                if (data != null)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }

                var response = await httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                }
                
                return JsonDocument.Parse("{}").RootElement;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API call error: {ex.Message}");
                return JsonDocument.Parse("{}").RootElement;
            }
        }
    }
}

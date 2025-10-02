using Microsoft.AspNetCore.Mvc;

namespace MongoDB_Libweb.Controllers
{
    public class AdminController : Controller
    {

        // Dashboard - Overview of system stats
        public IActionResult Dashboard()
        {
            return View();
        }

        // User Management
        public IActionResult UserManagement()
        {
            return View();
        }

        // User Details/Edit
        public IActionResult UserDetails(string id)
        {
            return View();
        }

        // Book Management
        public IActionResult BookManagement()
        {
            return View();
        }

        // Book Details/Form
        public IActionResult BookDetails(string? id = null)
        {
            return View();
        }

        // Category Management
        public IActionResult CategoryManagement()
        {
            return View();
        }

        // Category Form
        public IActionResult CategoryForm(string? id = null)
        {
            return View();
        }

        // Author Management
        public IActionResult AuthorManagement()
        {
            return View();
        }

        // Author Form
        public IActionResult AuthorForm(string? id = null)
        {
            return View();
        }

        // Borrow Management
        public IActionResult BorrowManagement()
        {
            return View();
        }

        // Borrow Details
        public IActionResult BorrowDetails(string id)
        {
            return View();
        }

        // Reports
        public IActionResult Reports()
        {
            return View();
        }

        // Profile/Settings
        public IActionResult Profile()
        {
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            return View();
        }
    }
}

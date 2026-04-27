using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using EduTrack.Data;
using EduTrack.Models;
using System.Security.Cryptography;
using System.Text;

namespace EduTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            // Hash the password
            string hashedPassword = HashPassword(password);

            // Find user
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user != null && user.IsActive)
            {
                // Update last login
                user.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();

                // Store in session
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role?.RoleName ?? "User");

                // Log the login
                var log = new SystemLog
                {
                    UserID = user.UserID,
                    Action = "Login",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Details = $"Successful login by {username}"
                };
                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Redirect based on role (optional - can all go to same dashboard)
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            // Validation
            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                ViewBag.Error = "Username must be at least 3 characters";
                return View();
            }

            if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
            {
                ViewBag.Error = "Please enter a valid email address";
                return View();
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            // Check if username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "Username already exists. Please choose another.";
                return View();
            }

            // Check if email already exists
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingEmail != null)
            {
                ViewBag.Error = "Email already registered. Please use another email.";
                return View();
            }

            // Create new user (default role: Student = 8)
            var hashedPassword = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Email = email,
                RoleID = 8, // Student role
                IsActive = true,
                LastLogin = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Registration successful! You can now login with your credentials.";
            return View();
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var log = new SystemLog
                {
                    UserID = userId.Value,
                    Action = "Logout",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Details = "User logged out"
                };
                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
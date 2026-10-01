using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using _091.Models;
using _091.Extensions;

namespace _091.Controllers
{
    public class CourseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private const string CourseCacheKey = "CourseListCacheKey";

        private readonly List<Course> _allCourses = new()
        {
            new Course { CourseId = 1, CourseName = "Full-Stack Web Development", Duration = "3 Months", Fees = 450.00m, DiscountPercentage = 15m },
            new Course { CourseId = 2, CourseName = "Advanced ASP.NET Core & MVC", Duration = "2 Months", Fees = 350.00m, DiscountPercentage = 10m },
            new Course { CourseId = 3, CourseName = "UI/UX Claymorphism Masterclass", Duration = "4 Weeks", Fees = 200.00m, DiscountPercentage = 20m },
            new Course { CourseId = 4, CourseName = "Cloud Computing with Azure", Duration = "2.5 Months", Fees = 500.00m, DiscountPercentage = 12m }
        };

        public CourseController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        // 1 & 4. Index with 5-minute Memory Caching
        public IActionResult Index(string studentName)
        {
            if (!string.IsNullOrEmpty(studentName))
            {
                // Cookie State Management
                Response.Cookies.Append("StudentName", studentName, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                ViewBag.StudentName = studentName;
            }
            else if (Request.Cookies.TryGetValue("StudentName", out var cookieName))
            {
                ViewBag.StudentName = cookieName;
            }

            // Memory Cache for Course List (5 minutes)
            if (!_memoryCache.TryGetValue(CourseCacheKey, out List<Course>? courses))
            {
                courses = _allCourses;
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _memoryCache.Set(CourseCacheKey, courses, cacheEntryOptions);
            }

            return View(courses);
        }

        // 3 & 4. Details Page with Response Caching & Query String (CourseId)
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Details(int courseId)
        {
            var course = _allCourses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return NotFound();

            // Session State Management for selected course
            HttpContext.Session.SetString("SelectedCourse", course.CourseName);

            return View(course);
        }

        // 2. Asynchronous Web Request
        public async Task<IActionResult> Users()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://jsonplaceholder.typicode.com/users");
            
            List<UserViewModel> users = new();
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                users = JsonSerializer.Deserialize<List<UserViewModel>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
            }

            return View(users);
        }

        // 3. Enrollment GET (using Session & Cookie)
        [HttpGet]
        public IActionResult Enroll(int courseId)
        {
            var course = _allCourses.FirstOrDefault(c => c.CourseId == courseId);
            string studentName = Request.Cookies["StudentName"] ?? "Guest Student";

            var model = new EnrollmentViewModel
            {
                CourseId = courseId,
                CourseName = course?.CourseName ?? HttpContext.Session.GetString("SelectedCourse") ?? "General Course",
                StudentName = studentName
            };

            return View(model);
        }

        // 3. Enrollment POST (Hidden Field Submission)
        [HttpPost]
        public IActionResult Enroll(EnrollmentViewModel model)
        {
            // Session State retrieval test
            TempData["SuccessMessage"] = $"Successfully enrolled {model.StudentName} in {model.CourseName} (Course ID: {model.CourseId})!";
            return RedirectToAction("Index");
        }
    }
}
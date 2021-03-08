using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ESA_AuthenicationB2C.Models;
using ESA_AuthenicationB2C.Services;

namespace ESA_AuthenicationB2C.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGraphApiClient _graphApiClient;

        public HomeController(ILogger<HomeController> logger, IGraphApiClient graphserviceClient)
        {
            _logger = logger;
            _graphApiClient = graphserviceClient;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Claims()
        {
            // var result = await _graphApiClient.GetUser("test1");

            // await _graphApiClient.UpdateSecurityQuestions(result[0].Id);

            // return View(result);

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

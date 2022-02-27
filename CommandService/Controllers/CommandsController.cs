using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

public class Commands : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoShopWeb.Models;
using MotoShopWeb.Services;
namespace MotoShopWeb.Controllers;
[AllowAnonymous]
public class HomeController(IStoreService store) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await store.BrowseAsync(null, 1, 6, ct));
    public async Task<IActionResult> Vehicles(string? q, int page = 1, string? brand=null, decimal? budget=null, string sort="new", CancellationToken ct = default)
        => View(await store.BrowseAsync(q, page, 12, ct, brand, budget, sort));
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var bike = await store.DetailAsync(id, ct);
        if (bike is not null) return View(bike);
        Response.StatusCode = 404;
        return View("NotFound");
    }
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

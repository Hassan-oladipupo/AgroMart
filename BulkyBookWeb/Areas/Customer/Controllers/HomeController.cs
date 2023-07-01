using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Model;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;
[Area("Customer")]
//Controller for Adding ShopppingCart items(Product)
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
    {
            _logger = logger;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }
    public IActionResult Index()
    {
     //To get all the product as model in this controller and include properties of category and coverType on index page
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        return View(productList);
    }
    [HttpGet]
    public async Task<IActionResult> Index(string SearchString)
    {
        ViewData["ProductFilter"] = SearchString;
        var ProductSearch = from u in _dbContext.Products
                            select u;
        if (!string.IsNullOrEmpty(SearchString))
        {
            ProductSearch = ProductSearch.Where(b => b.Title.Contains(SearchString));
        }
        return View(await ProductSearch.AsNoTracking().ToListAsync());
    }

    //Get (Get shoppingCart items to add on details page)
    public IActionResult Details(int productId)
    {
        ShoppingCart cartObj = new()
        {
            Count = 1,
            ProductId = productId,
            Product = _unitOfWork.Product.GetFirstOrDefault(u=>u.Id == productId, includeProperties: "Category,CoverType")
        };
        return View(cartObj);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    //only authorize user(sign in user can access this postActionMethod)
    [Authorize]
    //Add the shoppingCart items together with Application UserId 
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;

        //Adding shoppingCart to the existing shoppingCart the use added earlier
        ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
            u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

        if(cartFromDb == null)
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();
            //Adding session to the shoppingCart
            HttpContext.Session.SetInt32(SD.SessionCart,
               _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
            _unitOfWork.Save();
        }
        
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
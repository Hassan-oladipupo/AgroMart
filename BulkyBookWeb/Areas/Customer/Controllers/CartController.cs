using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Model;
using BulkyBook.Model.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;

[Area("Customer")]
//only signed in user can access the Package
[Authorize]
//Controller for retreving shoppingCart  item that has already been added
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    // To bind ShoppingCartVM and ShoppingCartVM when we post form(so we dont have to write in post method)
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }
    public int OrderTotal { get; set; }
    private readonly IEmailSender _emailSender;

    public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }
    //Add the shoppingCart items together with Application UserId in Index 
    public IActionResult Index()
    {
        var claimIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartVM()
        {
            ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),

            // Creating new instance for OrderHeader since we are using it insdie the ShoppingCartVM
            OrderHeader = new()
        };
        foreach(var cart in ShoppingCartVM.ListCart)
        {
            //get the price cart based on count
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            //taking the total cart price
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        

        return View(ShoppingCartVM);

    }
    //Get method for summary page
    //Add the shoppingCart items together with Application UserId in Summary
    public IActionResult Summary()
    {
        var claimIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartVM()
        {
            ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
            // Creating new instance for OrderHeader since we are using it insdie the ShoppingCartVM
            OrderHeader = new()

        };
        //Getting the ApplicationUser From OrderHeader
        ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
               u => u.Id == claim.Value);

        //Assigning the ApplicationUser  to OrderHeader Properties


        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


        foreach (var cart in ShoppingCartVM.ListCart)
        {
            //get the price cart based on count
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            //taking the total cart price
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVM);

    }

    //Post method for summary page
    //Add the shoppingCart items together with Application UserId in Summary
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public IActionResult SummaryPOST()
    {
        var claimIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);


        ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

        // Populating the Details need  for Order Header
        //ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        //ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

        foreach (var cart in ShoppingCartVM.ListCart)
        {
            //get the price cart based on count
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            //taking the total cart price
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }



        //Check if the it's companyUser and if it's companyUser implment the companyUser flow
        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        }
        else
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        }




        //Adding the pouplated details need  for Order Header to the DB

        _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
        _unitOfWork.Save();

        //  Populating the Details need  for Order Details
        foreach (var cart in ShoppingCartVM.ListCart)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count,
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        //Check if the it's companyUser and if it's companyUser implment the cstripe setting
        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ///Stripe API setting

            var domain = "https://localhost:44359/";
            var options = new SessionCreateOptions
            {
                //Add new session for card payment and LineItems
                PaymentMethodTypes = new List<string>
            {
                "card",
            },
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };
            foreach (var item in ShoppingCartVM.ListCart)
            {

                //LineItems option represent all of the item we have in our shoppingcart
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        else
        {
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        //Removing ShoppingCartVM.ListCart
        //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
        //_unitOfWork.Save();
        //return RedirectToAction("Index", "Home");
    }
    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
        if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            //check the stripe payment status
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentID(id, orderHeader.SessionId, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }
        //Sending email after order has been made
        _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created</p>");
        //Removing ShoppingCartVM.ListCart
        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId
        == orderHeader.ApplicationUserId).ToList();
        //To clear the cart after placing order
        HttpContext.Session.Clear();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();
        return View(id);
    }



    //Action method for increment when we click + button
    public IActionResult Plus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    //Action method for decrement when we click - button
    public IActionResult Minus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        if(cart.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);

            //Remove session
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
        }
        else
        {
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
        }
        
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }
  

    //Action method for delete when we click delete button
    public IActionResult Remove(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.Remove(cart);


        //Remove Session
        var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
        HttpContext.Session.SetInt32(SD.SessionCart, count);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    //method for getting the cart price based on count
    private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
    {
        if(quantity <= 50)
        {
            return price;
        }
        else
        {
            if(quantity <= 100)
            {
                return price50;
            }
            return price100;
        }
    }
}

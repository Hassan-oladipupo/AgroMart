using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Model;
using BulkyBook.Model.ViewModels;
using BulkyBook.Model.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using IServiceScope = Microsoft.Extensions.DependencyInjection.IServiceScope;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]

// Controller with one method for create and update(Upsert) and API for delete
//Use for CRUD Operation
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

       
    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        return View();
    }
    //Code for Create (create) in this controller
    public IActionResult Upsert(int? id)
    {

        Company company = new();
       
                

        if (id == null || id == 0)
        {
            
            return View(company);
        }

        else
        {
           
            company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            return View(company); 
        }

    }
    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    //Code for Edit(Update) in this controller
    public IActionResult Upsert(Company obj, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            if (obj.Id == 0)
            {
                _unitOfWork.Company.Add(obj);
                TempData["Success"] = "Comapny created successfully";

            }

            else
            {
                _unitOfWork.Company.Update(obj);
                TempData["Success"] = "Comapny Update successfully";

            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        return View(obj);

    }

   
    #region API CALLs
    [HttpGet]
    //To get what you want to delete
    public IActionResult GetAll()
    {
        var CompanyList = _unitOfWork.Company.GetAll();
        return Json(new {data=CompanyList});
    }
    //Post 

    [HttpDelete]
    //To delete it
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success= false, message = "Error while deleteing"});
        }
       
        _unitOfWork.Company.Remove(obj);
        _unitOfWork.Save();
         return Json(new { success = true, message = "Delete successful" });
    }
    #endregion

}


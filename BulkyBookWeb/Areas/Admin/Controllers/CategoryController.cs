using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
//Controller with no API
//use to perform CRUD operation
[Authorize(Roles = SD.Role_Admin)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
          
    }

    public IActionResult Index()
    {
        IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();
        return View(objCategoryList);
    }

    //Get (Get what to create)
    public IActionResult Create()
    {
        return View();
    }
    //POST (Create it)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The Display Order exactly match the Name");
        }
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Add(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Category Created successfully";
            return RedirectToAction("Index");
        }
        return View(obj);
    }
    //Get (Get what to Edit(Update))
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        //var categoryFromDb = _db.Categories.Find(id);
        var CategoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
        //var CategoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
        if (CategoryFromDbFirst == null)
        {
            return NotFound();
        }
        return View(CategoryFromDbFirst);
    }


    //POST (Update it)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The Display Order exactly match the Name");
        }
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Update(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Category edited successfully";
            return RedirectToAction("Index");
        }
        return View(obj);

    }

    //Get (Get what to delete)

    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        //var categoryFromDb = _db.Categories.Find(id);
        var categoryFromFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
        //Category = _db.Category.SingleOrDefault(u => u.Id == id);
        if (categoryFromFirst == null)
        {
            return NotFound();
        }
        return View(categoryFromFirst);
    }


    //POST (Delete it)
   
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return NotFound();
        }
        _unitOfWork.Category.Remove(obj);
        _unitOfWork.Save();
        TempData["Success"] = "Category Deleted successfully";
        return RedirectToAction("Index");
    }


}


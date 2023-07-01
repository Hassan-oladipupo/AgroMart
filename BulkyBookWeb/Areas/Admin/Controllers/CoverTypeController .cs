using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Model;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
//Controller with no API
//use to perform CRUD operation
[Authorize(Roles = SD.Role_Admin)]
public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<CoverType> objCoverTypeList = _unitOfWork.CoverType.GetAll();
        return View(objCoverTypeList);
    }

    //Get (Get what to create)
    public IActionResult Create()
    {
        return View();
    }
    //POST (Create it)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CoverType obj)
    {
        
        if (ModelState.IsValid)
        {
            _unitOfWork.CoverType.Add(obj);
            _unitOfWork.Save();
            TempData["Success"] = "CoverType Created successfully";
            return RedirectToAction("Index");
        }
        return View(obj);
    }
    //Get what to Edit(update)
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        //var categoryFromDb = _db.Categories.Find(id);
        var coverTypeFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
        //var CategoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
        if (coverTypeFromDbFirst == null)
        {
            return NotFound();
        }
        return View(coverTypeFromDbFirst);
    }


    //POST (Updating it)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CoverType obj)
    {
        
        if (ModelState.IsValid)
        {
            _unitOfWork.CoverType.Update(obj);
            _unitOfWork.Save();
            TempData["Success"] = "CoverType edited successfully";
            return RedirectToAction("Index");
        }
        return View(obj);

    }

    //Get (Get what to Delete)

    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        //var categoryFromDb = _db.Categories.Find(id);
        var coverTypeFromFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
        //Category = _db.Category.SingleOrDefault(u => u.Id == id);
        if (coverTypeFromFirst == null)
        {
            return NotFound();
        }
        return View(coverTypeFromFirst);
    }


    //POST (Delete it)

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return NotFound();
        }
        _unitOfWork.CoverType.Remove(obj);
        _unitOfWork.Save();
        TempData["Success"] = "CoverType Deleted successfully";
        return RedirectToAction("Index");
    }


}


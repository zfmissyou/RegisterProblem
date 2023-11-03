using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BulkBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        //private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        //Get
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            //IEnumerable<Category> objCategoryList = _db.Categories;  
            //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            //{
            //    Text = u.Name,
            //    Value = u.Id.ToString()
            //});
            //ViewBag.CategoryList = CategoryList;
            return View(objProductList);
        }

		//GET
		//     public IActionResult Create()
		//     {
		//         //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
		//         //{
		//         //	Text = u.Name,
		//         //	Value = u.Id.ToString()
		//         //});
		//         //ViewBag.CategoryList = CategoryList;
		//         //ViewData["CategoryList"] = CategoryList; 或者底下的也行 
		//         //ViewData[nameof(CategoryList)] = CategoryList;
		//         ProductVM productVM = new()
		//         {
		//             CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
		//             {
		//                 Text = u.Name,
		//                 Value = u.Id.ToString()
		//             }),
		//             Product = new Product()
		//         };
		//return View(productVM);
		//     }

		public IActionResult Upsert(int? id)
		{

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u =>u.Id == id);
                return View(productVM);
            } 
			
		}

		//POST
		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM ProductVM, IFormFile? file)//以前是Create
		{
            if (ProductVM.Product.Title == ProductVM.Product.Description.ToString())
            {
                ModelState.AddModelError("CustomError", "The Displayorder cannot match Name.");
            }
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    if (!string.IsNullOrEmpty(ProductVM.Product.ImageUrl))
                    { 
                        var oldImagePath = Path.Combine(wwwRootPath, ProductVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        { 
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    { 
                        file.CopyTo(fileStream);
                    }
                    ProductVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (ProductVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(ProductVM.Product);
                }
                else
                {
					_unitOfWork.Product.Update(ProductVM.Product);
				}
                
                _unitOfWork.Save();
                TempData["success"] = "Product create successfully";
                return RedirectToAction("Index");

            }
            else
            {

                ProductVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(ProductVM);
             }

        }

        //Get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
            //var categoryFromDbFirst = _db.Categories.FirstOrDefault(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }

            return View(productFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product obj)
        {
            if (obj.Title == obj.Description.ToString())
            {
                ModelState.AddModelError("CustomError", "The Displayorder cannot match Name.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Product edit successfully";
                return RedirectToAction("Index");

            }
            return View();

        }

        //Get
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
        //    //var categoryFromDbFirst = _db.Categories.FirstOrDefault(u => u.Id == id);
        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(productFromDb);
        //}

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {

            var obj = _unitOfWork.Product.Get(u => u.Id == id); ;
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product delete successfully";
            return RedirectToAction("Index");

        }

		#region call API
		[HttpGet]
        public IActionResult GetAll() 
        {
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList }); 
		}
       [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u=>u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}

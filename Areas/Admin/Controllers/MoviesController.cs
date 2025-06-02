using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebCinema.Models;

namespace WebCinema.Areas.Admin.Controllers
{
    public class MoviesController : Controller
    {
        private WebCinemaEntities db = new WebCinemaEntities();

        // GET: Admin/Movies
        public ActionResult Index(string search)
        {
            var movies = db.Movies.AsQueryable();
            if (!string.IsNullOrEmpty(search)) 
            {
                movies=movies.Where(m=>m.Title.Contains(search));
            }
            return View(movies.ToList());
        }

        // GET: Admin/Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Admin/Movies/Create
        public ActionResult Create()
        {
            var movie = new Movie
            {
                ReleaseDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1)
            };

            return View(movie);
        }

        // POST: Admin/Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Movie movie, HttpPostedFileBase fileUpload)
        {
            // Kiểm tra trùng tên phim
            if (db.Movies.Any(m => m.Title == movie.Title))
            {
                ModelState.AddModelError("Title", "Tên phim đã tồn tại");
            }

            // Kiểm tra file upload
            if (fileUpload == null || fileUpload.ContentLength == 0)
            {
                ModelState.AddModelError("fileUpload", "Vui lòng chọn hình ảnh");
            }
            else
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fileUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("fileUpload", "Chỉ chấp nhận .jpg, .jpeg, .png, .gif");
                }

               
            }

            // Kiểm tra ngày hợp lệ
            if (movie.ReleaseDate > movie.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày khởi chiếu");
            }

            if (ModelState.IsValid)
            {
                    // Lưu file ảnh
                    string uploadFolder = Server.MapPath("~/Content/Images/Movies");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string newFileName = "phim_" + Guid.NewGuid() + Path.GetExtension(fileUpload.FileName);
                    string filePath = Path.Combine(uploadFolder, newFileName);
                    fileUpload.SaveAs(filePath);

                    movie.ImageURL = newFileName;

                    db.Movies.Add(movie);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm phim thành công!";
                    return RedirectToAction("Index");
              
            }

            return View(movie);
        }


        // GET: Admin/Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Admin/Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Movie movie, HttpPostedFileBase fileUpload)
        {
            // Kiểm tra trùng tên
            if (db.Movies.Any(m => m.Title == movie.Title && m.MovieID != movie.MovieID))
            {
                ModelState.AddModelError("Title", "Tên phim đã tồn tại trong hệ thống");
            }

            // Kiểm tra ngày kết thúc
            if (movie.ReleaseDate > movie.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày khởi chiếu");
            }

            // Nếu có upload file mới
            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fileUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("fileUpload", "Chỉ chấp nhận .jpg, .jpeg, .png, .gif");
                }
            }

            if (ModelState.IsValid)
            {
                // Xử lý file nếu có
                if (fileUpload != null && fileUpload.ContentLength > 0)
                {
                    string uploadFolder = Server.MapPath("~/Content/Images/Movies");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    string newFileName = "phim_" + Guid.NewGuid() + Path.GetExtension(fileUpload.FileName);
                    string filePath = Path.Combine(uploadFolder, newFileName);
                    fileUpload.SaveAs(filePath);
                    movie.ImageURL = newFileName;
                }

                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật phim thành công!";
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);

            // Kiểm tra suất chiếu
            bool hasScreenings = db.Screenings.Any(s => s.MovieID == id && s.ShowDate >= DateTime.Today);

            if (hasScreenings)
            {
                return Json(new { success = false, message = "Không thể xóa phim vì có suất chiếu đang chiếu phim này" });
            }

            db.Movies.Remove(movie);
            db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult ToggleActive(int id)
        {
            var movie = db.Movies.Find(id);
            if (movie == null)
                return HttpNotFound();

            movie.IsActive = !movie.IsActive;
            db.SaveChanges();

            return new HttpStatusCodeResult(200);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebCinema.Models;
using WebCinema.Helper;
using System.Security.Cryptography;
using System.Text;

namespace WebCinema.Areas.Admin.Controllers
{

    public class UsersController : Controller
    {
        private WebCinemaEntities db = new WebCinemaEntities();

        // GET: Admin/Users
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Customer);
            return View(users.ToList());
        }

        // GET: Admin/Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            // Sử dụng UserRoles để tạo dropdown list
            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = UserRoles.Customer, Text = "Khách hàng" },
                new { Value = UserRoles.Staff, Text = "Nhân viên" },
                new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                new { Value = UserRoles.SuperAdmin, Text = "CEO" }
            }, "Value", "Text");

            ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName");
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,Username,Password,Role,IsConfirmed")] User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại chưa
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    ViewBag.Roles = new SelectList(new[]
                    {
                        new { Value = UserRoles.Customer, Text = "Khách hàng" },
                        new { Value = UserRoles.Staff, Text = "Nhân viên" },
                        new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                        new { Value = UserRoles.SuperAdmin, Text = "Quản trị viên cấp cao" }
                    }, "Value", "Text", user.Role);
                    ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName", user.UserID);
                    return View(user);
                }

                // Mã hóa mật khẩu trước khi lưu
                user.Password = HashPassword(user.Password);
                user.IsConfirmed = true; // Tài khoản được admin tạo thì mặc định đã xác thực
                user.VerificationCode = null;
                user.VerificationCodeGeneratedAt = null;

                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = UserRoles.Customer, Text = "Khách hàng" },
                new { Value = UserRoles.Staff, Text = "Nhân viên" },
                new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                new { Value = UserRoles.SuperAdmin, Text = "Quản trị viên cấp cao" }
            }, "Value", "Text", user.Role);
            ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName", user.UserID);
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = UserRoles.Customer, Text = "Khách hàng" },
                new { Value = UserRoles.Staff, Text = "Nhân viên" },
                new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                new { Value = UserRoles.SuperAdmin, Text = "Quản trị viên cấp cao" }
            }, "Value", "Text", user.Role);
            ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName", user.UserID);
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Username,Password,Role,IsConfirmed,VerificationCode,VerificationCodeGeneratedAt")] User user)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin user hiện tại từ database
                var existingUser = db.Users.Find(user.UserID);

                if (existingUser == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra username đã tồn tại chưa (trừ user hiện tại)
                if (db.Users.Any(u => u.Username == user.Username && u.UserID != user.UserID))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    ViewBag.Roles = new SelectList(new[]
                    {
                        new { Value = UserRoles.Customer, Text = "Khách hàng" },
                        new { Value = UserRoles.Staff, Text = "Nhân viên" },
                        new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                        new { Value = UserRoles.SuperAdmin, Text = "Quản trị viên cấp cao" }
                    }, "Value", "Text", user.Role);
                    ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName", user.UserID);
                    return View(user);
                }

                // Cập nhật thông tin
                existingUser.Username = user.Username;
                existingUser.Role = user.Role;
                existingUser.IsConfirmed = user.IsConfirmed;

                // Chỉ cập nhật mật khẩu nếu có thay đổi
                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    existingUser.Password = HashPassword(user.Password);
                }

                db.Entry(existingUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = UserRoles.Customer, Text = "Khách hàng" },
                new { Value = UserRoles.Staff, Text = "Nhân viên" },
                new { Value = UserRoles.CinemaManager, Text = "Quản lý rạp" },
                new { Value = UserRoles.SuperAdmin, Text = "CEO" }
            }, "Value", "Text", user.Role);
            ViewBag.UserID = new SelectList(db.Customers, "CustomerID", "FullName", user.UserID);
            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Hàm mã hóa mật khẩu sử dụng SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteAjax(int id)
        {
            try
            {
                User user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản để xóa." });
                }

                db.Users.Remove(user);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa tài khoản: " + ex.Message });
            }
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using WebCinema.Helper;
using WebCinema.Helpers;
using WebCinema.Models;
using WebCinema.ViewModel;
using WebCinema.ViewModel.Booking;
using WebCinema.ViewModel.Payment;

namespace WebCinema.Controllers
{
    public class PaymentController : Controller
    {
        WebCinemaEntities db = new WebCinemaEntities();
        private readonly PointService _pointService;

        // Existing config...
        private readonly string _vnpUrl = ConfigurationManager.AppSettings["vnp_Url"];
        private readonly string _vnpTmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
        private readonly string _vnpHashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

        public PaymentController()
        {
            _pointService = new PointService(db);
        }

        // Cập nhật method Info
        public ActionResult Info(string sessionId)
        {
            var session = Session[$"BookingSession_{sessionId}"] as BookingSessionViewModel;
            if (session == null) return RedirectToAction("Index", "Home");

            // Existing code for foods and combos...
            var foods = db.Foods.Select(f => new FoodComboVM
            {
                ID = f.FoodID,
                Name = f.Name,
                Description = f.Category,
                Price = f.Price,
                ImageUrl = f.ImageURL,
                Type = "food"
            }).ToList();

            var combos = db.Comboes.Select(c => new FoodComboVM
            {
                ID = c.ComboID,
                Name = c.Name,
                Description = c.Description,
                Price = c.SalePrice,
                ImageUrl = c.ImageURL,
                Type = "combo"
            }).ToList();

            var userId = GetCurrentUserId();
            var user = db.Users.Include("Customer.MemberLevel").FirstOrDefault(u => u.UserID == userId);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Lấy thông tin điểm của khách hàng
            var customerPointInfo = _pointService.GetCustomerPointInfo(user.Customer.CustomerID);

            // Tính điểm ước tính sẽ nhận được
            int estimatedPoints = _pointService.CalculatePointsFromAmount(session.TotalPrice);

            var paymentInfo = new PaymentInfoViewModel
            {
                // Existing properties...
                BookingSessionId = sessionId,
                UserName = user.Customer.FullName ?? user.Username,
                Email = user.Customer.Email,
                Phone = user.Customer.Phone,
                MovieTitle = session.MovieTitle,
                CinemaName = session.CinemaName,
                RoomName = session.RoomName,
                ShowDate = session.BookingStartTime.Date,
                StartTime = session.BookingStartTime.TimeOfDay,
                SelectedSeats = session.SelectedSeats ?? new List<string>(),
                SelectedFoods = session.SelectedFoods,
                SelectedCombos = session.SelectedCombos,
                TotalPrice = session.TotalPrice,
                AvailableItems = foods.Concat(combos).ToList(),

                // New point-related properties
                CustomerPointInfo = customerPointInfo,
                EstimatedEarnedPoints = estimatedPoints
            };

            return View("PaymentInfo", paymentInfo);
        }

        // Thêm method mới để áp dụng điểm
        [HttpPost]
        public JsonResult ApplyPoints(string sessionId, int pointsToUse)
        {
            try
            {
                var session = Session[$"BookingSession_{sessionId}"] as BookingSessionViewModel;
                if (session == null)
                    return Json(new { success = false, message = "Session không tồn tại" });

                var userId = GetCurrentUserId();
                var user = db.Users.Include("Customer").FirstOrDefault(u => u.UserID == userId);
                if (user?.Customer == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });

                var customer = user.Customer;
                var availablePoints = customer.Points ?? 0;

                // Validation
                if (pointsToUse < 0)
                    return Json(new { success = false, message = "Số điểm không hợp lệ" });

                if (pointsToUse > availablePoints)
                    return Json(new { success = false, message = "Không đủ điểm" });

                decimal pointDiscount = _pointService.CalculateDiscountFromPoints(pointsToUse);
                if (pointDiscount > session.TotalPrice)
                {
                    // Tự động điều chỉnh số điểm sử dụng để không vượt quá tổng tiền
                    pointsToUse = (int)(session.TotalPrice / 1000);
                    pointDiscount = pointsToUse * 1000;
                }

                // Lưu vào session
                session.PointsUsed = pointsToUse;
                session.PointDiscount = pointDiscount;
                Session[$"BookingSession_{sessionId}"] = session;

                return Json(new
                {
                    success = true,
                    pointsUsed = pointsToUse,
                    pointDiscount = pointDiscount,
                    finalPrice = session.TotalPrice - pointDiscount,
                    formattedPointDiscount = pointDiscount.ToString("N0"),
                    formattedFinalPrice = (session.TotalPrice - pointDiscount).ToString("N0")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public ActionResult CreatePaymentUrl(string sessionId)
        {
            var session = Session[$"BookingSession_{sessionId}"] as BookingSessionViewModel;
            if (session == null) return RedirectToAction("Index", "Home");

            var vnpay = new VnPayLibrary();
            string returnUrl = Url.Action("PaymentConfirm", "Payment", null, Request.Url.Scheme);
            string ipAddress = Request.UserHostAddress ?? "127.0.0.1";

            // Tạo mã giao dịch ngẫu nhiên
            string orderId = Guid.NewGuid().ToString();

            // 🌟 Lưu lại session với key mới
            Session[$"BookingSession_{orderId}"] = session;

            // Đảm bảo số tiền là integer (VNĐ * 100)
            int amount = (int)(session.FinalPrice * 100);
            if (amount <= 0)
                return View("PaymentFail", (object)"Số tiền thanh toán không hợp lệ");

            // Thêm tham số thanh toán
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán vé xem phim: {session.MovieTitle}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            vnpay.AddRequestData("vnp_TxnRef", orderId); // dùng orderId làm TxnRef
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(_vnpUrl, _vnpHashSecret);
            if (string.IsNullOrEmpty(paymentUrl))
                return View("PaymentFail", (object)"Lỗi khi tạo URL thanh toán");

            return Redirect(paymentUrl);
        }

        // Cập nhật PaymentConfirm để xử lý tích điểm
        public ActionResult PaymentConfirm()
        {
            string sessionId = Request.QueryString["vnp_TxnRef"];
            if (string.IsNullOrEmpty(sessionId))
                return View("PaymentFail", (object)"Không tìm thấy mã giao dịch");

            var booking = Session[$"BookingSession_{sessionId}"] as BookingSessionViewModel;
            if (booking == null)
                return View("PaymentFail", (object)"Phiên đặt vé không tồn tại hoặc đã hết hạn");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var user = db.Users.Include("Customer").FirstOrDefault(u => u.UserID == userId);
                    if (user?.Customer == null)
                        return View("PaymentFail", (object)"Không tìm thấy thông tin khách hàng");

                    var customer = user.Customer;
                    decimal finalAmount = booking.TotalPrice - (booking.PointDiscount ?? 0);

                    // Tạo tickets
                    foreach (var seat in booking.SelectedSeats)
                    {
                        var ticket = new Ticket
                        {
                            ScreeningID = booking.ScreeningId,
                            SeatNumber = seat,
                            BookingTime = DateTime.Now,
                            TotalPrice = finalAmount / booking.SelectedSeats.Count,
                            PaymentStatus = 1,
                            UserID = userId
                        };
                        db.Tickets.Add(ticket);
                    }

                    db.SaveChanges();

                    // Xử lý điểm
                    if (booking.PointsUsed > 0)
                    {
                        // Trừ điểm đã sử dụng
                        _pointService.RedeemPoints(
                            customer.CustomerID,
                            booking.PointsUsed ?? 0,
                            $"Sử dụng điểm thanh toán vé phim {booking.MovieTitle}",
                            "BOOKING"
                        );
                    }

                    // Tích điểm từ số tiền đã thanh toán (chỉ tích từ số tiền thực tế, không tính phần giảm giá)
                    if (finalAmount > 0)
                    {
                        _pointService.EarnPoints(
                            customer.CustomerID,
                            finalAmount,
                            $"Tích điểm từ đặt vé phim {booking.MovieTitle}",
                            "BOOKING"
                        );
                    }

                    // Gửi email xác nhận
                    var email = ((ClaimsIdentity)User.Identity)?.FindFirst(ClaimTypes.Email)?.Value ?? customer.Email;
                    string subject = "Xác nhận đặt vé thành công - WebCinema";

                    // Cập nhật thông tin customer mới nhất sau khi xử lý điểm
                    db.Entry(customer).Reload();

                    string body = GenerateBookingEmailBody(booking, customer, finalAmount);
                    EmailHelper.SendBookingSuccessEmail(email, subject, body);

                    transaction.Commit();
                    return View("PaymentSuccess", (object)sessionId);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return View("PaymentFail", (object)("Có lỗi xảy ra: " + ex.Message));
                }
            }
        }


        private string GenerateBookingEmailBody(BookingSessionViewModel booking, Customer customer, decimal finalAmount)
        {
            var pointsUsed = booking.PointsUsed ?? 0;
            var pointDiscount = booking.PointDiscount ?? 0;
            var earnedPoints = _pointService.CalculatePointsFromAmount(finalAmount);

            string body = $@"
            <h3>Xin chào {customer.FullName ?? "quý khách"},</h3>
            <p>Bạn đã đặt vé thành công cho phim <b>{booking.MovieTitle}</b>.</p>
            <ul>
                <li><b>Rạp:</b> {booking.CinemaName}</li>
                <li><b>Phòng chiếu:</b> {booking.RoomName}</li>
                <li><b>Thời gian:</b> {booking.BookingStartTime:dd/MM/yyyy HH:mm}</li>
                <li><b>Ghế:</b> {string.Join(", ", booking.SelectedSeats)}</li>
                <li><b>Giá vé:</b> {booking.TotalPrice:N0} VNĐ</li>";

            if (pointsUsed > 0)
            {
                body += $"<li><b>Điểm sử dụng:</b> {pointsUsed} điểm (-{pointDiscount:N0} VNĐ)</li>";
            }

            body += $@"
                <li><b>Số tiền thanh toán:</b> {finalAmount:N0} VNĐ</li>
            </ul>";

            if (earnedPoints > 0)
            {
                body += $"<p><b>🎉 Bạn đã tích được {earnedPoints} điểm từ giao dịch này!</b></p>";
                
            }

            if (!string.IsNullOrEmpty(customer.MemberLevel?.Name))
            {
                body += $"<p>Thẻ thành viên: <b>{customer.MemberLevel.Name}</b></p>";
            }

            // Thêm thông tin combo/food nếu có
            if (booking.SelectedCombos?.Any() == true)
            {
                body += "<p><b>Combo:</b><ul>";
                foreach (var combo in booking.SelectedCombos)
                    body += $"<li>{combo.Name} - {combo.Quantity} x {combo.SalePrice:N0}đ</li>";
                body += "</ul></p>";
            }

            if (booking.SelectedFoods?.Any() == true)
            {
                body += "<p><b>Đồ ăn:</b><ul>";
                foreach (var food in booking.SelectedFoods)
                    body += $"<li>{food.Name} - {food.Quantity} x {food.Price:N0}đ</li>";
                body += "</ul></p>";
            }

            body += "<p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>";

            return body;
        }

        // Existing methods...
        private int GetCurrentUserId()
        {
            if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int sessionUserId))
            {
                return sessionUserId;
            }
            throw new Exception("Không thể xác định UserID");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
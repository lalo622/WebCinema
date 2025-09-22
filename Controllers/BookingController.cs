using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCinema.Models;
using WebCinema.ViewModel;
using WebCinema.ViewModel.Booking;
using WebCinema.ViewModel.Seat;
using WebCinema.ViewModel.Shared;

namespace WebCinema.Controllers
{
    public class BookingController : Controller
    {
        private WebCinemaEntities db = new WebCinemaEntities();

        public ActionResult Index()
        {
            return View();
        }
        // Hiển thị giao diện chọn ngày và suất chiếu cho phim
        public ActionResult SelectDateTime(int movieId, DateTime? selectedDate = null)
        {
            var movie = db.Movies.Find(movieId);
            if (movie == null)
                return HttpNotFound();

            var today = DateTime.Today;
            var availableDates = new List<DateOption>();
            for (int i = 0; i <= 30; i++)
            {
                var date = today.AddDays(i);
                availableDates.Add(new DateOption
                {
                    Date = date,
                    DisplayText = date.ToString("dd/MM"),
                    DayOfWeek = GetDayOfWeek(date.DayOfWeek),
                    IsToday = i == 0,
                    IsSelected = selectedDate.HasValue && selectedDate.Value.Date == date.Date
                });
            }

            var viewModel = new MovieBookingViewModel
            {
                MovieId = movie.MovieID,
                MovieTitle = movie.Title,
                MoviePoster = movie.ImageURL,
                MovieDuration = movie.Duration + " phút",
                SelectedDate = selectedDate ?? today,
                AvailableDates = availableDates
            };

            if (selectedDate.HasValue)
            {
                viewModel.CinemaScreenings = GetScreeningsByDateInternal(movieId, selectedDate.Value);
            }

            return View(viewModel);
        }
        //Trả danh sách suất chiếu theo ngày
        [HttpGet]
        public ActionResult GetScreeningsByDate(int movieId, DateTime selectedDate)
        {
            try
            {
                var screenings = GetScreeningsByDateInternal(movieId, selectedDate);
                return PartialView("_CinemaScreeningsPartial", screenings); 
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }
        //Khởi tạo session đặt vé, kiểm tra suất chiếu còn hợp lệ, sinh giao diện chọn ghế từ sơ đồ
        public ActionResult SelectSeat(int screeningId)
        {
            var screening = db.Screenings
                .Include("Movie")
                .Include("CinemaRoom")
                .Include("CinemaRoom.Cinema")
                .Include("CinemaRoom.SeatTemplate")
                .FirstOrDefault(s => s.ScreeningID == screeningId);

            if (screening == null || screening.ShowDate < DateTime.Today ||
                (screening.ShowDate == DateTime.Today && screening.StartTime < DateTime.Now.TimeOfDay))
            {
                TempData["Error"] = "Suất chiếu không hợp lệ hoặc đã qua giờ.";
                return RedirectToAction("Index", "Home");
            }

            var bookingSessionId = Guid.NewGuid().ToString();
            var bookingStart = DateTime.Now;
            var bookingEnd = bookingStart.AddMinutes(15);

            Session[$"BookingSession_{bookingSessionId}"] = new BookingSessionViewModel
            {
                SessionId = bookingSessionId,
                ScreeningId = screening.ScreeningID,
                BookingStartTime = bookingStart,
                ExpireTime = bookingEnd,
                SelectedSeats = new List<string>(),
                MovieTitle = screening.Movie.Title,
                CinemaName = screening.CinemaRoom.Cinema.Name,
                RoomName = screening.CinemaRoom.RoomName
            };

            var seatMap = GenerateSeatMap(screening.CinemaRoom, screening.ScreeningID, bookingSessionId);

            var viewModel = new SeatSelectionViewModel
            {
                ScreeningId = screening.ScreeningID,
                MovieId = screening.MovieID,
                MovieTitle = screening.Movie.Title,
                CinemaName = screening.CinemaRoom.Cinema.Name,
                RoomName = screening.CinemaRoom.RoomName,
                ShowDate = screening.ShowDate,
                StartTime = screening.StartTime,
                SeatMap = seatMap,
                BookingStartTime = bookingStart,
                BookingExpireTime = bookingEnd
            };

            ViewBag.BookingSessionId = bookingSessionId;
            return View(viewModel);
        }

        [HttpPost]
        public JsonResult ToggleSeat(string sessionId, int screeningId, string seatNumber)
        {
            var sessionKey = $"BookingSession_{sessionId}";
            var session = Session[sessionKey] as BookingSessionViewModel;
            if (session == null || session.ExpireTime < DateTime.Now)
            {
                return Json(new AjaxResponseViewModel { Success = false, Message = "Phiên đặt vé đã hết hạn." });
            }

            var isOccupied = db.Tickets.Any(t => t.ScreeningID == screeningId && t.SeatNumber == seatNumber);
            if (isOccupied)
            {
                return Json(new AjaxResponseViewModel { Success = false, Message = "Ghế đã được đặt." });
            }

            if (session.SelectedSeats.Contains(seatNumber))
                session.SelectedSeats.Remove(seatNumber);
            else 
                session.SelectedSeats.Add(seatNumber);
           

            session.TotalPrice = CalculateTotalPrice(screeningId,session.SelectedSeats);
            Session[sessionKey] = session;

            return Json(new AjaxResponseViewModel
            {
                Success = true,
                Data = new
                {
                    selectedSeats = session.SelectedSeats,
                    totalPrice = session.TotalPrice,
                    seatCount = session.SelectedSeats.Count
                }
            });
        }

        [HttpPost]
        public JsonResult CheckBookingTime(string sessionId)
        {
            var session = Session[$"BookingSession_{sessionId}"] as BookingSessionViewModel;
            if (session == null)
                return Json(new { expired = true });

            var remaining = (int)(session.ExpireTime - DateTime.Now).TotalSeconds;
            return Json(new { expired = remaining <= 0, remainingSeconds = Math.Max(0, remaining) });
        }

        #region Private Helpers

        private List<CinemaScreeningViewModel> GetScreeningsByDateInternal(int movieId, DateTime date)
        {
            var screenings = db.Screenings
                .Include("CinemaRoom.Cinema")
                .Where(s => s.MovieID == movieId && s.ShowDate == date)
                .ToList();

            return screenings
                .GroupBy(s => s.CinemaRoom.Cinema)
                .Select(g => new CinemaScreeningViewModel
                {
                    CinemaId = g.Key.CinemaID,
                    CinemaName = g.Key.Name,
                    CinemaAddress = g.Key.Address,
                    ScreeningTimes = g.Select(s => new ScreeningTimeViewModel
                    {
                        ScreeningId = s.ScreeningID,
                        StartTime = s.StartTime,
                        DisplayTime = s.StartTime.Hours.ToString("D2") + ":" + s.StartTime.Minutes.ToString("D2"),

                        RoomId = s.RoomID,
                        RoomName = s.CinemaRoom.RoomName,
                        AvailableSeats = GetAvailableSeatsCount(s.ScreeningID),
                        BasePrice = 0
                    }).ToList()
                }).ToList();
        }

        private SeatMapViewModel GenerateSeatMap(CinemaRoom room, int screeningId, string sessionId)
        {
            var template = room.SeatTemplate;

            var occupiedSeats = db.Tickets
                .Where(t => t.ScreeningID == screeningId)
                .Select(t => t.SeatNumber)
                .ToList()
                .ToHashSet();

            // Lấy tất cả SeatTypePrices trước để dùng lại
            var seatPrices = db.SeatTypePrices.ToDictionary(p => p.SeatTypeID, p => p.Price);

            // Truy vấn toàn bộ ghế của Template
            var seatEntities = db.Seats
                .Where(s => s.TemplateID == template.TemplateID)
                .ToList();

            // Lấy danh sách ghế đang chọn từ Session
            var selectedSeats = new List<string>();
            if (!string.IsNullOrEmpty(sessionId))
            {
                var sessionKey = $"BookingSession_{sessionId}";
                var session = Session[sessionKey] as BookingSessionViewModel;
                if (session != null)
                {
                    selectedSeats = session.SelectedSeats;
                }
            }

            var seats = seatEntities
                .Select(s => new SeatViewModel
                {
                    SeatNumber = s.SeatCode,
                    Row = s.RowNumber,
                    Column = s.ColumnNumber,
                    SeatType = (SeatType)s.SeatType,
                    DisplayName = s.SeatCode,
                    Price = seatPrices.ContainsKey(s.SeatType) ? seatPrices[s.SeatType] : 0,
                    IsBooked = occupiedSeats.Contains(s.SeatCode),
                    IsSelected = selectedSeats.Contains(s.SeatCode)
                })
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToList();

            return new SeatMapViewModel
            {
                TemplateId = template.TemplateID,
                TotalRows = template.Row ?? 0,
                SeatsPerRow = template.ColumnCount ?? 0,
                Seats = seats,
                RegularSeatPrice = seatPrices.ContainsKey(0) ? seatPrices[0] : 0,
                VipSeatPrice = seatPrices.ContainsKey(1) ? seatPrices[1] : 0,
                CoupleSeatPrice = seatPrices.ContainsKey(2) ? seatPrices[2] : 0
            };
        }


        private decimal GetSeatPriceByType(SeatType type)
        {
            var seatTypeId = (byte)type;
            var priceEntry = db.SeatTypePrices.FirstOrDefault(p => p.SeatTypeID == seatTypeId);
            return priceEntry != null ? priceEntry.Price : 0;
        }

        private decimal CalculateTotalPrice(int screeningId, List<string> seatCodes)
        {
            return seatCodes.Sum(code => GetSeatPrice(screeningId, code));
        }

        private decimal GetSeatPrice(int screeningId, string seatCode)
        {
            var seat = db.Seats.FirstOrDefault(s => s.SeatCode == seatCode);
            return seat != null ? GetSeatPriceByType((SeatType)seat.SeatType) : 0;
        }

        private int GetAvailableSeatsCount(int screeningId)
        {
            var totalSeats = db.Screenings
                .Where(s => s.ScreeningID == screeningId)
                .Select(s => s.CinemaRoom.SeatTemplate.TotalSeats)
                .FirstOrDefault();
            var booked = db.Tickets.Count(t => t.ScreeningID == screeningId);
            return (totalSeats ?? 0) - booked;
        }

        private string GetDayOfWeek(DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Monday: return "Thứ 2";
                case DayOfWeek.Tuesday: return "Thứ 3";
                case DayOfWeek.Wednesday: return "Thứ 4";
                case DayOfWeek.Thursday: return "Thứ 5";
                case DayOfWeek.Friday: return "Thứ 6";
                case DayOfWeek.Saturday: return "Thứ 7";
                case DayOfWeek.Sunday: return "Chủ nhật";
                default: return "";
            }
        }
        [HttpPost]
        public JsonResult ConfirmBookingTest(string sessionId)
        {
            var sessionKey = $"BookingSession_{sessionId}";
            var session = Session[sessionKey] as BookingSessionViewModel;

            if (session == null || session.SelectedSeats == null || !session.SelectedSeats.Any())
            {
                return Json(new { success = false, message = "Không có ghế nào được chọn." });
            }

            var userId = 1; 
            var bookingTime = DateTime.Now;

            foreach (var seatNumber in session.SelectedSeats)
            {
                var ticket = new Ticket
                {
                    ScreeningID = session.ScreeningId,
                    SeatNumber = seatNumber,
                    BookingTime = bookingTime,
                    TotalPrice = GetSeatPrice(session.ScreeningId, seatNumber),
                    PaymentStatus = 0,
                    UserID = userId
                };

                db.Tickets.Add(ticket);
            }

            db.SaveChanges();

            return Json(new { success = true });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ProceedToPayment(string sessionId)
        {
            var sessionKey = $"BookingSession_{sessionId}";
            var session = Session[sessionKey] as BookingSessionViewModel;

            if (session == null || session.SelectedSeats == null || !session.SelectedSeats.Any())
            {
                TempData["Error"] = "Bạn chưa chọn ghế.";
                return RedirectToAction("SelectSeat", new { screeningId = session?.ScreeningId ?? 0 });
            }

            var model = new SeatSelectionViewModel
            {
                ScreeningId = session.ScreeningId,
                SelectedSeats = session.SelectedSeats,
                TotalPrice = session.TotalPrice
                
            };

            return View("~/Views/Payment/PaymentConfirm.cshtml", model);
        }

        #endregion
    }
}



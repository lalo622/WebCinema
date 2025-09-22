using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WebCinema.Helper;
using WebCinema.Models;
using WebCinema.ViewModel;

namespace WebCinema.Areas.Admin.Controllers
{
    [AdminAuthorize]

    public class TicketController : Controller
    {
        private WebCinemaEntities db = new WebCinemaEntities();

        // GET: Admin/Ticket
        public ActionResult Index()
        {
            var tickets = db.Tickets
                .Include(t => t.User)
                .Include(t => t.User.Customer)
                .Include(t => t.Screening)
                .Include(t => t.Screening.Movie)
                .Include(t => t.Screening.CinemaRoom)
                .OrderByDescending(t => t.BookingTime)
                .ToList();

            var ticketViewModels = tickets.Select(t => new TicketDetailViewModel
            {
                TicketID = t.TicketID,
                CustomerName = t.User.Customer?.FullName ?? "Khách vãng lai",
                Email = t.User.Customer?.Email ?? t.User.Username,
                Phone = t.User.Customer?.Phone ?? "N/A",
                MovieTitle = t.Screening.Movie.Title,
                ScreeningTime = t.Screening.StartTime,
                RoomName = t.Screening.CinemaRoom.RoomName,
                SeatNumber = t.SeatNumber,
                BookingTime = t.BookingTime,
                TotalPrice = t.TotalPrice,
                PaymentStatus = t.PaymentStatus,
            }).ToList();

            return View(ticketViewModels);
        }

        // Action để hiển thị trang in
        public ActionResult PrintTicket(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.User)
                .Include(t => t.User.Customer)
                .Include(t => t.Screening)
                .Include(t => t.Screening.Movie)
                .Include(t => t.Screening.CinemaRoom)
                .FirstOrDefault(t => t.TicketID == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var ticketViewModel = new TicketDetailViewModel
            {
                TicketID = ticket.TicketID,
                CustomerName = ticket.User.Customer?.FullName ?? "Khách vãng lai",
                Email = ticket.User.Customer?.Email ?? ticket.User.Username,
                Phone = ticket.User.Customer?.Phone ?? "N/A",
                MovieTitle = ticket.Screening.Movie.Title,
                ScreeningTime = ticket.Screening.StartTime,
                RoomName = ticket.Screening.CinemaRoom.RoomName,
                SeatNumber = ticket.SeatNumber,
                BookingTime = ticket.BookingTime,
                TotalPrice = ticket.TotalPrice,
                PaymentStatus = ticket.PaymentStatus,
            };

            return View(ticketViewModel);
        }

        // Action để xuất PDF
        public ActionResult ExportToPdf(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.User)
                .Include(t => t.User.Customer)
                .Include(t => t.Screening)
                .Include(t => t.Screening.Movie)
                .Include(t => t.Screening.CinemaRoom)
                .FirstOrDefault(t => t.TicketID == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var ticketViewModel = new TicketDetailViewModel
            {
                TicketID = ticket.TicketID,
                CustomerName = ticket.User.Customer?.FullName ?? "Khách vãng lai",
                Email = ticket.User.Customer?.Email ?? ticket.User.Username,
                Phone = ticket.User.Customer?.Phone ?? "N/A",
                MovieTitle = ticket.Screening.Movie.Title,
                ScreeningTime = ticket.Screening.StartTime,
                RoomName = ticket.Screening.CinemaRoom.RoomName,
                SeatNumber = ticket.SeatNumber,
                BookingTime = ticket.BookingTime,
                TotalPrice = ticket.TotalPrice,
                PaymentStatus = ticket.PaymentStatus,
            };

            // Tạo PDF
            byte[] pdfBytes = GenerateTicketPdf(ticketViewModel);

            // Trả về file PDF
            return File(pdfBytes, "application/pdf", $"Ve_{ticket.TicketID}.pdf");
        }

        private byte[] GenerateTicketPdf(TicketDetailViewModel ticket)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Tạo document PDF
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Font tiếng Việt
                string fontPath = Path.Combine(Server.MapPath("~/Content/fonts/"), "arial.ttf");
                BaseFont baseFont;

                try
                {
                    baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
                catch
                {
                    // Nếu không có font Arial, dùng font mặc định
                    baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                }

                Font titleFont = new Font(baseFont, 20, Font.BOLD);
                Font headerFont = new Font(baseFont, 14, Font.BOLD);
                Font normalFont = new Font(baseFont, 12, Font.NORMAL);
                Font smallFont = new Font(baseFont, 10, Font.NORMAL);

                // Header - Tiêu đề
                Paragraph title = new Paragraph("VÉ XEM PHIM", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Thông tin rạp
                Paragraph cinemaInfo = new Paragraph("CINEMA BOOKING SYSTEM", headerFont);
                cinemaInfo.Alignment = Element.ALIGN_CENTER;
                cinemaInfo.SpacingAfter = 30f;
                document.Add(cinemaInfo);

                // Tạo bảng thông tin vé
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 1.5f });
                table.SpacingBefore = 20f;

                // Thêm các dòng thông tin
                AddTableRow(table, "Mã vé:", ticket.TicketID.ToString(), headerFont, normalFont);
                AddTableRow(table, "Khách hàng:", ticket.CustomerName, headerFont, normalFont);
                AddTableRow(table, "Email:", ticket.Email, headerFont, normalFont);
                AddTableRow(table, "Điện thoại:", ticket.Phone, headerFont, normalFont);
                AddTableRow(table, "Phim:", ticket.MovieTitle, headerFont, normalFont);
                AddTableRow(table, "Suất chiếu:", ticket.ScreeningTime.ToString(@"hh\:mm"), headerFont, normalFont);
                AddTableRow(table, "Phòng:", ticket.RoomName, headerFont, normalFont);
                AddTableRow(table, "Ghế:", ticket.SeatNumber, headerFont, normalFont);
                AddTableRow(table, "Thời gian đặt:", ticket.BookingTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A", headerFont, normalFont);
                AddTableRow(table, "Tổng tiền:", ticket.TotalPrice.ToString("N0") + " đ", headerFont, normalFont);

                string paymentStatusText = ticket.PaymentStatus == 1 ? "Đã thanh toán" : "Chưa thanh toán";
                AddTableRow(table, "Trạng thái:", paymentStatusText, headerFont, normalFont);

                document.Add(table);

                // Footer
                Paragraph footer = new Paragraph("\n\nCảm ơn bạn đã sử dụng dịch vụ của chúng tôi!", smallFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 30f;
                document.Add(footer);

                Paragraph note = new Paragraph("Vui lòng mang theo vé này khi đến rạp", smallFont);
                note.Alignment = Element.ALIGN_CENTER;
                note.SpacingBefore = 10f;
                document.Add(note);

                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 10f;
            labelCell.PaddingTop = 5f;

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingBottom = 10f;
            valueCell.PaddingTop = 5f;

            table.AddCell(labelCell);
            table.AddCell(valueCell);
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
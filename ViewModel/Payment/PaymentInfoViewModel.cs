using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.ViewModel.Payment;

namespace WebCinema.ViewModel
{
    public class PaymentInfoViewModel
    {
        // Existing properties...
        public string BookingSessionId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<string> SelectedSeats { get; set; }
        public List<SelectedFoodViewModel> SelectedFoods { get; set; } = new List<SelectedFoodViewModel>();
        public List<SelectedComboViewModel> SelectedCombos { get; set; } = new List<SelectedComboViewModel>();
        public List<FoodComboVM> AvailableItems { get; set; } = new List<FoodComboVM>();
        public decimal TotalPrice { get; set; }

        // New properties for points system
        public CustomerPointInfo CustomerPointInfo { get; set; }
        public int PointsToUse { get; set; } = 0;
        public decimal PointDiscount { get; set; } = 0;
        public decimal FinalPrice => TotalPrice - PointDiscount;
        public int EstimatedEarnedPoints { get; set; }

        // Helper methods
        public int MaxUsablePoints => CustomerPointInfo?.CurrentPoints ?? 0;
        public decimal MaxPointDiscount => MaxUsablePoints * 1000; // 1 điểm = 1000 VNĐ
        public bool HasEnoughPoints(int points) => (CustomerPointInfo?.CurrentPoints ?? 0) >= points;

        public string CurrentLevelName => CustomerPointInfo?.CurrentLevel?.Name ?? "Chưa có thẻ";
        public string NextLevelName => CustomerPointInfo?.NextLevel?.Name ?? "Cao nhất";
        public decimal SpendingToNextLevel => CustomerPointInfo?.SpendingToNextLevel ?? 0;
    }

}
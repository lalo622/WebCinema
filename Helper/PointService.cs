using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.Models;
using WebCinema.ViewModel;

namespace WebCinema.Helper
{
    public class PointService
    {
        private readonly WebCinemaEntities _db;

        public PointService(WebCinemaEntities db)
        {
            _db = db;
        }

        public PointService() : this(new WebCinemaEntities()) { }

        /// <summary>
        /// Tính điểm tích lũy từ số tiền (1000 VNĐ = 1 điểm)
        /// </summary>
        public int CalculatePointsFromAmount(decimal amount)
        {
            return (int)(amount / 1000);
        }

        /// <summary>
        /// Tích điểm cho khách hàng
        /// </summary>
        public bool EarnPoints(int customerId, decimal amount, string description, string referenceType = "BOOKING", int? referenceId = null)
        {
            try
            {
                var customer = _db.Customers.Find(customerId);
                if (customer == null) return false;

                // Lấy tỷ lệ tích điểm theo level (nếu có)
                var memberLevel = customer.MemberLevel;
                double pointMultiplier = 1.0;

                if (memberLevel != null)
                {
                    // Sử dụng TicketPointPercent làm multiplier (VD: 1.5 = 150%)
                    pointMultiplier = memberLevel.TicketPointPercent;
                }

                // Tính điểm cơ bản
                int basePoints = CalculatePointsFromAmount(amount);
                int earnedPoints = (int)(basePoints * pointMultiplier);

                if (earnedPoints <= 0) return false;

                // Cập nhật điểm cho customer
                customer.Points = (customer.Points ?? 0) + earnedPoints;
                customer.TotalSpending = (customer.TotalSpending ?? 0) + amount;

                // Lưu lịch sử tích điểm
                var pointHistory = new PointHistory
                {
                    CustomerID = customerId,
                    Points = earnedPoints,
                    TransactionType = "EARN",
                    Description = description,
                    RelatedAmount = amount,
                    ReferenceType = referenceType,
                    ReferenceID = referenceId,
                    CreatedDate = DateTime.Now
                };

                _db.PointHistories.Add(pointHistory);

                // Kiểm tra và nâng cấp thẻ thành viên
                CheckAndUpgradeMemberLevel(customer);

                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        /// <summary>
        /// Sử dụng điểm để giảm giá
        /// </summary>
        public bool RedeemPoints(int customerId, int pointsToRedeem, string description, string referenceType = "BOOKING", int? referenceId = null)
        {
            try
            {
                var customer = _db.Customers.Find(customerId);
                if (customer == null || customer.Points < pointsToRedeem)
                    return false;

                // Trừ điểm
                customer.Points -= pointsToRedeem;

                // Lưu lịch sử sử dụng điểm
                var pointHistory = new PointHistory
                {
                    CustomerID = customerId,
                    Points = -pointsToRedeem, // Số âm để biểu thị việc trừ điểm
                    TransactionType = "REDEEM",
                    Description = description,
                    RelatedAmount = pointsToRedeem, // Số điểm đã sử dụng
                    ReferenceType = referenceType,
                    ReferenceID = referenceId,
                    CreatedDate = DateTime.Now
                };

                _db.PointHistories.Add(pointHistory);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        /// <summary>
        /// Tính số tiền giảm giá từ điểm (1 điểm = 1000 VNĐ)
        /// </summary>
        public decimal CalculateDiscountFromPoints(int points)
        {
            return points * 100;
        }

        /// <summary>
        /// Kiểm tra và nâng cấp thẻ thành viên
        /// </summary>
        private void CheckAndUpgradeMemberLevel(Customer customer)
        {
            var availableLevels = _db.MemberLevels
                .Where(ml => ml.RequiredSpending <= customer.TotalSpending)
                .OrderByDescending(ml => ml.RequiredSpending)
                .ToList();

            if (availableLevels.Any())
            {
                var highestLevel = availableLevels.First();
                if (customer.MemberLevelID != highestLevel.MemberLevelID)
                {
                    customer.MemberLevelID = highestLevel.MemberLevelID;

                    
                }
            }
        }

        /// <summary>
        /// Lấy thông tin điểm và level của khách hàng
        /// </summary>
        public CustomerPointInfo GetCustomerPointInfo(int customerId)
        {
            var customer = _db.Customers
                .Include("MemberLevel")
                .Include("User")
                .FirstOrDefault(c => c.CustomerID == customerId);

            if (customer == null) return null;

            var nextLevel = _db.MemberLevels
                .Where(ml => ml.RequiredSpending > (customer.TotalSpending ?? 0))
                .OrderBy(ml => ml.RequiredSpending)
                .FirstOrDefault();

            return new CustomerPointInfo
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                CurrentPoints = customer.Points ?? 0,
                TotalSpending = customer.TotalSpending ?? 0,
                CurrentLevel = customer.MemberLevel,
                NextLevel = nextLevel,
                SpendingToNextLevel = nextLevel != null ? nextLevel.RequiredSpending - (customer.TotalSpending ?? 0) : 0
            };
        }

        /// <summary>
        /// Lấy lịch sử tích điểm
        /// </summary>
        public List<PointHistory> GetPointHistory(int customerId, int pageSize = 10, int page = 1)
        {
            return _db.PointHistories
                .Where(ph => ph.CustomerID == customerId)
                .OrderByDescending(ph => ph.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
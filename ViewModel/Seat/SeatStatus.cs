using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Seat
{
    public enum SeatStatus
    {
        Available = 1,   // Có thể chọn
        Occupied = 2,    // Đã có người đặt
        Selected = 3,    // Đang được chọn
      
    }
}
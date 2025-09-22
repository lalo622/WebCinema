using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.Helper
{
    public static class UserRoles
    {
        public const byte Customer = 0;
        public const byte Staff = 1;
        public const byte CinemaManager = 2;
        public const byte SuperAdmin = 3;

        public static string GetRoleName(byte? role)
        {
            if (!role.HasValue) return "Customer";

            switch (role.Value)
            {
                case Staff: return "Staff";
                case CinemaManager: return "CinemaManager";
                case SuperAdmin: return "SuperAdmin";
                default: return "Customer";
            }
        }
    }
}
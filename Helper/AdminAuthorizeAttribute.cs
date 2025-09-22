
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace WebCinema.Helper
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly byte[] _allowedRoles;

        // Constructor không tham số - cho phép tất cả admin
        public AdminAuthorizeAttribute()
        {
            _allowedRoles = new byte[] { UserRoles.Staff, UserRoles.CinemaManager, UserRoles.SuperAdmin };
        }

        // Constructor với 1 role
        public AdminAuthorizeAttribute(byte allowedRole)
        {
            _allowedRoles = new byte[] { allowedRole };
        }

        // Constructor với nhiều roles
        public AdminAuthorizeAttribute(params byte[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            // Kiểm tra đã đăng nhập chưa
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            // Lấy role từ Session
            var userRole = httpContext.Session["UserRole"];
            if (userRole == null)
                return false;

            byte currentRole;
            if (userRole is byte)
            {
                currentRole = (byte)userRole;
            }
            else if (byte.TryParse(userRole.ToString(), out currentRole))
            {
                // Thành công parse
            }
            else
            {
                return false;
            }

            // Kiểm tra role có được phép không
            foreach (var allowedRole in _allowedRoles)
            {
                if (currentRole == allowedRole)
                    return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Chưa đăng nhập -> redirect về login
                filterContext.Result = new RedirectResult("~/Auth/Login");
            }
            else
            {
                // Đã đăng nhập nhưng không đủ quyền -> redirect về Unauthorized
                filterContext.Result = new RedirectResult("~/Auth/Unauthorized");
            }
        }
    }
}
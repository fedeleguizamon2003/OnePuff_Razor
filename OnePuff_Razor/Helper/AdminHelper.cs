using System.Security.Claims;

namespace OnePuff_Razor.Helpers
{
    public static class AdminHelper
    {
        private const string ADMIN_MAESTRO = "calopez@iiconcepcion.edu.ar";

        public static bool EsAdminMaestro(ClaimsPrincipal user)
        {
            var email = user.Identity?.Name;
            return email != null && email.Equals(ADMIN_MAESTRO, StringComparison.OrdinalIgnoreCase);
        }
    }
}

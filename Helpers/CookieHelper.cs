namespace WheresWaldoApi.Helpers;

public static class CookieHelper
{
    public static CookieOptions AuthCookie(
        IWebHostEnvironment env)
    {
        bool isProduction =
            env.IsProduction();

        return new CookieOptions
        {
            HttpOnly = true,

            Secure = isProduction,

            SameSite =
                isProduction
                    ? SameSiteMode.None
                    : SameSiteMode.Lax,

            Expires =
                DateTimeOffset.UtcNow.AddDays(3),

            Path = "/"
        };
    }
}
namespace AceBackend.Helpers
{
    public static class OtpHelper
    {
        private static readonly Random _random = new Random();

        public static string Generate(int length = 6)
        {
            var otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                otp += _random.Next(0, 10).ToString();
            }
            return otp;
        }
    }
}

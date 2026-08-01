namespace Application.Dtos.DigiKala
{
    public static class DigiKalaErrors
    {
        public const string InvalidUrlFormat = "DIGIKALA_INVALID_URL";
        public const string RateLimited = "DIGIKALA_RATE_LIMITED";
        public const string HttpFetchFailed = "DIGIKALA_HTTP_FAILED";
        public const string MalformedResponse = "DIGIKALA_MALFORMED";
        public const string NoVariants = "DIGIKALA_NO_VARIANTS";
        public const string ParseFailed = "DIGIKALA_PARSE_FAILED";
    }
}

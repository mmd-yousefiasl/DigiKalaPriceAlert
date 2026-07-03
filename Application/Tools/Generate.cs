using System.Security.Cryptography;

namespace Application.Tools
{
    public static class Generate
    {
        public static string UserLoginCode()
        {
            Random rnd = new Random();
            string code = "";

            for (int i = 0; i < 6; i++)
            {
                code += rnd.Next(0, 10);
            }

            return code;
        }

        public static string UniqueStringCode() => Guid.NewGuid().ToString().Replace("-", "");

        public static string InvoiceId()
        {
            // 1. Time Anchor (e.g., "2607" for July 2026)
            string datePart = DateTime.UtcNow.ToString("yyMM");

            // 2. High-readability Base32 set (Excludes confusing pairs like 0/O, 1/I)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            // Allocate space for prefix (3) + dash (1) + date (4) + dash (1) + suffix (6) = 15 chars
            Span<char> idBuffer = stackalloc char[15];

            // Generate 3 random prefix letters
            idBuffer[0] = chars[RandomNumberGenerator.GetInt32(24)]; // Only pick from letters (A-Z indices)
            idBuffer[1] = chars[RandomNumberGenerator.GetInt32(24)];
            idBuffer[2] = chars[RandomNumberGenerator.GetInt32(24)];

            idBuffer[3] = '-';

            // Copy date strings efficiently without allocations
            idBuffer[4] = datePart[0];
            idBuffer[5] = datePart[1];
            idBuffer[6] = datePart[2];
            idBuffer[7] = datePart[3];

            idBuffer[8] = '-';

            // Generate 6 random alphanumeric suffix characters
            for (int i = 9; i < 15; i++)
            {
                idBuffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            // Output Example: KTF-2607-H7N9P4
            return new string(idBuffer);
        }

    }
}
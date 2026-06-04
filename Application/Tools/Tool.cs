using Domain.Shared;

namespace Application.Tools
{
    public static class Tool
    {
        public async static Task<string> SaveImageFromByte(this byte[] imageBytes, string name, string path)
        {
            string fileName = $"{name}.jpg";
            string localFilePath = Path.Combine(path, fileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            await File.WriteAllBytesAsync(localFilePath, imageBytes);

            return fileName;
        }

        public static Result<string> NormalizeIranPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return Result.Failure<string>(AppStatics.OnlyIranPhoneNumberError);

            string cleaned = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

            if (cleaned.StartsWith("+98"))
            {
                cleaned = "0" + cleaned.Substring(3);
            }
            else if (cleaned.StartsWith("98"))
            {
                cleaned = "0" + cleaned.Substring(2);
            }

            if (cleaned.Length == 11 && cleaned.StartsWith("09") && cleaned.All(char.IsDigit))
            {
                return cleaned;
            }

            return Result.Failure<string>(AppStatics.OnlyIranPhoneNumberError);
        }
    }
}

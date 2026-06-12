using System.ComponentModel.DataAnnotations;

namespace Application.ValidationAttributes
{
    public class RequiredEachAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IEnumerable<string>;
            if (list == null) return false;

            return list.All(item => !string.IsNullOrWhiteSpace(item));
        }
    }
}

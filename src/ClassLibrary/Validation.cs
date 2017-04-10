using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Text;

namespace ClassLibrary
{
    public class CannotBeInFuture: ValidationAttribute
    {
        public CannotBeInFuture()
        {
            ErrorMessage = "{0} cannot be in future";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if ((DateTime)value < DateTime.Now)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(string.Format(ErrorMessage, validationContext.DisplayName));
            } catch
            {
                return new ValidationResult(string.Format("{0} must be a valid date like 23 Jun 2016", validationContext.DisplayName));
            }
        }
    }

    public class CanadianPostalCode: ValidationAttribute
    {
        string moreInfo = "https://en.wikipedia.org/wiki/Postal_codes_in_Canada#Number_of_possible_postal_codes";
        Regex postalCodePattern = new Regex(@"^[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ][\s-]?[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]$");
        public CanadianPostalCode()
        {
            ErrorMessage = "{0} is not a valid canadian postal code. The format is A1A1A1 or A1A-A1A. See: {1}";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string postalCode = (string)value;
            // Because it is optional
            if (postalCode == "" || postalCode == null)
            {
                return ValidationResult.Success;
            }
            Match match = postalCodePattern.Match(postalCode.ToUpper());
            if (postalCodePattern.IsMatch(postalCode) && match.Index == 0) {
                return ValidationResult.Success;
            }
            return new ValidationResult(string.Format(ErrorMessage, postalCode, moreInfo));
        }
    }

    public static class Validations
    {
        public static string Capitalize(string value)
        {
            if (value == null || value == "") return value;
            // function to remove white space and capatilize first letter of all words
            value = value.Trim().ToLower();
            StringBuilder capitalized = new StringBuilder();
            if (value.Length > 0) {
                capitalized.Append(char.ToUpper(value[0]));
                int i = 1;
                while (i < value.Length - 1)
                {
                    if (value[i] == ' ')
                    {
                        capitalized.Append(' ');
                        capitalized.Append(char.ToUpper(value[i + 1]));
                        i += 2;
                    } else
                    {
                        capitalized.Append(value[i]);
                        i += 1;
                    }
                }
                capitalized.Append(value[i]);
            }
            return capitalized.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LCBusService.Models
{
    [ModelMetadataType(typeof(DriverMetadata))]
    public partial class Driver : IValidatableObject
    {
        private BusServiceContext _context; // database connection injection

        public Driver(BusServiceContext context)
        {
            _context = context;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Format names
            FirstName = ClassLibrary.Validations.Capitalize(FirstName);
            LastName = ClassLibrary.Validations.Capitalize(LastName);
            FullName = string.Format("{0}, {1}", LastName, FirstName);
            // check privince code; in database; upper case
            string message = "";
            try
            {
                ProvinceCode = ProvinceCode.ToUpper();
                if (!ProvinceCodeExists(ProvinceCode))
                {

                    message = string.Format("{0} is not a valid province code, please ensure it is only two letters", ProvinceCode);
                }
            } catch (Exception e) {
                message = string.Format("Unknown error occured: {0}. Please enter a 2 digit code", e.ToString());
            }
            if (message != "")
            {
                yield return new ValidationResult(message);
            }
            // add in final format Q1A 1A1, custom validator validates the format
            formatPostalCode();
            // check home phone is required, work optional; check formatting as well
            // writing phone to database requires the format 519-555-1234
            bool homePhoneValid = validatePhoneNumber(HomePhone, false);
            if (homePhoneValid)
            {
                HomePhone = formatPhoneNumber(HomePhone);
            } else
            {
                yield return new ValidationResult(string.Format("Phone number {0} was not valid, please enter a 10 digit phone number (i.e. 519-123-4567)", HomePhone));
            }
            bool workPhoneValid = validatePhoneNumber(WorkPhone, true);
            if (workPhoneValid)
            {
                WorkPhone = formatPhoneNumber(WorkPhone);
            }
            else
            {
                yield return new ValidationResult(string.Format("Phone number {0} was not valid, please  leave empty or enter a 10 digit number (i.e. 519-123-4567", WorkPhone));
            }
            // date is required and cannot be in future; display like 23 Oct 2013 in form
            yield return ValidationResult.Success;
        }

        public bool ProvinceCodeExists(string provinceCode)
        {
            if (provinceCode.Length != 2) {
                return false;
            }
            if (_context == null)
            {
                _context = Context.GetContext();
            }
            var checkCode = _context.Province.Where(p => p.ProvinceCode == provinceCode).ToList();
            return checkCode.Count() > 0;
        }
        private void formatPostalCode()
        {
            PostalCode.ToUpper();
            if (PostalCode.Length > 6)
            {
                PostalCode.Replace('-', ' ');
            }
            else
            {
                PostalCode.Insert(3, " ");
            }
        }

        private bool validatePhoneNumber(string phoneNumber, bool optional)
        {
            if (optional && (phoneNumber == null || phoneNumber == ""))
            {
                return true;
            }
            if (phoneNumber.Length < 10) { return false; }
            char[] digits = new char[10];
            int idx = 0;
            for (int i = 0; i < phoneNumber.Length; i++)
            {
                if (char.IsDigit(phoneNumber[i]))
                {
                    if (idx >= 10)
                    {
                        return false;
                    }
                    digits[idx] = phoneNumber[i];
                    idx += 1;
                }
            }
            return digits.Length == 10;
        }
        private string formatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null || phoneNumber == "")
            {
                return phoneNumber;
            }
            StringBuilder formatted = new StringBuilder(10);
            int stringIdx = 0;
            for (int i = 0; i < phoneNumber.Length; i++)
            {
                if (char.IsDigit(phoneNumber[i]))
                {
                    if (stringIdx == 3 || stringIdx == 6)
                    {
                        formatted.Append("-");
                    }
                    formatted.Append(phoneNumber[i]);
                    stringIdx += 1;
                }
            }
            return formatted.ToString();
        }
    }

    public class DriverMetadata
    {
        public int DriverId { get; set; }
        [Required()]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required()]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set;}
        [Required()]
        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }
        [Display(Name = "Work Phone")]
        public string WorkPhone { get; set; }
        [Display(Name = "Street Address")]
        public string Street { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "Postal Code")]
        [ClassLibrary.CanadianPostalCode()]
        [Required()]
        public string PostalCode { get; set; }
        [Remote("ProvinceCodeRemoteValidator", "Drivers")]
        [Display(Name = "Province")]
        public string ProvinceCode { get; set; }
        // @Html.EditorFor(m => m.StartDate, new { htmlAttributes = new { @class = "datepicker" } })
        [Display(Name = "Date Hired")]
        [DisplayFormat(DataFormatString = "{0:d MMM yyyy}", ApplyFormatInEditMode = true)]
        [ClassLibrary.CannotBeInFuture()]
        [Required()]
        public DateTime? DateHired { get; set; }
    }
}

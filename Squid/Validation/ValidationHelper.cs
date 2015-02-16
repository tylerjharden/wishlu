using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Squid.Validation
{
    public class ValidationException : ApplicationExceptionBase
    {
        public List<ValidationError> ValidationErrors { get; private set; }

        public ValidationException(String message, List<ValidationError> validationErrors, params Object[] additionalInformation)
            : base(message, additionalInformation)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(validationErrors.Count > 0);

            ValidationErrors = validationErrors;
        }
    }

    public static class ValidationHelper
    {
        public static void ValidatePattern(this List<ValidationError> validationErrors, String memberName, String value, Regex regex, String errorTextPath)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(regex != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));
            Debug.Assert(!String.IsNullOrEmpty(errorTextPath));

            if (value == null)
                return;

            if (String.IsNullOrEmpty(value))
                return;

            if (regex.IsMatch(value))
                return;

            String message = "Validation error"; //WishLuSession.GetText(errorTextPath,memberName);

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateRange<T>(this List<ValidationError> validationErrors, String memberName, T? value, T minValue, T maxValue) where T : struct, IComparable<T>
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value == null)
                return;

            if (value.Value.CompareTo(minValue) < 0)
            {
                String message = "Service.Validation.ValueTooLowError";

                validationErrors.Add(new ValidationError(message, memberName));
            }
            if (value.Value.CompareTo(maxValue) > 0)
            {
                String message = "Service.Validation.ValueTooHighError";

                validationErrors.Add(new ValidationError(message, memberName));
            }
        }

        public static void ValidateMaxLength(this List<ValidationError> validationErrors, String memberName, String value, int maximumLength)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value == null)
                return;
            if (value.Length <= maximumLength)
                return;

            String message = "Service.Validation.LengthTooLongError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateMaxLength(this List<ValidationError> validationErrors, String memberName, Byte[] value, int maximumLength)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value == null)
                return;
            if (value.Length <= maximumLength)
                return;

            String message = "Service.Validation.LengthTooLongError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateMinLength(this List<ValidationError> validationErrors, String memberName, String value, int minimumLength)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value == null)
                return;
            if (value.Length >= minimumLength)
                return;

            String message = "Service.Validation.LengthTooShortError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateMinLength(this List<ValidationError> validationErrors, String memberName, Byte[] value, int minimumLength)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value == null)
                return;
            if (value.Length >= minimumLength)
                return;

            String message = "Service.Validation.LengthTooShortError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateNotNull(this List<ValidationError> validationErrors, String memberName, String value)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (!String.IsNullOrEmpty(value))
                return;

            String message = "Service.Validation.NullError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateNotNull(this List<ValidationError> validationErrors, String memberName, Byte[] value)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value != null && value.Length > 0)
                return;

            String message = "Service.Validation.NullError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateNotNull(this List<ValidationError> validationErrors, String memberName, Guid? value)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (value != null && value.Value != Guid.Empty)
                return;

            String message = "Service.Validation.NullError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ValidateEnum<T>(this List<ValidationError> validationErrors, String memberName, T value)
        {
            Debug.Assert(validationErrors != null);
            Debug.Assert(!String.IsNullOrEmpty(memberName));

            if (Enum.IsDefined(typeof(T), value))
                return;
            String message = "Service.Validation.EnumerationError";

            validationErrors.Add(new ValidationError(message, memberName));
        }

        public static void ThrowValidationException(this List<ValidationError> validationErrors)
        {
            Debug.Assert(validationErrors != null);

            if (validationErrors.Count == 0)
                return;

            String message = "Invalid input"; //WishLuSession.GetText("Service.Validation.InvalidInputs");

            throw new ValidationException(message, validationErrors);
        }
    }
}
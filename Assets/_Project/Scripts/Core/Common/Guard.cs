using System;

namespace TW08.Common
{
    public static class Guard
    {
        public static T NotNull<T>(T value, string name) where T : class
        {
            return value ?? throw new ArgumentNullException(name);
        }

        public static string NotBlank(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be blank.", name);
            }

            return value;
        }

        public static int Positive(int value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value must be positive.");
            }

            return value;
        }
    }
}

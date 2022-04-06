using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Common
{
    public static class AffCodeIdentifier
    {
        /// <summary>
        /// Generates the identifier.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string GenerateIdentifier(int id)
        {
            int firstLength = 5;
            if (id.ToString().Length < 5) firstLength = firstLength - id.ToString().Length;
            return string.Format("{0}{1}{2}", Guid.NewGuid().ToString("n").Substring(0, firstLength), id, AppendCheckDigit(id));
        }

        private static int AppendCheckDigit(int id)
        {
            int checkDigit = 0;
            string input = id.ToString();
            for (int i = 0; i < input.Length; i++)
                checkDigit += System.Convert.ToInt32(input.Substring(i, 1));
            checkDigit = checkDigit * 9;
            if (checkDigit < 10) return checkDigit;
            int endValue = checkDigit / (10 * (checkDigit.ToString().Length - 1));
            return endValue;
        }
    }
}

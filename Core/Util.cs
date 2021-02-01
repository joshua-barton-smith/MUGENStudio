using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.Core
{
    public class Util
    {
        // converts a value from an INI KV pair into a regular string.
        // generally just removes leading/trailing quotes.
        public static string ParseStringFromINIValue(string value)
        {
            string output = "";
            bool bounded = false;
            foreach (char c in value.ToCharArray())
            {
                if (c == '"' && !bounded)
                {
                    bounded = true;
                } else if (c == '"' && bounded)
                {
                    return output;
                } else
                {
                    output += c;
                }
            }
            throw new FormatException(string.Format("Input string {0} is not well-formed for conversion!", value));
        }
    }
}

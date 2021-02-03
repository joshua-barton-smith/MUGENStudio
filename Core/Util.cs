using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.Core
{
    /// <summary>
    /// Utility functions for parsing and processing
    /// </summary>
    public class Util
    {

        /// <summary>
        /// converts a value from an INI KV pair into a regular string.
        /// generally just removes leading/trailing quotes.
        /// </summary>
        /// <param name="value">string to parse</param>
        /// <returns>processed string</returns>
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

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// update by Ziddia: default to Shift-JIS here instead since a lot of JP authors have this encoding
        /// Credit to 2Toad on SO https://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.GetEncoding(932);
        }
    }
}

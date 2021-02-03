using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents the CMD file for a character
    /// </summary>
    public class MugenCMD : MugenINI
    {

        /// <summary>
        /// parses + processes an input CMD file
        /// </summary>
        /// <param name="path">CMD file to process</param>
        public MugenCMD(string path) : base(path)
        {
        }
    }
}

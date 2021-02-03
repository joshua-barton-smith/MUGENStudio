using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents an ST file for a character
    /// </summary>
    public class MugenST : MugenINI
    {
        /// <summary>
        /// parses + processes an input ST file
        /// </summary>
        /// <param name="path">ST file to process</param>
        public MugenST(string path) : base(path)
        {
        }
    }
}

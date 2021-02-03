using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents the CNS file for a character
    /// </summary>
    public class MugenCNS : MugenINI
    {
        /// <summary>
        /// parses + processes an input CNS file
        /// </summary>
        /// <param name="path">CNS file to process</param>
        public MugenCNS(string path) : base(path)
        {
        }
    }
}

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
        /// <param name="fileKey">name of the file from the DEF</param>
        public MugenCNS(string path, string fileKey) : base(path, fileKey)
        {
        }
    }
}

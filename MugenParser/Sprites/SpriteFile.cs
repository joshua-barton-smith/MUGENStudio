using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Sprites
{
    /// <summary>
    /// generic file for SFF
    /// </summary>
    public abstract class SpriteFile
    {
        /// <summary>
        /// generic constructor
        /// </summary>
        public SpriteFile() { }
        /// <summary>
        /// constructor with header processing
        /// </summary>
        /// <param name="headerData"></param>
        public SpriteFile(byte[] headerData) { }
    }
}

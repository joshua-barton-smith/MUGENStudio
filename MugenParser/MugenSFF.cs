using MUGENStudio.MugenParser.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents a set of sprites.
    /// </summary>
    public class MugenSFF
    {
        private readonly SpriteFile handler;
        private readonly FileStream backing;
        /// <summary>
        /// parses + processes an input SFF file
        /// </summary>
        /// <param name="path">SFF file to process</param>
        /// <param name="fileKey">name of the file from the DEF</param>
        public MugenSFF(string path, string fileKey)
        {
            this.FileKey = fileKey;
            this.FilePath = path;
            if (!File.Exists(path))
            {
                File.Create(path);
                this.backing = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                // generic handler
                this.handler = new SFF2File();
                return;
            }
            // load the sff as a stream
            this.backing = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            // read actual header
            this.handler = this.ReadSFFHeader();
        }

        // reads the header + applies correct version handler based on it.
        private SpriteFile ReadSFFHeader()
        {
            byte[] headTmp = new byte[68];
            this.backing.Read(headTmp, 0, 68);

            byte[] versionBytes = headTmp.Skip(12).Take(4).ToArray();
            string ver = string.Format("{0}.{1}{2}{3}", versionBytes[3], versionBytes[2], versionBytes[1], versionBytes[0]);

            if (ver.Equals("1.010")) return new SFF1File(headTmp);
            else return new SFF2File(headTmp);
        }

        

        /// <summary>
        /// key to be displayed in the window
        /// </summary>
        public string FileKey { get; }
        /// <summary>
        /// direct path to file
        /// </summary>
        public string FilePath { get; }
    }
}

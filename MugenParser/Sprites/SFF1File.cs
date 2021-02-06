using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Sprites
{
    /// <summary>
    /// SFFv1 file parser
    /// </summary>
    public class SFF1File : SpriteFile
    {

        private SFF1Header header;

        /// <summary>
        /// parse the header and validate SFFv1 file
        /// </summary>
        /// <param name="headerData"></param>
        public SFF1File(byte[] headerData) : base(headerData)
        {
            // check
            if (headerData.Length < 32) throw new ArgumentException("Invalid header length for SFFv1!");

            // validation
            string sig = Encoding.UTF8.GetString(headerData.Take(12).ToArray());
            if (!sig.Equals("ElecbyteSpr\0")) throw new FileFormatException("SFF file has a malformed SFF header!");
            // read version
            byte[] versionBytes = headerData.Skip(12).Take(4).ToArray();
            string ver = string.Format("{0}.{1}{2}{3}", versionBytes[3], versionBytes[2], versionBytes[1], versionBytes[0]);
            // validate version
            if (!ver.Equals("1.010")) throw new ArgumentException("Invalid header version for SFFv1!");

            // get values for header
            int groups = BitConverter.ToInt32(headerData, 16);
            int sprites = BitConverter.ToInt32(headerData, 20);

            int offset = BitConverter.ToInt32(headerData, 24);
            int headlen = BitConverter.ToInt32(headerData, 28);

            this.header = new SFF1Header(sig, ver, groups, sprites, offset, headlen);
        }

        private struct SFF1Header
        {
            string sig;
            string ver;
            int groups;
            int sprites;
            int offset;
            int headlen;

            public SFF1Header(string sig,
                              string ver,
                              int groups,
                              int sprites,
                              int offset,
                              int headlen)
            {
                this.sig = sig;
                this.ver = ver;
                this.groups = groups;
                this.sprites = sprites;
                this.offset = offset;
                this.headlen = headlen;
            }
        }
    }
}

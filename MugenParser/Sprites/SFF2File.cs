using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Sprites
{
    /// <summary>
    /// SFFv2 file
    /// </summary>
    public class SFF2File : SpriteFile
    {
        private SFF2Header header;

        /// <summary>
        /// create a generic SFFv2 file
        /// </summary>
        public SFF2File()
        {
            this.header = new SFF2Header("ElecbyteSpr\0", "2.100", 0, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// parse the header of SFFv2 file
        /// </summary>
        /// <param name="headerData"></param>
        public SFF2File(byte[] headerData) : base(headerData)
        {
            // check
            if (headerData.Length < 68) throw new ArgumentException("Invalid header length for SFFv2!");

            // validation
            string sig = Encoding.UTF8.GetString(headerData.Take(12).ToArray());
            if (!sig.Equals("ElecbyteSpr\0")) throw new FileFormatException("SFF file has a malformed SFF header!");
            // read version
            byte[] versionBytes = headerData.Skip(12).Take(4).ToArray();
            string ver = string.Format("{0}.{1}{2}{3}", versionBytes[3], versionBytes[2], versionBytes[1], versionBytes[0]);
            // validate version
            if (ver.Equals("1.010")) throw new ArgumentException("Invalid header version for SFFv2!");

            // get values for header
            int first_sprite = BitConverter.ToInt32(headerData, 36);
            int sprite_count = BitConverter.ToInt32(headerData, 40);

            int first_palette = BitConverter.ToInt32(headerData, 44);
            int palette_count = BitConverter.ToInt32(headerData, 48);

            int ldata_offset = BitConverter.ToInt32(headerData, 52);
            int ldata_length = BitConverter.ToInt32(headerData, 56);

            int tdata_offset = BitConverter.ToInt32(headerData, 60);
            int tdata_length = BitConverter.ToInt32(headerData, 64);

            this.header = new SFF2Header(sig, ver, first_sprite, sprite_count, first_palette, palette_count, ldata_offset, ldata_length, tdata_offset, tdata_length);
        }

        private struct SFF2Header
        {
            string sig;
            string ver;
            int first_sprite;
            int sprite_count;
            int first_palette;
            int palette_count;
            int ldata_offset;
            int ldata_length;
            int tdata_offset;
            int tdata_length;

            public SFF2Header(string sig,
                             string ver,
                             int first_sprite,
                             int sprite_count,
                             int first_palette,
                             int palette_count,
                             int ldata_offset,
                             int ldata_length,
                             int tdata_offset,
                             int tdata_length)
            {
                this.sig = sig;
                this.ver = ver;
                this.first_sprite = first_sprite;
                this.first_palette = first_palette;
                this.sprite_count = sprite_count;
                this.palette_count = palette_count;
                this.ldata_offset = ldata_offset;
                this.tdata_offset = tdata_offset;
                this.ldata_length = ldata_length;
                this.tdata_length = tdata_length;
            }
        }
    }
}

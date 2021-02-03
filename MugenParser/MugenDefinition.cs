using MUGENStudio.Core;
using MUGENStudio.MugenParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{

    /// <summary>
    /// represents a .DEF file and stores references to the other files
    /// in the project (defined by this .DEF)
    /// </summary>
    public class MugenDefinition : MugenINI
    {

        /// <summary>
        /// character internal name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// character display name
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// declared mugenversion of character
        /// </summary>
        public string MugenVersion { get; }
        /// <summary>
        /// localcoord setting of character
        /// </summary>
        public (int, int) LocalCoord { get; }

        /// <summary>
        /// CMD file of character
        /// </summary>
        public MugenCMD CmdFile { get; }
        /// <summary>
        /// CNS file of character
        /// </summary>
        public MugenCNS CnsFile { get; }
        /// <summary>
        /// Common statefile of character
        /// </summary>
        public MugenST CommonFile { get; }
        /// <summary>
        /// Set of statefiles for the character, along with their numbering in the DEF
        /// </summary>
        public Dictionary<string, MugenST> StFiles { get; }

        /// <summary>
        /// DEF file for the character
        /// </summary>
        /// <param name="path">path to the DEF file to load</param>
        public MugenDefinition(string path) : base(path, false)
        {
            // populate relevant fields

            // 1. Info section
            this.Name = Util.ParseStringFromINIValue(this.GetValueWithFallback("Info", "name", "New character"));
            this.DisplayName = Util.ParseStringFromINIValue(this.GetValueWithFallback("Info", "displayname", "NewCharacter"));
            this.MugenVersion = this.GetValueWithFallback("Info", "mugenversion", "win");
            string[] tmpLc = this.GetValueWithFallback("Info", "localcoord", "320,240").Split(',');
            this.LocalCoord = (Int32.Parse(tmpLc[0].Trim()), Int32.Parse(tmpLc[1].Trim()));

            // 2. Files section
            // files consists of:
            // singulars: cmd, cns, def, sprite, anim, sound, ai, stcommon
            // multiple state files st + st0~9
            this.CmdFile = new MugenCMD(this.RelativePathToDef(path, this.GetValueWithFallback("Files", "cmd", "blank.cmd")));

            // 3. Arcade section -- TODO
        }

        private string RelativePathToDef(string defFile, string filePath)
        {
            string basePath = Path.GetDirectoryName(defFile);
            return string.Format("{0}{1}{2}", basePath, Path.DirectorySeparatorChar, filePath);
        }
    }
}

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
        public Dictionary<string, MugenST> StateFiles { get; }

        /// <summary>
        /// DEF file for the character
        /// </summary>
        /// <param name="path">path to the DEF file to load</param>
        public MugenDefinition(string path) : base(path, Path.GetFileName(path), false)
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
            this.CmdFile = new MugenCMD(this.RelativePathToDef(path, this.GetValueWithFallback("Files", "cmd", "blank.cmd")), this.GetValueWithFallback("Files", "cmd", "blank.cmd"));
            this.CnsFile = new MugenCNS(this.RelativePathToDef(path, this.GetValueWithFallback("Files", "cns", "blank.cns")), this.GetValueWithFallback("Files", "cns", "blank.cns"));
            // determine if stcommon is default or overridden
            bool usesDefaultCommon = this.GetValueWithFallback("Files", "stcommon", "common1.cns").Equals("common1.cns");
            // if default, we can try and load the actual stcommon from a relative path
            // if relative path is incorrect (due to folder structure or the project being outside of the MUGEN folder),
            // we load an appropriate common1.cns for the mugenversion
            if (usesDefaultCommon)
            {
                // try to find the actual common1.cns
                if (File.Exists(string.Format("{0}/../../data/common1.cns", Path.GetDirectoryName(path)))) 
                {
                    // if it exists, load it
                    this.CommonFile = new MugenST(string.Format("{0}/../../data/common1.cns", Path.GetDirectoryName(path)), "common1.cns [DEFAULT]");
                } else
                {
                    // load an appropriate common1.cns for the mugenversion
                    if (MugenVersion.Equals("1.0")) this.CommonFile = new MugenST("TextResources/CommonFiles/V1Common.txt", "common1.cns [1.0 DEFAULT]");
                    else if (MugenVersion.Equals("1.1")) this.CommonFile = new MugenST("TextResources/CommonFiles/V11Common.txt", "common1.cns [1.1 DEFAULT]");
                    else this.CommonFile = new MugenST("TextResources/CommonFiles/WinCommon.txt", "common1.cns [WIN DEFAULT]");
                }
            } else
            {
                // load the override stcommon
                this.CommonFile = new MugenST(this.RelativePathToDef(path, this.GetValueWithFallback("Files", "stcommon", "stcommon.st")), this.GetValueWithFallback("Files", "stcommon", "stcommon.st"));
            }

            // load st files
            // st + st0~9 at most
            // st seems to process first
            this.StateFiles = new Dictionary<string, MugenST>();
            if (this.GetValueWithFallback("Files", "st", null) != null)
            {
                this.StateFiles.Add("st", new MugenST(this.RelativePathToDef(path, this.GetValueWithFallback("Files", "st", null)), this.GetValueWithFallback("Files", "st", null)));
            }
            // loop the possible other statefiles
            int i;
            for (i = 0; i < 10; i++)
            {
                if (this.GetValueWithFallback("Files", string.Format("st{0}", i), null) == null)
                {
                    Trace.WriteLine(string.Format("No st file with index {0} in DEF file, ignoring", i));
                } else
                {
                    this.StateFiles.Add(string.Format("st{0}", i), new MugenST(this.RelativePathToDef(path, this.GetValueWithFallback("Files", string.Format("st{0}", i), null)), this.GetValueWithFallback("Files", string.Format("st{0}", i), null)));
                }
            }

            // 3. Arcade section -- TODO

            // validates the project, checks syntax errors, etc
            this.ValidateProject();
        }

        // function to validate syntax/structure of files in the project
        private void ValidateProject()
        {

        }

        // converts a filepath given in the DEF (e.g. `stcommon = mycommon.cns`) to an absolute path based on DEF file location
        private string RelativePathToDef(string defFile, string filePath)
        {
            string basePath = Path.GetDirectoryName(defFile);
            return string.Format("{0}{1}{2}", basePath, Path.DirectorySeparatorChar, filePath);
        }
    }
}

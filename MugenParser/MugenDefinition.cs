using MUGENStudio.Core;
using MUGENStudio.MugenParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio
{
    // represents a .DEF file and stores references to the other files
    // in the project (defined by this .DEF)
    public class MugenDefinition
    {
        // backing storage for the DEF file contents
        private readonly SimpleINI defFile;

        public string Name { get; }
        public string DisplayName { get; }
        public string MugenVersion { get; }
        public (int, int) LocalCoord { get; }

        public MugenDefinition(string path)
        {
            // load the DEF file as a simple INI file
            this.defFile = new SimpleINI(path);
            // populate relevant fields

            // 1. Info section
            this.Name = Util.ParseStringFromINIValue(this.GetValueByKeys("Info", "name"));
            this.DisplayName = Util.ParseStringFromINIValue(this.GetValueByKeys("Info", "displayname"));
            this.MugenVersion = this.GetValueByKeys("Info", "mugenversion");
            string[] tmpLc = this.GetValueByKeys("Info", "localcoord").Split(',');
            this.LocalCoord = (Int32.Parse(tmpLc[0].Trim()), Int32.Parse(tmpLc[1].Trim()));

            // 2. Files section

            // 3. Arcade section -- TODO
        }

        // gets a value when given a section and key to search for.
        // the DEF file should not have duplicate sections or keys,
        // but if it does, this will return the first one only.
        // this is basically just util since the MugenDefinition class
        // exposes properties for most useful DEF file keys
        public string GetValueByKeys(string section, string key)
        {
            return this.defFile.GetNamedProperty(section.ToLower(), key.ToLower());
        }
    }
}

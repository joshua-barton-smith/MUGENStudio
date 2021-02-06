using MUGENStudio.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents an INI file used by MUGEN's processing
    /// </summary>
    public class MugenINI
    {
        /// <summary>
        /// backing INI handler for this file
        /// </summary>
        protected SimpleINI backing;
        // original encoding of the file
        private Encoding encoding;

        /// <summary>
        /// filename used in the DEF
        /// </summary>
        public string FileKey { get; }

        /// <summary>
        /// path to the file used to generate this object
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// parses + processes an input INI file
        /// generic class from which other MUGEN code file handlers should extend
        /// </summary>
        /// <param name="path">INI file to process</param>
        /// <param name="shouldCreate">Indicates if we should create a missing file or not, defaults to true</param>
        /// <param name="fileKey">name of the file from the DEF</param>
        public MugenINI(string path, string fileKey, bool shouldCreate = true)
        {
            this.backing = new SimpleINI(path, shouldCreate);
            this.FileKey = fileKey;
            this.FilePath = path;
            this.encoding = Util.GetEncoding(this.FilePath);
        }

        /// <summary>
        /// fetch a value from a named section, with a fallback for a non-existent key or section.
        /// </summary>
        /// <param name="section">section to read from</param>
        /// <param name="key">key to read</param>
        /// <param name="fallback">value to use if either section or key do not exist</param>
        /// <returns></returns>
        public string GetValueWithFallback(string section, string key, string fallback)
        {
            if (this.backing.GetSectionByName(section.ToLower()) is null) return fallback;
            if (this.backing.GetSectionByName(section.ToLower()).GetKVPair(key.ToLower()) is null) return fallback;
            return this.backing.GetNamedProperty(section.ToLower(), key.ToLower());
        }

        // gets the raw file contents
        internal string GetFileRawContents()
        {
            return File.ReadAllText(this.FilePath, this.encoding);
        }

        internal void WriteFileContents(string content)
        {
            File.WriteAllText(this.FilePath, content, this.encoding);
        }
    }
}

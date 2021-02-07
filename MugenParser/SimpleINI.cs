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
    /// very simple INI parser and representation, generates a List of sections
    /// and allows access to named sections.
    /// can store multiple sections with identical names and multiple identical keys.
    /// </summary>
    public class SimpleINI
    {

        private readonly List<SimpleINISection> sections;

        /// <summary>
        /// initialize a SimpleINI and parse a file
        /// </summary>
        /// <param name="path">path of the INI file to parse</param>
        /// <param name="shouldCreate">indicates if a missing file should be created</param>
        public SimpleINI(string path, bool shouldCreate)
        {
            // check input - fail if does not exist + should not create
            if (!File.Exists(path) && !shouldCreate) throw new FileNotFoundException(string.Format("INI file specified by {0} does not exist!", path));
            // create file + init this file blank if we can create it
            if (!File.Exists(path))
            {
                File.Create(path);
                sections = new List<SimpleINISection>();
                return;
            }
            // set up section list
            this.sections = new List<SimpleINISection>();
            // read by lines
            var lines = File.ReadLines(path);
            // current section
            SimpleINISection currSection = new SimpleINISection();
            // iterate lines
            foreach (string line in lines)
            {
                // trim whitespace
                var lstrip = line.Trim();

                if (lstrip.StartsWith("["))
                {
                    // case for new section header
                    // save the current section if not null
                    if (currSection.Name != null)
                    {
                        sections.Add(currSection);
                    }
                    // read up until end of section header
                    string head = "";
                    bool headValid = false;
                    foreach (char c in lstrip.Substring(1).ToCharArray())
                    {
                        // found end, break as valid
                        if (c == ']')
                        {
                            headValid = true;
                            break;
                        }
                        // continue to build header
                        head += c;
                    }
                    // check header
                    if (!headValid) throw new FileFormatException(string.Format("INI file {0} has a malformed section header starting with {1}.", path, head));
                    // build new section
                    currSection = new SimpleINISection(head.ToLower(), this.sections.Count);
                }
                else
                {
                    // case for potential KV pair
                    // if current section is null, skip this line
                    if (currSection.Name == null)
                    {
                        continue;
                    }
                    // vars for storing kv
                    string key = "", value = "";
                    // convert to char array and use an index
                    char[] letters = lstrip.ToCharArray();
                    int index = 0;
                    // flags for completing read
                    bool isComment = false;
                    // read into the key until an = character
                    while (index < letters.Length)
                    {
                        // this letter
                        char c = letters[index++];
                        // read until either = or ; (swap to value or start comment)
                        if (c == ';')
                        {
                            // comment started, drop immediately
                            isComment = true;
                            break;
                        }
                        else if (c == '=')
                        {
                            // key finished, start reading value
                            break;
                        }

                        // append
                        key += c;
                    }
                    // drop if comment started
                    // no parsing errors emitted from KV pairs for now
                    if (isComment) continue;
                    // start reading value
                    while (index < letters.Length)
                    {
                        // this letter
                        char c = letters[index++];
                        // read until either EOL or ; (start comment)
                        if (c == ';')
                        {
                            // comment started, drop immediately
                            break;
                        }

                        // append
                        value += c;
                    }
                    // trim whitespace
                    key = key.Trim().ToLower();
                    value = value.Trim();
                    // insert if meaningful
                    if (key != "" && value != "")
                    {
                        currSection.AddKVPair(key, value);
                    }
                }
            }
            if (currSection.Name != null)
            {
                sections.Add(currSection);
            }
        }

        /// <summary>
        /// returns a copy of the backing list of sections.
        /// </summary>
        /// <returns></returns>
        public List<SimpleINISection> GetAllSections()
        {
            SimpleINISection[] sects = new SimpleINISection[this.sections.Count];
            this.sections.CopyTo(sects);
            return sects.ToList();
        }

        /// <summary>
        /// fetches the first section with a matching name
        /// </summary>
        /// <param name="name">section name to find</param>
        /// <returns>the contents of the section as a SimpleINISection</returns>
        public SimpleINISection GetSectionByName(string name)
        {
            foreach(SimpleINISection section in this.sections)
            {
                if (section.Name.Equals(name)) return section;
            }
            return null;
        }

        /// <summary>
        /// fetches a list of all sections with a matching name
        /// </summary>
        /// <param name="name">section name to find</param>
        /// <returns>the contents of the section as a list of SimpleINISection</returns>
        public List<SimpleINISection> GetAllSectionsByName(string name)
        {
            List<SimpleINISection> results = new List<SimpleINISection>();
            foreach (SimpleINISection section in this.sections)
            {
                if (section.Name.Equals(name)) results.Add(section);
            }
            return results;
        }


        /// <summary>
        /// gets the section at a specific index
        /// currently does NO range checking!!!
        /// </summary>
        /// <param name="position">position to read from</param>
        /// <returns>the SimpleINISection matching that position</returns>
        public SimpleINISection GetSectionByPosition(int position)
        {
            return this.sections[position];
        }


        /// <summary>
        /// fetches a key from a named section in the INI file.
        /// if more than once section or key by that name exists,
        /// it fetches the first.
        /// </summary>
        /// <param name="section">section name to find</param>
        /// <param name="key">key to find</param>
        /// <returns>value corresponding to the section and key</returns>
        public string GetNamedProperty(string section, string key)
        {
            return this.GetSectionByName(section).GetKVPair(key).Value;
        }
    }

    /// <summary>
    /// represents a single section in parsed INI, with a list of SimpleINIKV
    /// </summary>
    public class SimpleINISection
    {

        /// <summary>
        /// 
        /// </summary>
        public SimpleINISection() { }

        /// <summary>
        /// instantiate a section with no initial KV pairs
        /// </summary>
        /// <param name="name">section name</param>
        /// <param name="position">position in the SimpleINI list, for quick reference</param>
        public SimpleINISection(string name, int position)
        {
            this.Name = name;
            this.Position = position;
            this.Keys = new List<SimpleINIKV>();
        }


        /// <summary>
        /// adds a KV pair to the list
        /// </summary>
        /// <param name="key">key to add</param>
        /// <param name="value">corresponding value</param>
        public void AddKVPair(string key, string value)
        {
            this.Keys.Add(new SimpleINIKV(key, value));
        }


        /// <summary>
        /// fetches a single KV pair that matches the given key
        /// </summary>
        /// <param name="key">key to fetch</param>
        /// <returns>corresponding value</returns>
        public SimpleINIKV GetKVPair(string key)
        {
            if (!this.Keys.Any(kv => kv.Key.Equals(key))) return null;
            return this.Keys.Find(kv => kv.Key.Equals(key));
        }

        /// <summary>
        /// fetches all KV pairs that match the given key
        /// </summary>
        /// <param name="key">key to fetch</param>
        /// <returns>list of corresponding values</returns>
        public List<SimpleINIKV> GetAllKVPairs(string key)
        {
            return this.Keys.FindAll(kv => kv.Key.Equals(key));
        }

        /// <summary>
        /// name of the section (from section header)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// position of the section (count of sections from the start of the document)
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// all KV pairs in the section
        /// </summary>
        public List<SimpleINIKV> Keys { get; }
    }

    /// <summary>
    /// represents a single KV pair from the INI
    /// </summary>
    public class SimpleINIKV
    {
        /// <summary>
        /// represents a single KV pair
        /// </summary>
        /// <param name="key">key for the pair</param>
        /// <param name="value">corresponding value</param>
        public SimpleINIKV(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Key for the pair
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Value for the pair
        /// </summary>
        public string Value { get; }
    }
}

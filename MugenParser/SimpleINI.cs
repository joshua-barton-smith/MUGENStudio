using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    // very simple INI parser and representation, generates a List of sections
    // and allows access to named sections.
    // can store multiple sections with identical names and multiple identical keys.
    public class SimpleINI
    {

        private readonly List<SimpleINISection> sections;

        public SimpleINI(string path)
        {
            // check input
            if (!File.Exists(path)) throw new FileNotFoundException(string.Format("INI file specified by {0} does not exist!", path));
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
        }

        // fetches the first section with a matching name
        public SimpleINISection GetSectionByName(string name)
        {
            foreach(SimpleINISection section in this.sections)
            {
                if (section.Name.Equals(name)) return section;
            }
            return null;
        }

        // fetches a list of all sections with a matching name
        public List<SimpleINISection> GetAllSectionsByName(string name)
        {
            List<SimpleINISection> results = new List<SimpleINISection>();
            foreach (SimpleINISection section in this.sections)
            {
                if (section.Name.Equals(name)) results.Add(section);
            }
            return results;
        }

        // gets the section at a specific index
        // currently does NO range checking!!!
        public SimpleINISection GetSectionByPosition(int position)
        {
            return this.sections[position];
        }

        // fetches a key from a specific section in the INI file.
        // if more than once section or key by that name exists,
        // it fetches the first.
        public string GetNamedProperty(string section, string key)
        {
            return this.GetSectionByName(section).GetKVPair(key).Value;
        }
    }

    // represents a single section in parsed INI, with a list of SimpleINIKV
    public class SimpleINISection
    {
        
        // null constructor for checking
        public SimpleINISection()
        {

        }

        public SimpleINISection(string name, int position)
        {
            this.Name = name;
            this.Position = position;
            this.Keys = new List<SimpleINIKV>();
        }

        // adds a KV pair to the list
        public void AddKVPair(string key, string value)
        {
            this.Keys.Add(new SimpleINIKV(key, value));
        }

        // fetches a single KV pair that matches the given key
        public SimpleINIKV GetKVPair(string key)
        {
            return this.Keys.Find(kv => kv.Key.Equals(key));
        }

        // fetches all KV pairs that match the given key
        public List<SimpleINIKV> GetAllKVPairs(string key)
        {
            return this.Keys.FindAll(kv => kv.Key.Equals(key));
        }

        // name of the section (from section header)
        public string Name { get; }
        // position of the section (count of sections from the start of the document)
        public int Position { get; }
        // all KV pairs in the section
        public List<SimpleINIKV> Keys { get; }
    }

    // represents a single KV pair from the INI
    public class SimpleINIKV
    {
        public SimpleINIKV(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}

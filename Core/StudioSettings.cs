using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MUGENStudio
{
    public class StudioSettings
    {
        
        // list of previously opened projects
        public List<(string, string)> PreviousProjects { get; }

        // internal xml document representation
        private XDocument settingsDoc;
        // filepath when settings were initialized
        private readonly string xmlFile;

        public StudioSettings(string xmlFile)
        {
            // save settings file name
            this.xmlFile = xmlFile;

            // create or load settings file
            if (!File.Exists(this.xmlFile))
            {
                this.settingsDoc = this.GenerateBaseSettingsDoc();
            }
            else
            {
                this.settingsDoc = XDocument.Load(this.xmlFile);
            }

            // load all previous projects
            this.PreviousProjects = new List<(string, string)>();
            foreach (XElement item in this.settingsDoc.Element("mugenSettings").Element("previousProjects").Elements())
            {
                // only read project elements
                if (item.Name.LocalName.Equals("project")) this.PreviousProjects.Add((item.Attribute("defFile").Value, item.Attribute("charName").Value));
            }
        }

        // Adds a project to the list of previous projects,
        // and updates the charName if the project was already included and if it has changed  
        public void AddPreviousProject(string defFile, string charName)
        {
            // iterate through and remove if the defFile matches (charName need not match since it can update)
            foreach ((string, string) project in this.PreviousProjects.ToList())
            {
                if (project.Item1.Equals(defFile))
                {
                    this.PreviousProjects.Remove(project);
                }
            }
            // add to the beginning
            this.PreviousProjects.Insert(0, (defFile, charName));
        }

        // writes current settings state out to the settings file
        public void SaveSettings()
        {
            // reset the document
            this.settingsDoc = this.GenerateBaseSettingsDoc();

            // write past projects out
            foreach ((string, string) project in this.PreviousProjects)
            {
                this.settingsDoc.Element("mugenSettings").Element("previousProjects").Add(
                    new XElement("project",
                        new XAttribute("defFile", project.Item1),
                        new XAttribute("charName", project.Item2)
                    )
                );
            }

            // save the document
            this.settingsDoc.Save(this.xmlFile);
        }

        // used as a template for settings, core settings changes go here!!
        private XDocument GenerateBaseSettingsDoc()
        {
            return new XDocument(
                new XDeclaration("1.0", Encoding.UTF8.HeaderName, String.Empty),
                new XElement("mugenSettings",
                    new XElement("previousProjects")
                )
            );
        }
    }
}

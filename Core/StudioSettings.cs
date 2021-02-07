using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MUGENStudio.MugenParser.Validation;

namespace MUGENStudio.Core
{
    /// <summary>
    /// contains settings for the editor and environment
    /// </summary>
    public class StudioSettings
    {

        /// <summary>
        /// list of previously opened projects
        /// </summary>
        public List<(string, string)> PreviousProjects { get; }
        /// <summary>
        /// is code completion enabled?
        /// </summary>
        public bool EnableCodeCompletion { get; }

        /// <summary>
        /// lowest severity of errors to show
        /// </summary>
        public ValidationSeverity ShowSeverity { get; }

        // internal xml document representation
        private XDocument settingsDoc;
        // filepath when settings were initialized
        private readonly string xmlFile;

        /// <summary>
        /// loads settings from the path specified
        /// </summary>
        /// <param name="xmlFile">path to settings file</param>
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

            // load the code completion flag
            if (this.settingsDoc.Element("mugenSettings").Element("codeCompletion") != null)
                this.EnableCodeCompletion = Boolean.Parse(this.settingsDoc.Element("mugenSettings").Element("codeCompletion").Attribute("value").Value);
            else
                this.EnableCodeCompletion = true;

            if (this.settingsDoc.Element("mugenSettings").Element("syntaxChecking") != null)
                this.ShowSeverity = (ValidationSeverity) Int32.Parse(this.settingsDoc.Element("mugenSettings").Element("syntaxChecking").Attribute("severity").Value);
            else
                this.ShowSeverity = ValidationSeverity.WARNING;
        }

        /// <summary>
        /// Adds a project to the list of previous projects,
        /// and updates the charName if the project was already included and if it has changed  
        /// </summary>
        /// <param name="defFile">path to the DEF file for the project</param>
        /// <param name="charName">name of the character loaded from this DEF</param>
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
            // reduce to 5 projects for now
            if (this.PreviousProjects.Count > 5)
            {
                this.PreviousProjects.RemoveRange(5, this.PreviousProjects.Count - 5);
            }
        }

        /// <summary>
        /// writes current settings state out to the settings file
        /// </summary>
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

            this.settingsDoc.Element("mugenSettings").Element("codeCompletion").Attribute("value").SetValue(EnableCodeCompletion);

            // save the document
            this.settingsDoc.Save(this.xmlFile);
        }

        // used as a template for settings, core settings changes go here!!
        private XDocument GenerateBaseSettingsDoc()
        {
            return new XDocument(
                new XDeclaration("1.0", Encoding.UTF8.HeaderName, String.Empty),
                new XElement("mugenSettings",
                    new XElement("previousProjects"),
                    new XElement("codeCompletion",
                        new XAttribute("value", "true")
                    ),
                    new XElement("syntaxChecking",
                        new XAttribute("severity", (int) ValidationSeverity.WARNING)
                    )
                )
            );
        }
    }
}

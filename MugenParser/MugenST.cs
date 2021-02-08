using MUGENStudio.Core;
using MUGENStudio.MugenParser.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser
{
    /// <summary>
    /// represents an ST file for a character
    /// </summary>
    public class MugenST : MugenINI
    {
        // map of statedef numbers to their position in the backing INI
        private Dictionary<int, int> statedefLocator;
        /// <summary>
        /// parses + processes an input ST file
        /// </summary>
        /// <param name="path">ST file to process</param>
        /// <param name="fileKey">name of the file from the DEF</param>
        public MugenST(string path, string fileKey) : base(path, fileKey) { }

        public override void Validate(MugenDefinition project)
        {
            this.Validate(project, "st");
        }

        public override void Validate(MugenDefinition project, string stName)
        {
            this.Validate(project, stName, false);
        }

        public void Validate(MugenDefinition project, string stName, bool isCommon)
        {
            this.statedefLocator = new Dictionary<int, int>();
            // convenience...
            StateValidator sv = Globals.stateValidator;
            // iterate over all sections in the file
            int currentStatedef = -4; // invalid
            foreach (SimpleINISection section in this.backing.GetAllSections())
            {
                // build the statedef map
                if (section.Name.StartsWith("statedef"))
                {
                    // get the statedef num
                    string[] statedefSplit = section.Name.Split(' ');
                    currentStatedef = Int32.Parse(statedefSplit[1]);
                    // validate statedef...
                    // 1. check if statedef already exists, if not common states
                    if (project.StatedefMapping.ContainsKey(currentStatedef) && !isCommon)
                    {
                        ValidationError err = new ValidationError(string.Format("Statedef {0} in file {1} already defined in file {2}!", currentStatedef, this.FileKey, project.StateFiles[project.StatedefMapping[currentStatedef]].FileKey), ValidationSeverity.INFO);
                        project.ValidationErrors.Add(err);
                    }
                    else if (!project.StatedefMapping.ContainsKey(currentStatedef))
                    {
                        project.StatedefMapping.Add(currentStatedef, stName);
                    }
                    // 2. check statedef params for validity
                    foreach(var kv in section.Keys)
                    {
                        if(!sv.GetStatedefProperties().Any(x => x.Name.Equals(kv.Key)))
                        {
                            ValidationError err = new ValidationError(string.Format("Statedef {0} in file {1} has invalid parameter {2}!", currentStatedef, this.FileKey, kv.Key), ValidationSeverity.WARNING);
                            project.ValidationErrors.Add(err);
                        }
                    }
                    // 3. check if required params are present
                    foreach (var prop in sv.GetStatedefProperties())
                    {
                        if (!prop.Optional && !section.Keys.Any(x => x.Key.Equals(prop.Name)))
                        {
                            ValidationError err = new ValidationError(string.Format("Statedef {0} in file {1} is missing required parameter {2}!", currentStatedef, this.FileKey, prop.Name), ValidationSeverity.WARNING);
                            project.ValidationErrors.Add(err);
                        }
                    }
                    // 4. check enum values for enum types
                    foreach (var prop in sv.GetStatedefProperties())
                    {
                        if (prop.Types.First().Equals(ValidProperty.PropType.Enum) && prop.Types.Count == 1 && section.Keys.Any(x => x.Key.Equals(prop.Name)))
                        {
                            var kv = section.Keys.First(x => x.Key.Equals(prop.Name));
                            if (!prop.EnumOpts.Contains(kv.Value))
                            {
                                ValidationError err = new ValidationError(string.Format("Statedef {0} in file {1} has invalid option {2} for property {3}!", currentStatedef, this.FileKey, kv.Value, prop.Name), ValidationSeverity.WARNING);
                                project.ValidationErrors.Add(err);
                            }
                        }
                    }
                    // TODO: validate RHS type

                    statedefLocator.Add(currentStatedef, section.Position);
                }
                else if (section.Name.StartsWith("state") && currentStatedef != -4)
                {
                    // validate state...
                    // 1. check if statedef number matches (debug sev, very minor)
                    string[] stateSplit1 = section.Name.Split(' ');
                    string[] stateSplit = { "" };
                    if (stateSplit1.Length > 1) stateSplit = stateSplit1[1].Split(',');
                    if (stateSplit[0].Trim() == "")
                    {
                        // case for no number, debug sev
                        ValidationError err = new ValidationError(string.Format("State `{0}` has number omitted for statedef group {1}!", section.Name, currentStatedef), ValidationSeverity.DEBUG);
                        project.ValidationErrors.Add(err);
                    }
                    else if (Int32.Parse(stateSplit[0]) != currentStatedef)
                    {
                        // case for wrong number, debug sev
                        ValidationError err = new ValidationError(string.Format("State `{0}` not numbered correctly for statedef group {1}!", section.Name, currentStatedef), ValidationSeverity.DEBUG);
                        project.ValidationErrors.Add(err);
                    }
                    // 2. check if state is missing trailing space after `state` (error sev, will fail to run)
                    if (stateSplit1.Length == 1)
                    {
                        ValidationError err = new ValidationError(string.Format("State `{0}` in statedef group {1} has no trailing space!", section.Name, currentStatedef), ValidationSeverity.ERROR);
                        project.ValidationErrors.Add(err);
                    }
                    // 3. if `type` exists, check value for validity
                    if (section.Keys.Any(x => x.Key.Equals("type", StringComparison.InvariantCultureIgnoreCase)) && sv.GetControllerProperties(section.GetKVPair("type").Value).Count > 0)
                    {
                        List<string> typeEnum = sv.GetStateProperties().First(x => x.Name.Equals("type", StringComparison.InvariantCultureIgnoreCase)).EnumOpts;
                        if (!typeEnum.Any(x => x.Equals(section.GetKVPair("type").Value, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            ValidationError err = new ValidationError(string.Format("State `{0}` in statedef group {1} has invalid type {2}!", section.Name, currentStatedef, section.GetKVPair("type").Value), ValidationSeverity.ERROR);
                            project.ValidationErrors.Add(err);
                        }
                    }
                    // 4. check state+controller params for validity
                    // INFO sev even though it should probably be WARNING due to users commonly making a mess of this
                    foreach (var kv in section.Keys)
                    {
                        List<ValidProperty> allProps = new List<ValidProperty>();
                        allProps = allProps.Concat(sv.GetStateProperties()).ToList();
                        if (section.Keys.Any(x => x.Key.Equals("type", StringComparison.InvariantCultureIgnoreCase)) && sv.GetControllerProperties(section.GetKVPair("type").Value).Count > 0)
                            allProps = allProps.Concat(sv.GetControllerProperties(section.GetKVPair("type").Value)).ToList();

                        if (!allProps.Any(x => x.Name.Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase)) && !kv.Key.StartsWith("trigger", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // TODO: fix for var notation
                            ValidationError err = new ValidationError(string.Format("State `{0}` with type {3} in statedef group {1} has invalid parameter {2}!", section.Name, currentStatedef, kv.Key, section.GetKVPair("type").Value), ValidationSeverity.INFO);
                            project.ValidationErrors.Add(err);
                        }
                    }
                } else if(section.Name.StartsWith("state"))
                {
                    // ungrouped state controller
                    ValidationError err = new ValidationError(string.Format("State `{0}` found outside of statedef group!", section.Name), ValidationSeverity.INFO);
                    project.ValidationErrors.Add(err);
                }
            }
        }
    }
}

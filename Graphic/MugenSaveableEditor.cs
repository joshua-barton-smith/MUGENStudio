using ICSharpCode.AvalonEdit;
using MUGENStudio.MugenParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MUGENStudio.Graphic
{
    /// <summary>
    /// editor tab that can be set dirty + triggered to save
    /// </summary>
    public class MugenSaveableEditor : TabItem
    {
        /// <summary>
        /// indicates whether the tab is edited + ready for saving
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// backing file for this tab
        /// </summary>
        public MugenINI BackingFile { get; set; }
        public int ProjectID { get; set; }
        public int StartLine { get; set; }

        /// <summary>
        /// updates title based on whether the file is modified or not.
        /// </summary>
        public void UpdateTitle()
        {
            if (this.IsDirty) this.Header = string.Format("* {0}", this.BackingFile.FileKey);
            else this.Header = this.BackingFile.FileKey;
        }

        /// <summary>
        /// saves the file if it has been modified since last save.
        /// </summary>
        public void SaveIfModified()
        {
            // can't save if nothing has changed!
            if (!this.IsDirty) return;
            // get the content
            TextEditor editor = this.Content as TextEditor;
            string content = editor.Text;
            // save contents
            this.BackingFile.WriteFileContents(content);
            this.BackingFile.Reparse();
            // update title
            this.IsDirty = false;
            this.UpdateTitle();
        }
    }
}

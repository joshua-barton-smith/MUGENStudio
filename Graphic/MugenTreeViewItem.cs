using MUGENStudio.MugenParser;
using System;
using System.Windows.Controls;

namespace MUGENStudio.Graphic
{
    /// <summary>
    /// wrapper for TreeViewItem to pass the backing MugenINI through
    /// </summary>
    public class MugenTreeViewItem : TreeViewItem
    {
        /// <summary>
        /// the backing MugenINI object for this tree item
        /// </summary>
        public MugenINI BackingFile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MugenTreeViewItem() { }

        // gets the file contents of the MugenINI backing this item
        internal string GetBackingFileContents()
        {
            return BackingFile.GetFileRawContents();
        }
    }
}

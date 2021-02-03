using MUGENStudio.MugenParser;
using System;
using System.Windows.Controls;

namespace MUGENStudio.Graphic
{
    public class MugenTreeViewItem : TreeViewItem
    {
        public MugenINI BackingFile { get; set; }
        public MugenTreeViewItem()
        {
        }

        internal string GetBackingFileContents()
        {
            return BackingFile.GetFileRawContents();
        }
    }
}

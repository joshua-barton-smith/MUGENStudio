using ICSharpCode.AvalonEdit;
using MUGENStudio.Core;
using MUGENStudio.MugenParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MUGENStudio.Graphic
{
    /// <summary>
    /// Interaction logic for CodeEditorWindow.xaml
    /// </summary>
    public partial class CodeEditorWindow : Window
    {
        /// <summary>
        /// constructs the editor window
        /// </summary>
        public CodeEditorWindow()
        {
            InitializeComponent();
            // set title
            if (Globals.projects.Count > 1)
            {
                codeWindow.Title = "Code Editor - Multiple Projects";
            } else
            {
                codeWindow.Title = string.Format("Code Editor - {0}", Globals.projects.First().DisplayName);
            }
            // load files into the tree
            ReloadTree();
        }

        public void ReloadTree()
        {
            // empty the tree
            projectTree.Items.Clear();
            // add project files to the display
            foreach (MugenDefinition project in Globals.projects)
            {
                // root element
                TreeViewItem proj = new TreeViewItem()
                {
                    Header = string.Format("{0} ({1})", project.DisplayName, project.FileKey)
                };

                // single leaves: CMD, CNS, SPRITE, ANIM, SOUND
                MugenTreeViewItem cmd = new MugenTreeViewItem()
                {
                    Header = string.Format("CMD {0}", project.CmdFile.FileKey),
                    BackingFile = project.CmdFile
                };
                MugenTreeViewItem cns = new MugenTreeViewItem()
                {
                    Header = string.Format("CNS {0}", project.CnsFile.FileKey),
                    BackingFile = project.CnsFile
                };
                cmd.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(OpenOrFocusEditorTab));
                cns.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(OpenOrFocusEditorTab));
                proj.Items.Add(cmd);
                proj.Items.Add(cns);

                // header for state files
                TreeViewItem st = new TreeViewItem()
                {
                    Header = "State Files"
                };
                // add commons
                MugenTreeViewItem common = new MugenTreeViewItem()
                {
                    Header = string.Format("COMMON {0}", project.CommonFile.FileKey),
                    BackingFile = project.CommonFile
                };
                common.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(OpenOrFocusEditorTab));
                st.Items.Add(common);
                // add all statefiles
                foreach (KeyValuePair<string, MugenST> states in project.StateFiles)
                {
                    MugenTreeViewItem tmp = new MugenTreeViewItem()
                    {
                        Header = string.Format("{0} {1}", states.Key.ToUpper(), states.Value.FileKey),
                        BackingFile = states.Value
                    };
                    tmp.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(OpenOrFocusEditorTab));
                    st.Items.Add(tmp);
                }
                proj.Items.Add(st);

                // add overall project
                projectTree.Items.Add(proj);
            }
        }

        private void OpenOrFocusEditorTab(object sender, MouseButtonEventArgs e)
        {
            MugenTreeViewItem item = sender as MugenTreeViewItem;
            if (item != null)
            {
                bool exists = false;
                foreach (TabItem tab in editorTabs.Items)
                {
                    if (tab.Header.ToString().Equals(item.BackingFile.FileKey))
                    {
                        tab.Focus();
                        exists = true;
                    }
                }
                if (!exists)
                {
                    ContextMenu tabContext = new ContextMenu();
                    MenuItem closeTab = new MenuItem()
                    {
                        Header = "Close Tab"
                    };
                    closeTab.Click += new RoutedEventHandler(CloseTab);
                    tabContext.Items.Add(closeTab);

                    // avalon editor
                    TextEditor editor = new TextEditor()
                    {
                        Text = item.GetBackingFileContents()
                    };

                    TabItem newItem = new TabItem()
                    {
                        Header = item.BackingFile.FileKey,
                        ContextMenu = tabContext,
                        Content = editor
                    };
                    editorTabs.Items.Add(newItem);
                    newItem.Focus();
                }
            }
        }

        private void CloseTab(object sender, RoutedEventArgs e)
        {
            // wicked unsafe, please check for null later
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.Parent as ContextMenu;
            TabItem tab = menu.PlacementTarget as TabItem;
            // we should have a confirmation popup if contents havent been saved
            editorTabs.Items.Remove(tab);
            // refocus
            if (editorTabs.Items.Count > 0) ((TabItem)editorTabs.Items.GetItemAt(0)).Focus();
        }

        private void OpenLoadProjectView(object sender, RoutedEventArgs e)
        {
            SelectProjectWindow pw = new SelectProjectWindow();
            pw.Show();
        }
    }
}

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
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
using System.Xml;

namespace MUGENStudio.Graphic
{
    /// <summary>
    /// Interaction logic for CodeEditorWindow.xaml
    /// </summary>
    public partial class CodeEditorWindow : Window
    {
        private readonly LinkedList<TabItem> closedTabs;
        /// <summary>
        /// constructs the editor window
        /// </summary>
        public CodeEditorWindow()
        {
            InitializeComponent();
            // initialize tab history
            closedTabs = new LinkedList<TabItem>();
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

        /// <summary>
        /// clears and repopulates the project browsing tree
        /// </summary>
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

        // handler to open a new editor tab
        private void OpenOrFocusEditorTab(object sender, MouseButtonEventArgs e)
        {
            // verify this is actually an openable item
            if (sender is MugenTreeViewItem item)
            {
                // if the item exists, just focus it
                bool exists = false;
                foreach (TabItem tab in editorTabs.Items)
                {
                    // confirm existence
                    if (tab.Header.ToString().Equals(item.BackingFile.FileKey))
                    {
                        tab.Focus();
                        exists = true;
                    }
                }
                // otherwise, create a new tab
                if (!exists)
                {
                    // contextmenu to easily close the tab
                    ContextMenu tabContext = new ContextMenu();
                    MenuItem closeTab = new MenuItem()
                    {
                        Header = "Close Tab"
                    };
                    closeTab.Click += new RoutedEventHandler(CloseTab);
                    tabContext.Items.Add(closeTab);

                    // initialize the INI highlighter
                    XmlReader iniReader = XmlReader.Create("TextResources/Highlighters/INIHighlight.xshd");

                    // avalon editor containing file contents + syntax highlighter
                    TextEditor editor = new TextEditor()
                    {
                        Text = item.GetBackingFileContents(),
                        SyntaxHighlighting = HighlightingLoader.Load(iniReader, HighlightingManager.Instance)
                    };

                    // insert the tab with the editor as its contents
                    TabItem newItem = new TabItem()
                    {
                        Header = item.BackingFile.FileKey,
                        ContextMenu = tabContext,
                        Content = editor
                    };
                    // add keyboard handler for shortcuts
                    newItem.AddHandler(KeyDownEvent, new KeyEventHandler(TabHandleKeys));
                    editorTabs.Items.Add(newItem);
                    newItem.Focus();
                }
            }
        }

        // handle keyboard input (for shortcuts)
        private void TabHandleKeys(object sender, KeyEventArgs e)
        {
            TabItem tab = sender as TabItem;
            if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.CloseTabSafely(tab);
            }
        }

        // reopens a recently-closed tab
        private void ReopenLastClosedTab()
        {
            if (closedTabs.Count == 0) return;
            TabItem last = closedTabs.First();
            bool exists = false;
            foreach (TabItem tab in editorTabs.Items)
            {
                // confirm existence
                if (tab.Header.ToString().Equals(last.Header.ToString()))
                {
                    tab.Focus();
                    exists = true;
                }
            }
            if (!exists)
            {
                editorTabs.Items.Add(last);
                last.Focus();
            }
            closedTabs.RemoveFirst();
        }

        // handler to close a tab
        private void CloseTab(object sender, RoutedEventArgs e)
        {
            // wicked unsafe, please check for null later
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.Parent as ContextMenu;
            TabItem tab = menu.PlacementTarget as TabItem;
            this.CloseTabSafely(tab);
        }

        // opens a new 'select project' window for multi-project edit
        private void OpenLoadProjectView(object sender, RoutedEventArgs e)
        {
            SelectProjectWindow pw = new SelectProjectWindow();
            pw.Show();
        }

        // safely closes a tab and adds it to history
        private void CloseTabSafely(TabItem tab)
        {
            // we should have a confirmation popup if contents havent been saved
            editorTabs.Items.Remove(tab);
            // push to recently-closed stack
            closedTabs.AddFirst(tab);
            // cap at 20
            while (closedTabs.Count > 20)
            {
                closedTabs.RemoveLast();
            }
            // refocus
            if (editorTabs.Items.Count > 0) ((TabItem)editorTabs.Items.GetItemAt(0)).Focus();
        }

        private void GlobalKeyEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.ReopenLastClosedTab();
            }
        }
    }
}

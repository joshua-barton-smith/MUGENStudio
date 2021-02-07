using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MUGENStudio.Core;
using MUGENStudio.MugenParser;
using MUGENStudio.MugenParser.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private CompletionWindow completionWindow;
        private bool tabWasClosed = false;
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

                    // setup for code completion
                    if(Globals.settingsSingleton.EnableCodeCompletion) editor.TextArea.TextEntered += HandleEditorAutoCompletion;
                    if (Globals.settingsSingleton.EnableCodeCompletion) editor.TextArea.TextEntering += HandleEditorAutoCompletionSubmit;

                    // insert the tab with the editor as its contents
                    MugenSaveableEditor newItem = new MugenSaveableEditor()
                    {
                        Header = item.BackingFile.FileKey,
                        BackingFile = item.BackingFile,
                        ContextMenu = tabContext,
                        Content = editor
                    };
                    // add keyboard handler for shortcuts
                    newItem.AddHandler(KeyDownEvent, new KeyEventHandler(TabHandleKeysDown));
                    newItem.AddHandler(KeyUpEvent, new KeyEventHandler(TabHandleKeysUp));
                    editorTabs.Items.Add(newItem);
                    newItem.Focus();
                }
            }
        }

        private void HandleEditorAutoCompletionSubmit(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (e.Text[0] == ' ')
                {
                    // space inserts
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        // autocompletion
        private void HandleEditorAutoCompletion(object sender, TextCompositionEventArgs e)
        {
            if (completionWindow != null) return;
            if (tabWasClosed)
            {
                tabWasClosed = false;
                return;
            }
            // fetch the input so far
            string input = e.Text;
            // fetch the tab text up to cursor position
            TabItem currentTab = editorTabs.Items[editorTabs.SelectedIndex] as TabItem;
            TextEditor editor = currentTab.Content as TextEditor;
            string prevText = editor.Text.Substring(0, editor.CaretOffset);
            // determine how we want to perform code completion
            // scenarios:
            // 1. autocomplete a section header
            // 2. autocomplete a property LHS
            // 3. autocomplete a controller type RHS
            // in all of these we must ignore comments
            int lineStart = prevText.LastIndexOf('\n') + 1;
            string currLine = prevText.Substring(lineStart, prevText.Length - lineStart);
            // move backwards on the line to determine the type
            int end = currLine.Length - 1;
            char[] lineChars = currLine.ToCharArray();
            MugenCompletionItem.CompletionMode mode = MugenCompletionItem.CompletionMode.NONE; // switch for determining completion later
            int tmprhs = 0;
            int tmpeq = 0;
            int start = 0; // used for updating code completion offsets
            // iterate chars backwards
            for( ; end >= 0; end--)
            {
                // current char
                char curr = lineChars[end];
                // if in no-found mode
                if (mode == MugenCompletionItem.CompletionMode.NONE)
                {
                    // if find the end of a header
                    if (curr == ']')
                    {
                        mode = MugenCompletionItem.CompletionMode.ABORT; // swap to no-match mode
                        continue;
                    }
                    // if find the start of a header
                    if (curr == '[')
                    {
                        mode = MugenCompletionItem.CompletionMode.SECTION_HEADER; // swap to header mode
                        start = end + 1; // get start of the section header offset
                    }
                    // if find an = sign
                    if (curr == '=')
                    {
                        mode = MugenCompletionItem.CompletionMode.PROPERTY_RHS; // swap to RHS mode
                        tmprhs = end;
                        tmpeq = end;
                        while (lineChars[tmprhs] != '\n' && tmprhs > 0)
                        {
                            tmprhs--;
                        }
                    }
                }
                // if find a comment
                if (curr == ';')
                {
                    // break regardless of previous mode
                    mode = MugenCompletionItem.CompletionMode.ABORT;
                    break;
                }
            }
            if (mode == MugenCompletionItem.CompletionMode.NONE) mode = MugenCompletionItem.CompletionMode.PROPERTY_LHS; // LHS if no other mode matched
            // header mode: provide valid list of headers
            // hardcoded for now... should be based on filetype
            if (mode == MugenCompletionItem.CompletionMode.SECTION_HEADER)
            {
                // initialize completion window
                completionWindow = new CompletionWindow(editor.TextArea)
                {
                    StartOffset = start + lineStart,
                    CloseWhenCaretAtBeginning = false
                };
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MugenCompletionItem("Statedef ", mode, "Defines a new state."));
                data.Add(new MugenCompletionItem("State ", mode, "Defines a new state controller."));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (mode == MugenCompletionItem.CompletionMode.PROPERTY_LHS)
            {
                // for a LHS property, we need to identify the current section.
                // if the section is a Statedef, we show Statedef props.
                // if the section is a State, we add the State props, and also any Controller props
                // if we identify a Controller type.
                string section = this.FindSectionForPos(lineStart);
                switch (section.ToLower())
                {
                    case "statedef":
                        // show only statedef-relevant props
                        List<ValidProperty> defProps = Globals.stateValidator.GetStatedefProperties();
                        completionWindow = new CompletionWindow(editor.TextArea)
                        {
                            StartOffset = start + lineStart,
                            CloseWhenCaretAtBeginning = false
                        };
                        // build list of properties
                        IList<ICompletionData> dataDef = completionWindow.CompletionList.CompletionData;
                        foreach (ValidProperty prop in defProps)
                        {
                            dataDef.Add(new MugenCompletionItem(prop.Name, mode, prop.GetPropertyDesc()));
                        }
                        completionWindow.Show();
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                        break;
                    case "state":
                        // show only state-relevant props
                        List<ValidProperty> stateProps = Globals.stateValidator.GetStateProperties();
                        // fetch sctrl-relevant props if type can be identified, otherwise this will be empty
                        List<ValidProperty> sctrlProps = Globals.stateValidator.GetControllerProperties(this.FindSctrlForPos(lineStart).ToLower());
                        // remove `type` from state props if we found an sctrl (i.e. is already defined)
                        if (sctrlProps.Count > 0)
                        {
                            stateProps.Remove(stateProps.Find(x => x.Name.Equals("type")));
                        }
                        completionWindow = new CompletionWindow(editor.TextArea)
                        {
                            StartOffset = start + lineStart,
                            CloseWhenCaretAtBeginning = false
                        };
                        // build list of properties
                        IList<ICompletionData> dataState = completionWindow.CompletionList.CompletionData;
                        foreach (ValidProperty prop in stateProps)
                        {
                            dataState.Add(new MugenCompletionItem(prop.Name, mode, prop.GetPropertyDesc()));
                        }
                        foreach (ValidProperty prop in sctrlProps)
                        {
                            // todo, add sctrl descs when available
                            dataState.Add(new MugenCompletionItem(prop.Name, mode, prop.GetPropertyDesc()));
                        }
                        completionWindow.Show();
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                        break;
                }
            }
            else if (mode == MugenCompletionItem.CompletionMode.PROPERTY_RHS)
            {
                // need to get the LHS in order to determine the property we are filling 
                // split on equals for trigger lines
                string propName = currLine.Substring(tmprhs, tmpeq).Split('=')[0].Trim();
                string section = this.FindSectionForPos(lineStart);
                List<ValidProperty> defProps = new List<ValidProperty>();
                switch (section.ToLower())
                {
                    case "statedef":
                        defProps = Globals.stateValidator.GetStatedefProperties();
                        break;
                    case "state":
                        defProps = Globals.stateValidator.GetStateProperties();
                        defProps = defProps.Concat(Globals.stateValidator.GetControllerProperties(this.FindSctrlForPos(lineStart).ToLower())).ToList();
                        break;
                }
                if(defProps.Any(x => x.Name.ToLower().Equals(propName)))
                {
                    ValidProperty finProp = defProps.First(x => x.Name.ToLower().Equals(propName));
                    // check for enum in first value
                    if(finProp.Types.First() == ValidProperty.PropType.Enum || finProp.Types.First() == ValidProperty.PropType.MultiEnum)
                    {
                        // populate with enum values
                        completionWindow = new CompletionWindow(editor.TextArea)
                        {
                            StartOffset = editor.CaretOffset,
                            CloseWhenCaretAtBeginning = false
                        };
                        // build list of properties
                        IList<ICompletionData> dataState = completionWindow.CompletionList.CompletionData;
                        foreach (string eval in finProp.EnumOpts)
                        {
                            // todo, add descs when available?
                            dataState.Add(new MugenCompletionItem(eval, finProp.Types.First() == ValidProperty.PropType.MultiEnum ? MugenCompletionItem.CompletionMode.PROPERTY_RHS_MULTIENUM : MugenCompletionItem.CompletionMode.PROPERTY_RHS, ""));
                        }
                        completionWindow.Show();
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                    }
                }
            }
        }

        // finds the section name for a given position in the editor
        private string FindSectionForPos(int lineStart)
        {
            // get the relevant text
            TabItem currentTab = editorTabs.Items[editorTabs.SelectedIndex] as TabItem;
            TextEditor editor = currentTab.Content as TextEditor;
            string prevText = editor.Text.Substring(0, lineStart);
            // work backwards
            char[] allChar = prevText.ToCharArray();
            int end = allChar.Length - 1;
            string sect = "";
            bool sectOpen = false;
            for (; end >= 0; end--)
            {
                char curr = allChar[end];
                if (curr == ']' && sect.Equals(""))
                {
                    // section closer, start building section name
                    sectOpen = true;
                }
                else if (curr == '[' && sectOpen)
                {
                    // section opener, stop building section name
                    sectOpen = false;
                }
                else if (curr == ';')
                {
                    // comment, abort build
                    sectOpen = false;
                    sect = "";
                }
                else if (curr == '\n' && !sect.Equals(""))
                {
                    // section found, break
                    break;
                } else if (sectOpen)
                {
                    sect = curr + sect;
                }
            }
            if (!sectOpen && !sect.Equals(""))
            {
                // if a section name was found AND the section header was closed properly
                // return the first word in the header.
                return sect.Split(' ')[0];
            }
            return "";
        }

        private string FindSctrlForPos(int lineStart)
        {
            // get the relevant text
            TabItem currentTab = editorTabs.Items[editorTabs.SelectedIndex] as TabItem;
            TextEditor editor = currentTab.Content as TextEditor;
            string prevText = editor.Text.Substring(0, lineStart);
            // work backwards
            char[] allChar = prevText.ToCharArray();
            int end = allChar.Length - 1;
            // similar code to finding section since we care about finding `type =` before the section header.
            string sect = "";
            bool sectOpen = false;
            // used to build the type
            string type = "";
            bool typeOpen = false;
            for (; end >= 0; end--)
            {
                char curr = allChar[end];
                if (curr == ']' && sect.Equals(""))
                {
                    // section closer, start building section name
                    sectOpen = true;
                }
                else if (curr == '[' && sectOpen)
                {
                    // section opener, stop building section name
                    sectOpen = false;
                }
                else if (curr == ';')
                {
                    // comment, abort build
                    sectOpen = false;
                    sect = "";
                }
                else if (curr == '\n' && !sect.Equals(""))
                {
                    // section found, break
                    break;
                }
                else if (curr == '\n' && !type.Trim().ToLower().StartsWith("type") && !type.Equals(""))
                {
                    // failed to match type
                    typeOpen = false;
                    type = "";
                }
                else if (curr == '\n' && type.Trim().ToLower().StartsWith("type"))
                {
                    // type found, break
                    break;
                }
                else if (curr == '\r' && type.Equals(""))
                {
                    // newline, start building type string
                    typeOpen = true;
                }

                if (sectOpen && curr != ']')
                {
                    sect = curr + sect;
                }
                if (typeOpen && curr != '\n' && curr != '\r')
                {
                    type = curr + type;
                }
            }
            if (!type.Equals("") && type.Trim().ToLower().StartsWith("type"))
            {
                return type.Split('=')[1].Trim();
            }
            return "";
        }

        // handle keyboard input (for shortcuts)
        private void TabHandleKeysDown(object sender, KeyEventArgs e)
        {
            MugenSaveableEditor tab = sender as MugenSaveableEditor;
            if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.CloseTabSafely(tab);
            }
            else if (e.Key == Key.S && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                tab.SaveIfModified();
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Space))
            {
                // update saveable state
                tab.IsDirty = true;
                tab.UpdateTitle();
            }
        }

        // because backspace doesn't trigger KeysDown, somehow
        private void TabHandleKeysUp(object sender, KeyEventArgs e)
        {
            MugenSaveableEditor tab = sender as MugenSaveableEditor;
            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Back)
            {
                // update saveable state
                tab.IsDirty = true;
                tab.UpdateTitle();
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
            MugenSaveableEditor tab = menu.PlacementTarget as MugenSaveableEditor;
            this.CloseTabSafely(tab);
        }

        // opens a new 'select project' window for multi-project edit
        private void OpenLoadProjectView(object sender, RoutedEventArgs e)
        {
            SelectProjectWindow pw = new SelectProjectWindow();
            pw.Show();
        }

        // safely closes a tab and adds it to history
        private void CloseTabSafely(MugenSaveableEditor tab)
        {
            if (tab.IsDirty)
            {
                // confirmation popup for save
                string messageBoxText = string.Format("Do you want to save changes to {0} before closing?", tab.BackingFile.FileKey);
                string caption = "Code Editor";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                // display
                MessageBoxResult res = MessageBox.Show(messageBoxText, caption, button, icon);

                // Process message box results
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        // save changes
                        tab.SaveIfModified();
                        goto case MessageBoxResult.No; // c#.......
                    case MessageBoxResult.No:
                        // for sync with autocomplete
                        tabWasClosed = true;
                        // remove
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
                        break;
                    case MessageBoxResult.Cancel:
                        tab.Focus();
                        break;
                }
            } else
            {
                // for sync with autocomplete
                tabWasClosed = true;
                // remove
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
        }

        // globally-active key shortcuts go here
        private void GlobalKeyEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.ReopenLastClosedTab();
            } else if (e.Key == Key.J && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                // jump to statedef -- will open or focus the correct tab -- todo
            }
        }
    }
}

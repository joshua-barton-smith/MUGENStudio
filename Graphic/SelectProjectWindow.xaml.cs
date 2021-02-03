using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using MUGENStudio.Core;
using MUGENStudio.Graphic;
using MUGENStudio.MugenParser;

namespace MUGENStudio.Graphic
{
    /// <summary>
    /// Interaction logic for SelectProjectWindow.xaml
    /// </summary>
    public partial class SelectProjectWindow : Window
    {

        // used to map labels in pastProjectPanel to DEF files
        private readonly Dictionary<string, string> labelMap = new Dictionary<string, string>();

        /// <summary>
        /// Build project selector with a list of recent projects.
        /// </summary>
        public SelectProjectWindow()
        {
            InitializeComponent();
            // show the list of previous projects for selection
            foreach ((string, string) project in Globals.settingsSingleton.PreviousProjects)
            {
                Label labPast = new Label
                {
                    Content = string.Format("{0} ({1})", project.Item2, project.Item1),
                    Style = this.FindResource("LabelWithHover") as Style
                };

                labelMap.Add((string)labPast.Content, project.Item1);

                // handle click event for selecting past project
                labPast.AddHandler(MouseDownEvent, new MouseButtonEventHandler(LabPast_HandleClick), false);
                pastProjectPanel.Children.Add(labPast);
            }
        }

        // used to launch the editor after choosing a DEF file
        private void LabPast_HandleClick(object sender, MouseButtonEventArgs e)
        {
            string content = ((Label)sender).Content as string;
            string defFile = labelMap[content];
            this.LaunchEditor(defFile);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // open dialog to choose DEF file
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "DEF files (*.def)|*.def"
            };
            if (dialog.ShowDialog() == true)
            {
                // get selected DEF file
                string defFilePath = dialog.FileName;

                this.LaunchEditor(defFilePath);
            }
        }

        private void LaunchEditor(string project)
        {
            // try to load the project
            if (this.LoadProject(project))
            {
                // swap to the editor window
                if (Globals.editor == null) Globals.editor = new CodeEditorWindow();
                Globals.editor.ReloadTree();
                Globals.editor.Show();
                Application.Current.MainWindow = Globals.editor;
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                this.Close();
            }
        }

        // loads a project from the given def file
        // return true for success, false for fail
        private bool LoadProject(string defFile)
        {
            // parse DEF file
            MugenDefinition loadingDef = new MugenDefinition(defFile);

            // insert project into previousProjects
            Globals.settingsSingleton.AddPreviousProject(defFile, loadingDef.DisplayName);
            // save immediately if no errors occurred 
            Globals.settingsSingleton.SaveSettings();

            // add new project to the list
            Globals.projects.Add(loadingDef);

            // this currently never fails
            return true;
        }

    }
}

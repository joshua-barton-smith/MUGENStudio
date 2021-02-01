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

namespace MUGENStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            // show the list of previous projects for selection
            foreach ((string, string) project in Globals.settingsSingleton.PreviousProjects)
            {
                pastProjectPanel.Children.Add(new Label { Content = string.Format("{0} ({1})", project.Item2, project.Item1) });
            }
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

                // parse DEF file
                MugenDefinition loadingDef = new MugenDefinition(defFilePath);

                // insert project into previousProjects
                Globals.settingsSingleton.AddPreviousProject(defFilePath, loadingDef.DisplayName);
                // save immediately if no errors occurred 
                Globals.settingsSingleton.SaveSettings();
            }
        }

    }
}

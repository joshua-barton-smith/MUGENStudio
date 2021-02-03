using MUGENStudio.Core;
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
            codeWindow.Title = string.Format("Code Editor - {0}", Globals.projects.First().DisplayName);
        }
    }
}

using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace FolderManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            var openFolderBrowserDialogResult = openFileDialog.ShowDialog();
            if (openFolderBrowserDialogResult == System.Windows.Forms.DialogResult.OK)
            {
                btnBrowse.IsEnabled = false;
                prgBar.IsIndeterminate = true;
                var selectedPath = openFileDialog.SelectedPath;

                Task task = new Task(() =>
                {
                    RecursiveTraversal(selectedPath);
                });

                task.Start();

                task.ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnBrowse.IsEnabled = true;
                    });
                    System.Windows.MessageBox.Show("Done moving files");
                });
            }
        }

        private void RecursiveTraversal(string path)
        {
            var childDirs = Directory.GetDirectories(path);
            if (childDirs.Length == 0)
            {
                var files = Directory.GetFiles(path);

                foreach (var file in files)
                {
                    var destination = Directory.GetParent(path).FullName + "\\" + Path.GetFileName(file);
                    if (!File.Exists(destination))
                    {
                        File.Move(file, destination);
                    }
                }
                if (Directory.GetFiles(path).Length == 0)
                {
                    Directory.Delete(path);
                }
            }
            else
            {
                foreach (var dir in childDirs)
                {
                    RecursiveTraversal(dir);
                }
            }


        }
    }
}

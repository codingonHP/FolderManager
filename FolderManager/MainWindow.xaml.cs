using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using TagLib;
using System;

namespace FolderManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IComparer<string> comparer;

        public MainWindow()
        {
            InitializeComponent();
            comparer = new StringComparer();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            var openFolderBrowserDialogResult = openFileDialog.ShowDialog();
            if (openFolderBrowserDialogResult == System.Windows.Forms.DialogResult.OK)
            {
                btnBrowse.IsEnabled = false;
                prgBar.IsIndeterminate = true;
                prgBar.IsEnabled = true;

                var selectedPath = openFileDialog.SelectedPath;
                int rdepth;

                bool parsedSuccessfully = int.TryParse(txtRecursiveDepth.Text.Trim(), out rdepth);
                if (!parsedSuccessfully)
                {
                    rdepth = 2;
                }

                Task task = new Task(() =>
                {
                    for (int i = 0; i < rdepth; i++)
                    {
                        RecursiveTraversal(selectedPath, 0);
                    }
                        
                });

                Stopwatch sw = new Stopwatch();

                task.Start();

                sw.Start();
                task.ContinueWith(t =>
                {
                    sw.Stop();

                    Dispatcher.Invoke(() =>
                    {
                        btnBrowse.IsEnabled = true;
                        prgBar.IsIndeterminate = false;
                        prgBar.IsEnabled = false;

                    });
                    System.Windows.MessageBox.Show($"Done moving files in {sw.Elapsed.TotalHours} hours {sw.Elapsed.TotalMinutes} minutes {sw.Elapsed.TotalSeconds} secs {sw.Elapsed.TotalMilliseconds} ms"
                                                    , "Process done"
                                                    , MessageBoxButton.OK
                                                    , MessageBoxImage.Information
                                                    , MessageBoxResult.OK
                                                    , System.Windows.MessageBoxOptions.ServiceNotification);
                });
            }
        }

        private bool ItWasNotAlreadySortedBefore(string path)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                int fileNameInInt;
                var validFileNameInInt = int.TryParse(fileName, out fileNameInInt);
                if (validFileNameInInt)
                {
                    return fileNameInInt.ToString().Length > 2;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
           
            return true;
        }

        private int RecursiveTraversal(string path, int fileIndex)
        {
            //System.Threading.Thread.Sleep(5000);
            int step = 0;
           
            var childDirs = SortedSubDirectories(path);

            if (childDirs.Length == 0)
            {
                var files = Directory.GetFiles(path);
                int index = 0;
                int maxIndex = 0;
                foreach (var file in files)
                {
                        maxIndex = fileIndex + index++;
                        var destination = Directory.GetParent(path).FullName + "\\" + RoundIndexToTwoPlaces(maxIndex) + Path.GetExtension(file);
                        if (!System.IO.File.Exists(destination))
                        {
                            System.IO.File.Move(file, destination);

                            if (ItWasNotAlreadySortedBefore(file))
                            {
                                TagLib.File tFile = TagLib.File.Create(destination);
                                tFile.Tag.Title = "";
                                tFile.Save();
                            }
                            
                        }
                }
                if (Directory.GetFiles(path).Length == 0)
                {
                    Directory.Delete(path);
                }
                
                 return ++maxIndex;
            }
            else
            {
                foreach (var dir in childDirs)
                {
                    var maxIndex = RecursiveTraversal(dir,step);
                   
                    step = maxIndex;
                }
            }

            return step;
        }

        public string[] SortedSubDirectories(string path)
        {
            var childDirs = Directory.GetDirectories(path);
            childDirs = childDirs.OrderBy(d => d, comparer).ToArray();
            return childDirs;
        }

        private static string RoundIndexToTwoPlaces(int index)
        {
            if (index / 10 != 0)
            {
                return index.ToString();
            }
            return string.Format("0{0}", index);
        }

        private class StringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xDirName = x.Split('\\').Last();
                var yDirName = y.Split('\\').Last();

                int xresult;
                int yresult;

                int.TryParse(xDirName, out xresult);
                int.TryParse(yDirName, out yresult);

                return xresult - yresult;

            }
        }
    }
}

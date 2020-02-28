using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstroImagingCatalog
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

        private void Button_BrowseArchiveClick(object sender, RoutedEventArgs e)
        {
            // Browse for a source folder to start cataloging
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                txtbx_srcfolder.Text = folderBrowser.SelectedPath;
            }
        }

        private void Button_DestArchiveClick(object sender, RoutedEventArgs e)
        {
            // Browse for a dest folder to start cataloging
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                txtbx_dstfolder.Text = folderBrowser.SelectedPath;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuItem_ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_adddeploy_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtbx_srcfolder.Text) && !string.IsNullOrWhiteSpace(txtbx_dstfolder.Text) && !string.IsNullOrWhiteSpace(application_DropDown.Text))
            {
                if (string.IsNullOrWhiteSpace(txtbx_targetName.Text))
                {
                    System.Windows.MessageBox.Show("Unable to catalog images. The target name was left empty.", "Unable to Catalog Images", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    // Both source folder and dest folder selected
                    lblStatus.Text = "Creating image catalog from " + txtbx_srcfolder.Text + " to " + txtbx_dstfolder.Text + ". Please wait...";

                    string dstPath = txtbx_dstfolder.Text;
                    string targetName = txtbx_targetName.Text;
                    DateTime dateTaken = new DateTime();

                    System.IO.Directory.CreateDirectory(dstPath + @"\" + targetName);

                    // Create array of all file names in the image folder
                    string[] files = System.IO.Directory.GetFiles(txtbx_srcfolder.Text);
                    dateTaken = System.IO.File.GetCreationTime(files[0]);
                    for (int i = 0; i < files.Length; i++)
                    {
                        var section = files[i].Split('\\');
                        files[i] = section[section.Length - 1];
                    }

                    HashSet<string> fileTypesHashSet = new HashSet<string>();
                    List<String> fileTypes = new List<string>();

                    foreach (var fileName in files)
                    {
                        // loop through each of the file names
                        if (application_DropDown.Text == "Astro Photography Tools")
                        {
                            string firstTwoLetters = fileName[0].ToString() + fileName[1].ToString();
                            if (!fileTypesHashSet.Contains(firstTwoLetters))
                            {
                                // Does not already have the first two letters in the hashset
                                fileTypesHashSet.Add(firstTwoLetters);
                                fileTypes.Add(firstTwoLetters);
                                lblStatus.Text = firstTwoLetters;
                            }
                        }
                    }

                    // Set date for object
                    
                    if (date_DateTaken.SelectedDate.HasValue)
                    {
                        dateTaken = (DateTime)date_DateTaken.SelectedDate;
                    }
                    var dateFolder = dateTaken.ToString("yyyyMMdd");

                    // Create LightFrames directories
                    if (application_DropDown.Text == "Astro Photography Tools")
                    {
                        foreach (var fileType in fileTypes)
                        {
                            // If the files are lightframes
                            if (fileType[0] == 'L')
                            {
                                // Create Lightframe folder
                                System.IO.Directory.CreateDirectory(dstPath + "\\" + targetName + "\\" + dateFolder + "\\" + @"\LightFrames\" + fileType[1].ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to catalog images. The source or destination fields were left empty.", "Unable to Catalog Images", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using nom.tam.fits;
using LiteDB;

namespace AstroImagingCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<FITInformation> testDatabase = new List<FITInformation>();
        public string dateToStore;
        public BasicHDU hduToGetInfoFrom;
        public string destToImages;

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

        private void Button_DestDBClick(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                txtbx_dbDstFolder.Text = folderBrowser.SelectedPath;
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
                    string[] files;
                    string[] filesFullPath;
                    GenerateArrayOfFileNames(out dateTaken, out files, out filesFullPath);

                    HashSet<string> fileTypesHashSet = new HashSet<string>();
                    List<String> fileTypes = new List<string>();

                    foreach (var fileName in files)
                    {
                        // loop through each of the file names
                        if (application_DropDown.Text == "Astro Photography Tools")
                        {
                            string firstThreeLetters = fileName[0].ToString() + fileName[1].ToString() + fileName[2].ToString();
                            if (!fileTypesHashSet.Contains(firstThreeLetters))
                            {
                                // Does not already have the first two letters in the hashset
                                fileTypesHashSet.Add(firstThreeLetters);
                                fileTypes.Add(firstThreeLetters);
                                lblStatus.Text = firstThreeLetters;
                            }
                        }
                    }

                    // Set date for object
                    if (date_DateTaken.SelectedDate.HasValue)
                    {
                        dateTaken = (DateTime)date_DateTaken.SelectedDate;
                    }
                    var dateFolder = dateTaken.ToString("yyyyMMdd");
                    dateToStore = dateFolder;

                    CreateDirectoryAndSortImages(dstPath, targetName, files, filesFullPath, fileTypes, dateFolder);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to catalog images. The source or destination fields were left empty.", "Unable to Catalog Images", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CreateDirectoryAndSortImages(string dstPath, string targetName, string[] files, string[] filesFullPath, List<string> fileTypes, string dateFolder)
        {
            destToImages = dstPath + "\\" + targetName + "\\" + dateFolder;
            // Create LightFrames directories
            if (application_DropDown.Text == "Astro Photography Tools")
            {
                foreach (var fileType in fileTypes)
                {
                    switch (fileType[0])
                    {
                        case 'L':
                            // Create Lightframe folder
                            System.IO.Directory.CreateDirectory(dstPath + "\\" + targetName + "\\" + dateFolder + "\\" + @"\LightFrames\" + fileType[2].ToString());
                            break;
                        case 'F':
                            // Create Flats folder
                            System.IO.Directory.CreateDirectory(dstPath + "\\" + targetName + "\\" + dateFolder + "\\" + @"\Flats\" + fileType[2].ToString());
                            break;
                        case 'D':
                            // Create Darks folder
                            System.IO.Directory.CreateDirectory(dstPath + "\\" + targetName + "\\" + dateFolder + "\\" + @"\Darks\" + fileType[2].ToString());
                            break;
                        default:
                            // Create UNKNOWN folder
                            System.IO.Directory.CreateDirectory(dstPath + "\\" + targetName + "\\" + dateFolder + "\\" + @"\UNKNOWN\" + fileType[2].ToString());
                            break;
                    }
                }

                HashSet<string> firstFileToHold = new HashSet<string>();

                // Read each full file path
                for (int i = 0; i < filesFullPath.Length; i++)
                {
                    string fileTypeTemp;


                    switch (files[i][0])
                    {
                        case 'L':
                            // Current image is a light frame
                            fileTypeTemp = "\\LightFrames" + "\\" + files[i][2].ToString() + "\\";
                            break;
                        case 'F':
                            // Current image is a flat frame
                            fileTypeTemp = "\\Flats" + "\\" + files[i][2].ToString() + "\\";
                            break;
                        case 'D':
                            // Current image is a dark frame
                            fileTypeTemp = "\\Darks" + "\\" + files[i][2].ToString() + "\\";
                            break;
                        default:
                            fileTypeTemp = "\\UNKNOWN" + "\\" + files[i][2].ToString() + "\\";
                            break;
                    }

                    System.IO.File.Copy(filesFullPath[i], txtbx_dstfolder.Text + "\\" + txtbx_targetName.Text + "\\" + dateFolder + "\\" + fileTypeTemp + files[i]);
                    // Get the FITS image data information
                    BasicHDU hdu = ReadFIT(filesFullPath[i]);

                    if (files[i][0] == 'L' && !firstFileToHold.Contains(files[i]))
                    {
                        hduToGetInfoFrom = hdu;
                        firstFileToHold.Add(files[i]);
                    }
                    lblStatus.Text = hdu.Instrument;
                }
            }
        }

        private static BasicHDU ReadFIT(string fileName)
        {
            Fits f = new Fits(fileName);
            try
            {
                BasicHDU hdus = f.ReadHDU();
                if (hdus != null)
                {
                    hdus.Info();
                    Header hduHeader = hdus.Header;
                    var testHeader = hduHeader.FindCard("CCD-TEMP").Value;
                }
                return hdus;
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        private void GenerateArrayOfFileNames(out DateTime dateTaken, out string[] files, out string[] filesFullPath)
        {
            files = System.IO.Directory.GetFiles(txtbx_srcfolder.Text);
            filesFullPath = System.IO.Directory.GetFiles(txtbx_srcfolder.Text);
            dateTaken = System.IO.File.GetLastWriteTime(filesFullPath[0]);
            for (int i = 0; i < files.Length; i++)
            {
                var section = files[i].Split('\\');
                files[i] = section[section.Length - 1];
            }
        }

        private void AddItemToDatabase(string objectName, string date, string fileDirectory, BasicHDU hdu)
        {
            long tempID = testDatabase.Count;
            Header hduHeader = hdu.Header;

            if (!string.IsNullOrWhiteSpace(txtbx_dbDstFolder.Text))
            {
                // Open a database or create one if none exists
                using (var db = new LiteDatabase(txtbx_dbDstFolder.Text + "\\ImageCatalog.db"))
                {
                    // get a collection or create one if none exists
                    var col = db.GetCollection<FITInformation>("Images");

                    // create your new image entry
                    var image = new FITInformation
                    {
                        ObjectName = objectName,
                        DateTaken = date,
                        ImagingCamera = hdu.Instrument,
                        FilesDirectory = fileDirectory,
                        FocalLength = Convert.ToInt32(hduHeader.FindCard("FOCALLEN").Value),
                        CameraTemp = Convert.ToDecimal(hduHeader.FindCard("CCD-TEMP").Value),
                        Binning = hduHeader.FindCard("XBINNING").Value + "x" + hduHeader.FindCard("YBINNING").Value,
                        CameraGain = Convert.ToInt32(hduHeader.FindCard("GAIN").Value),
                        SiteLat = hduHeader.FindCard("SITELAT").Value.ToString(),
                        SiteLong = hduHeader.FindCard("SITELONG").Value.ToString()
                    };

                    // insert new image (it will be auto-incremented)
                    col.Insert(image);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("No DB folder selected, please select the location for the DB.", "No DB Folder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_catalogImages_Click(object sender, RoutedEventArgs e)
        {
            AddItemToDatabase(txtbx_targetName.Text, dateToStore, destToImages, hduToGetInfoFrom);
        }

        private void btn_searchDB_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtbx_dbDstFolder.Text))
            {
                rtbx_searchResults.Document.Blocks.Clear();

                List<FITInformation> results = GenerateSearchQuery();

                int i = 1;
                foreach (var item in results)
                {
                    var p = new Paragraph();
                    p.Inlines.Add("Result #" + i);
                    p.Inlines.Add(new LineBreak());
                    //rtbx_searchResults.Document.Blocks.Add(paragraph1);

                    p.Inlines.Add("Object Name: " + item.ObjectName + " | ");
                    p.Inlines.Add("Date Taken: " + item.DateTaken);
                    p.Inlines.Add(new LineBreak());
                    p.Inlines.Add("File Location: " + item.FilesDirectory);
                    p.Inlines.Add(new LineBreak());

                    p.Inlines.Add("Binning: " + item.Binning + " | ");
                    p.Inlines.Add("CCD Temp: " + item.CameraTemp + " | ");
                    p.Inlines.Add("Gain: " + item.CameraGain + " | ");
                    p.Inlines.Add("Focal Length: " + item.FocalLength + " | ");
                    p.Inlines.Add("Latitude: " + item.SiteLat + " | ");
                    p.Inlines.Add("Longitude: " + item.SiteLong);

                    rtbx_searchResults.Document.Blocks.Add(p);

                    i++;
                }
            }
            else
            {
                // No DB directory selected
                System.Windows.MessageBox.Show("No DB folder selected, please select the location for the DB.", "No DB Folder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private List<FITInformation> GenerateSearchQuery()
        {
            using (var db = new LiteDatabase(txtbx_dbDstFolder.Text + "\\ImageCatalog.db"))
            {
                var col = db.GetCollection<FITInformation>("Images");
                col.EnsureIndex(x => x.ObjectName);

                var whatToQuery = col.Query();

                if (!string.IsNullOrWhiteSpace(txtbx_objectName.Text))
                {
                    whatToQuery.Where(x => x.ObjectName.ToUpper() == txtbx_objectName.Text);
                }

                if (!string.IsNullOrWhiteSpace(txtbx_DateSearch.Text))
                {
                    whatToQuery.Where(x => x.DateTaken.ToUpper() == txtbx_DateSearch.Text);
                }

                if (!string.IsNullOrWhiteSpace(txtbx_Gain.Text))
                {
                    whatToQuery.Where(x => x.CameraGain == Convert.ToInt32(txtbx_Gain.Text));
                }

                if (!string.IsNullOrWhiteSpace(txtbx_Temp.Text))
                {
                    whatToQuery.Where(x => x.CameraTemp == Convert.ToDecimal(txtbx_Temp.Text));
                }

                if (!string.IsNullOrWhiteSpace(txtbx_Bin.Text))
                {
                    whatToQuery.Where(x => x.Binning.ToUpper() == txtbx_Bin.Text);
                }

                var results = whatToQuery.ToList();

                return results;
            }

            throw new Exception("DB could not query");
         }
    }
}

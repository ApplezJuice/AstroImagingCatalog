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
        public Dictionary<string, string> dateToStore = new Dictionary<string, string>();
        public List<BasicHDU> hduToGetInfoFrom = new List<BasicHDU>();
        public Dictionary<string, string> destToImages = new Dictionary<string, string>();
        public NumSharpTest numSharp = new NumSharpTest();

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

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //}

        private void MenuItem_ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_adddeploy_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtbx_srcfolder.Text) && !string.IsNullOrWhiteSpace(txtbx_dstfolder.Text)) // && !string.IsNullOrWhiteSpace(application_DropDown.Text)
            {
                // Both source folder and dest folder selected
                lblStatus.Text = "Creating image catalog from " + txtbx_srcfolder.Text + " to " + txtbx_dstfolder.Text + ". Please wait...";

                string dstPath = txtbx_dstfolder.Text;
                string targetName = "NOTARGET";

                if (!string.IsNullOrWhiteSpace(txtbx_targetName.Text))
                {
                    targetName = txtbx_targetName.Text;
                }

                DateTime dateTaken = new DateTime();

                //System.IO.Directory.CreateDirectory(dstPath + @"\" + targetName);

                // Create array of all file names in the image folder
                string[] files;
                string[] filesFullPath;

                // This date may not be needed
                GenerateArrayOfFileNames(out dateTaken, out files, out filesFullPath);

                // Set date for object
                if (date_DateTaken.SelectedDate.HasValue)
                {
                    dateTaken = (DateTime)date_DateTaken.SelectedDate;
                }
                var dateFolder = dateTaken.ToString("yyyyMMdd");

                CreateDirectoryAndSortImages(dstPath, targetName, files, filesFullPath, dateFolder);
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to catalog images. The source or destination fields were left empty.", "Unable to Catalog Images", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CreateDirectoryAndSortImages(string dstPath, string targetName, string[] files, string[] filesFullPath, string dateFolder)
        {

            HashSet<string> fileTypesHashSet = new HashSet<string>();
            //destToImages = dstPath + "\\" + targetName + "\\" + dateFolder + "\\";

            // Create Light Frames directories
            //if (application_DropDown.Text == "Astro Photography Tools")
            //{

            HashSet<string> objectNames = new HashSet<string>();

            for (int i = 0; i < filesFullPath.Length; i++)
            {
                string fileTypeTemp;

                // Get the header of each file
                var headerInfo = ReadFIT(filesFullPath[i]).Header;
                var imageType = headerInfo.FindCard("IMAGETYP").Value;
                string filterType = "";

                if (!date_DateTaken.SelectedDate.HasValue)
                {
                    var tempDate = System.IO.File.GetLastWriteTime(filesFullPath[i]);
                    //var tempDate = DateTime.Parse(headerInfo.FindCard("DATE-OBS").Value);
                    dateFolder = tempDate.ToString("yyyMMdd");
                    // TODO: temp dont store second date
                    if (!dateToStore.ContainsValue(dateFolder) && !dateToStore.ContainsKey(headerInfo.FindCard("OBJECT").Value))
                    {
                        // Same value with different date in the same folder causes this to be an error
                        dateToStore.Add(headerInfo.FindCard("OBJECT").Value, dateFolder);
                    }
                }


                switch (imageType)
                {
                case "Light Frame":
                    if (string.IsNullOrWhiteSpace(txtbx_objectName.Text))
                    {
                        targetName = headerInfo.FindCard("OBJECT").Value;
                        if (string.IsNullOrEmpty(targetName))
                        {
                            targetName = "NOTARGET";
                        }
                    }
                    break;
                default:
                    targetName = "NOTARGET";
                    //destToImages = dstPath + "\\" + targetName + "\\" + dateFolder + "\\";
                    break;
                }
                if (!destToImages.ContainsKey(headerInfo.FindCard("OBJECT").Value))
                    destToImages.Add(headerInfo.FindCard("OBJECT").Value, dstPath + "\\" + targetName + "\\" + dateFolder + "\\");
                var cureDirToCreate = dstPath + "\\" + targetName + "\\" + dateFolder + "\\";

                if (imageType == "Light Frame" || imageType == "Flat Frame")
                {
                    filterType = headerInfo.FindCard("FILTER").Value;
                }

                if (!fileTypesHashSet.Contains(imageType + "/" + filterType))
                {
                    // If the file type is not already in the hash set then add it
                    fileTypesHashSet.Add(imageType + "/" + filterType);
                    if (filterType == "")
                    {
                        System.IO.Directory.CreateDirectory(cureDirToCreate + imageType + "s");
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(cureDirToCreate + imageType + "s\\" + filterType);
                    }
                }

                switch (imageType)
                {
                    case "Light Frame":
                        fileTypeTemp = "\\Light Frames\\" + headerInfo.FindCard("FILTER").Value + "\\";
                        break;
                    case "Dark Frame":
                        fileTypeTemp = "\\Dark Frames\\";
                        break;
                    case "Flat Frame":
                        fileTypeTemp = "\\Flat Frames\\" + filterType + "\\";
                        break;
                    default:
                        fileTypeTemp = "\\UNKNOWN\\";
                        break;
                }

                System.IO.File.Copy(filesFullPath[i], txtbx_dstfolder.Text + "\\" + targetName + "\\" + dateFolder + "\\" + fileTypeTemp + files[i]);
                // Get the FITS image data information
                BasicHDU hdu = ReadFIT(filesFullPath[i]);

                if (string.IsNullOrWhiteSpace(txtbx_objectName.Text))
                {
                    // No object name
                    if (imageType == "Light Frame" && !objectNames.Contains(hdu.Header.FindCard("OBJECT").Value))
                    {
                        hduToGetInfoFrom.Add(hdu);
                        objectNames.Add(hdu.Header.FindCard("OBJECT").Value);
                    }
                }
                else
                {
                    if (imageType == "Light Frame" && !objectNames.Contains(txtbx_objectName.Text))
                    {
                        hduToGetInfoFrom.Add(hdu);
                        objectNames.Add(txtbx_objectName.Text);
                    }
                }
                
                lblStatus.Text = hdu.Instrument;
            }
            //}
        }

        private BasicHDU ReadFIT(string fileName)
        {
            Fits f = new Fits(fileName);
            try
            {
                BasicHDU hdus = f.ReadHDU();
                if (hdus != null)
                {
                    hdus.Info();
                    // TEST: NUM SHARP TEST
                    ImageHDU imgHdu = (ImageHDU)f.GetHDU(0);
                    numSharp.InitImage(imgHdu, fileName);
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

        private void AddItemToDatabase(Dictionary<string,string> date, Dictionary<string, string> fileDirectory, List<BasicHDU> hdu)
        {
            long tempID = testDatabase.Count;

            if (!string.IsNullOrWhiteSpace(txtbx_dbDstFolder.Text))
            {
                foreach (var curHdu in hdu)
                {
                    Header hduHeader = curHdu.Header;
                    string objectName;

                    if (!string.IsNullOrWhiteSpace(txtbx_objectName.Text))
                    {
                        // if there is a target name
                        objectName = txtbx_objectName.Text;
                    }
                    else
                    {
                        objectName = hduHeader.FindCard("OBJECT").Value;
                        if (string.IsNullOrEmpty(objectName))
                        {
                            objectName = "NOTARGET";
                        }
                    }

                    // Open a database or create one if none exists
                    using (var db = new LiteDatabase(txtbx_dbDstFolder.Text + "\\ImageCatalog.db"))
                    {
                        // get a collection or create one if none exists
                        var col = db.GetCollection<FITInformation>("Images");

                        int gain = 0;
                        string dateToPass;
                        string fileToStore;

                        try
                        {
                            var temp = hduHeader.FindCard("GAIN").Value;
                            gain = Convert.ToInt32(temp);
                        }
                        catch (Exception)
                        {

                            gain = 0;
                        }

                        dateToStore.TryGetValue(hduHeader.FindCard("OBJECT").Value, out dateToPass);
                        destToImages.TryGetValue(hduHeader.FindCard("OBJECT").Value, out fileToStore);

                        // create your new image entry
                        var focalLength = hduHeader.FindCard("FOCALLEN").Value;
                        var splitLength = focalLength.Split('.');

                        var ccd = Convert.ToDecimal(hduHeader.FindCard("CCD-TEMP").Value);
                        var roundedCCD = decimal.Round(ccd, 2, MidpointRounding.AwayFromZero);

                        var tempExp = hduHeader.FindCard("EXPTIME").Value;
                        var expToStore = tempExp.Split('.');

                        var image = new FITInformation
                        {
                            ObjectName = objectName,
                            DateTaken = dateToPass,
                            ImagingCamera = curHdu.Instrument,
                            FilesDirectory = fileToStore,
                            FocalLength = Convert.ToInt32(splitLength[0]),
                            CameraTemp = roundedCCD,
                            Binning = hduHeader.FindCard("XBINNING").Value + "x" + hduHeader.FindCard("YBINNING").Value,
                            CameraGain = gain,
                            SiteLat = hduHeader.FindCard("SITELAT").Value,
                            SiteLong = hduHeader.FindCard("SITELONG").Value,
                            ExposureTime = expToStore[0]
                        };

                        // insert new image (it will be auto-incremented)
                        col.Insert(image);
                    }
                }
                
            }
            else
            {
                System.Windows.MessageBox.Show("No DB folder selected, please select the location for the DB.", "No DB Folder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_catalogImages_Click(object sender, RoutedEventArgs e)
        {
            AddItemToDatabase(dateToStore, destToImages, hduToGetInfoFrom);
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

                    p.Inlines.Add("Object Name: ");
                    p.Inlines.Add(new Bold(new Run(item.ObjectName)));
                    p.Inlines.Add(" | ");
                    p.Inlines.Add("Date Taken: " + item.DateTaken);
                    p.Inlines.Add(new LineBreak());
                    p.Inlines.Add("File Location: " + item.FilesDirectory);
                    p.Inlines.Add(new LineBreak());

                    p.Inlines.Add("Binning: " + item.Binning + " | ");
                    p.Inlines.Add("CCD Temp: " + item.CameraTemp + " | ");
                    p.Inlines.Add("Gain: " + item.CameraGain + " | ");
                    p.Inlines.Add("Exposure Seconds: " + item.ExposureTime + " | ");
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

        private void MenuItem_SaveSettings(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtbx_dbDstFolder.Text))
            {
                Properties.Settings.Default.Properties["DatabaseLocation"].DefaultValue = txtbx_dbDstFolder.Text;
            }

            Properties.Settings.Default.Save();
        }
    }
}

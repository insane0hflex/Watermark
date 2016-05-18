using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Watermark.Utilities;
using System.IO;
using System.Collections.Generic;


namespace Watermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Ideas:
    ///     Create thumbnails:
    ///             Bitmap resized = new Bitmap(original,new Size(original.Width/4,original.Height/4));
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {

        private string rootFolderFilePath = Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\"));
        private string baseImageFilePath;
        private string waterMarkImageFilePath;

        Bitmap baseImage;
        Bitmap waterMarkImage;
        Bitmap finalImage;


        public MainWindow()
        {
            InitializeComponent();

            baseImageFilePath = rootFolderFilePath + @"Images\testBaseFallout4.png";
            waterMarkImageFilePath = rootFolderFilePath + @"Images\testSvaalbardWatermark.png";

            img_basePreview.Source = new BitmapImage(new Uri(baseImageFilePath));
            img_watermarkPreview.Source = new BitmapImage(new Uri(waterMarkImageFilePath));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceImageFilePath"></param>
        /// <param name="watermarkImageFilePath"></param>
        public void CreateWatermarkOnImage(string sourceImageFilePath, string watermarkImageFilePath, bool isPreviewOnly)
        {

            try
            {
                if (sourceImageFilePath != "")
                {
                    baseImage = (Bitmap)System.Drawing.Image.FromFile(sourceImageFilePath);
                }
                else
                {
                    baseImage = (Bitmap)System.Drawing.Image.FromFile(rootFolderFilePath + @"Images\placeholder.png");
                }

                //wtf? why was this sourceFilePath before? changing to watermarkImageFilePath
                if (watermarkImageFilePath != "")
                {
                    waterMarkImage = (Bitmap)System.Drawing.Image.FromFile(watermarkImageFilePath);
                }
                else
                {
                    waterMarkImage = (Bitmap)System.Drawing.Image.FromFile(rootFolderFilePath + @"Images\placeholder.png");
                }

                // 5 17 2016 - this was an old issue - is it still relevant later on perhaps...?
                //for some reason the (implicit?) casting up from 24bit depth to 32bit depth messes up the bmp
                //properties of Horizontal and Veritical resolution to like 95.9. This explicitly sets it back to 100%
                //baseImage.SetResolution(100, 100);


                finalImage = new Bitmap(baseImage.Width, baseImage.Height, PixelFormat.Format32bppArgb);

                //set final image to the base image horizontal and vertical resolution
                //this fixes? the weird stretching resolution bug.
                finalImage.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);

                //Create a graphics which will have the watermark image applied over the final image
                using (Graphics graphics = Graphics.FromImage(finalImage))
                {
                    //Specifies that when a color is rendered, it is blended with the background color.
                    //The blend is determined by the alpha component of the color being rendered.
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    graphics.DrawImage(baseImage, 0, 0);

                    //cast to ComboBoxItem is needed?
                    string alignmentSelection = ((ComboBoxItem)comboBox_watermarkAlignment.SelectedItem).Content.ToString();
                    switch (alignmentSelection)
                    {
                        case "Top Left":
                            graphics.DrawImage(waterMarkImage, 0, 0);
                            break;
                        case "Top Right":
                            graphics.DrawImage(waterMarkImage, baseImage.Width - waterMarkImage.Width, 0);
                            break;
                        case "Bottom Left":
                            graphics.DrawImage(waterMarkImage, 0, baseImage.Height - waterMarkImage.Height);
                            break;
                        case "Bottom Right":
                            graphics.DrawImage(waterMarkImage, baseImage.Width - waterMarkImage.Width, baseImage.Height - waterMarkImage.Height);
                            break;
                        case "Center":
                        default:
                            graphics.DrawImage(waterMarkImage, (baseImage.Width / 2 - waterMarkImage.Width / 2), baseImage.Height / 2 - waterMarkImage.Height / 2);
                            break;
                    }

                    System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
                    string dateFileTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm", invariantCulture).Replace('/', '-').Replace(':', '-').Replace(' ', '-');

                    string finalImageFileName = "Watermarked_" + Path.GetFileNameWithoutExtension(baseImageFilePath) + "_" + dateFileTime;

                    //Save the watermarked image to disk if it is not a preview
                    //preview image is stored in memory
                    if (!isPreviewOnly)
                    {

                        string finalImageFormatSelection = ((ComboBoxItem)comboBox_finalImageFormat.SelectedItem).Content.ToString();
                        switch (finalImageFormatSelection)
                        {
                            case "JPG":
                                finalImage.Save(rootFolderFilePath + @"\Images\" + finalImageFileName + ".jpg", ImageFormat.Jpeg);
                                break;
                            case "BMP":
                                finalImage.Save(rootFolderFilePath + @"\Images\" + finalImageFileName + ".bmp", ImageFormat.Bmp);
                                break;
                            case "PNG":
                            default:
                                finalImage.Save(rootFolderFilePath + @"\Images\" + finalImageFileName + ".png", ImageFormat.Png);
                                break;
                        }


                        //Update final watermarked image preview with the new watermarked image
                        img_previewWatermarkAppliedToImage.Source = new BitmapImage(new Uri(rootFolderFilePath + @"\Images\" + finalImageFileName + ".png", UriKind.Absolute));
                    }
                    else
                    {
                        //the preview image is stored in memory
                        //no memory leaks tested so far
                        using (var memory = new MemoryStream())
                        {
                            finalImage.Save(memory, ImageFormat.Png);

                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = memory;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();

                            img_previewWatermarkAppliedToImage.Source = image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Error("Failure in creating/saving the watermarked image.", ex);
            }
            finally
            {
                //cleanup memory
                if(baseImage != null)
                {
                    baseImage.Dispose();
                }

                if(waterMarkImage != null)
                {
                    waterMarkImage.Dispose();
                }

                if(finalImage != null)
                {
                    finalImage.Dispose();
                }
            }


        }



        /// <summary>
        /// PREVIEW image
        /// </summary>
        private void btn_createWatermarkImagePreview_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(baseImageFilePath) && !String.IsNullOrEmpty(waterMarkImageFilePath))
            {
                CreateWatermarkOnImage(baseImageFilePath, waterMarkImageFilePath, true);
            }
        }


        /// <summary>
        /// Create actual file image
        /// </summary>
        private void btn_createWatermarkOnImage_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(baseImageFilePath) && !String.IsNullOrEmpty(waterMarkImageFilePath))
            {
                CreateWatermarkOnImage(baseImageFilePath, waterMarkImageFilePath, false);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }



        /// <summary>
        /// 
        /// </summary>
        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {

        }



        /// <summary>
        /// return a string of the selected image's file path.
        /// Used to create a new bitmap image from the image's file path (uri), like so:  new BitmapImage(new Uri(baseImageFilePath));
        /// </summary>
        /// <returns>string of the image's filename or an empty string</returns>
        private string GetImageFilePath()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".png";
            fileDialog.Filter = "Images (*.png)|*.png|(*.jpg)|*.jpg|(*.jpeg)|*.jpeg|(*.bmp)|*.bmp|(*.gif)|*.gif";

            //fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

            bool? userConfirmed = fileDialog.ShowDialog();

            try
            {
                // Get the selected file name and display in a TextBox 
                if (userConfirmed == true)
                {
                    return fileDialog.FileName;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error occurred trying to load (read) the selected image file." + Environment.NewLine;
                CustomMessageBox.Error(errorMessage, ex);

                return "";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetFolderPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (String.IsNullOrEmpty(dialog.SelectedPath))
            {
                return "";
            }

            var confirmation = CustomMessageBox.GetUserYesNoChoice("Are you sure you want to watermark all images in " + dialog.SelectedPath);

            if (confirmation == MessageBoxResult.Yes)
            {
                return dialog.SelectedPath;
            }
            else
            {
                return "";
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private void btn_loadBaseImage_Click(object sender, RoutedEventArgs e)
        {

            baseImageFilePath = GetImageFilePath();

            //if image selected - create image
            if (!String.IsNullOrEmpty(baseImageFilePath))
            {
                img_basePreview.Source = new BitmapImage(new Uri(baseImageFilePath));
                img_basePreview.ToolTip = baseImageFilePath;
            }
            else
            {
                CustomMessageBox.Notification("No image selected.");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void btn_loadWatermarkImage_Click(object sender, RoutedEventArgs e)
        {
            waterMarkImageFilePath = GetImageFilePath();

            if (!String.IsNullOrEmpty(waterMarkImageFilePath))
            {
                img_watermarkPreview.Source = new BitmapImage(new Uri(waterMarkImageFilePath));
                img_watermarkPreview.ToolTip = waterMarkImageFilePath;
            }
            else
            {
                CustomMessageBox.Notification("No image selected.");
            }
        }

        /// <summary>
        /// Trigger WatermarkAllImagesInThisDirectory() to Watermark all images in a choosen folder
        /// </summary>
        private void btn_createWatermarksOnAllImagesInFolder_Click(object sender, RoutedEventArgs e)
        {
            WatermarkAllImagesInSelectedDirectory();
        }


        /// <summary>
        /// WORK IN PROGRESS
        /// Watermark all images in a choosen folder
        /// </summary>
        private void WatermarkAllImagesInSelectedDirectory()
        {
            //need to make sure there is a watermark image selected to use as the watermark
            if (!String.IsNullOrEmpty(waterMarkImageFilePath))
            {
                //TODO make this get the chosen folder/directory location of files to replace
                string directoryPathOfImages = GetFolderPath();

                if (!String.IsNullOrEmpty(directoryPathOfImages))
                {
                    foreach (string fileName in Directory.GetFiles(directoryPathOfImages))
                    {
                        string fileExtension = Path.GetExtension(fileName).ToLower();

                        if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".bmp")
                        {
                            CreateWatermarkOnImage(fileName, waterMarkImageFilePath, true);
                        }
                    }
                }
            }
            else
            {
                CustomMessageBox.Error("No watermark image selected!");
            }
        }

    }
}

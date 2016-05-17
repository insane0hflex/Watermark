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

            img_basePreview.Source = new BitmapImage(new Uri(rootFolderFilePath + @"Images\placeholder.png"));
            img_watermarkPreview.Source = new BitmapImage(new Uri(rootFolderFilePath + @"Images\placeholder.png"));

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="watermarkImageFilePath"></param>
        public void CreateWatermarkOnImage(string sourceFilePath, string watermarkImageFilePath)
        {

            try
            {
                if(sourceFilePath != "")
                {
                    baseImage = (Bitmap)System.Drawing.Image.FromFile(sourceFilePath);
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

                finalImage = new Bitmap(baseImage.Width, baseImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //set final image to the base image horizontal and vertical resolution
                //this fixes? the weird stretching resolution bug.
                finalImage.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);

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

                    //graphics.DrawImage(overlayImage, baseImage.Width - overlayImage.Width, baseImage.Height - overlayImage.Height);

                    //Save the watermarked image to disk

                    string finalImageFileName = "watermarked_" + DateTime.Now.ToFileTime().ToString();


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
            }
            catch (Exception ex)
            {
                CustomMessageBox.Error("Failure in creating/saving the watermarked image.", ex);
            }
            finally
            {
                //cleanup memory
                baseImage.Dispose();
                waterMarkImage.Dispose();
                finalImage.Dispose();
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_createWatermarkOnImage_Click(object sender, RoutedEventArgs e)
        {

            //CreateWatermarkOnImage(rootFolderFilePath + @"Images\testBaseFallout4.png", rootFolderFilePath + @"\Images\testSvaalbardWatermark.png");

            CreateWatermarkOnImage(baseImageFilePath, waterMarkImageFilePath);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// return a string of the selected image's file path.
        /// Used to create a new bitmap image from the image's file path (uri), like so:  new BitmapImage(new Uri(baseImageFilePath));
        /// </summary>
        private string GetImageFilePath()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".png";
            fileDialog.Filter = "Images (*.png)|*.png";

            //fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\"));

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
        private void btn_loadBaseImage_Click(object sender, RoutedEventArgs e)
        {

            baseImageFilePath = GetImageFilePath();

            //if image selected - create image
            if (baseImageFilePath != "")
            {
                img_basePreview.Source = new BitmapImage(new Uri(baseImageFilePath));
                img_basePreview.ToolTip = baseImageFilePath;
            }
            else
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void btn_loadWatermarkImage_Click(object sender, RoutedEventArgs e)
        {
            waterMarkImageFilePath = GetImageFilePath();

            if(waterMarkImageFilePath != "")
            {
                img_watermarkPreview.Source = new BitmapImage(new Uri(waterMarkImageFilePath));
                img_watermarkPreview.ToolTip = waterMarkImageFilePath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void btn_createWatermarkImagePreview_Click(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// WORK IN PROGRESS
        /// Watermark all images in a choosen folder
        /// </summary>
        private void WatermarkAllImagesInThisDirectory()
        {
            //need to make sure there is a watermark image selected to use as the watermark
            if(waterMarkImageFilePath != null)
            {
                //TODO make this get the chosen folder/directory location of files to replace
                string directoryPathOfImages = Environment.CurrentDirectory;

                foreach (string fileName in Directory.GetFiles(directoryPathOfImages))
                {
                    string fileExtension = Path.GetExtension(fileName).ToLower();

                    if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".bmp")
                    {
                        CreateWatermarkOnImage(fileName, waterMarkImageFilePath);
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

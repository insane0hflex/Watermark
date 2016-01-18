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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Watermark.Utilities;

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

        private string rootFolderFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\"));
        private string baseImageFilePath;
        private string waterMarkImageFilePath;

        Bitmap baseImage;
        Bitmap waterMarkImage;
        Bitmap finalImage;

        Bitmap missingImage;


        public MainWindow()
        {
            InitializeComponent();

            missingImage = (Bitmap)System.Drawing.Image.FromFile(rootFolderFilePath + @"Images\placeholder.png");

        }


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

                if (sourceFilePath != "")
                {
                    waterMarkImage = (Bitmap)System.Drawing.Image.FromFile(watermarkImageFilePath);
                }
                else
                {
                    waterMarkImage = (Bitmap)System.Drawing.Image.FromFile(rootFolderFilePath + @"Images\placeholder.png");
                }


                //for some reason the (implicit?) casting up from 24bit depth to 32bit depth messes up the bmp
                //properties of Horizontal and Veritical resolution to like 95.9. This explicitly sets it back to 100%
                //baseImage.SetResolution(100, 100);


                finalImage = new Bitmap(baseImage.Width, baseImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //set final image to the base image horizontal and vertical resolution
                //this fixes? the weird stretching resolution bug.
                finalImage.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);

                using (Graphics graphics = Graphics.FromImage(finalImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceOver;


                    graphics.DrawImage(baseImage, 0, 0);

                    //cast to ComboBoxItem is needed?
                    string alignmentSelection = ((ComboBoxItem)comboBox_watermarkAlignment.SelectedItem).Content.ToString();

                    if (alignmentSelection == "Top Left")
                    {
                        graphics.DrawImage(waterMarkImage, 0, 0);
                    }
                    else if(alignmentSelection == "Top Right")
                    {
                        graphics.DrawImage(waterMarkImage, baseImage.Width - waterMarkImage.Width, 0);

                    }
                    else if (alignmentSelection == "Bottom Left")
                    {
                        graphics.DrawImage(waterMarkImage, 0, baseImage.Height- waterMarkImage.Height);

                    }
                    else if (alignmentSelection == "Bottom Right")
                    {
                        graphics.DrawImage(waterMarkImage, baseImage.Width - waterMarkImage.Width, baseImage.Height - waterMarkImage.Height);

                    }
                    else if (alignmentSelection == "Center")
                    {
                        graphics.DrawImage(waterMarkImage, (baseImage.Width / 2 - waterMarkImage.Width / 2), baseImage.Height / 2 - waterMarkImage.Height / 2);
                    }

                    //graphics.DrawImage(overlayImage, baseImage.Width - overlayImage.Width, baseImage.Height - overlayImage.Height);

                    //Save the watermarked image to disk

                    string finalImageFileName = "watermarked_" + DateTime.Now.ToFileTime().ToString();

                    finalImage.Save(rootFolderFilePath + @"\Images\" + finalImageFileName + ".png", ImageFormat.Png);


                    //Update final watermarked image preview with the new watermarked image
                    img_previewWatermarkAppliedToImage.Source = new BitmapImage(new Uri(rootFolderFilePath + @"\Images\" + finalImageFileName + ".png", UriKind.Absolute));

                    //maybe think of making a small library/nuget package of these "custom wpf message boxes"
                    //CustomMessageBox.NotificationMessageBox("Created image!");

                }


            }
            catch (Exception)
            {

                throw;

            }
            finally
            {
                baseImage.Dispose();
                waterMarkImage.Dispose();
                finalImage.Dispose();
            }


        }


        private void btn_createWatermarkOnImage_Click(object sender, RoutedEventArgs e)
        {

            //CreateWatermarkOnImage(rootFolderFilePath + @"Images\testBaseFallout4.png", rootFolderFilePath + @"\Images\testSvaalbardWatermark.png");

            CreateWatermarkOnImage(baseImageFilePath, waterMarkImageFilePath);

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// return a string of the image's file path
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

        private void btn_loadBaseImage_Click(object sender, RoutedEventArgs e)
        {

            baseImageFilePath = GetImageFilePath();
            img_basePreview.Source = new BitmapImage(new Uri(baseImageFilePath));
        }

        private void btn_loadWatermarkImage_Click(object sender, RoutedEventArgs e)
        {
            waterMarkImageFilePath = GetImageFilePath();
            img_watermarkPreview.Source = new BitmapImage(new Uri(waterMarkImageFilePath));
        }
    }
}

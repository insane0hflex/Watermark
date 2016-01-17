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
using Waterwark.Utilities;

namespace Waterwark
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

        string rootFolderFilePath;
        Bitmap baseImage;
        Bitmap waterMarkImage;
        Bitmap finalImage;

        public MainWindow()
        {
            InitializeComponent();

        }


        public void CreateWatermarkOnImage(string sourceFilePath, string watermarkImageFilePath)
        {

            try
            {


                //baseImage = (Bitmap)System.Drawing.Image.FromFile(sourceFilePath);

                baseImage = GetImageFile();

                //for some reason the (implicit?) casting up from 24bit depth to 32bit depth messes up the bmp
                //properties of Horizontal and Veritical resolution to like 95.9. This explicitly sets it back to 100%
                //baseImage.SetResolution(100, 100);

                img_basePreview.Source = new BitmapImage(new Uri(sourceFilePath));


                waterMarkImage = (Bitmap)System.Drawing.Image.FromFile(watermarkImageFilePath);

                img_watermarkPreview.Source = new BitmapImage(new Uri(watermarkImageFilePath));


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
            rootFolderFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\"));

            CreateWatermarkOnImage(rootFolderFilePath + @"Images\testBaseFallout4.png", rootFolderFilePath + @"\Images\testSvaalbardWatermark.png");
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// Load a user specified .ini file by prompting the user to select the location with a FileDialog
        /// </summary>
        private Bitmap GetImageFile()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".png";
            fileDialog.Filter = "Images (*.png)|*.png";

            //fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.InitialDirectory = Environment.CurrentDirectory;

            bool? userConfirmed = fileDialog.ShowDialog();

            try
            {
                // Get the selected file name and display in a TextBox 
                if (userConfirmed == true)
                {
                    Bitmap selectedImage = new Bitmap(fileDialog.FileName);
                    return selectedImage;
                }
                else
                {
#warning return a dummy image instead ie a small 40x40 image with "missing" or something
                    return null;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error occurred trying to load (read) the selected image file.\n\n";
                CustomMessageBox.Error(errorMessage, ex);

                return null;
            }
        }

    }
}

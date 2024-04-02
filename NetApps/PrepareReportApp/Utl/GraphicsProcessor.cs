using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PrepareReport.Utl
{
    public class GraphicsProcessor
    {
        public GraphicsProcessor()
        {
        }

        public static bool TiffTo(string filePath, string outputPath, int[] pgNumbers, ImageFormat imgFormat)
        {
            string ext = "";
            if (imgFormat == ImageFormat.Png)
                ext = "png";
            else if (imgFormat == ImageFormat.Tiff)
                ext = "tiff";
            else if (imgFormat == ImageFormat.Jpeg)
                ext = "jpeg";
            else if (imgFormat == ImageFormat.Gif)
                ext = "gif";
            else
            {
                imgFormat = ImageFormat.Bmp;
                ext = "bmp";
            }

            Image imgTiff = Image.FromFile(filePath);
            int nCount = imgTiff.GetFrameCount(FrameDimension.Page);
            Image[] images = getTiffImages(imgTiff, pgNumbers);
            for (int i = 0; i < images.Length; i++)
            {
                string pathToSave = Path.Combine(outputPath, pgNumbers[i].ToString() + "." + ext);
                images[i].Save(pathToSave, imgFormat);
            }
            return true;
        }

        public static Image[] getTiffImages(Image sourceImage, int[] pageNumbers)
        {
            MemoryStream ms = null;
            Image[] returnImage = new Image[pageNumbers.Length];

            try
            {
                Guid objGuid = sourceImage.FrameDimensionsList[0];
                FrameDimension objDimension = new FrameDimension(objGuid);

                for (int i = 0; i < pageNumbers.Length; i++)
                {
                    ms = new MemoryStream();
                    sourceImage.SelectActiveFrame(objDimension, pageNumbers[i]-1);
                    sourceImage.Save(ms, ImageFormat.Tiff);
                    returnImage[i] = Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ms.Close();
            }
            return returnImage;
        }


        public static string[] ConvertJpegToTiff(string[] fileNames, bool isMultipage)
        {
            EncoderParameters encoderParams = new EncoderParameters(1);
            ImageCodecInfo tiffCodecInfo = ImageCodecInfo.GetImageEncoders()
                .First(ie => ie.MimeType == "image/tiff");

            string[] tiffPaths = null;
            if (isMultipage)
            {
                tiffPaths = new string[1];
                System.Drawing.Image tiffImg = null;
                try
                {
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        if (i == 0)
                        {
                            tiffPaths[i] = String.Format("{0}\\{1}.tif",
                                Path.GetDirectoryName(fileNames[i]),
                                Path.GetFileNameWithoutExtension(fileNames[i]));

                            // Initialize the first frame of multipage tiff.
                            tiffImg = System.Drawing.Image.FromFile(fileNames[i]);
                            encoderParams.Param[0] = new EncoderParameter(
                                System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
                            tiffImg.Save(tiffPaths[i], tiffCodecInfo, encoderParams);
                        }
                        else
                        {
                            // Add additional frames.
                            encoderParams.Param[0] = new EncoderParameter(
                                System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                            using (System.Drawing.Image frame = System.Drawing.Image.FromFile(fileNames[i]))
                            {
                                tiffImg.SaveAdd(frame, encoderParams);
                            }
                        }

                        if (i == fileNames.Length - 1)
                        {
                            // When it is the last frame, flush the resources and closing.
                            encoderParams.Param[0] = new EncoderParameter(
                                System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.Flush);
                            tiffImg.SaveAdd(encoderParams);
                        }
                    }
                }
                finally
                {
                    if (tiffImg != null)
                    {
                        tiffImg.Dispose();
                        tiffImg = null;
                    }
                }
            }
            else
            {
                tiffPaths = new string[fileNames.Length];

                for (int i = 0; i < fileNames.Length; i++)
                {
                    tiffPaths[i] = String.Format("{0}\\{1}.tif",
                        Path.GetDirectoryName(fileNames[i]),
                        Path.GetFileNameWithoutExtension(fileNames[i]));

                    // Save as individual tiff files.
                    using (System.Drawing.Image tiffImg = System.Drawing.Image.FromFile(fileNames[i]))
                    {
                        tiffImg.Save(tiffPaths[i], ImageFormat.Tiff);
                    }
                }
            }

            return tiffPaths;
        }
    }
}

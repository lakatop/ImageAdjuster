using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageAdjuster
{
	/// <summary>
	/// Abstract base class for every converter
	/// </summary>
	abstract class Converter
	{
		/// <summary>
		/// Converts image to the class specific style
		/// </summary>
		/// <param name="font">Font with which image will be converted, or based on its size the matrix for conversion will be created</param>
		/// <param name="img">Source image which will be converted</param>
		/// <param name="subset">Either some character subset or null if there is none</param>
		/// <param name="colorConversion">Defines if conversion is colorful or black-and-white</param>
		/// <returns>Converted source image</returns>
		abstract public Image Convert(Font font, Image img, string subset, bool colorConversion);
	}
	/// <summary>
	/// Abstract base class for every image converter
	/// </summary>
	abstract class ImageConverter : Converter
	{
		/// <summary>
		/// Converts only selected part of source image.
		/// </summary>
		/// <param name="font"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="img"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="subset"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="colorConversion"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <returns>Source image with converted selected area</returns>
		abstract public Image ConvertSelectedArea(Font font, Image img, string subset, bool colorConversion);
		/// <summary>
		/// Converts full source image.
		/// </summary>
		/// <param name="font"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="img"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="subset"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <param name="colorConversion"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <returns>Fully converted source image</returns>
		abstract public Image ConvertFullImage(Font font, Image img, string subset, bool colorConversion);
		/// <value>Determines if source image has some selected area</value>
		public bool selectedArea { get; set; }
	}

	/// <summary>
	/// Class for converting image to ASCII Art
	/// </summary>
	class ImageToASCIIConverter : ImageConverter
	{
		/// <value>Static pointer to this instance of <see cref="ImageToASCIIConverter"/></value>
		private static ImageToASCIIConverter instance = null;
		/// <summary>
		/// Static method to retrieve pointer to this instance.
		/// </summary>
		/// <returns><see cref="instance"/></returns>
		public static ImageToASCIIConverter GetInstance
		{
			get
			{
				if(instance == null)
				{
					instance = new ImageToASCIIConverter();
				}

				return instance;
			}
		}
		/// <summary>
		/// Constructor to this <see cref="ImageToASCIIConverter"/> instance
		/// </summary>
		private ImageToASCIIConverter()
		{
			charHolders = new List<ASCIICharHolder>();
			selectedArea = false;
		}

		/// <summary>
		/// Based on given font and subset returns appropriate ASCIICharHolder
		/// </summary>
		/// <param name="font">font of desired holder</param>
		/// <param name="subset">string subset of desired holder</param>
		/// <returns>Appropriate holder with given font and subset</returns>
		ASCIICharHolder GetAppropriateHolder(Font font, string subset)
		{
			ASCIICharHolder holder = null;
			if ((holder = charHolders.Find(x => x.Equals(font, subset))) == null)
			{
				if (subset == null)
				{
					holder = new FullASCIICharHolder(font);
				}
				else
				{
					holder = new FixedASCIICharHolder(font, subset);
				}
				charHolders.Add(holder);
			}

			return holder;
		}

		/// <see cref="Converter.Convert(Font, Image, string, bool)"/>
		public override Image Convert(Font font, Image img, string subset, bool colorConversion)
		{
			if (selectedArea)
			{
				return ConvertSelectedArea(font, img, subset, colorConversion);
			}
			else
			{
				return ConvertFullImage(font, img, subset, colorConversion);
			}
		}

		/// <see cref="ImageConverter.ConvertSelectedArea(Font, Image, string, bool)"/>
		public override Image ConvertSelectedArea(Font font, Image img, string subset, bool colorConversion)
		{
			ASCIICharHolder holder = GetAppropriateHolder(font, subset);
			try
			{
				int x0 = Math.Min(Form1.rectStartPoint.X, Form1.rectEndPoint.X);
				int y0 = Math.Min(Form1.rectStartPoint.Y, Form1.rectEndPoint.Y);
				int width = Math.Abs(Form1.rectStartPoint.X - Form1.rectEndPoint.X);
				int height = Math.Abs(Form1.rectStartPoint.Y - Form1.rectEndPoint.Y);
				if(font.Size > width || font.Height > height)
				{
					return img;
				}
				Rectangle cloneRect = new Rectangle(x0, y0, width, height);
				Image convertPartImage = ((Bitmap)img).Clone(cloneRect, img.PixelFormat);
				ImageGrayScaler imgProcesser = new ImageGrayScaler((Bitmap)convertPartImage);
				originalImg = (Bitmap)convertPartImage;
				convertPartImage = imgProcesser.ConvertToGrayScale();
				convertPartImage = ConvertToASCII((Bitmap)convertPartImage, font, holder, colorConversion);
				using (Graphics g = Graphics.FromImage(img))
				{
					g.DrawImage(convertPartImage, cloneRect);
				}

				return img;
			}
			catch (Exception)
			{
				throw new Exception("Image conversion failed");
			}

		}
		/// <see cref="ImageConverter.ConvertFullImage(Font, Image, string, bool)"/>
		public override Image ConvertFullImage(Font font, Image img, string subset, bool colorConversion)
		{
			ImageGrayScaler imgProcesser = new ImageGrayScaler((Bitmap)img);
			originalImg = (Bitmap)img;
			img = imgProcesser.ConvertToGrayScale();
			ASCIICharHolder holder = GetAppropriateHolder(font, subset);
			try
			{
				return ConvertToASCII((Bitmap)img, font, holder, colorConversion);
			}
			catch (Exception)
			{
				throw new Exception("Image conversion failed");
			}
		}
		/// <summary>
		/// Converts given image to ascii_art.
		/// </summary>
		/// <param name="img">Image that will be converted</param>
		/// <param name="font">Font of ascii_art image. Also determines width and height of conversion matrix</param>
		/// <param name="holder">Appropriate holder for converting this image</param>
		/// <param name="colorConversion"><see cref="Converter.Convert(Font, Image, string,bool)"/></param>
		/// <returns>Image covnerted to ascii_art</returns>
		private Image ConvertToASCII(Bitmap img, Font font, ASCIICharHolder holder, bool colorConversion)
		{
			Bitmap retBmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
			int width = (int)font.Size;
			int height = font.Height;

			PixelFormat iFormat = img.PixelFormat;

			unsafe
			{
				BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, iFormat);
				BitmapData dstData = retBmp.LockBits(new Rectangle(0, 0, retBmp.Width, retBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				BitmapData origSrcData = originalImg.LockBits(new Rectangle(0, 0, originalImg.Width, originalImg.Height), ImageLockMode.ReadOnly, iFormat);

				int dataSrcBytesPerPixel = Image.GetPixelFormatSize(iFormat) / 8;
				int dataDstBytesPerPixel = Image.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;

				byte* srcPointer, asciiPointer, dstPointer, orgPointer;

				Point leftUpperCorner = new Point(0, 0);

				for (int imgY = 0; imgY < img.Height; imgY += height)
				{
					for(int imgX = 0; imgX < img.Width; imgX += width)
					{
						uint aRed = 0, aGreen = 0, aBlue = 0;
						Bitmap asciiRestrictedBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
						BitmapData asciiData = asciiRestrictedBmp.LockBits(new Rectangle(0, 0, asciiRestrictedBmp.Width, asciiRestrictedBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
						for(int y = 0; y < height; y++)
						{
							for (int x = 0; x < width; x++)
							{
								srcPointer = (byte*)srcData.Scan0 + (y + imgY) * srcData.Stride + (x + imgX) * dataSrcBytesPerPixel;
								orgPointer = (byte*)origSrcData.Scan0 + (y + imgY) * origSrcData.Stride + (x + imgX) * dataSrcBytesPerPixel;
								asciiPointer = (byte*)asciiData.Scan0 + y * asciiData.Stride + x * dataDstBytesPerPixel;
								asciiPointer[0] = srcPointer[0];
								asciiPointer[1] = srcPointer[1];
								asciiPointer[2] = srcPointer[2];

								aBlue += orgPointer[0];
								aGreen += orgPointer[1];
								aRed += orgPointer[2];
							}
						}

						asciiRestrictedBmp.UnlockBits(asciiData);
						ASCIIChar asciiChar = new ASCIIChar(asciiRestrictedBmp);
						Bitmap asciiCharBmp = holder.GetClosestChar(asciiChar.fullIntensity).bmp;

						PixelFormat iFormat2 = asciiCharBmp.PixelFormat;

						int dataAsciiBytesPerPixel = Image.GetPixelFormatSize(iFormat2) / 8;

						BitmapData asciiBmpData = asciiCharBmp.LockBits(new Rectangle(0, 0, asciiCharBmp.Width, asciiCharBmp.Height), ImageLockMode.ReadOnly, iFormat);

						if (colorConversion)
						{
							aBlue /= (uint)(width * height);
							aGreen /= (uint)(width * height);
							aRed /= (uint)(width * height);

							for (int y = 0; y < height; y++)
							{
								for (int x = 0; x < width; x++)
								{
									dstPointer = (byte*)dstData.Scan0 + (y + imgY) * dstData.Stride + (x + imgX) * dataDstBytesPerPixel;
									asciiPointer = (byte*)asciiBmpData.Scan0 + y * asciiBmpData.Stride + x * dataAsciiBytesPerPixel;
									dstPointer[0] = (asciiPointer[0] < 255) ? (byte)aBlue : (byte)255;
									dstPointer[1] = (asciiPointer[1] < 255) ? (byte)aGreen : (byte)255;
									dstPointer[2] = (asciiPointer[2] < 255) ? (byte)aRed : (byte)255;
								}
							}
						}
						else
						{
							for (int y = 0; y < height; y++)
							{
								for (int x = 0; x < width; x++)
								{
									dstPointer = (byte*)dstData.Scan0 + (y + imgY) * dstData.Stride + (x + imgX) * dataDstBytesPerPixel;
									asciiPointer = (byte*)asciiBmpData.Scan0 + y * asciiBmpData.Stride + x * dataAsciiBytesPerPixel;
									dstPointer[0] = asciiPointer[0];
									dstPointer[1] = asciiPointer[1];
									dstPointer[2] = asciiPointer[2];
								}
							}
						}

						asciiCharBmp.UnlockBits(asciiBmpData);

						leftUpperCorner.X += width;

						if((imgX + (width * 2) > img.Width) && (imgX + width != img.Width))
						{
							imgX = img.Width - (width * 2);
							leftUpperCorner.X = img.Width - width;
						}
					}

					leftUpperCorner.Y += height;
					leftUpperCorner.X = 0;

					if((imgY + (height * 2) >= img.Height) && (imgY + height != img.Height))
					{
						imgY = img.Height - (height * 2);
						leftUpperCorner.Y = img.Height - height;
					}
				}

				retBmp.UnlockBits(dstData);
				img.UnlockBits(srcData);
				originalImg.UnlockBits(origSrcData);
			}

			return retBmp;
		}
		/// <value>Ascii char subset which can be used for ascii_art conversion. Already sorted based on intensity</value>
		public readonly static string asciiSubset = " .,:;ox%#@";
		/// <see cref="asciiSubset"/>
		public readonly static string asciiSubset2 = " o@";
		///<value>Represents original image before grayscale conversion</value>
		private Bitmap originalImg { get; set; }

		/// <value>List of already created ASCIICharHolders</value>
		protected List<ASCIICharHolder> charHolders { get; set; }
	}
	/// <summary>
	/// Class for converting image to pixelization
	/// </summary>
	class ImagePixelizationConverter : ImageConverter
	{
		/// <value>Static pointer to this instance of <see cref="ImagePixelizationConverter"/></value>
		private static ImagePixelizationConverter instance = null;
		/// <summary>
		/// Static method to retrieve pointer to this instance.
		/// </summary>
		/// <returns><see cref="instance"/></returns>
		public static ImagePixelizationConverter GetInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new ImagePixelizationConverter();
				}

				return instance;
			}
		}
		/// <summary>
		/// Constructor to this <see cref="ImageToASCIIConverter"/> instance
		/// </summary>
		private ImagePixelizationConverter()
		{
			selectedArea = false;
		}
		/// <see cref="Converter.Convert(Font, Image, string, bool)"/>
		public override Image Convert(Font font, Image img, string subset, bool colorConversion = true)
		{
			try
			{
				if (selectedArea)
				{
					return ConvertSelectedArea(font, img, subset);
				}
				else
				{
					return ConvertFullImage(font, img, subset);
				}
			}
			catch (Exception)
			{
				throw new Exception("Image conversion failed");
			}
		}
		/// <see cref="ImageConverter.ConvertSelectedArea(Font, Image, string, bool)"/>
		public override Image ConvertSelectedArea(Font font, Image img, string subset, bool colorConversion = true)
		{
			int x0 = Math.Min(Form1.rectStartPoint.X, Form1.rectEndPoint.X);
			int y0 = Math.Min(Form1.rectStartPoint.Y, Form1.rectEndPoint.Y);
			int width = Math.Abs(Form1.rectStartPoint.X - Form1.rectEndPoint.X);
			int height = Math.Abs(Form1.rectStartPoint.Y - Form1.rectEndPoint.Y);
			Rectangle cloneRect = new Rectangle(x0, y0, width, height);
			Image convertPartImage = ((Bitmap)img).Clone(cloneRect, img.PixelFormat);
			convertPartImage = GetPixelizedImage((Bitmap)convertPartImage, (int)font.Size);
			using (Graphics g = Graphics.FromImage(img))
			{
				g.DrawImage(convertPartImage, cloneRect);
			}

			return img;
		}
		/// <see cref="ImageConverter.ConvertFullImage(Font, Image, string, bool)"/>
		public override Image ConvertFullImage(Font font, Image img, string subset, bool colorConversion = true)
		{
			return GetPixelizedImage((Bitmap)img, (int)font.Size);
		}
		/// <summary>
		/// Applies pixelization effect on given image.
		/// </summary>
		/// <param name="img">Source image which will bew converted</param>
		/// <param name="size">Width and height of pixelization matrix</param>
		/// <returns>Converted image</returns>
		private Image GetPixelizedImage(Bitmap img, int size)
		{
			Bitmap retBmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
			PixelFormat iFormat = img.PixelFormat;

			unsafe
			{
				BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, iFormat);
				BitmapData dstData = retBmp.LockBits(new Rectangle(0, 0, retBmp.Width, retBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				byte* dstPointer;
				byte* srcPointer;
				byte R, G, B;
				int sumR, sumG, sumB;
				int area;
				int dataSrcBytesPerPixel = Image.GetPixelFormatSize(iFormat) / 8;
				int dataDstBytesPerPixel = Image.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;
				for(int imgY = 0; imgY < img.Height; imgY += size)
				{
					for(int imgX = 0; imgX < img.Width; imgX += size)
					{
						sumR = sumG = sumB = 0;
						int matrixX = Math.Min(imgX + size, img.Width);
						int matrixY = Math.Min(imgY + size, img.Height);
						for(int y = imgY; y < matrixY; y++)
						{
							srcPointer = (byte*)srcData.Scan0 + y * srcData.Stride + imgX * dataSrcBytesPerPixel;
							for(int x = imgX; x < matrixX; x++)
							{
								sumB += srcPointer[0];
								sumG += srcPointer[1];
								sumR += srcPointer[2];
								srcPointer += dataSrcBytesPerPixel;
							}
						}

						area = (matrixX - imgX) * (matrixY - imgY);
						R = (byte)(sumR / area);
						G = (byte)(sumG / area);
						B = (byte)(sumB / area);

						for (int y = imgY; y < matrixY; y++)
						{
							dstPointer = (byte*)dstData.Scan0 + y * dstData.Stride + imgX * dataDstBytesPerPixel;
							for (int x = imgX; x < matrixX; x++)
							{
								dstPointer[0] = B;
								dstPointer[1] = G;
								dstPointer[2] = R;
								dstPointer += dataDstBytesPerPixel;
							}
						}

					}
				}

				retBmp.UnlockBits(dstData);
				img.UnlockBits(srcData);
			}

			return retBmp;
		}
	}
}
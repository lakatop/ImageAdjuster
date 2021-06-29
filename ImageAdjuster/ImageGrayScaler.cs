using System.Drawing;
using System.Drawing.Imaging;

namespace ImageAdjuster
{
	/// <summary>
	/// Class for converting colored images to grayscale
	/// </summary>
	class ImageGrayScaler
	{
		/// <summary>
		/// Contructor of this <see cref="ImageGrayScaler"/> instance
		/// </summary>
		/// <param name="img"><see cref="img"/></param>
		public ImageGrayScaler(Bitmap img)
		{
			this.img = img;
		}
		/// <summary>
		/// Converts <see cref="img"/> into its grayscale representation
		/// </summary>
		/// <returns>Converted image</returns>
		public Image ConvertToGrayScale()
		{
			Bitmap retBmp = new Bitmap(img.Width, img.Height);

			unsafe
			{
				BitmapData srcData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
				BitmapData dstData = retBmp.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);

				byte* dstPointer = (byte*)dstData.Scan0.ToPointer();
				byte* srcPointer = (byte*)srcData.Scan0.ToPointer();
				byte bitsPerPixel = (byte)Image.GetPixelFormatSize(img.PixelFormat);
				int height = img.Height;
				int width = img.Width;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						byte* dst = dstPointer + i * dstData.Stride + j * bitsPerPixel / 8;
						byte* src = srcPointer + i * srcData.Stride + j * bitsPerPixel / 8;
						byte gray = ComputeGrayScale(src[0], src[1], src[2]);
						dst[0] = gray;
						dst[1] = gray;
						dst[2] = gray;
					}
				}
				img.UnlockBits(srcData);
				retBmp.UnlockBits(dstData);
			}

			return retBmp;
		}
		/// <summary>
		/// Takes 3 color components and returns their average
		/// </summary>
		/// <param name="r">Red color component</param>
		/// <param name="g">Green color component</param>
		/// <param name="b">Blue color component</param>
		/// <returns>Average of color components</returns>
		byte ComputeGrayScale(byte r, byte g, byte b)
		{
			return (byte)((r + b + g) / 3);
		}
		/// <value>Image which will be converted to grayscale</value>
		private Bitmap img { get; set; }
	}
}

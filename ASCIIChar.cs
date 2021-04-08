using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace ImageAdjuster
{
	/// <summary>
	/// Interface for representing ASCII character
	/// </summary>
	interface IChars : IComparable
	{
		/// <summary>
		/// Computes intensity of top area of ASCII char bitmap.
		/// </summary>
		/// <returns>Sum of intensities of bitmaps top area</returns>
		uint GetTopIntensity();
		/// <summary>
		/// Computes intensity of left area of ASCII char bitmap.
		/// </summary>
		/// <returns>Sum of intensities of bitmaps left area</returns>
		uint GetLeftIntesity();
		/// <summary>
		/// Computes intensity of right area of ASCII char bitmap.
		/// </summary>
		/// <returns>Sum of intensities of bitmaps right area</returns>
		uint GetRightIntensity();
		/// <summary>
		/// Computes intensity of bottom area of ASCII char bitmap.
		/// </summary>
		/// <returns>Sum of intensities of bitmaps bottom area</returns>
		uint GetBottomIntensity();
		/// <summary>
		/// Computes intensity of center area of ASCII char bitmap.
		/// </summary>
		/// <returns>Sum of intensities of bitmaps center area</returns>
		uint GetCenterIntensity();
		/// <summary>
		/// Computes normalized intensity of whole ASCII char bitmap.
		/// </summary>
		/// <returns>Normalized intensity of whole ASCII char bitmap</returns>
		uint GetFullIntensity();
		/// <value>Get bitmap which represents this ASCII character</value>
		Bitmap bmp { get; }
	}
	/// <summary>
	/// Class for representing ASCII character
	/// </summary>
	class ASCIIChar : IChars
	{
		/// <summary>
		/// Constructor to ASCIIChar instance.
		/// </summary>
		/// <param name="bmp"><see cref="bmp"/></param>
		public ASCIIChar(Bitmap bmp)
		{
			this.bmp = bmp;
			SetSizes();
			this.computeArea = GetComputeAreaSize();
			fullIntensity = GetFullIntensity();
		}
		/// <summary>
		/// Defines CompareTo method since it derives from ICOmparable
		/// </summary>
		/// <param name="obj">Object to compare against</param>
		/// <returns>Result of comparison of fullIntensity</returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			ASCIIChar otherChar = obj as ASCIIChar;
			if (otherChar != null)
			{
				return this.fullIntensity.CompareTo(otherChar.fullIntensity);
			}
			else
			{
				throw new ArgumentException("Object passed to comparison is not ASCIIChar object");
			}
		}
		/// <summary>
		/// Defines IChars.GetTopIntensity
		/// </summary>
		public uint GetTopIntensity()
		{
			uint res = GetAreaIntensity(new Point(0, 0), new Point(topSizeWidth, topSizeHeight));
			return res;
		}
		/// <summary>
		/// Defines IChars.GetLeftIntesity
		/// </summary>
		public uint GetLeftIntesity()
		{
			uint res = GetAreaIntensity(new Point(0, 0), new Point(sideSizeWidth, sideSizeHeight));
			return res;
		}
		/// <summary>
		/// Defines IChars.GetRightIntensity
		/// </summary>
		public uint GetRightIntensity()
		{
			uint res = GetAreaIntensity(new Point(bmp.Width - sideSizeWidth, 0), new Point(bmp.Width, bmp.Height));
			return res;
		}
		/// <summary>
		/// Defines IChars.GetBottomIntensity
		/// </summary>
		public uint GetBottomIntensity()
		{
			uint res = GetAreaIntensity(new Point(0, bmp.Height - topSizeHeight), new Point(bmp.Width, bmp.Height));
			return res;
		}
		/// <summary>
		/// Defines IChars.GetCenterIntensity
		/// </summary>
		public uint GetCenterIntensity()
		{
			uint res = GetAreaIntensity(new Point(sideSizeWidth, topSizeHeight),
				new Point(sideSizeWidth + centerSizeWidth, topSizeHeight + centerSizeHeight));
			return res;
		}
		/// <summary>
		/// Defines IChars.GetFullIntensity
		/// </summary>
		public uint GetFullIntensity()
		{
			uint res = GetTopIntensity();
			res += GetLeftIntesity();
			res += GetRightIntensity();
			res += GetBottomIntensity();
			res += GetCenterIntensity();

			res = (res) / computeArea;

			return res;
		}
		/// <summary>
		/// Defines all area sizes which will later divide image on top,bottom,left,right and center area.
		/// </summary>
		private void SetSizes()
		{
			topSizeWidth = bmp.Width;
			sideSizeHeight = bmp.Height;

			if (bmp.Width % 3 == 0)
			{
				sideSizeWidth = centerSizeWidth = bmp.Width / 3;
			}
			else
			{
				sideSizeWidth = bmp.Width / 3;
				sideSizeWidth = (sideSizeWidth == 0) ? 1 : sideSizeWidth;
				centerSizeWidth = bmp.Width - (2 * sideSizeWidth);
				centerSizeWidth = (centerSizeWidth <= 0) ? 1 : centerSizeWidth;
			}

			if (bmp.Height % 3 == 0)
			{
				topSizeHeight = centerSizeHeight = bmp.Height / 3;
			}
			else
			{
				topSizeHeight = bmp.Height / 3;
				topSizeHeight = (topSizeHeight == 0) ? 1 : topSizeHeight;
				centerSizeHeight = bmp.Height - (2 * topSizeHeight);
				centerSizeHeight = (centerSizeHeight <= 0) ? 1 : centerSizeHeight;
			}
		}
		/// <summary>
		/// Computes intensity of rectangle given by 2 points passed as parameters
		/// </summary>
		/// <param name="leftUpperCorner">Point that represents left upper corner of area rectangle</param>
		/// <param name="righLowerCorner">Point that represents right lower corner of area rectangle</param>
		/// <returns></returns>
		private uint GetAreaIntensity(Point leftUpperCorner, Point righLowerCorner)
		{
			uint res = 0;
			unsafe
			{
				BitmapData srcData = bmp.LockBits(new Rectangle(leftUpperCorner.X,leftUpperCorner.Y,righLowerCorner.X - leftUpperCorner.X, righLowerCorner.Y - leftUpperCorner.Y), 
					ImageLockMode.ReadOnly, bmp.PixelFormat);

				byte bitsPerPixel = (byte)Image.GetPixelFormatSize(bmp.PixelFormat);

				/*This time we convert the IntPtr to a ptr*/
				byte* srcPointer;

				int height = srcData.Height;
				int width = srcData.Width;
				for (int i = 0; i < height; ++i)
				{
					for (int j = 0; j < width; ++j)
					{
						srcPointer = (byte*)srcData.Scan0 + i * srcData.Stride + j * bitsPerPixel / 8;
						res += srcPointer[0];
					}
				}

				bmp.UnlockBits(srcData);

			}
			return res;
		}
		/// <summary>
		/// Computes total number of pixels which are counted in computing whole area intensity
		/// </summary>
		/// <returns>Total number of pixels which are counted in computing whole area intensity</returns>
		private uint GetComputeAreaSize()
		{
			uint ret = 0;

			ret += (uint)(topSizeHeight * topSizeWidth * 2);
			ret += (uint)(sideSizeHeight * sideSizeWidth * 2);
			ret += (uint)(centerSizeHeight * centerSizeWidth);

			return ret;
		}
		/// <summary>
		/// Changes current fullIntensity by intensity passed as parameters
		/// </summary>
		/// <param name="intensity">New intensity</param>
		public void ChangeFullIntensity(uint intensity)
		{
			fullIntensity = intensity;
		}

		/// <see cref="IChars.bmp"/>
		public Bitmap bmp { get; }
		/// <value>Width of left/right area</value>
		private int sideSizeWidth { get; set; }
		/// <value>Height of left/right area</value>
		private int sideSizeHeight { get; set; }
		/// <value>Width of top area</value>
		private int topSizeWidth { get; set; }
		/// <value>Height of top area</value>
		private int topSizeHeight { get; set; }
		/// <value>Width of center area</value>
		private int centerSizeWidth { get; set; }
		/// <value>Height of center area</value>
		private int centerSizeHeight { get; set; }
		/// <value>Total number of pixels which are counted in computing whole area intensity</value>
		private uint computeArea { get; set; }
		/// <value>Intensity of full bmp area</value>
		public uint fullIntensity { get; private set; }
	}
	abstract class ASCIICharHolder
	{
		/// <summary>
		/// Creates new Equals method for comparing 2 ASCIICharHolder instances
		/// </summary>
		/// <param name="obj">Font member variable of second instance</param>
		/// <param name="obj2">String member variable of second instance. Representing asciiSubset</param>
		/// <returns>If these two instances equals</returns>
		public new bool Equals(object obj, object obj2)
		{
			if (obj == null || !obj.GetType().Equals(typeof(Font)))
			{
				return false;
			}
			else
			{
				if (font.Equals(obj as Font))
				{
					if (fixedAscii)
					{
						if (obj2 == null || !obj2.GetType().Equals(typeof(string)))
						{
							return false;
						}
						else
						{
							return asciiSubset.Equals(obj2 as string);
						}
					}
					else
					{
						if (obj2 == null)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					return false;
				}
			}
		}
		/// <summary>
		/// Creates and returns new instance of ASCIIChar
		/// </summary>
		/// <param name="charValue">Printable asii character which will be printed on new ASCIIChar bitmap</param>
		/// <returns>new instance of ASCIIChar</returns>
		protected IChars CreateIChar(byte charValue)
		{
			try
			{
				Image img = CreateImage(System.Text.Encoding.ASCII.GetString(new byte[] { charValue }), font, Color.Black);
				return new ASCIIChar((Bitmap)img);
			}
			catch (Exception)
			{
				throw new Exception("Creation of ASCII char failed");
			}
		}
		/// <summary>
		/// Creates and returns new Image with given text
		/// </summary>
		/// <param name="text">text which will be printed on returned Image</param>
		/// <param name="font">font of printed text</param>
		/// <param name="textColor">color of printed text</param>
		/// <returns>new Image with given text</returns>
		protected Image CreateImage(string text, Font font, Color textColor)
		{
			//set the stringformat flags
			StringFormat sf = new StringFormat();
			sf.Trimming = StringTrimming.Word;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			//create a new image of the right size
			Image img = new Bitmap((int)font.Size, font.Height);

			Graphics drawing = Graphics.FromImage(img);

			//paint the background
			drawing.Clear(Color.White);

			drawing.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;

			//create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			drawing.DrawString(text, font, textBrush, new RectangleF(0, 0, img.Width, img.Height), sf);

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			return img;
		}
		/// <summary>
		/// Finds and returns ASCIIChar whose intensity is closest to the given intensity passed as argument
		/// </summary>
		/// <param name="intensity">Intensity to which is method finding closest ASCIIChar</param>
		/// <returns>ASCIIChar with closest instensity to the passed argument</returns>
		abstract public IChars GetClosestChar(uint intensity);
		/// <summary>
		/// Initialize and fill up <see cref="asciiChars"/> array
		/// </summary>
		abstract public void InitChars();
		/// <summary>
		/// Normalizes intensities in <see cref="asciiChars"/> array
		/// </summary>
		abstract protected void NormalizeIntensities();
		/// <value>Font with whom this holder prints ascii chars</value>
		public Font font { get; protected set; }
		/// <value>Determines whether this holder consists of fixed ascii subset, or whole printable character range</value>
		public bool fixedAscii { get; protected set; }
		/// <value>Fixed ascii characters subset from which <see cref="asciiChars"/> will be filled</value>
		public string asciiSubset { get; protected set; }
		/// <value>ascii characters of this holder</value>
		public IChars[] asciiChars { get; protected set; }
		/// <value>maximal intensity of ascii character from all <see cref="asciiChars"/> in this holder</value>
		protected uint maxIntensity { get; set; }
		/// <value>minimal intensity of ascii character from all <see cref="asciiChars"/> in this holder</value>
		protected uint minIntensity { get; set; }
	}

	class FixedASCIICharHolder : ASCIICharHolder
	{
		/// <summary>
		/// Constructor to FixedASCIICharHolder instance
		/// </summary>
		/// <param name="font"><see cref="ASCIICharHolder.font"/></param>
		/// <param name="asciiSubset"><see cref="ASCIICharHolder.asciiSubset"/></param>
		public FixedASCIICharHolder(Font font, string asciiSubset)
		{
			asciiChars = new IChars[asciiSubset.Length];
			fixedAscii = true;
			this.font = font;
			this.asciiSubset = asciiSubset;
			InitChars();
			NormalizeIntensities();
		}
		/// <see cref="ASCIICharHolder.InitChars"/>
		public override void InitChars()
		{
			for (int i = 0; i < asciiSubset.Length; i++)
			{
				asciiChars[i] = CreateIChar((byte)asciiSubset[i]);
			}
		}
		/// <see cref="ASCIICharHolder.GetClosestChar(uint)"/>
		public override IChars GetClosestChar(uint intensity)
		{
			return asciiChars[(((255 - intensity) * asciiChars.Length) / 256)];
		}
		/// <see cref="ASCIICharHolder.NormalizeIntensities"/>
		protected override void NormalizeIntensities()
		{
			maxIntensity = (asciiChars[0] as ASCIIChar).fullIntensity;
			minIntensity = (asciiChars[asciiChars.Length - 1] as ASCIIChar).fullIntensity;
			foreach (var item in asciiChars)
			{
				uint intensity = (item as ASCIIChar).fullIntensity;
				intensity = ((intensity - minIntensity) * 255) / (maxIntensity - minIntensity);
				(item as ASCIIChar).ChangeFullIntensity(intensity);
			}
		}
	}

	class FullASCIICharHolder : ASCIICharHolder
	{
		/// <summary>
		/// Constructor to FullASCIICharHolder
		/// </summary>
		/// <param name="font"><see cref="ASCIICharHolder.font"/></param>
		public FullASCIICharHolder(Font font)
		{
			asciiChars = new IChars[95];
			this.font = font;
			InitChars();
			NormalizeIntensities();
		}
		/// <see cref="ASCIICharHolder.GetClosestChar(uint)"/>
		public override IChars GetClosestChar(uint intensity)
		{
			ASCIIChar retVal = asciiChars[0] as ASCIIChar;
			int lowerBound = 0;
			int upperBound = asciiChars.Length - 1;
			while (lowerBound <= upperBound)
			{
				int index = lowerBound + (upperBound - lowerBound) / 2;
				retVal = (ASCIIChar)asciiChars[index];
				if (retVal.fullIntensity == intensity)
				{
					return retVal;
				}
				if (retVal.fullIntensity < intensity)
				{
					lowerBound = index + 1;
				}
				else
				{
					upperBound = index - 1;
				}
			}

			return retVal;
		}
		/// <see cref="ASCIICharHolder.InitChars"/>
		public override void InitChars()
		{
			for (int i = 32; i < 127; i++)
			{
				asciiChars[i - 32] = CreateIChar((byte)i);
			}

			Array.Sort(asciiChars);
		}
		/// <see cref="ASCIICharHolder.NormalizeIntensities"/>
		protected override void NormalizeIntensities()
		{
			minIntensity = (asciiChars[0] as ASCIIChar).fullIntensity;
			maxIntensity = (asciiChars[asciiChars.Length - 1] as ASCIIChar).fullIntensity;
			foreach (var item in asciiChars)
			{
				uint intensity = (item as ASCIIChar).fullIntensity;
				intensity = ((intensity - minIntensity) * 255) / (maxIntensity - minIntensity);
				(item as ASCIIChar).ChangeFullIntensity(intensity);
			}
		}
	}
}

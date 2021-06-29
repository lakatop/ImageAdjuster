using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageAdjuster
{
	/// <summary>
	/// Class for saving converted images
	/// </summary>
	class ImageSaver
	{
		/// <summary>
		/// constructor of this <see cref="ImageSaver"/> instance
		/// </summary>
		/// <param name="fileDialog"><see cref="fileDialog"/></param>
		public ImageSaver(SaveFileDialog fileDialog)
		{
			this.fileDialog = fileDialog;
		}
		/// <summary>
		/// Saves given image to folder given by <see cref="fileDialog"/>
		/// </summary>
		/// <param name="img">Image that will be saved</param>
		public void SaveImage(Image img)
		{
			fileDialog = new SaveFileDialog();
			fileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG;)|*.BMP;*.JPG;*.PNG;";
			ImageFormat format = ImageFormat.Png;
			if(fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string extension = System.IO.Path.GetExtension(fileDialog.FileName).ToLower();
				switch (extension)
				{
					case ".bmp":
						{
							format = ImageFormat.Bmp;
							break;	
						}
					case ".jpg":
						{
							format = ImageFormat.Jpeg;
							break;
						}
					case ".png":
						{
							format= ImageFormat.Png;
							break;
						}
				}
				img.Save(fileDialog.FileName, format);
			}
		}
		/// <value>SaveFileDialog which handles saving dialog</value>
		private SaveFileDialog fileDialog { get; set; }
	}
}

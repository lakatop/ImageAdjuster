using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;


namespace ImageAdjuster
{
	/// <summary>
	/// ImageLoader class
	/// </summary>
	class ImageLoader
	{
		/// <summary>
		/// Constructor of this <see cref="ImageLoader"/> instance
		/// </summary>
		/// <param name="fileDialog"><see cref="fileDialog"/></param>
		public ImageLoader(OpenFileDialog fileDialog)
		{
			this.fileDialog = fileDialog;
		}
		/// <summary>
		/// Opens file dialog and lets user choose image file that will be loaded.
		/// </summary>
		/// <returns>Image that user chose</returns>
        public Image LoadImage()
        {
			try
			{
				Image img = null;
				string filePath;
				fileDialog = new OpenFileDialog();
				fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
				fileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG;)|*.BMP;*.JPG;*.PNG;";
				fileDialog.FilterIndex = 2;
				fileDialog.RestoreDirectory = true;
				fileDialog.ShowHelp = true;

				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					filePath = fileDialog.FileName;
					Form1.filePath = filePath;
					img = Image.FromFile(filePath);
				}

				return img;
			}
			catch (Exception)
			{
				throw new Exception("Image loading failed");
			}
        }
		/// <value>OpenFileDialog of this class which will handle file dialog</value>
        private OpenFileDialog fileDialog { get; set; }
	}
}

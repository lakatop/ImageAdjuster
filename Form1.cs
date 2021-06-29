using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageAdjuster
{
	/// <summary>
	/// Defines application components and their functionality
	/// </summary>
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			InitComponents();
		}

		private void InitComponents()
		{
			button1.Text = "Convert";
			button4.Text = "Load Image";
			button2.Text = "Save Image";
			label1.Text = "Font Name";
			label2.Text = "Size";
			label3.Text = "ASCII Chars";
			label4.Text = "Convert method";
			checkBox1.Text = "Selection Mode";
			checkBox2.Text = "Color Conversion Mode";
			numericUpDown1.Value = 5;
			numericUpDown1.Minimum = 2;

			comboBox1.Items.AddRange(new object[] {"Arial",
						"Times New Roman", "Comic Sans MS", "Bookman Old Style",
						"Calibri", "Stencil", "Monospace", "Monaco"});
			comboBox1.SelectedIndex = 0;
			comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

			comboBox2.Items.AddRange(new object[] { "All printables", ImageToASCIIConverter.asciiSubset, ImageToASCIIConverter.asciiSubset2 });
			comboBox2.SelectedIndex = 0;
			comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

			comboBox3.Items.AddRange(new object[] { "ASCII_Art", "Pixelization" });
			comboBox3.SelectedIndex = 0;
			comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

			checkBox1.Checked = false;
			selecting = false;
		}
		/// <summary>
		/// Conversion
		/// </summary>
		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				string fontName = comboBox1.Text;
				int fontSize = (int)numericUpDown1.Value;
				Font font = new Font(fontName, fontSize);
				ConverterFactory sorter = new ConverterFactory((Conversion)comboBox3.SelectedIndex, (checkBox1.Checked) ? selected : false);
				Converter converter = sorter.GetAppropriateConverter();
				if (converter == null)
				{
					return;
				}
				pictureBox2.Image = null;

				try
				{
					if (checkBox1.Checked && selected == false)
					{
						pictureBox2.Image = Image.FromFile(filePath);
					}
					else if (pictureBox1.Image == null)
					{
						pictureBox2.Image = null;
						throw new ArgumentNullException();
					}
					else
					{
						Image convertedImg = converter.Convert(font, Image.FromFile(filePath),
											(comboBox2.SelectedIndex == 0) ? null : comboBox2.Text, checkBox2.Checked);
						pictureBox2.Image = convertedImg;
					}
				}
				catch (System.ArgumentNullException)
				{
					MessageBox.Show("No image selected", "Image error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch (Exception)
				{
					throw;
				}

				pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>
		/// Loading image
		/// </summary>
		private void button4_Click(object sender, EventArgs e)
		{
			try
			{
				ImageLoader loader = new ImageLoader(openFileDialog1);
				Image img = loader.LoadImage();
				pictureBox1.Image = img;
				selected = false;
				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			}
			catch (System.IO.FileNotFoundException)
			{
				MessageBox.Show("File could not be found", "File error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>
		/// Saving image
		/// </summary>
		private void button2_Click(object sender, EventArgs e)
		{
			if (pictureBox2.Image != null)
			{
				ImageSaver saver = new ImageSaver(saveFileDialog1);
				saver.SaveImage(pictureBox2.Image);
			}
			else
			{
				MessageBox.Show("There is no file to save", "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Start of selection
		/// </summary>
		private void pictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (checkBox1.Checked && pictureBox1.Image != null)
			{
				selected = true;
				rectStartPoint = e.Location;
				selecting = true;
			}
		}
		/// <summary>
		/// End of selection
		/// </summary>
		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			if (!selecting)
			{
				return;
			}

			if (checkBox1.Checked && pictureBox1.Image != null)
			{
				rectEndPoint = e.Location;
				selecting = false;
				// define stretch factor of image (if image is bigger/smaller than pictureBox, it will be stretched by this factor)
				float stretch1X = 1f * pictureBox1.Image.Width / pictureBox1.ClientSize.Width;
				float stretch1Y = 1f * pictureBox1.Image.Height / pictureBox1.ClientSize.Height;
				int x = e.X;
				if (e.X > pictureBox1.Width)
				{
					x = pictureBox1.Width;
				}
				else if (e.X < 0)
				{
					x = 0;
				}

				int y = e.Y;
				if (e.Y > pictureBox1.Height)
				{
					y = pictureBox1.Height;
				}
				else if (e.Y < 0)
				{
					y = 0;
				}

				rectEndPoint = new Point((int)(x * stretch1X), (int)(y * stretch1Y));
				rectStartPoint = new Point((int)(rectStartPoint.X * stretch1X), (int)(rectStartPoint.Y * stretch1Y));
				pictureBox1.Image = Image.FromFile(filePath);
				using (Graphics gr = Graphics.FromImage(pictureBox1.Image))
				{
					Pen pen = new Pen(Brushes.Red);
					pen.Width = 3.0f;
					gr.DrawRectangle(pen, Math.Min(rectStartPoint.X, rectEndPoint.X), Math.Min(rectStartPoint.Y, rectEndPoint.Y),
						Math.Abs(rectStartPoint.X - rectEndPoint.X), Math.Abs(rectStartPoint.Y - rectEndPoint.Y));
					pen.Dispose();
				}
			}
		}
		/// <summary>
		/// Changing selection mode
		/// </summary>
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				pictureBox1.Image = Image.FromFile(filePath);
				selecting = false;
				selected = false;
			}
		}

		///<value>Filepath to currently selected image</value>
		public static string filePath { get; set; }
		///<value>Left upper corner of selected area</value>
		public static Point rectStartPoint { get; private set; }
		///<value>Right lower corner of selected area</value>
		public static Point rectEndPoint { get; private set; }
		///<value>Defines wheter user is currently selecting area</value>
		private bool selecting { get; set; }
		///<value>Defines wheter some area is selected</value>
		private bool selected { get; set; }
	}
}

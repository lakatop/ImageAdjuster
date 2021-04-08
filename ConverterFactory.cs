namespace ImageAdjuster
{
	/// <value>Types of supported conversions</value>
	enum Conversion { ASCII, Pixelization}
	class ConverterFactory
	{
		/// <summary>
		/// Constructor to this <see cref="ConverterFactory"/> instance
		/// </summary>
		/// <param name="conversion"><see cref="conversion"/></param>
		/// <param name="selectedArea"><see cref="selectedArea"/></param>
		public ConverterFactory(Conversion conversion, bool selectedArea)
		{
			this.conversion = conversion;
			this.selectedArea = selectedArea;
		}
		/// <summary>
		/// Based on given class members chooses and returns appropriate converter
		/// </summary>
		/// <returns>Appropriate converter</returns>
		public Converter GetAppropriateConverter()
		{
			switch (conversion)
			{
				case Conversion.ASCII:
					{
						var imageConverter = ImageToASCIIConverter.GetInstance;
						imageConverter.selectedArea = selectedArea;
						return imageConverter;
					}
				case Conversion.Pixelization:
					{
						var imageConverter = ImagePixelizationConverter.GetInstance;
						imageConverter.selectedArea = selectedArea;
						return imageConverter;
					}
				default:
					return null;
			}
		}
		/// <value>Type of conversion</value>
		private Conversion conversion { get; }
		/// <value>Determines if there is selected area on given image</value>
		private bool selectedArea { get; }
	}
}

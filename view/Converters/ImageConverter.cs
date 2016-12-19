﻿using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Cognitivo.Converters
{
    class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();From DB
            byte[] imageData = value as byte[];
            if (imageData == null || imageData.Length == 0) return null;
            using (var ms = new System.IO.MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();To DB
            BitmapImage image = value as BitmapImage;
            if (image == null) return null;
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(memStream);
            return memStream.GetBuffer();
        }
    }
}

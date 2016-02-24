﻿using System;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media;

namespace Cognitivo.Converters
{
    public class Status2ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value.ToString() == entity.Status.Documents_General.Approved.ToString())
            {
                return new SolidColorBrush(Colors.PaleGreen);
            }
            else if (value != null && value.ToString() == entity.Status.Documents_General.Annulled.ToString())
            {
                return new SolidColorBrush(Colors.Crimson);
            }
            else
            {
                return new SolidColorBrush(Colors.Gainsboro);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, 
                                                System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}

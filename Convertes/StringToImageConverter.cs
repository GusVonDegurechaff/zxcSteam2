using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

//"C:\\Users\\Gus\\Pictures\\NIGHT-RUNNERS_SCREENSHOTS\\2024-09-16_00h32_52.png"

namespace zxcSteam2.Converters // Измените пространство имен, если нужно
{
    // Обработчик добавления изображения игры
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = value as string;

            if (!string.IsNullOrEmpty(imagePath))
            {
                // Формируем полный путь из относительного пути
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath);
                if (File.Exists(fullPath))
                {
                    return new BitmapImage(new Uri(fullPath));
                }
            }

            // Если изображение не найдено, возвращаем пустое изображение или дефолтное
            return new BitmapImage(new Uri("pack://application:,,,/Resources/default_image.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}


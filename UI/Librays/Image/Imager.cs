﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Core.Librarys.Image
{
    public class Imager
    {
        public static BitmapImage Load(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            if (filePath.IndexOf("pack://") != -1)
            {
                StreamResourceInfo info = Application.GetResourceStream(new Uri(filePath, UriKind.RelativeOrAbsolute));
                using (var st = info.Stream)
                {
                    bitmap.StreamSource = st;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }
                byte[] imageData;
                using (var fileStream = new FileStream(@filePath, FileMode.Open, FileAccess.Read))

                using (var binaryReader = new BinaryReader(fileStream))
                {
                    imageData = binaryReader.ReadBytes((int)fileStream.Length);
                    bitmap.StreamSource = new MemoryStream(imageData);
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }

            return bitmap;
        }
    }
}

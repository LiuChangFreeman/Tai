﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace Core.Librarys
{
    public class Iconer
    {
        public static string Get(string processname, string desc)
        {
            string iconName = (processname + desc).Replace(" ", "") + ".png";
            string iconPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path,
                         "AppIcons", iconName);
            if (!File.Exists(iconPath))
            {
                return "pack://application:,,,/Tai;component/Resources/Icons/defaultIcon.png";
            }
            return iconPath;
        }
        /// <summary>
        /// 提取icon为Png格式并保存到程序目录下
        /// </summary>
        /// <param name="file"></param>
        /// <param name="processname"></param>
        /// <param name="desc"></param>
        /// <returns>返回提取到程序目录下的路径</returns>
        public static string ExtractFromFile(string file, string processname, string desc, bool isCheck = true)
        {
            try
            {
                string iconName = (processname + desc).Replace(" ", "") + ".png";
                string iconPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path,
                             "AppIcons", iconName);
                if (isCheck && File.Exists(iconPath))
                {
                    return iconPath;
                }
                string dir = Path.GetDirectoryName(iconPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //  uwp app icon handle

                if (file.IndexOf("WindowsApps") != -1)
                {
                    //  只有在包含此关键字时才去处理

                    //  继续判断是否是uwp程序
                    string appdir = file.Substring(0, file.Length - file.Split('\\').Last().Length);
                    string appxManifestPath = appdir + "AppxManifest.xml";
                    if (File.Exists(appxManifestPath))
                    {
                        //  是uwp程序
                        Debug.WriteLine("is uwp!" + appxManifestPath);
                        //  读取描述文件
                        string manifestText = File.ReadAllText(appxManifestPath);
                        var match = Regex.Match(manifestText, @"<Logo>(.*?)</Logo>");
                        string logoName = match.Groups[1].Value;

                        string iconFile = string.Empty;

                        string logo100 = logoName.Replace(".png", ".scale-100.png");
                        string logo125 = logoName.Replace(".png", ".scale-125.png");
                        string logo150 = logoName.Replace(".png", ".scale-150.png");
                        string logo200 = logoName.Replace(".png", ".scale-200.png");
                        string logo400 = logoName.Replace(".png", ".scale-400.png");

                        if (File.Exists(appdir + logo100))
                        {
                            iconFile = appdir + logo100;
                        }
                        else if (File.Exists(appdir + logo125))
                        {
                            iconFile = appdir + logo125;
                        }
                        else if (File.Exists(appdir + logo150))
                        {
                            iconFile = appdir + logo150;
                        }
                        else if (File.Exists(appdir + logo200))
                        {
                            iconFile = appdir + logo200;
                        }
                        else if (File.Exists(appdir + logo400))
                        {
                            iconFile = appdir + logo400;
                        }
                        else
                        {
                            return string.Empty;
                        }

                        if (!string.IsNullOrEmpty(iconFile) && File.Exists(iconFile))
                        {
                            //  copy to tai dir

                            File.Copy(iconFile, iconPath);
                            return iconPath;
                        }
                        return string.Empty;
                    }
                }



                //  exe app icon handle

                Icon ico = Icon.ExtractAssociatedIcon(file);

                using (var fileStream = new FileStream(iconPath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(ToImageSource(ico) as BitmapSource));
                    encoder.Save(fileStream);
                }
                return iconPath;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + "，File: " + file + "，Process: " + processname);
                return string.Empty;
            }
        }
        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
    }
}

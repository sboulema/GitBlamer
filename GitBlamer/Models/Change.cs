using Caliburn.Micro;
using GitBlamer.Helpers;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GitBlamer.Models
{
    public class Change : PropertyChangedBase
    {
        public Change(string name, string path = "", string status = "")
        {
            Name = name;
            Status = ToStatus(status);
            Path = path;
            Changes = new List<Change>();
        }

        public string Status { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public List<Change> Changes { get; set; }
        public ImageSource ImageSource
        {
            get
            {
                if (Changes.Any())
                {
                    return ToImageSource(IsExpanded ? KnownMonikers.FolderOpened : KnownMonikers.FolderClosed);
                }
                else if (File.Exists(Path))
                {
                    return ToImageSource(Icon.ExtractAssociatedIcon(Path));
                }

                return ToImageSource(KnownMonikers.Document);
            }
        }

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange("ImageSource");
            }
        }

        private static string ToStatus(string status)
        {
            switch (status)
            {
                case "A":
                    return "[Added]";
                case "C":
                    return "[Copied]";
                case "D":
                    return "[Deleted]";
                case "M":
                    return "[Modified]";
                case "R":
                    return "[Renamed]";
                case "T":
                    return "[Type Changed]";
                case "U":
                    return "[Unmerged]";
                case "X":
                    return "[Unknown]";
                case "B":
                    return "[Broken]";
                default:
                    return string.Empty;
            }
        }

        private static ImageSource ToImageSource(Icon icon) 
            => Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

        private ImageSource ToImageSource(ImageMoniker imageMoniker)
        {
            var attributes = new ImageAttributes
            {
                Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags,
                ImageType = (uint)_UIImageType.IT_Bitmap,
                Format = (uint)_UIDataFormat.DF_WPF,
                LogicalHeight = 16,
                LogicalWidth = 16,
                StructSize = Marshal.SizeOf(typeof(ImageAttributes))
            };

            var imageObject = CommandHelper.ImageService.GetImage(imageMoniker, attributes);
            imageObject.get_Data(out var data);
            return data as BitmapSource;
        }
    }
}

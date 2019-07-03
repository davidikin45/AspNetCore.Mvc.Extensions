using AspNetCore.Mvc.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class FileHelperExtensions
    {
        public async static Task<Boolean> SaveToDirectoryAsync(this IFormFile fileUpload, IHostingEnvironment environment, string contentFolder)
        {
            var physicalFolder = Path.Combine(environment.ContentRootPath, contentFolder);

            if (fileUpload != null && fileUpload.Length > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                using (var stream = new FileStream(Path.Combine(physicalFolder, fileName), FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }
                return true;
            }
            return false;
        }

        public async static Task<Boolean> SaveToDirectoryAsync(this IFormFile fileUpload, string physicalFolder)
        {
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                using (var stream = new FileStream(Path.Combine(physicalFolder, fileName), FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }
                return true;
            }
            return false;
        }

        public static Boolean SaveAs(this IFormFile fileUpload, string path)
        {
            if (fileUpload != null && fileUpload.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    fileUpload.CopyTo(stream);
                }
                return true;
            }
            return false;
        }

        public static string ToSlug(this string s)
        {
            return s.ToLower().Replace(" ", "-");
        }

        public static string NameWithoutExtensionAndMain(this FileInfo fileInfo)
        {
            return System.IO.Path.GetFileNameWithoutExtension(fileInfo.FullName).Replace("main", "");
        }

        public static string VirtualPathSlug(this FileInfo fileinfo, IHostingEnvironment hostingEnvironment, int width = 0, int height = 0, int size = 0, int maxWidth = 0, int maxHeight = 0, bool watermark = false)
        {
            var absoluteVirtual = GetAbsoluteVirtualPath(fileinfo.FullName, hostingEnvironment);
            return VirtualPathSlug(absoluteVirtual, width, height, size, maxWidth, maxHeight, watermark);
        }

        public static string ReadFileLine(this FileInfo fileinfo, int line)
        {
            var lines = File.ReadAllLines(fileinfo.FullName);
            if (lines.Length >= line + 1)
            {
                return lines[line];
            }
            return "";
        }

        public static string VirtualPathSlug(this string absoluteVirtual, int width = 0, int height = 0, int size = 0, int maxWidth = 0, int maxHeight = 0, bool watermark = false)
        {
            var parameters = new Dictionary<string, string>();
            if (width != 0)
            {
                parameters.Add("width", width.ToString());
            }

            if (height != 0)
            {
                parameters.Add("height", height.ToString());
            }

            if (size != 0)
            {
                parameters.Add("size", height.ToString());
            }

            if (maxWidth != 0)
            {
                parameters.Add("maxwidth", maxWidth.ToString());
            }

            if (maxHeight != 0)
            {
                parameters.Add("maxheight", maxHeight.ToString());
            }

            if (watermark)
            {
                parameters.Add("watermark", "Y");
            }

            if (parameters.Count > 0)
            {
                absoluteVirtual = absoluteVirtual + "?" + string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value));

            }

            return absoluteVirtual.ToLower().Replace(" ", "-");
        }

        //app-relative virtual path ~/image.jpg
        //absolute virtual path /image.jpg
        public static string VirtualPath(this FileInfo fileinfo, IHostingEnvironment hostingEnvironment)
        {
            return GetAbsoluteVirtualPath(fileinfo.FullName, hostingEnvironment);
        }

        public static string GetAbsoluteVirtualPath(this string physicalPath, IHostingEnvironment hostingEnvironment)
        {
            string appRelativeVirtualPath = "~/" + physicalPath.Replace(hostingEnvironment.WebRootPath + @"\", String.Empty).Replace("\\", "/");
            var absolute = FileHelper.MakeVirtualPathAppAbsolute(appRelativeVirtualPath, "/");

            return absolute;
        }

    }

    public static class FileHelper
    {

        //MapPath
        internal const char appRelativeCharacter = '~';
        public static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath)
        {

            // If the path is exactly "~", just return the app root path
            if (virtualPath.Length == 1 && virtualPath[0] == appRelativeCharacter)
                return applicationPath;

            // If the virtual path starts with "~/" or "~\", replace with the app path
            // relative (ASURT 68628)
            if (virtualPath.Length >= 2 && virtualPath[0] == appRelativeCharacter &&
                (virtualPath[1] == '/' || virtualPath[1] == '\\'))
            {

                if (applicationPath.Length > 1)
                {
                    return applicationPath + virtualPath.Substring(2);
                }
                else
                    return "/" + virtualPath.Substring(2);
            }

            // Return it unchanged
            return virtualPath;
        }

        public static FileInfo GetFileInfo(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path);
            }
            return null;
        }

        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public static DirectoryInfo GetDirectoryInfo(string path)
        {
            if (FolderExists(path))
            {
                return new DirectoryInfo(path);
            }
            return null;
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        private static extern uint FindMimeFromData(uint pBC, [MarshalAs(UnmanagedType.LPStr)] string pwzUrl, [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer, uint cbSize, [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed, uint dwMimeFlags, ref uint ppwzMimeOut, uint dwReserverd);

        public static string GetMimeTypeFromFileName(string fileName)
        {
            return FileHelper.GetMimeTypeFromFileAndRegistry(fileName);
        }

        public static string GetMimeTypeFromFileExtension(string extension)
        {
            return FileHelper.GetMimeTypeFromFileAndRegistry(extension);
        }

        public static string GetMimeTypeFromFileAndRegistry(string filename)
        {
            string result;
            try
            {
                string mimeTypeFromFile = FileHelper.GetMimeTypeFromFile(filename);
                if (!string.IsNullOrWhiteSpace(mimeTypeFromFile))
                {
                    if (mimeTypeFromFile != "text/plain")
                    {
                        if (mimeTypeFromFile != "application/octet-stream")
                        {
                            if (mimeTypeFromFile != "application/x-zip-compressed")
                            {
                                result = mimeTypeFromFile.Replace("pjpeg", "jpeg").Replace("x-png", "png");
                                return result;
                            }
                        }
                    }
                }
                result = FileHelper.GetMimeFromRegistry(filename);
            }
            catch
            {
                result = FileHelper.GetMimeFromRegistry(filename);
            }
            return result;
        }

        public static string GetMimeTypeFromFileNameAndBytes(string filename, byte[] bytes)
        {
            string result;
            try
            {
                string mimeTypeFromFileBytes = FileHelper.GetMimeTypeFromFileBytes(bytes);
                if (!string.IsNullOrWhiteSpace(mimeTypeFromFileBytes))
                {
                    if (mimeTypeFromFileBytes != "text/plain")
                    {
                        if (mimeTypeFromFileBytes != "application/octet-stream")
                        {
                            if (mimeTypeFromFileBytes != "application/x-zip-compressed")
                            {
                                result = mimeTypeFromFileBytes.Replace("pjpeg", "jpeg").Replace("x-png", "png");
                                return result;
                            }
                        }
                    }
                }
                result = FileHelper.GetMimeFromRegistry(filename);
            }
            catch (Exception)
            {
                result = FileHelper.GetMimeFromRegistry(filename);
            }
            return result;
        }

        private static string GetMimeFromRegistry(string Filename)
        {
            string result = "application/octetstream";
            string name = System.IO.Path.GetExtension(Filename).ToLower();
            RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(name);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
            {
                result = registryKey.GetValue("Content Type").ToString();
            }
            return result;
        }

        public static string GetMimeTypeFromFile(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                return FileHelper.GetMimeFromRegistry(filename);
            }
            byte[] array = new byte[256];
            using (System.IO.FileStream fileStream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fileStream.Length >= 256L)
                {
                    fileStream.Read(array, 0, 256);
                }
                else
                {
                    fileStream.Read(array, 0, checked((int)fileStream.Length));
                }
            }
            string result = "";
            try
            {
                uint num = 0;
                FileHelper.FindMimeFromData(0u, null, array, 256u, null, 0u, ref num, 0u);
                IntPtr ptr = new IntPtr((long)((ulong)num));
                result = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
            }
            catch
            {

            }
            return result;
        }

        public static string GetMimeTypeFromFileBytes(byte[] bytes)
        {
            byte[] pBuffer = new byte[256];
            if (bytes.Length >= 256)
            {
                pBuffer = bytes.Take(256).ToArray<byte>();
            }
            else
            {
                pBuffer = bytes;
            }
            string result = "";
            try
            {
                uint num = 0;
                FileHelper.FindMimeFromData(0u, null, pBuffer, 256u, null, 0u, ref num, 0u);
                IntPtr ptr = new IntPtr((long)((ulong)num));
                result = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
            }
            catch
            {

            }
            return result;
        }

        public static string GetDefaultExtension(string mimeType)
        {
            RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("MIME\\Database\\Content Type\\" + mimeType, false);
            object objectValue = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue((registryKey != null) ? registryKey.GetValue("Extension", null) : null);
            return (objectValue != null) ? objectValue.ToString() : string.Empty;
        }

        public static long FileSize(string filename)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filename);
            return fileInfo.Length;
        }

        public static string GetAssociatedProgramE(string FileExtension)
        {
            RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot;
            RegistryKey registryKey2 = Microsoft.Win32.Registry.ClassesRoot;
            string result;
            try
            {
                if (FileExtension.Substring(0, 1) != ".")
                {
                    FileExtension = "." + FileExtension;
                }
                registryKey = registryKey.OpenSubKey(FileExtension.Trim());
                string str = registryKey.GetValue("").ToString();
                registryKey2 = registryKey2.OpenSubKey(str + "\\shell\\open\\command");
                string[] array = registryKey2.GetValue(null).ToString().Split("\"".ToCharArray());
                if (array[0].Trim().Length > 0)
                {
                    result = array[0].Replace("%1", "");
                }
                else
                {
                    result = array[1].Replace("%1", "");
                }
            }
            catch
            {
                result = "";
            }
            return result;
        }

        public static System.Drawing.Image GetFileIconImageFromFileNameAndBytes(string fileName, byte[] bytes, bool large)
        {
            return FileHelper.GetFileIconFromFileNameAndBytes(fileName, bytes, large).ToBitmap();
        }

        public static System.Drawing.Icon GetFileIconFromFileNameAndBytes(string fileName, byte[] bytes, bool large)
        {
            string text = System.IO.Path.GetExtension(fileName);
            if (fileName != "" && (fileName.Last<char>().ToString() == "/" || fileName.Last<char>().ToString() == "\\"))
            {
                return FileHelper.getIcon(text, large);
            }
            if (text != "")
            {
                return FileHelper.getIcon(text, large);
            }
            string mimeTypeFromFileBytes = FileHelper.GetMimeTypeFromFileBytes(bytes);
            text = FileHelper.GetDefaultExtension(mimeTypeFromFileBytes);
            return FileHelper.getIcon(text, large);
        }

        public static System.Drawing.Image GetInstragramLogo(bool large)
        {
            return FileHelper.GetFileIconFromFileName(".insta", large).ToBitmap();
        }

        public static System.Drawing.Image GetFBLogo(bool large)
        {
            return FileHelper.GetFileIconFromFileName(".fb", large).ToBitmap();
        }

        public static System.Drawing.Image GetLinkedInLogo(bool large)
        {
            return FileHelper.GetFileIconFromFileName(".lin", large).ToBitmap();
        }

        public static System.Drawing.Image GetFileIconImageFromFileName(string fileName, bool large)
        {
            return FileHelper.GetFileIconFromFileName(fileName, large).ToBitmap();
        }

        public static System.Drawing.Icon GetFileIconFromFileName(string fileName, bool large)
        {
            string text = System.IO.Path.GetExtension(fileName);
            if (fileName != "" && (fileName.Last<char>().ToString() == "/" || fileName.Last<char>().ToString() == "\\"))
            {
                return FileHelper.getIcon(text, large);
            }
            if (text != "")
            {
                return FileHelper.getIcon(text, large);
            }
            string mimeTypeFromFileAndRegistry = FileHelper.GetMimeTypeFromFileAndRegistry(fileName);
            text = FileHelper.GetDefaultExtension(mimeTypeFromFileAndRegistry);
            return FileHelper.getIcon(text, large);
        }

        public static System.Drawing.Icon GetFileIconFromBytes(byte[] bytes, bool large)
        {
            return FileHelper.GetFileIconFromFileNameAndBytes("", bytes, large);
        }

        private static System.Drawing.Icon getIcon(string ext, bool large)
        {
            string str = Registry.ClassesRoot.ToString() + "\\";
            object objectValue = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(Registry.GetValue(str + ext, "", null));
            string text = (objectValue != null) ? objectValue.ToString() : "";
            string text2 = "";

            bool flag = false;
            if (flag)
            {
            }
            else
            {
                if (ext == ".mov")
                {
                    if (ext == ".avi")
                    {
                        if (ext == ".mp4")
                        {
                            if (ext == ".mpg")
                            {
                                if (ext == ".mpeg")
                                {
                                    goto IL_1E0;
                                }
                            }
                        }
                    }
                }
                if (flag)
                {

                }
            IL_1E0:
                if (flag)
                {

                }
                else
                {
                    if (ext == ".vb")
                    {
                        if (ext == ".aspx")
                        {
                            if (ext == ".ascx")
                            {
                                if (ext == ".cs")
                                {
                                    goto IL_33D;
                                }
                            }
                        }
                    }
                    if (flag)
                    {

                    }
                IL_33D:
                    if (flag)
                    {

                    }
                    else if (ext == ".exe")
                    {
                        text2 = "shell32.dll,2";
                    }
                    else if (text != "")
                    {
                        if (Registry.GetValue(str + text + "\\DefaultIcon", "", null) != null)
                        {
                            text2 = Registry.GetValue(str + text + "\\DefaultIcon", "", null).ToString();
                        }
                        else
                        {
                            text2 = "shell32.dll,0";
                        }
                    }
                    else if (ext == "dir")
                    {
                        text2 = "shell32.dll,4";
                    }
                }
            }

            string[] array = text2.Split(new char[]
            {
                ','
            });
            string file = array[0].Trim(new char[]
            {
                ' '
            }).Trim(new char[]
            {
                char.ConvertFromUtf32(34).ToCharArray().First()
            });
            int index = 0;
            if (array.Length > 1)
            {
                index = int.Parse(array[1].Trim());
            }
            return FileHelper.getIconFromEx(file, index, large);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr ExtractIconEx([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpszFile, IntPtr nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

        private static System.Drawing.Icon getIconFromEx(string file, int index, bool large)
        {
            System.Drawing.Icon result = null;
            IntPtr intPtr = new IntPtr(index);
            IntPtr zero = IntPtr.Zero;
            if (!large)
            {
                IntPtr arg_1F_1 = intPtr;
                IntPtr intPtr2 = new IntPtr();
                FileHelper.ExtractIconEx(ref file, arg_1F_1, ref intPtr2, ref zero, 1);
            }
            else
            {
                IntPtr arg_33_1 = intPtr;
                IntPtr intPtr2 = new IntPtr();
                FileHelper.ExtractIconEx(ref file, arg_33_1, ref zero, ref intPtr2, 1);
            }
            result = System.Drawing.Icon.FromHandle(zero);
            return result;
        }

        public static decimal FileSizeKB(byte[] bytes)
        {
            return new decimal((double)bytes.Length / 1024.0);
        }

        public static decimal FileSizeMB(byte[] bytes)
        {
            return decimal.Divide(FileHelper.FileSizeKB(bytes), new decimal(1024L));
        }

        public static string[] GetFilesFromDirectory(string path, string searchPattern = "")
        {
            if (searchPattern != "")
            {
                return System.IO.Directory.GetFiles(path, searchPattern);
            }
            return System.IO.Directory.GetFiles(path);
        }

        public static bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public static string GetFileExtension(string path)
        {
            return System.IO.Path.GetExtension(path);
        }

        public static string ReadTextFromFile(string path)
        {
            return System.IO.File.OpenText(path).ReadToEnd();
        }

        public static string FileNameFromPath(string pathwithFileName)
        {
            return System.IO.Path.GetFileName(pathwithFileName);
        }

        public static void CreateFolderInApplicationDirectory(string ChildFolder, string folderName)
        {
        }

        public static void CreateFolder(string path, string folderName)
        {
            if (!System.IO.Directory.Exists(path + "\\" + folderName))
            {
                System.IO.Directory.CreateDirectory(path + "\\" + folderName);
            }
        }

        public static void CreateFolder(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public static byte[] GetBytesFromPostedFile(IFormFile file)
        {
            byte[] result = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static bool SaveBytes(string path, byte[] data)
        {
            System.IO.File.WriteAllBytes(path, data);
            return true;
        }

        public static byte[] GetBytes(string path, int count = 0)
        {
            if (count == 0)
            {
                return System.IO.File.ReadAllBytes(path);
            }
            else
            {
                using (var file = File.OpenRead(path))
                {
                    file.Position = 0;
                    int offset = 0;
                    byte[] buffer = new byte[count];
                    int read;
                    while (count > 0 && (read = file.Read(buffer, offset, count)) > 0)
                    {
                        offset += read;
                        count -= read;
                    }

                    if (count < 0) throw new EndOfStreamException();
                    return buffer;
                }

            }
        }

        public async static Task<byte[]> GetBytesAsync(string path, int count = 0)
        {
            if (count == 0)
            {
                byte[] result;
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    result = new byte[stream.Length];
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                }
                return result;
            }
            else
            {
                using (var file = File.OpenRead(path))
                {
                    file.Position = 0;
                    int offset = 0;
                    byte[] buffer = new byte[count];
                    int read;
                    while (count > 0 && (read = await file.ReadAsync(buffer, offset, count)) > 0)
                    {
                        offset += read;
                        count -= read;
                    }

                    if (count < 0) throw new EndOfStreamException();
                    return buffer;
                }

            }
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long position = 0L;
            if (stream.CanSeek)
            {
                position = stream.Position;
                stream.Position = 0L;
            }
            checked
            {
                byte[] result;
                try
                {
                    byte[] array = new byte[4096];
                    int num = 0;
                    int num2 = 0;
                    while (FileHelper.InlineAssignHelper(ref num2, stream.Read(array, num, array.Length - num)) > 0)
                    {
                        num += num2;
                        if (num == array.Length)
                        {
                            int num3 = stream.ReadByte();
                            if (num3 != -1)
                            {
                                byte[] array2 = new byte[array.Length * 2 - 1 + 1];
                                Buffer.BlockCopy(array, 0, array2, 0, array.Length);
                                Buffer.SetByte(array2, num, (byte)num3);
                                array = array2;
                                num++;
                            }
                        }
                    }
                    byte[] array3 = array;
                    if (array.Length != num)
                    {
                        array3 = new byte[num - 1 + 1];
                        Buffer.BlockCopy(array, 0, array3, 0, num);
                    }
                    result = array3;
                }
                finally
                {
                    if (stream.CanSeek)
                    {
                        stream.Position = position;
                    }
                }
                return result;
            }
        }

        private static int InlineAssignHelper(ref int var, int val)
        {
            var = val;
            return val;
        }

        public static IEnumerable<System.IO.FileInfo> GetFilesByExtensions(this System.IO.DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new System.ArgumentNullException("extensions");
            }
            IEnumerable<System.IO.FileInfo> source = dir.EnumerateFiles();
            return from f in source
                   where extensions.Contains(f.Extension)
                   select f;
        }

        public static IEnumerable<System.IO.FileInfo> GetFilesByExtensionsStringCSV(this System.IO.DirectoryInfo dir, string extensions)
        {
            if (extensions == null)
            {
                throw new System.ArgumentNullException("extensions");
            }
            IEnumerable<System.IO.FileInfo> enumerable = dir.EnumerateFiles();
            if (extensions == null)
            {
                extensions = "";
            }
            string[] array = extensions.Split(new char[]
            {
        ','
            });
            if (extensions == "" || extensions == "*")
            {
                return enumerable;
            }
            return from f in enumerable
                   where extensions.Split(new char[]
                   {
        ','
                   }).Contains(f.Extension)
                   select f;
        }
    }
}

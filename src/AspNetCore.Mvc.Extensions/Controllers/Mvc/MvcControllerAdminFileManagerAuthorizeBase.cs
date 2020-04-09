using AspNetCore.Mvc.Extensions.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions.Controllers.Mvc
{
    [Route("FileManager")]
    public abstract class MvcControllerAdminFileManagerAuthorizeBase : MvcControllerAuthorizeBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public MvcControllerAdminFileManagerAuthorizeBase(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        Dictionary<string, string> _settings = null;
        Dictionary<string, string> _lang = null;
        string confFile = "~/FileManager/conf.json";

        [Route("DIRLIST")]
        public ActionResult DIRLIST(string type)
        {
            try
            {
                return ListDirTree(type);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

        }

        [Route("FILESLIST")]
        public ActionResult FILESLIST(string d, string type)
        {
            try
            {
                return ListFiles(d, type);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

        }

        [Route("COPYDIR")]
        public ActionResult COPYDIR(string d, string n)
        {
            try
            {
                return CopyDir(d, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("COPYFILE")]
        public ActionResult COPYFILE(string f, string n)
        {
            try
            {
                return CopyFile(f, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("CREATEDIR")]
        public ActionResult CREATEDIR(string d, string n)
        {
            try
            {
                return CreateDir(d, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("DELETEDIR")]
        public ActionResult DELETEDIR(string d)
        {
            try
            {
                return DeleteDir(d);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("DELETEFILE")]
        public ActionResult DELETEFILE(string f)
        {
            try
            {
                return DeleteFile(f);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("DOWNLOAD")]
        public ActionResult DOWNLOAD(string f)
        {
            try
            {
                return DownloadFile(f);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("DOWNLOADDIR")]
        public ActionResult DOWNLOADDIR(string d)
        {
            try
            {
                return DownloadDir(d);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("MOVEDIR")]
        public ActionResult MOVEDIR(string d, string n)
        {
            try
            {
                return MoveDir(d, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("MOVEFILE")]
        public ActionResult MOVEFILE(string f, string n)
        {
            try
            {
                return MoveFile(f, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("RENAMEDIR")]
        public ActionResult RENAMEDIR(string d, string n)
        {
            try
            {
                return RenameDir(d, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("RENAMEFILE")]
        public ActionResult RENAMEFILE(string f, string n)
        {
            try
            {
                return RenameFile(f, n);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("GENERATETHUMB")]
        public ActionResult GENERATETHUMB(string width, string height, string f, string d)
        {
            try
            {
                int w = 140, h = 0;
                int.TryParse(width.Replace("px", ""), out w);
                int.TryParse(height.Replace("px", ""), out h);
                return ShowThumbnail(f, w, h);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [Route("UPLOAD")]
        public ActionResult UPLOAD(string d)
        {
            try
            {
                return Upload(d);
            }
            catch (Exception ex)
            {
                return HandleException(ex, true);
            }
        }

        private ActionResult HandleException(Exception ex, bool upload = false)
        {
            StringBuilder sb = new StringBuilder();
            if (upload && !IsAjaxUpload())
            {
                sb.Append("<script>");
                sb.Append("parent.fileUploaded(" + GetErrorRes(LangRes("E_UploadNoFiles")) + ");");
                sb.Append("</script>");
            }
            else
            {
                sb.Append(GetErrorRes(ex.Message));
            }
            return Content(sb.ToString());
        }

        private string FixPath(string path)
        {
            if (!path.StartsWith("~"))
            {
                if (!path.StartsWith("/"))
                    path = "/" + path;
                path = "~" + path;
            }
            return _hostingEnvironment.MapWwwPath(path);
        }
        private string GetLangFile()
        {
            string filename = "~/FileManager/lang/" + GetSetting("LANG") + ".json";
            if (!System.IO.File.Exists(_hostingEnvironment.MapWwwPath(filename)))
                filename = "~/FileManager/lang/en.json";
            return filename;
        }
        protected string LangRes(string name)
        {
            string ret = name;
            if (_lang == null)
                _lang = ParseJSON(GetLangFile());
            if (_lang.ContainsKey(name))
                ret = _lang[name];

            return ret;
        }
        protected string GetFileType(string ext)
        {
            string ret = "file";
            ext = ext.ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
                ret = "image";
            else if (ext == ".swf" || ext == ".flv")
                ret = "flash";
            else if (ext == ".mp4" || ext == ".mpg" || ext == ".mpeg" || ext == ".avi" || ext == ".mp3" || ext == ".wmv")
                ret = "media";
            return ret;
        }
        protected bool CanHandleFile(string filename)
        {
            bool ret = false;
            FileInfo file = new FileInfo(filename);
            string ext = file.Extension.Replace(".", "").ToLower();
            string setting = GetSetting("FORBIDDEN_UPLOADS").Trim().ToLower();
            if (setting != "")
            {
                ArrayList tmp = new ArrayList();
                tmp.AddRange(Regex.Split(setting, "\\s+"));
                if (!tmp.Contains(ext))
                    ret = true;
            }
            setting = GetSetting("ALLOWED_UPLOADS").Trim().ToLower();
            if (setting != "")
            {
                ArrayList tmp = new ArrayList();
                tmp.AddRange(Regex.Split(setting, "\\s+"));
                if (!tmp.Contains(ext))
                    ret = false;
            }

            return ret;
        }
        protected Dictionary<string, string> ParseJSON(string file)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(_hostingEnvironment.MapWwwPath(file), System.Text.Encoding.UTF8);
            }
            catch { }

            json = json.Trim();
            if (json != "")
            {
                if (json.StartsWith("{"))
                    json = json.Substring(1, json.Length - 2);
                json = json.Trim();
                json = json.Substring(1, json.Length - 2);
                string[] lines = Regex.Split(json, "\"\\s*,\\s*\"");
                foreach (string line in lines)
                {
                    string[] tmp = Regex.Split(line, "\"\\s*:\\s*\"");
                    try
                    {
                        if (tmp[0] != "" && !ret.ContainsKey(tmp[0]))
                        {
                            ret.Add(tmp[0], tmp[1]);
                        }
                    }
                    catch { }
                }
            }
            return ret;
        }
        protected string GetFilesRoot()
        {
            string ret = GetSetting("FILES_ROOT");

            if (ret == "")
                ret = _hostingEnvironment.MapWwwPath("~/FileManager/Uploads");
            else
                ret = FixPath(ret);
            return ret;
        }
        protected void LoadConf()
        {
            if (_settings == null)
                _settings = ParseJSON(confFile);
        }
        protected string GetSetting(string name)
        {
            string ret = "";
            LoadConf();
            if (_settings.ContainsKey(name))
                ret = _settings[name];

            return ret;
        }
        protected void CheckPath(string path)
        {
            if (FixPath(path).IndexOf(GetFilesRoot()) != 0)
            {
                throw new Exception("Access to " + path + " is denied");
            }
        }
        protected void VerifyAction(string action)
        {
            string setting = GetSetting(action);
            if (setting.IndexOf("?") > -1)
                setting = setting.Substring(0, setting.IndexOf("?"));
            if (!setting.StartsWith("/"))
                setting = "/" + setting;
            setting = ".." + setting;

            if (_hostingEnvironment.MapWwwPath(setting) != _hostingEnvironment.MapWwwPath(ControllerContext.HttpContext.Request.Path.Value))
                throw new Exception(LangRes("E_ActionDisabled"));
        }
        protected string GetResultStr(string type, string msg)
        {
            return "{\"res\":\"" + type + "\",\"msg\":\"" + msg.Replace("\"", "\\\"") + "\"}";
        }
        protected string GetSuccessRes(string msg)
        {
            return GetResultStr("ok", msg);
        }
        protected string GetSuccessRes()
        {
            return GetSuccessRes("");
        }
        protected string GetErrorRes(string msg)
        {
            return GetResultStr("error", msg);
        }
        private void _copyDir(string path, string dest)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            foreach (string f in Directory.GetFiles(path))
            {
                FileInfo file = new FileInfo(f);
                if (!System.IO.File.Exists(Path.Combine(dest, file.Name)))
                {
                    System.IO.File.Copy(f, Path.Combine(dest, file.Name));
                }
            }
            foreach (string d in Directory.GetDirectories(path))
            {
                DirectoryInfo dir = new DirectoryInfo(d);
                _copyDir(d, Path.Combine(dest, dir.Name));
            }
        }
        protected ActionResult CopyDir(string path, string newPath)
        {
            StringBuilder sb = new StringBuilder();

            CheckPath(path);
            CheckPath(newPath);
            DirectoryInfo dir = new DirectoryInfo(FixPath(path));
            DirectoryInfo newDir = new DirectoryInfo(FixPath(newPath + "/" + dir.Name));

            if (!dir.Exists)
            {
                throw new Exception(LangRes("E_CopyDirInvalidPath"));
            }
            else if (newDir.Exists)
            {
                throw new Exception(LangRes("E_DirAlreadyExists"));
            }
            else
            {
                _copyDir(dir.FullName, newDir.FullName);
            }
            sb.Append(GetSuccessRes());

            return Content(sb.ToString());
        }
        protected string MakeUniqueFilename(string dir, string filename)
        {
            string ret = filename;
            int i = 0;
            while (System.IO.File.Exists(Path.Combine(dir, ret)))
            {
                i++;
                ret = Path.GetFileNameWithoutExtension(filename) + " - Copy " + i.ToString() + Path.GetExtension(filename);
            }
            return ret;
        }

        protected ActionResult CopyFile(string path, string newPath)
        {
            StringBuilder sb = new StringBuilder();

            CheckPath(path);
            FileInfo file = new FileInfo(FixPath(path));
            newPath = FixPath(newPath);
            if (!file.Exists)
                throw new Exception(LangRes("E_CopyFileInvalidPath"));
            else
            {
                string newName = MakeUniqueFilename(newPath, file.Name);
                try
                {
                    System.IO.File.Copy(file.FullName, Path.Combine(newPath, newName));
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_CopyFile"));
                }
            }
            return Content(sb.ToString());
        }
        protected ActionResult CreateDir(string path, string name)
        {
            StringBuilder sb = new StringBuilder();

            CheckPath(path);
            path = FixPath(path);
            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_CreateDirInvalidPath"));
            else
            {
                try
                {
                    foreach (string folder in name.Split(","[0]))
                    {
                        var folderTrim = folder.Trim(" "[0]);
                        var create = Path.Combine(path, folderTrim);
                        if (!Directory.Exists(create))
                            Directory.CreateDirectory(create);
                    }

                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_CreateDirFailed"));
                }
            }
            return Content(sb.ToString());

        }
        protected ActionResult DeleteDir(string path)
        {
            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            path = FixPath(path);
            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_DeleteDirInvalidPath"));
            else if (path == GetFilesRoot())
                throw new Exception(LangRes("E_CannotDeleteRoot"));
            else if (Directory.GetDirectories(path).Length > 0 || Directory.GetFiles(path).Length > 0)
                throw new Exception(LangRes("E_DeleteNonEmpty"));
            else
            {
                try
                {
                    Directory.Delete(path);
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_CannotDeleteDir"));
                }
            }
            return Content(sb.ToString());
        }
        protected ActionResult DeleteFile(string path)
        {
            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            path = FixPath(path);
            if (!System.IO.File.Exists(path))
                throw new Exception(LangRes("E_DeleteFileInvalidPath"));
            else
            {
                try
                {
                    System.IO.File.Delete(path);
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_DeletеFile"));
                }
            }
            return Content(sb.ToString());
        }
        private List<string> GetFiles(string path, string type)
        {
            List<string> ret = new List<string>();
            if (type == "#" || type is null)
                type = "";
            string[] files = Directory.GetFiles(path);
            foreach (string f in files)
            {
                if ((GetFileType(new FileInfo(f).Extension) == type) || (type == ""))
                    ret.Add(f);
            }
            return ret;
        }
        private ArrayList ListDirs(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            ArrayList ret = new ArrayList();
            foreach (string dir in dirs)
            {
                ret.Add(dir);
                ret.AddRange(ListDirs(dir));
            }
            return ret;
        }
        protected ActionResult ListDirTree(string type)
        {
            DirectoryInfo d = new DirectoryInfo(GetFilesRoot());
            if (!d.Exists)
                throw new Exception("Invalid files root directory. Check your configuration.");

            ArrayList dirs = ListDirs(d.FullName);
            dirs.Insert(0, d.FullName);

            string localPath = _hostingEnvironment.MapWwwPath("~/");

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < dirs.Count; i++)
            {
                string dir = (string)dirs[i];
                sb.Append("{\"p\":\"/" + dir.Replace(localPath, "").Replace("\\", "/") + "\",\"f\":\"" + GetFiles(dir, type).Count.ToString() + "\",\"d\":\"" + Directory.GetDirectories(dir).Length.ToString() + "\"}");
                if (i < dirs.Count - 1)
                    sb.Append(",");
            }
            sb.Append("]");

            return Content(sb.ToString());
        }

        protected double LinuxTimestamp(DateTime d)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            TimeSpan timeSpan = (d.ToLocalTime() - epoch);

            return timeSpan.TotalSeconds;

        }
        protected ActionResult ListFiles(string path, string type)
        {
            CheckPath(path);
            string fullPath = FixPath(path);
            List<string> files = GetFiles(fullPath, type);

            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            for (int i = 0; i < files.Count; i++)
            {
                FileInfo f = new FileInfo(files[i]);
                int w = 0, h = 0;
                if (GetFileType(f.Extension) == "image")
                {
                    try
                    {
                        FileStream fs = new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        Image img = Image.FromStream(fs);
                        w = img.Width;
                        h = img.Height;
                        fs.Close();
                        fs.Dispose();
                        img.Dispose();
                    }
                    catch (Exception ex) { throw ex; }
                }
                sb.Append("{");
                sb.Append("\"p\":\"" + path + "/" + f.Name + "\"");
                sb.Append(",\"t\":\"" + Math.Ceiling(LinuxTimestamp(f.LastWriteTime)).ToString() + "\"");
                sb.Append(",\"s\":\"" + f.Length.ToString() + "\"");
                sb.Append(",\"w\":\"" + w.ToString() + "\"");
                sb.Append(",\"h\":\"" + h.ToString() + "\"");
                sb.Append("}");
                if (i < files.Count - 1)
                    sb.Append(",");
            }
            sb.Append("]");

            return Content(sb.ToString());
        }

        public ActionResult DownloadDir(string path)
        {
            path = FixPath(path);
            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_CreateArchive"));
            string dirName = new FileInfo(path).Name;
            string tmpZip = _hostingEnvironment.MapWwwPath("~/FileManager/tmp/" + dirName + ".zip");
            if (System.IO.File.Exists(tmpZip))
                System.IO.File.Delete(tmpZip);
            ZipFile.CreateFromDirectory(path, tmpZip, CompressionLevel.Fastest, true);

            byte[] bytes = System.IO.File.ReadAllBytes(tmpZip);

            System.IO.File.Delete(tmpZip);
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = dirName + ".zip",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(bytes, "application/force-download");
        }

        protected ActionResult DownloadFile(string path)
        {
            CheckPath(path);
            FileInfo file = new FileInfo(FixPath(path));
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = file.Name,

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(file.FullName, "application/force-download");
        }
        protected ActionResult MoveDir(string path, string newPath)
        {
            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            CheckPath(newPath);
            DirectoryInfo source = new DirectoryInfo(FixPath(path));
            DirectoryInfo dest = new DirectoryInfo(FixPath(Path.Combine(newPath, source.Name)));
            if (dest.FullName.IndexOf(source.FullName) == 0)
                throw new Exception(LangRes("E_CannotMoveDirToChild"));
            else if (!source.Exists)
                throw new Exception(LangRes("E_MoveDirInvalisPath"));
            else if (dest.Exists)
                throw new Exception(LangRes("E_DirAlreadyExists"));
            else
            {
                try
                {
                    source.MoveTo(dest.FullName);
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_MoveDir") + " \"" + path + "\"");
                }
            }

            return Content(sb.ToString());
        }
        protected ActionResult MoveFile(string path, string newPath)
        {

            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            CheckPath(newPath);
            FileInfo source = new FileInfo(FixPath(path));
            FileInfo dest = new FileInfo(FixPath(newPath));

            if (!source.Exists)
                throw new Exception(LangRes("E_MoveFileInvalisPath"));
            else if (dest.Exists)
                throw new Exception(LangRes("E_MoveFileAlreadyExists"));
            else if (!CanHandleFile(dest.Name))
                throw new Exception(LangRes("E_FileExtensionForbidden"));
            else
            {
                try
                {
                    source.MoveTo(dest.FullName);
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_MoveFile") + " \"" + path + "\"");
                }
            }
            return Content(sb.ToString());
        }
        protected ActionResult RenameDir(string path, string name)
        {

            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            DirectoryInfo source = new DirectoryInfo(FixPath(path));
            DirectoryInfo dest = new DirectoryInfo(Path.Combine(source.Parent.FullName, name));
            if (source.FullName == GetFilesRoot())
                throw new Exception(LangRes("E_CannotRenameRoot"));
            else if (!source.Exists)
                throw new Exception(LangRes("E_RenameDirInvalidPath"));
            else if (dest.Exists)
                throw new Exception(LangRes("E_DirAlreadyExists"));
            else
            {
                try
                {
                    source.MoveTo(dest.FullName);
                    sb.Append(GetSuccessRes());
                }
                catch
                {
                    throw new Exception(LangRes("E_RenameDir") + " \"" + path + "\"");
                }
            }
            return Content(sb.ToString());
        }
        protected ActionResult RenameFile(string path, string name)
        {

            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            FileInfo source = new FileInfo(FixPath(path));
            FileInfo dest = new FileInfo(Path.Combine(source.Directory.FullName, name));
            if (!source.Exists)
                throw new Exception(LangRes("E_RenameFileInvalidPath"));
            else if (!CanHandleFile(name))
                throw new Exception(LangRes("E_FileExtensionForbidden"));
            else
            {
                try
                {
                    source.MoveTo(dest.FullName);
                    sb.Append(GetSuccessRes());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "; " + LangRes("E_RenameFile") + " \"" + path + "\"");
                }
            }
            return Content(sb.ToString());
        }
        public bool ThumbnailCallback()
        {
            return false;
        }

        protected ActionResult ShowThumbnail(string path, int width, int height)
        {
            CheckPath(path);
            FileStream fs = new FileStream(FixPath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Bitmap img = new Bitmap(Bitmap.FromStream(fs));
            fs.Close();
            fs.Dispose();
            int cropWidth = img.Width, cropHeight = img.Height;
            int cropX = 0, cropY = 0;

            double imgRatio = (double)img.Width / (double)img.Height;

            if (height == 0)
                height = Convert.ToInt32(Math.Floor((double)width / imgRatio));

            if (width > img.Width)
                width = img.Width;
            if (height > img.Height)
                height = img.Height;

            double cropRatio = (double)width / (double)height;
            cropWidth = Convert.ToInt32(Math.Floor((double)img.Height * cropRatio));
            cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
            if (cropWidth > img.Width)
            {
                cropWidth = img.Width;
                cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
            }
            if (cropHeight > img.Height)
            {
                cropHeight = img.Height;
                cropWidth = Convert.ToInt32(Math.Floor((double)cropHeight * cropRatio));
            }
            if (cropWidth < img.Width)
            {
                cropX = Convert.ToInt32(Math.Floor((double)(img.Width - cropWidth) / 2));
            }
            if (cropHeight < img.Height)
            {
                cropY = Convert.ToInt32(Math.Floor((double)(img.Height - cropHeight) / 2));
            }

            Rectangle area = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropImg = img.Clone(area, System.Drawing.Imaging.PixelFormat.DontCare);
            img.Dispose();
            Image.GetThumbnailImageAbort imgCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            MemoryStream ms = new MemoryStream();
            cropImg.GetThumbnailImage(width, height, imgCallback, IntPtr.Zero).Save(ms, ImageFormat.Png);

            byte[] bytes = ms.ToArray();
            ms.Dispose();

            return File(bytes, "image/png");
        }
        private ImageFormat GetImageFormat(string filename)
        {
            ImageFormat ret = ImageFormat.Jpeg;
            switch (new FileInfo(filename).Extension.ToLower())
            {
                case ".png": ret = ImageFormat.Png; break;
                case ".gif": ret = ImageFormat.Gif; break;
            }
            return ret;
        }
        protected void ImageResize(string path, string dest, int width, int height)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Image img = Image.FromStream(fs);
            fs.Close();
            fs.Dispose();
            float ratio = (float)img.Width / (float)img.Height;
            if ((img.Width <= width && img.Height <= height) || (width == 0 && height == 0))
                return;

            int newWidth = width;
            int newHeight = Convert.ToInt16(Math.Floor((float)newWidth / ratio));
            if ((height > 0 && newHeight > height) || (width == 0))
            {
                newHeight = height;
                newWidth = Convert.ToInt16(Math.Floor((float)newHeight * ratio));
            }
            Bitmap newImg = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage((Image)newImg);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, newWidth, newHeight);
            img.Dispose();
            g.Dispose();
            if (dest != "")
            {
                newImg.Save(dest, GetImageFormat(dest));
            }
            newImg.Dispose();
        }
        protected bool IsAjaxUpload()
        {
            return (ControllerContext.HttpContext.Request.Method != null && ControllerContext.HttpContext.Request.Method == "ajax");
        }
        protected ActionResult Upload(string path)
        {
            StringBuilder sb = new StringBuilder();
            CheckPath(path);
            path = FixPath(path);
            string res = GetSuccessRes();
            bool hasErrors = false;
            try
            {
                for (int i = 0; i < ControllerContext.HttpContext.Request.Form.Files.Count; i++)
                {
                    if (CanHandleFile(ControllerContext.HttpContext.Request.Form.Files[i].FileName))
                    {
                        FileInfo f = new FileInfo(ControllerContext.HttpContext.Request.Form.Files[i].FileName);
                        string filename = MakeUniqueFilename(path, f.Name);
                        string dest = Path.Combine(path, filename);
                        ControllerContext.HttpContext.Request.Form.Files[i].SaveAs(dest);
                        if (GetFileType(new FileInfo(filename).Extension) == "image")
                        {
                            int w = 0;
                            int h = 0;
                            int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out w);
                            int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out h);
                            ImageResize(dest, dest, w, h);
                        }
                    }
                    else
                    {
                        hasErrors = true;
                        res = GetSuccessRes(LangRes("E_UploadNotAll"));
                    }
                }
            }
            catch (Exception ex)
            {
                res = GetErrorRes(ex.Message);
            }
            if (IsAjaxUpload())
            {
                if (hasErrors)
                    res = GetErrorRes(LangRes("E_UploadNotAll"));
                sb.Append(res);
            }
            else
            {
                sb.Append("<script>");
                sb.Append("parent.fileUploaded(" + res + ");");
                sb.Append("</script>");
            }

            return Content(sb.ToString());
        }
    }
}

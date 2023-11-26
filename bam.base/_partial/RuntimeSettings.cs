using System;
using System.IO;
using System.Text;
using Bam.Net.Configuration;

namespace Bam.Net
{
    public static partial class RuntimeSettings
    {
        // TODO: change this to direct to ~/.bam/opt
        public static string ProcessDataFolder
        {
            get
            {
                return _appDataFolderLock.DoubleCheckLock(ref _appDataFolder, () =>
                {
                    if (!OSInfo.IsUnix)
                    {
                        StringBuilder path = new StringBuilder();
                        path.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                        if (!path.ToString().EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            path.Append(Path.DirectorySeparatorChar);
                        }

                        path.Append(DefaultConfiguration.GetAppSetting("ApplicationName", ApplicationNameProvider.Default.GetApplicationName()) + Path.DirectorySeparatorChar);
                        FileInfo fileInfo = new FileInfo(path.ToString());
                        if (!Directory.Exists(fileInfo.Directory.FullName))
                        {
                            Directory.CreateDirectory(fileInfo.Directory.FullName);
                        }
                        _appDataFolder = path.ToString();
                        return _appDataFolder;
                    }
                    else
                    {
                        return Path.Combine(BamHome.DataPath, Config.Current?.ApplicationName ?? ApplicationDiagnosticInfo.UnknownApplication);
                    }
                });
            }
            set => _appDataFolder = value;
        }
    }
}

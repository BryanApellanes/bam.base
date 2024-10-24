﻿using System;
using System.IO;
using Bam.Data.Repositories;
using Bam.Logging;

namespace Bam.Data
{
    public interface IDataDirectoryProvider
    {
        DirectoryInfo GetAppDataDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetAppDataDirectory(IApplicationNameProvider applicationNameProvider, params string[] directoryName);
        DirectoryInfo GetAppUsersDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetAppDatabaseDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetAppRepositoryDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetAppFilesDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetAppEmailTemplatesDirectory(IApplicationNameProvider applicationNameProvider);
        DirectoryInfo GetRootDataDirectory();
        DirectoryInfo GetSysDataDirectory();
        DirectoryInfo GetSysAssemblyDirectory();
        DirectoryInfo GetChunksDirectory();
        DirectoryInfo GetFilesDirectory();

        DirectoryInfo GetWorkspaceDirectory(Type type);

        /// <summary>
        /// When implemented, returns the path to the specified directory at the root 
        /// of the SysData directory (as returned by GetSysDataDirectory).
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        DirectoryInfo GetSysDataDirectory(params string[] directoryName);

        DirectoryInfo GetRootDataDirectory(params string[] directoryName);
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Snak.BuildDrop
{
    public interface IDropNamingStrategy
    {
        /// <summary>
        /// The root directory for the drop, it may be a common drop directory or perhaps a labelled directory e.g. Build_23
        /// </summary>
        DirectoryInfo DropDirectory { get; }
        /// <summary>
        /// The root directory for the current drop 
        /// </summary>
        DirectoryInfo DropDirectoryCurrent { get; }
        /// <summary>
        /// The last drop directory or null if its not found
        /// </summary>
        DirectoryInfo DropDirectoryLast { get; }
        /// <summary>
        /// 
        /// </summary>
        void CreateDropDirectories();
        /// <summary>
        /// 
        /// </summary>
        void DeleteDropDirectories();

        /// <summary>
        /// returns the common labelled drop location for packages
        /// </summary>
        /// <returns></returns>
        DirectoryInfo PackagesCommonDropDirectory { get; }
        /// <summary>
        /// returns the current drop directory for packages
        /// </summary>
        /// <returns></returns>
        DirectoryInfo PackagesCommonDropDirectoryCurrent { get; }
        /// <summary>
        /// returns the last labelled drop directory for packages or null if its not found
        /// </summary>
        /// <returns></returns>
        DirectoryInfo PackagesCommonDropDirectoryLast { get; }
        /// <summary>
        /// 
        /// </summary>
        void CreatePackagesCommonDirectories();
        /// <summary>
        /// 
        /// </summary>
        void DeletePackagesCommonDirectories();

        /// <summary>
        /// returns the labelled drop directory for the given package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dropToOwnDirectory"></param>
        /// <returns></returns>
        DirectoryInfo GetPackageDropDirectory(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        /// <summary>
        /// returns the current drop for a given package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dropToOwnDirectory"></param>
        /// <returns></returns>
        DirectoryInfo GetPackageDropDirectoryCurrent(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        /// <summary>
        /// returns the last labelled drop directory for the given package or null if its not found
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dropToOwnDirectory"></param>
        /// <returns></returns>
        DirectoryInfo GetPackageLastDropDirectoryLast(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        /// <summary>
        /// 
        /// </summary>
        void CreatePackageDirectories(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dropToOwnDirectory"></param>
        void DeletePackageDirectories(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
    }
}
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

using NAnt.Core;

namespace Snak.Core
{
    /// <summary>
    /// Takes a version string of either numeric e.g. "1234" or version e.g. "1.0.0.0" and allows the users access to the individual 
    /// increments (obviously more useful for version numbers). If the version number contains '*' This class normalises the version 
    /// to a 4 part number e.g. will turn 1.0.* into 0.0.0.0.
    /// 
    /// Once constructed with a version this version information is available via the property Version.
    /// </summary>
    public class BuildVersion
    {
        private Version _version;
        private BuildVersionType _buildVersionType = BuildVersionType.Numeric;
        private string _buildVersionString;

        public Version Version
        {
            get
            {
                if (_buildVersionType != BuildVersionType.VersionNumber)
                    throw new InvalidOperationException("The property '" + System.Reflection.MethodInfo.GetCurrentMethod().Name + "' is only accessible when '" + this.GetType().ToString() + ".BuildVersionType' is set to VersionNumber. This property is set internally, the likely cause of the problem is you are not specifying a valid build version.");

                return _version;
            }
        }
        public int BuildVersionNumber
        {
            get
            {
                if (this._buildVersionType != BuildVersionType.Numeric)
                    throw new InvalidOperationException("The property '" + System.Reflection.MethodInfo.GetCurrentMethod().Name + "' is only accessible when '" + this.GetType().ToString() + ".BuildVersionType' is set to Numeric. This property is set internally, the likely cause of the problem is you are not specifying a valid numerical build version.");
                else
                    return int.Parse(this._buildVersionString);
            }
        }

        public BuildVersionType BuildVersionType
        {
            get { return _buildVersionType; }
        }

        public string BuildVersionString
        {
            get
            {
                if (this._buildVersionType == BuildVersionType.VersionNumber)
                    return _version.ToString();
                else
                    return _buildVersionString;
            }
        }

        public string BuildVersionStringPath
        {
            get
            {
                if (this._buildVersionType == BuildVersionType.VersionNumber)
                    return _version.ToString().Replace(',', '_');
                else
                    return _buildVersionString;
            }
        }

        public BuildVersion(string buildVersion)
        {
            _buildVersionString = buildVersion;
            Init();
        }

        private void Init()
        {
            if (_buildVersionString != String.Empty)
            {
                string buildVersionFormatErrorMessage = "The value for the build version must be either numeric (e.g. 156) or else a version number (e.g. 1.0.0.0).";

                if (Regex.Match(_buildVersionString, @"[^0-9|\.|\*]").Success)
                {
                    throw new BuildException(buildVersionFormatErrorMessage);
                }

                try
                {
                    string[] versionParts = _buildVersionString.Split('.');

                    if (versionParts.Length > 1 && versionParts.Length < 5)
                    {
                        _buildVersionType = BuildVersionType.VersionNumber;

                        int[] versionPartNumbers = new int[versionParts.Length];

                        for (int i = 0; i < versionParts.Length; i++)
                        {
                            int value = 0;
                            // if there are any * in the build version they get replaced with 0
                            int.TryParse(versionParts[i], out value); 
                            versionPartNumbers[i] = value;
                        }

                        switch (versionPartNumbers.Length)
                        {
                            case 2:
                                _version = new Version(versionPartNumbers[0], versionPartNumbers[1], 0, 0);
                                break;
                            case 3:
                                _version = new Version(versionPartNumbers[0], versionPartNumbers[1], versionPartNumbers[2], 0);
                                break;
                            case 4:
                                _version = new Version(versionPartNumbers[0], versionPartNumbers[1], versionPartNumbers[2], versionPartNumbers[3]);
                                break;
                            default:
                                throw new BuildException(buildVersionFormatErrorMessage);
                        }
                    }
                    else if (versionParts.Length == 1)
                    {
                        _buildVersionType = BuildVersionType.Numeric;
                    }
                    else
                        throw new BuildException(buildVersionFormatErrorMessage);
                }
                catch (ArgumentException)
                {
                    // the Version class throws ArgumentException s, we could catch them here an give a more informative message to the user
                    // but the code above checking what we pass to the Version class so we don’t really need to catch anything here...
                    // throw new BuildException("", ex);
                    throw;
                }
            }
            else
            {
                _buildVersionString = "0";
                _buildVersionType = BuildVersionType.Numeric;
            }
        }
    }
}
﻿using System;
using System.IO;
using System.Reflection;

namespace Cave
{
    /// <summary>
    /// Provides an installation guid.
    /// </summary>
    public class InstallationGuid
    {
        static Guid? programGuid;

        /// <summary>Gets the installation unique identifier.</summary>
        /// <returns>unique id as GUID.</returns>
        public static Guid SystemGuid
        {
            get
            {
#if NETSTANDARD20
#else
                if (Platform.IsMicrosoft)
                {
                    var software = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE");
                    byte[] data = software.GetValue("SystemGuid") as byte[];
                    if (data == null)
                    {
                        data = Guid.NewGuid().ToByteArray();
                        software.SetValue("SystemGuid", data, Microsoft.Win32.RegistryValueKind.Binary);
                    }
                    return new Guid(data);
                }
#endif
                var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string fileName = Path.Combine(root, "system.guid");
                if (!File.Exists(fileName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    File.WriteAllText(fileName, Guid.NewGuid().ToString());
                }

                return new Guid(File.ReadAllLines(fileName)[0]);
            }
        }

        /// <summary>Gets the installation identifier.</summary>
        /// <value>The installation identifier.</value>
        /// <exception cref="NotSupportedException">if <see cref="Guid.GetHashCode()"/> failes.</exception>
        public static Guid ProgramGuid
        {
            get
            {
                if (!programGuid.HasValue)
                {
                    var guidBytes = SystemGuid.ToByteArray();
                    long programLong =
                        (AppDomain.CurrentDomain.BaseDirectory?.GetHashCode() ?? 0) ^
                        (Assembly.GetEntryAssembly()?.FullName.GetHashCode() ?? 0) << 32;
                    var programBytes = BitConverter.GetBytes(programLong);
                    for (int i = 0; i < 8; i++)
                    {
                        guidBytes[guidBytes.Length - i - 1] ^= programBytes[i];
                    }
                    programGuid = new Guid(guidBytes);
                }
                return programGuid.Value;
            }
        }
    }
}

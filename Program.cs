using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;

class Program
{
    // P/Invoke declarations
    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
    static extern int NetShareAdd(string servername, int level, ref SHARE_INFO_2 buf, out int parm_err);

    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
    static extern int NetShareSetInfo(string servername, string netname, int level, IntPtr buf, out int parm_err);

    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
    static extern int NetShareDel(string servername, string netname, int reserved);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHARE_INFO_2
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string shi2_netname;
        public uint shi2_type;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi2_remark;
        public uint shi2_permissions;
        public uint shi2_max_uses;
        public uint shi2_current_uses;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi2_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi2_passwd;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SHARE_INFO_502
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string shi502_netname;
        public uint shi502_type;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi502_remark;
        public uint shi502_permissions;
        public uint shi502_max_uses;
        public uint shi502_current_uses;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi502_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string shi502_passwd;
        public uint shi502_reserved;
        public IntPtr shi502_security_descriptor;
    }

    static void Main(string[] args)
    {
        if (args.Length == 0 || args[0].ToLower() == "help" || args[0].ToLower() == "--help" || args[0].ToLower() == "-h")
        {
            ShowDetailedHelp();
            return;
        }

        try
        {
            switch (args[0].ToLower())
            {
                case "create":
                    HandleCreateCommand(args);
                    break;
                case "setace":
                    if (args.Length != 5) throw new ArgumentException("Invalid number of arguments for setace command.");
                    SetAce(args[1], args[2], args[3], args[4]);
                    break;
                case "setshareperms":
                    if (args.Length != 5) throw new ArgumentException("Invalid number of arguments for setshareperms command.");
                    SetSharePermissions(args[1], args[2], args[3], args[4]);
                    break;
                case "delete":
                    if (args.Length != 2) throw new ArgumentException("Invalid number of arguments for delete command.");
                    DeleteShare(args[1]);
                    break;
                default:
                    throw new ArgumentException("Invalid command.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            PrintUsage();
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Sharelord.exe <command> [arguments]");
        Console.WriteLine("\nCommands:");
        Console.WriteLine("  create <directory> <sharename> [-r <remark>]");
        Console.WriteLine("  setace <directory> <username> <rights> <type>");
        Console.WriteLine("  setshareperms <sharename> <username> <permissions> <type>");
        Console.WriteLine("  delete <sharename>");
        Console.WriteLine("  help                   Show detailed help and list of available permissions");
        Console.WriteLine("\nUse 'Sharelord.exe help' for more detailed information.");
    }

    static void ShowDetailedHelp()
    {
        Console.WriteLine("Sharelord - Detailed Help");
        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("  create <directory> <sharename> [-r <remark>]");
        Console.WriteLine("  setace <directory> <username> <rights> <type>");
        Console.WriteLine("  setshareperms <sharename> <username> <permissions> <type>");
        Console.WriteLine("  delete <sharename>");
        Console.WriteLine("  help");

        Console.WriteLine("\nFile System Rights (for setace command):");
        foreach (FileSystemRights right in Enum.GetValues(typeof(FileSystemRights)))
        {
            Console.WriteLine($"  {right}");
        }

        Console.WriteLine("\nAccess Control Types:");
        Console.WriteLine("  Allow");
        Console.WriteLine("  Deny");

        Console.WriteLine("\nShare Permissions (for setshareperms command):");
        Console.WriteLine("  Read        - Users can view file and subfolder names, read data, and run programs");
        Console.WriteLine("  Change      - Users have Read permissions plus the ability to create, change, and delete files and subfolders");
        Console.WriteLine("  FullControl - Users have Change permissions plus the ability to change permissions and take ownership");

        Console.WriteLine("\nExamples:");
        Console.WriteLine("  Sharelord.exe create C:\\SharedFolder MyShare -r \"Shared folder for team\"");
        Console.WriteLine("  Sharelord.exe setace C:\\SharedFolder Domain\\User Modify Allow");
        Console.WriteLine("  Sharelord.exe setshareperms MyShare Domain\\User Change Allow");
        Console.WriteLine("  Sharelord.exe delete MyShare");
    }

    static void HandleCreateCommand(string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Invalid number of arguments for create command.");
        }

        string directory = args[1];
        string shareName = args[2];
        string remark = null;

        // Check for optional remark flag
        for (int i = 3; i < args.Length; i++)
        {
            if (args[i] == "-r" && i + 1 < args.Length)
            {
                remark = args[i + 1];
                break;
            }
        }

        CreateShare(directory, shareName, remark);
    }

    static void CreateShare(string directory, string shareName, string remark)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
        }

        SHARE_INFO_2 shareInfo = new SHARE_INFO_2
        {
            shi2_netname = shareName,
            shi2_type = 0, // Disk share
            shi2_remark = remark,
            shi2_permissions = 0, // Use NT security
            shi2_max_uses = unchecked((uint)-1), // Maximum allowed
            shi2_current_uses = 0,
            shi2_path = directory,
            shi2_passwd = null
        };

        int parmErr = 0;
        int result = NetShareAdd(null, 2, ref shareInfo, out parmErr);

        if (result != 0)
        {
            throw new Exception($"Failed to create share. Error code: {result}, Parameter Error: {parmErr}");
        }

        Console.WriteLine($"Share '{shareName}' created successfully for directory '{directory}'.");
        if (!string.IsNullOrEmpty(remark))
        {
            Console.WriteLine($"Remark: {remark}");
        }
    }

    static void SetAce(string directory, string username, string rights, string type)
    {
        FileSystemRights fsr = (FileSystemRights)Enum.Parse(typeof(FileSystemRights), rights);
        AccessControlType act = (AccessControlType)Enum.Parse(typeof(AccessControlType), type);

        DirectoryInfo dirInfo = new DirectoryInfo(directory);
        DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

        dirSecurity.AddAccessRule(new FileSystemAccessRule(username, fsr, 
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, 
            PropagationFlags.None, act));

        dirInfo.SetAccessControl(dirSecurity);

        Console.WriteLine($"ACE set for {username} on {directory}");
    }

    static void SetSharePermissions(string shareName, string username, string permissions, string type)
    {
        NTAccount account = new NTAccount(username);
        SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

        uint permissionsMask = (uint)Enum.Parse(typeof(FileSystemRights), permissions);
        string sdString = $"D:(A;;{permissionsMask:X};;;{sid.Value})";

        RawSecurityDescriptor rsd = new RawSecurityDescriptor(sdString);
        byte[] binarySD = new byte[rsd.BinaryLength];
        rsd.GetBinaryForm(binarySD, 0);

        IntPtr securityDescriptor = Marshal.AllocHGlobal(binarySD.Length);
        Marshal.Copy(binarySD, 0, securityDescriptor, binarySD.Length);

        SHARE_INFO_502 shareInfo = new SHARE_INFO_502
        {
            shi502_netname = shareName,
            shi502_type = 0,
            shi502_remark = null,
            shi502_permissions = 0,
            shi502_max_uses = unchecked((uint)-1),
            shi502_current_uses = 0,
            shi502_path = null,
            shi502_passwd = null,
            shi502_reserved = 0,
            shi502_security_descriptor = securityDescriptor
        };

        IntPtr pShareInfo = Marshal.AllocHGlobal(Marshal.SizeOf(shareInfo));
        Marshal.StructureToPtr(shareInfo, pShareInfo, false);

        try
        {
            int parmErr = 0;
            int result = NetShareSetInfo(null, shareName, 502, pShareInfo, out parmErr);
            if (result != 0)
            {
                throw new Exception($"Failed to set share permissions. Error code: {result}");
            }

            Console.WriteLine($"Share permissions set for {username} on {shareName}");
        }
        finally
        {
            Marshal.FreeHGlobal(securityDescriptor);
            Marshal.FreeHGlobal(pShareInfo);
        }
    }

    static void DeleteShare(string shareName)
    {
        int result = NetShareDel(null, shareName, 0);
        if (result == 0)
        {
            Console.WriteLine($"Share '{shareName}' deleted successfully.");
        }
        else
        {
            throw new Exception($"Failed to delete share. Error code: {result}");
        }
    }
}

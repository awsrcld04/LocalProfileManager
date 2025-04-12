using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.DirectoryServices;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LocalProfileManager
{
    class LPMMain
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool RemoveDirectory(string lpPathName);

        struct OSCaptionVersion
        {
            public int intcaptionvalue;
            public int intversionvalue;

            public OSCaptionVersion(int intCapVal, int intVerVal)
            {
                intcaptionvalue = intCapVal;
                intversionvalue = intVerVal;
            }
        }

        struct CMDArguments
        {
            public string strSwitch;
            public bool bParseCmdArguments;
        }

        struct AccountData
        {
            public bool bAccountValid;
            public string strAccountName;
        }

        static ManagementObjectCollection funcSysQueryData(string sysQueryString, string sysServerName)
        {

            // [Comment] Connect to the server via WMI
            System.Management.ConnectionOptions objConnOptions = new System.Management.ConnectionOptions();
            string strServerNameforWMI = "\\\\" + sysServerName + "\\root\\cimv2";

            // [DebugLine] Console.WriteLine("Construct WMI scope...");
            System.Management.ManagementScope objManagementScope = new System.Management.ManagementScope(strServerNameforWMI, objConnOptions);

            // [DebugLine] Console.WriteLine("Construct WMI query...");
            System.Management.ObjectQuery objQuery = new System.Management.ObjectQuery(sysQueryString);
            //if (objQuery != null)
            //    Console.WriteLine("objQuery was created successfully");

            // [DebugLine] Console.WriteLine("Construct WMI object searcher...");
            System.Management.ManagementObjectSearcher objSearcher = new System.Management.ManagementObjectSearcher(objManagementScope, objQuery);
            //if (objSearcher != null)
            //    Console.WriteLine("objSearcher was created successfully");

            // [DebugLine] Console.WriteLine("Get WMI data...");

            System.Management.ManagementObjectCollection objReturnCollection = null;

            try
            {
                objReturnCollection = objSearcher.Get();
                return objReturnCollection;
            }
            catch (SystemException ex)
            {
                // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
                string strRPCUnavailable = "The RPC server is unavailable. (Exception from HRESULT: 0x800706BA)";
                // [DebugLine] System.Console.WriteLine(ex.Message);
                if (ex.Message == strRPCUnavailable)
                {
                    Console.WriteLine("WMI: Server unavailable");
                    objReturnCollection = null;
                }
                else
                {
                    System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
                    System.Console.WriteLine(ex.Message);
                    objReturnCollection = null;
                }

                // Next line will return an object that is equal to null
                return objReturnCollection;
            }
        }

        static AccountData funcSysQueryData2(string sysQueryString, string sysServerName)
        {
            AccountData tmpAccountData = new AccountData();

            tmpAccountData.bAccountValid = false;
            tmpAccountData.strAccountName = "";

            // [DebugLine] Console.WriteLine("Inside funcSysQueryData2...");
            // [Comment] Connect to the server via WMI
            //System.Management.ConnectionOptions objConnOptions = new System.Management.ConnectionOptions();
            string strServerNameforWMI = "\\\\" + sysServerName + "\\root\\cimv2:";
            string strQueryString = sysQueryString;
            string strWMIManagementPath = strServerNameforWMI + strQueryString;

            // [DebugLine] Console.WriteLine("Construct WMI scope...");
            //System.Management.ManagementScope objManagementScope = new System.Management.ManagementScope(strServerNameforWMI, objConnOptions);

            // [DebugLine] Console.WriteLine(strWMIManagementPath);
            System.Management.ManagementPath objManagementPath = new ManagementPath(strWMIManagementPath);
            if (objManagementPath == null)
            {
                Console.WriteLine("{0} was not created.", objManagementPath);
                tmpAccountData.bAccountValid = false;
            }
            else
            {
                // [DebugLine] Console.WriteLine("Constructing ManagementObject...");
                System.Management.ManagementObject objManagementObject = new ManagementObject(objManagementPath);
                if (objManagementObject == null)
                {
                    Console.WriteLine("{0} was not created.", objManagementObject);
                    tmpAccountData.bAccountValid = false;
                }
                else
                {
                    if (objManagementObject != null)
                    {
                        string strAcctName = objManagementObject["AccountName"].ToString();
                        if (strAcctName != "")
                        {
                            //[DebugLine] Console.WriteLine("Account: {0}", objManagementObject["AccountName"].ToString());
                            tmpAccountData.bAccountValid = true;
                            tmpAccountData.strAccountName = strAcctName;
                        }
                        else
                        {
                            //[DebugLine] Console.WriteLine("No valid account exists.");
                            tmpAccountData.bAccountValid = false;
                        }
                    }
                }
            }

            return tmpAccountData;

            //objManagementScope.Path;

            // [DebugLine] Console.WriteLine("Construct WMI query...");
            //System.Management.ObjectQuery objQuery = new System.Management.ObjectQuery(sysQueryString);

            //Console.WriteLine(objQuery.QueryString);
            //Console.WriteLine(objQuery.ToString());
            //if (objQuery != null)
            //    Console.WriteLine("objQuery was created successfully");

            // [DebugLine] Console.WriteLine("Construct WMI object searcher...");
            //System.Management.ManagementObjectSearcher objSearcher = new System.Management.ManagementObjectSearcher(objManagementScope, objQuery);
            //if (objSearcher != null)
            //    Console.WriteLine("objSearcher was created successfully");

            // [DebugLine] Console.WriteLine("Get WMI data...");

            // System.Management.ManagementObject objReturn = null;

            //try
            //{
            //    objReturn = objSearcher.Get();
            //    return objReturn;
            //}
            //catch (SystemException ex)
            //{
            //    // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
            //    string strRPCUnavailable = "The RPC server is unavailable. (Exception from HRESULT: 0x800706BA)";
            //    // [DebugLine] System.Console.WriteLine(ex.Message);
            //    if (ex.Message == strRPCUnavailable)
            //    {
            //        Console.WriteLine("WMI: Server unavailable");
            //        objReturn = null;
            //    }
            //    else
            //    {
            //        System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
            //        System.Console.WriteLine(ex.Message);
            //        objReturn = null;
            //    }

            //    // Next line will return an object that is equal to null
            //    return objReturn;
            //}
        }

        static DirectorySearcher funcCreateDSSearcher()
        {
            // [Comment] Get local domain context
            string rootDSE;

            System.DirectoryServices.DirectorySearcher objrootDSESearcher = new System.DirectoryServices.DirectorySearcher();
            rootDSE = objrootDSESearcher.SearchRoot.Path;
            // [DebugLine]Console.WriteLine(rootDSE);

            // [Comment] Construct DirectorySearcher object using rootDSE string
            System.DirectoryServices.DirectoryEntry objrootDSEentry = new System.DirectoryServices.DirectoryEntry(rootDSE);
            System.DirectoryServices.DirectorySearcher objDSSearcher = new System.DirectoryServices.DirectorySearcher(objrootDSEentry);
            // [DebugLine]Console.WriteLine(objDSSearcher.SearchRoot.Path);
            return objDSSearcher;
        }

        static void funcPrintParameterWarning()
        {
            Console.WriteLine("Parameters must be specified properly to run LocalProfileManager.");
            Console.WriteLine("Run LocalProfileManager -? to get the parameter syntax.");
        }

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("LocalProfileManager v1.0");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following required parameter:");
            Console.WriteLine("-run                required parameter");
            Console.WriteLine();
            Console.WriteLine("Use either of the following parameters:");
            Console.WriteLine("-list               to list local profiles only");
            Console.WriteLine("-check              to check local profiles for valid accounts");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("LocalProfileManager -run -list");
            Console.WriteLine("LocalProfileManager -run -check");
        }

         static void funcLogToEventLog(string strAppName, string strEventMsg, int intEventType)
        {
            string sLog;

            sLog = "Application";

            if (!EventLog.SourceExists(strAppName))
                EventLog.CreateEventSource(strAppName, sLog);

            //EventLog.WriteEntry(strAppName, strEventMsg);
            EventLog.WriteEntry(strAppName, strEventMsg, EventLogEntryType.Information, intEventType);

        } // LogToEventLog

        static void funcGetProfileData(int intOS, bool bCheckProfile)
        {
            try
            {
                TextWriter twCurrent = funcOpenOutputLog();
                string strOutputMsg = "";

                if (intOS == 1)
                {
                    string strRegistryProfilesPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
                    RegistryKey objRootKey = Microsoft.Win32.Registry.LocalMachine;
                    RegistryKey objProfileKey = objRootKey.OpenSubKey(strRegistryProfilesPath);
                    string[] arrProfileList = objProfileKey.GetSubKeyNames();
                    foreach (string strTemp in arrProfileList)
                    {
                        //[DebugLine] Console.WriteLine("*****************Profile*****************");
                        //[DebugLine] Console.WriteLine(strTemp);
                        strOutputMsg = "Profile SID: " + strTemp;
                        funcWriteToOutputLog(twCurrent, strOutputMsg);
                        string strRegProfPath = strRegistryProfilesPath + "\\" + strTemp;
                        // [DebugLine] Console.WriteLine(strRegProfPath);
                        RegistryKey objProfileImagePathKey = objProfileKey.OpenSubKey(strTemp);
                        string strProfileImagePath = objProfileImagePathKey.GetValue("ProfileImagePath", "", RegistryValueOptions.DoNotExpandEnvironmentNames).ToString();
                        //[DebugLine] Console.WriteLine(strProfileImagePath);
                        strOutputMsg = "Profile Path: " + strProfileImagePath;
                        funcWriteToOutputLog(twCurrent, strOutputMsg);

                        if ((strTemp != "S-1-5-18") & (strTemp != "S-1-5-19") & (strTemp != "S-1-5-20"))
                        {
                            // [DebugLine] Console.WriteLine("Getting account for SID: " + strTemp);

                            string strWin32Query = "Win32_SID.SID=\"" + strTemp + "\"";
                            string strFullProfileImagePath = "";
                            string strSystemDriveEnvVarPath = Environment.GetEnvironmentVariable("%SystemDrive%");
                            if (strProfileImagePath.Contains("%SystemDrive%"))
                            {
                                strFullProfileImagePath = strProfileImagePath.Replace("%SystemDrive%", strSystemDriveEnvVarPath);
                            }

                            // [DebugLine] Console.WriteLine("Calling funcSysQueryData2...");
                            AccountData stAccountData = funcSysQueryData2(strWin32Query, ".");

                            if (stAccountData.bAccountValid)
                            {
                                strOutputMsg = "Profile Account name: " + stAccountData.strAccountName;
                                funcWriteToOutputLog(twCurrent, strOutputMsg);
                            }
                            else
                            {
                                strOutputMsg = "No valid account exists for: " + strTemp;
                                funcWriteToOutputLog(twCurrent, strOutputMsg);
                            }

                            if (!stAccountData.bAccountValid & bCheckProfile)
                            {
                                bool bProfileRemoved = funcRemoveProfile(strTemp, strFullProfileImagePath, intOS);
                                if (bProfileRemoved)
                                {
                                    strOutputMsg = "Profile & Profile Path(" + strFullProfileImagePath + ") was succesfully removed";
                                    funcWriteToOutputLog(twCurrent, strOutputMsg);
                                }
                                else
                                {
                                    strOutputMsg = "Profile & Profile Path(" + strFullProfileImagePath + ") was NOT succesfully removed";
                                    funcWriteToOutputLog(twCurrent, strOutputMsg);
                                }
                            }
                        }

                    }

                }
                if (intOS == 2)
                {

                    //**********************************
                    //Begin-Win32_UserProfile
                    //**********************************
                    //Get the query results for Win32_UserProfile
                    ManagementObjectCollection oQueryCollection = null;
                    oQueryCollection = funcSysQueryData("select * from Win32_UserProfile", ".");

                    if (oQueryCollection != null)
                    {
                        foreach (ManagementObject oReturn in oQueryCollection)
                        {
                            // "SID", "LocalPath", "Loaded", "refCount", "Special", "RoamingConfigured", "RoamingPath", "RoamingPreference", "Status", "LastUseTime", "LastDownloadTime", "LastUploadTime"

                            string[] strElementBag = new string[] { "SID", "LocalPath", "Loaded", "refCount", "Special", "RoamingConfigured", "RoamingPath", "RoamingPreference", "Status", "LastUseTime", "LastDownloadTime", "LastUploadTime" };
                            foreach (string strElement in strElementBag)
                            {
                                string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));

                                switch (strElement)
                                {
                                    case "SID":
                                        //[DebugLine] Console.WriteLine("\"" + strElementTemp + "\"" + " : " + "\"" + oReturn[strElement].ToString().Trim() + "\"");
                                        break;
                                    case "LocalPath":
                                        //[DebugLine] Console.WriteLine("\"" + strElementTemp + "\"" + " : " + "\"" + oReturn[strElement].ToString().Trim() + "\"");
                                        break;
                                    default:
                                        break;
                                } // end of switch

                            }
                        }
                    }
                    //**********************************
                    //End-Win32_UserProfile
                    //**********************************

                    string strRegistryProfilesPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
                    RegistryKey objRootKey = Microsoft.Win32.Registry.LocalMachine;
                    RegistryKey objProfileKey = objRootKey.OpenSubKey(strRegistryProfilesPath);
                    string[] arrProfileList = objProfileKey.GetSubKeyNames();
                    foreach (string strTemp in arrProfileList)
                    {
                        //[DebugLine] Console.WriteLine("*****************Profile*****************");
                        //[DebugLine] Console.WriteLine(strTemp);
                        strOutputMsg = "Profile SID: " + strTemp;
                        funcWriteToOutputLog(twCurrent, strOutputMsg);
                        string strRegProfPath = strRegistryProfilesPath + "\\" + strTemp;
                        // [DebugLine] Console.WriteLine(strRegProfPath);
                        RegistryKey objProfileImagePathKey = objProfileKey.OpenSubKey(strTemp);
                        string strProfileImagePath = objProfileImagePathKey.GetValue("ProfileImagePath", "", RegistryValueOptions.DoNotExpandEnvironmentNames).ToString();
                        //[DebugLine] Console.WriteLine(strProfileImagePath);
                        strOutputMsg = "Profile Path: " + strProfileImagePath;
                        funcWriteToOutputLog(twCurrent, strOutputMsg);

                        if ((strTemp != "S-1-5-18") & (strTemp != "S-1-5-19") & (strTemp != "S-1-5-20"))
                        {
                            // [DebugLine] Console.WriteLine("Getting account for SID: " + strTemp);

                            string strWin32Query = "Win32_SID.SID=\"" + strTemp + "\"";
                            string strFullProfileImagePath = "";
                            string strSystemDriveEnvVarPath = Environment.GetEnvironmentVariable("%SystemDrive%");
                            if (strProfileImagePath.Contains("%SystemDrive%"))
                            {
                                strFullProfileImagePath = strProfileImagePath.Replace("%SystemDrive%", strSystemDriveEnvVarPath);
                            }
                            else
                            {
                                strFullProfileImagePath = strProfileImagePath;
                            }

                            // [DebugLine] Console.WriteLine("Calling funcSysQueryData2...");
                            AccountData stAccountData = funcSysQueryData2(strWin32Query, ".");

                            if (stAccountData.bAccountValid)
                            {
                                strOutputMsg = "Profile Account name: " + stAccountData.strAccountName;
                                funcWriteToOutputLog(twCurrent, strOutputMsg);
                            }
                            else
                            {
                                strOutputMsg = "No valid account exists for: " + strTemp;
                                funcWriteToOutputLog(twCurrent, strOutputMsg);
                            }

                            if (!stAccountData.bAccountValid & bCheckProfile)
                            {
                                bool bProfileRemoved = funcRemoveProfile(strTemp, strFullProfileImagePath, intOS);
                                if (bProfileRemoved)
                                {
                                    strOutputMsg = "Profile & Profile Path(" + strFullProfileImagePath + ") was succesfully removed";
                                    funcWriteToOutputLog(twCurrent, strOutputMsg);
                                }
                                else
                                {
                                    strOutputMsg = "Profile & Profile Path(" + strFullProfileImagePath + ") was NOT succesfully removed";
                                    funcWriteToOutputLog(twCurrent, strOutputMsg);
                                }
                            }
                        }

                    }

                }

                funcCloseOutputLog(twCurrent);
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static OSCaptionVersion funcCheckOSCaptionVersion(OSCaptionVersion tmpOSCapVer)
        {
            //**********************************
            //Begin-Win32_OperatingSystem
            //**********************************
            //Get the query results for Win32_ComputerSystem
            ManagementObjectCollection oQueryCollection0 = null;
            oQueryCollection0 = funcSysQueryData("select * from Win32_OperatingSystem", ".");

            if (oQueryCollection0 != null)
            {
                foreach (ManagementObject oReturn in oQueryCollection0)
                {
                    // "Caption","CSDVersion","Version","BootDevice","SystemDevice","SystemDirectory","WindowsDirectory","InstallDate"
                    string[] strElementBag = new string[] { "Caption", "CSDVersion", "Version", "BootDevice", "SystemDevice", "SystemDirectory", "WindowsDirectory", "InstallDate" };
                    foreach (string strElement in strElementBag)
                    {
                        string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                        //[DebugLine] Console.WriteLine("\"" + strElementTemp + "\"" + " : " + "\"" + oReturn[strElement].ToString().Trim() + "\"");
                        if (strElement == "Caption")
                        {
                            if (oReturn[strElement].ToString().Contains("2003"))
                            {
                                tmpOSCapVer.intcaptionvalue = 1;
                            }
                            if (oReturn[strElement].ToString().Contains("2008"))
                            {
                                tmpOSCapVer.intcaptionvalue = 2;
                            }
                        }
                        if (strElement == "Version")
                        {
                            if (oReturn[strElement].ToString().Substring(0, 1) == "5")
                            {
                                tmpOSCapVer.intversionvalue = 1;
                            }
                            if (oReturn[strElement].ToString().Substring(0, 1) == "6")
                            {
                                tmpOSCapVer.intversionvalue = 2;
                            }
                        }
                    }

                }
            }
            //**********************************
            //End-Win32_OperatingSystem
            //**********************************

            return tmpOSCapVer;
        }

        static bool funcRemoveProfile(string strProfileSID, string strProfilePath, int intOS)
        {
            try
            {
                bool bResult = false;
                //[DebugLine Console.WriteLine("Called funcRemoveProfile....");
                if (intOS == 1)
                {
                    //[DebugLine Console.WriteLine("Removing Profile registry entry....");
                    string strRegistryProfilesPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
                    RegistryKey objRootKey = Microsoft.Win32.Registry.LocalMachine;
                    RegistryKey objProfileKey = objRootKey.OpenSubKey(strRegistryProfilesPath, true);
                    objProfileKey.DeleteSubKeyTree(strProfileSID);

                    //[DebugLine Console.WriteLine("Remove Profile directory,sub-directories,files...");

                    funcRemoveDirectory(strProfilePath);

                    RegistryKey objResultKey = objRootKey.OpenSubKey(strRegistryProfilesPath + "\\" + strProfileSID);

                    if (objResultKey == null & !Directory.Exists(strProfilePath))
                    {
                        bResult = true;
                    }
                    else
                    {
                        bResult = false;
                    }
                }
                if (intOS == 2)
                {
                    //**********************************
                    //Begin-Win32_UserProfile
                    //**********************************
                    //Get the query results for Win32_UserProfile
                    ManagementObjectCollection oQueryCollection = null;
                    oQueryCollection = funcSysQueryData("select * from Win32_UserProfile", ".");

                    if (oQueryCollection != null)
                    {
                        foreach (ManagementObject oReturn in oQueryCollection)
                        {
                            // "SID", "LocalPath", "Loaded", "refCount", "Special", "RoamingConfigured", "RoamingPath", "RoamingPreference", "Status", "LastUseTime", "LastDownloadTime", "LastUploadTime"

                            string[] strElementBag = new string[] { "SID", "LocalPath", "Loaded", "refCount", "Special", "RoamingConfigured", "RoamingPath", "RoamingPreference", "Status", "LastUseTime", "LastDownloadTime", "LastUploadTime" };
                            foreach (string strElement in strElementBag)
                            {
                                string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));

                                switch (strElement)
                                {
                                    case "SID":
                                        //[DebugLine] Console.WriteLine("\"" + strElementTemp + "\"" + " : " + "\"" + oReturn[strElement].ToString().Trim() + "\"");
                                        break;
                                    case "LocalPath":
                                        //[DebugLine] Console.WriteLine("\"" + strElementTemp + "\"" + " : " + "\"" + oReturn[strElement].ToString().Trim() + "\"");
                                        break;
                                    default:
                                        break;
                                } // end of switch

                            }
                        }
                    }
                    //**********************************
                    //End-Win32_UserProfile
                    //**********************************

                    //[DebugLine Console.WriteLine("Removing Profile registry entry....");
                    string strRegistryProfilesPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
                    RegistryKey objRootKey = Microsoft.Win32.Registry.LocalMachine;
                    RegistryKey objProfileKey = objRootKey.OpenSubKey(strRegistryProfilesPath, true);
                    objProfileKey.DeleteSubKeyTree(strProfileSID);

                    //[DebugLine Console.WriteLine("Remove Profile directory,sub-directories,files...");

                    funcRemoveDirectory(strProfilePath);

                    RegistryKey objResultKey = objRootKey.OpenSubKey(strRegistryProfilesPath + "\\" + strProfileSID);

                    if (objResultKey == null & !Directory.Exists(strProfilePath))
                    {
                        bResult = true;
                    }
                    else
                    {
                        bResult = false;
                    }
                }

                return bResult;
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static void funcRecurse(DirectoryInfo directory)
        {
	        foreach(FileInfo fi in directory.GetFiles())
            {
                fi.Attributes = FileAttributes.Normal;
            }

            foreach (DirectoryInfo di in directory.GetDirectories())
            {
                di.Attributes = FileAttributes.Normal;
                FileAttributes dirAttrib = di.Attributes;
                if((dirAttrib & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    //Console.WriteLine("Junction Point: " + di.Name + " " + di.FullName);
                    bool bRemoveJunction = RemoveDirectory(di.FullName);
                    //if (bRemoveJunction)
                    //{
                    //    Console.WriteLine("Junction Point: " + di.Name + " was removed");
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Junction Point: " + di.Name + " was NOT removed");
                    //}
                }
            }

            foreach (DirectoryInfo subdir2 in directory.GetDirectories())
            {
                funcRecurse(subdir2);
            }
        }

        static void funcRemoveDirectory(string strDirectoryPath)
        {
            try
            {
                DirectoryInfo tmpDirectoryInfo = new DirectoryInfo(strDirectoryPath);

                funcRecurse(tmpDirectoryInfo);

                System.IO.Directory.Delete(strDirectoryPath, true);

            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static bool funcCheckForFile(string strInputFileName)
        {
            try
            {
                if (System.IO.File.Exists(strInputFileName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static void funcGetFuncCatchCode(string strFunctionName, Exception currentex)
        {
            string strCatchCode = "";

            Dictionary<string, string> dCatchTable = new Dictionary<string, string>();
            dCatchTable.Add("funcGetFuncCatchCode", "f0");
            dCatchTable.Add("funcPrintParameterWarning", "f2");
            dCatchTable.Add("funcPrintParameterSyntax", "f3");
            dCatchTable.Add("funcParseCmdArguments", "f4");
            dCatchTable.Add("funcProgramExecution", "f5");
            dCatchTable.Add("funcCreateDSSearcher", "f7");
            dCatchTable.Add("funcCreatePrincipalContext", "f8");
            dCatchTable.Add("funcCheckNameExclusion", "f9");
            dCatchTable.Add("funcMoveDisabledAccounts", "f10");
            dCatchTable.Add("funcFindAccountsToDisable", "f11");
            dCatchTable.Add("funcCheckLastLogin", "f12");
            dCatchTable.Add("funcRemoveUserFromGroup", "f13");
            dCatchTable.Add("funcToEventLog", "f14");
            dCatchTable.Add("funcCheckForFile", "f15");
            dCatchTable.Add("funcCheckForOU", "f16");
            dCatchTable.Add("funcWriteToErrorLog", "f17");
            dCatchTable.Add("funcGetUserGroups", "f18");
            dCatchTable.Add("funcOpenOutputLog", "f20");
            dCatchTable.Add("funcWriteToOutputLog", "f21");
            dCatchTable.Add("funcSearchForUser", "f22");
            dCatchTable.Add("funcSearchForGroup", "f23");
            dCatchTable.Add("funcGetGroup", "f24");
            dCatchTable.Add("funcGetUser", "f25");
            dCatchTable.Add("funcParseUserName", "f26");
            dCatchTable.Add("funcAddUserToGroup", "f27");
            dCatchTable.Add("funcGetColumnSelection", "f28");
            dCatchTable.Add("funcPrintColumnSelect", "f29");
            dCatchTable.Add("funcProcessFiles", "f30");
            dCatchTable.Add("funcCheckFileRowsForDelimiter", "f31");
            dCatchTable.Add("funcSysQueryData", "f32");
            dCatchTable.Add("funcSysQueryData2", "f33");
            dCatchTable.Add("funcGetProfileData", "f34");
            dCatchTable.Add("funcCheckOSCaptionVersion", "f35");
            dCatchTable.Add("funcRemoveProfile", "f36");
            dCatchTable.Add("funcRecurse", "f37");
            dCatchTable.Add("funcRemoveDirectory", "f38");

            if (dCatchTable.ContainsKey(strFunctionName))
            {
                strCatchCode = "err" + dCatchTable[strFunctionName] + ": ";
            }

            //[DebugLine] Console.WriteLine(strCatchCode + currentex.GetType().ToString());
            //[DebugLine] Console.WriteLine(strCatchCode + currentex.Message);

            funcWriteToErrorLog(strCatchCode + currentex.GetType().ToString());
            funcWriteToErrorLog(strCatchCode + currentex.Message);

        }

        static void funcWriteToErrorLog(string strErrorMessage)
        {
            try
            {
                FileStream newFileStream = new FileStream("Err-LocalProfileManager.log", FileMode.Append, FileAccess.Write);
                TextWriter twErrorLog = new StreamWriter(newFileStream);

                DateTime dtNow = DateTime.Now;

                string dtFormat = "MMddyyyy HH:mm:ss";

                twErrorLog.WriteLine("{0} \t {1}", dtNow.ToString(dtFormat), strErrorMessage);

                twErrorLog.Close();
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

        }

        static TextWriter funcOpenOutputLog()
        {
            try
            {
                DateTime dtNow = DateTime.Now;

                string dtFormat2 = "MMddyyyy"; // for log file directory creation

                string strPath = Directory.GetCurrentDirectory();

                string strLogFileName = strPath + "\\LocalProfileManager" + dtNow.ToString(dtFormat2) + ".log";

                FileStream newFileStream = new FileStream(strLogFileName, FileMode.Append, FileAccess.Write);
                TextWriter twOuputLog = new StreamWriter(newFileStream);

                return twOuputLog;
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return null;
            }

        }

        static void funcWriteToOutputLog(TextWriter twCurrent, string strOutputMessage)
        {
            try
            {
                DateTime dtNow = DateTime.Now;

                string dtFormat = "MMddyyyy HH:mm:ss";

                twCurrent.WriteLine("{0} \t {1}", dtNow.ToString(dtFormat), strOutputMessage);
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static void funcCloseOutputLog(TextWriter twCurrent)
        {
            try
            {
                twCurrent.Close();
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static void funcProgramExecution(CMDArguments objCMDArguments2)
        {
            try
            {
                OSCaptionVersion varOSCaptionVersion = new OSCaptionVersion();

                funcProgramRegistryTag("LocalProfileManager");

                varOSCaptionVersion = funcCheckOSCaptionVersion(varOSCaptionVersion);

                if (objCMDArguments2.strSwitch == "-list")
                {
                    if (varOSCaptionVersion.intcaptionvalue == 1 & varOSCaptionVersion.intversionvalue == 1)
                    {
                        funcGetProfileData(1, false);
                    }
                    else
                    {
                        funcGetProfileData(2, false);
                    }
                }

                if (objCMDArguments2.strSwitch == "-check")
                {
                    if (varOSCaptionVersion.intcaptionvalue == 1 & varOSCaptionVersion.intversionvalue == 1)
                    {
                        funcGetProfileData(1, true);
                    }
                    else
                    {
                        funcGetProfileData(2, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static CMDArguments funcParseCmdArguments(string[] cmdargs)
        {
            CMDArguments objCMDArguments = new CMDArguments();

            try
            {
                if (cmdargs[0] == "-run" & cmdargs.Length > 1)
                {
                    if (cmdargs[1] == "-list" | cmdargs[1] == "-check")
                    {
                        if (cmdargs[1] == "-list")
                        {
                            objCMDArguments.strSwitch = "-list";
                        }
                        else
                        {
                            objCMDArguments.strSwitch = "-check";
                        }
                        objCMDArguments.bParseCmdArguments = true;
                    }
                }
                else
                {
                    objCMDArguments.bParseCmdArguments = false;
                }

            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                objCMDArguments.bParseCmdArguments = false;
            }

            return objCMDArguments;
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    funcPrintParameterWarning();
                }
                else
                {
                    if (args[0] == "-?")
                    {
                        funcPrintParameterSyntax();
                    }
                    else
                    {
                        string[] arrArgs = args;
                        CMDArguments objArgumentsProcessed = funcParseCmdArguments(arrArgs);

                        if (objArgumentsProcessed.bParseCmdArguments)
                        {
                            funcProgramExecution(objArgumentsProcessed);
                        }
                        else
                        {
                            funcPrintParameterWarning();
                        } // check objArgumentsProcessed.bParseCmdArguments
                    } // check args[0] = "-?"
                } // check args.Length == 0
            }
            catch (Exception ex)
            {
                Console.WriteLine("errm0: {0}", ex.Message);
            }
        }
    }
}

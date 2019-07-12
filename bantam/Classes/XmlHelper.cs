﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace bantam.Classes
{
    static class XmlHelper
    {
        /// <summary>
        /// Loads Shells into the UI from an XML file
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public async static Task LoadShells(string configFile)
        {
            if (File.Exists(configFile)) {

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configFile);

                XmlNodeList itemNodes = xmlDoc.SelectNodes("//servers/server");

                if (itemNodes != null && itemNodes.Count > 0) {
                    foreach (XmlNode itemNode in itemNodes) {
                        string hostTarget = (itemNode.Attributes?["host"] != null) ? itemNode.Attributes["host"].Value : string.Empty;
                        string requestArg = (itemNode.Attributes?["request_arg"] != null) ? itemNode.Attributes["request_arg"].Value : string.Empty;
                        string requestMethod = (itemNode.Attributes?["request_method"] != null) ? itemNode.Attributes["request_method"].Value : string.Empty;
                        string ResponseEncryption = (itemNode.Attributes?["response_encryption"] != null) ? itemNode.Attributes["response_encryption"].Value : string.Empty;
                        string ResponseEncryptionMode = (itemNode.Attributes?["response_encryption_mode"] != null) ? itemNode.Attributes["response_encryption_mode"].Value : string.Empty;
                        string gzipRequest = (itemNode.Attributes?["gzip_request"] != null) ? itemNode.Attributes["gzip_request"].Value : string.Empty;
                        string requestEncryption = (itemNode.Attributes?["request_encryption"] != null) ? itemNode.Attributes["request_encryption"].Value : string.Empty;
                        string requestEncryptionKey = (itemNode.Attributes?["request_encryption_key"] != null) ? itemNode.Attributes["request_encryption_key"].Value : string.Empty;
                        string requestEncryptionIV = (itemNode.Attributes?["request_encryption_iv"] != null) ? itemNode.Attributes["request_encryption_iv"].Value : string.Empty;
                        string requestEncryptionIVVarName = (itemNode.Attributes?["request_encryption_iv_var_name"] != null) ? itemNode.Attributes["request_encryption_iv_var_name"].Value : string.Empty;
                        
                        if (string.IsNullOrEmpty(hostTarget)) {
                            continue;
                        }

                        if (BantamMain.Shells.ContainsKey(hostTarget)) {
                            continue;
                        }

                        if (!BantamMain.Shells.TryAdd(hostTarget, new ShellInfo())) {
                            LogHelper.AddGlobalLog("Unable to add (" + hostTarget + ") to shells from XML", "LoadShells failure", LogHelper.LOG_LEVEL.ERROR);
                            continue;
                        }

                        if (string.IsNullOrEmpty(requestArg) == false
                        && requestArg != "command") {
                            BantamMain.Shells[hostTarget].RequestArgName = requestArg;
                        }

                        if (string.IsNullOrEmpty(requestMethod) == false
                         && requestMethod == "cookie") {
                            BantamMain.Shells[hostTarget].SendDataViaCookie = true;
                        }

                        if (string.IsNullOrEmpty(ResponseEncryption) == false) {
                            if (ResponseEncryption == "1") {
                                BantamMain.Shells[hostTarget].ResponseEncryption = true;
                            } else {
                                BantamMain.Shells[hostTarget].ResponseEncryption = false;
                            }
                        }

                        if (string.IsNullOrEmpty(requestEncryption) == false && requestEncryption == "1") {
                            BantamMain.Shells[hostTarget].RequestEncryption = true;

                            if (string.IsNullOrEmpty(requestEncryptionKey) == false) {
                                BantamMain.Shells[hostTarget].RequestEncryptionKey = requestEncryptionKey;
                            }

                            if (string.IsNullOrEmpty(requestEncryptionIV) == false) {
                                BantamMain.Shells[hostTarget].RequestEncryptionIV = requestEncryptionIV;
                                BantamMain.Shells[hostTarget].RequestEncryptionIVRequestVarName = string.Empty;
                                BantamMain.Shells[hostTarget].SendRequestEncryptionIV = false;
                            } else {
                                if (string.IsNullOrEmpty(requestEncryptionIVVarName) == false) {
                                    BantamMain.Shells[hostTarget].SendRequestEncryptionIV = true;
                                    BantamMain.Shells[hostTarget].RequestEncryptionIV = string.Empty;
                                    BantamMain.Shells[hostTarget].RequestEncryptionIVRequestVarName = requestEncryptionIVVarName;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(gzipRequest) == false) {
                            if (gzipRequest == "1") {
                                BantamMain.Shells[hostTarget].GzipRequestData = true;
                            } else {
                                BantamMain.Shells[hostTarget].GzipRequestData = false;
                            }
                        } else {
                            BantamMain.Shells[hostTarget].GzipRequestData = false;
                        }

                        if (string.IsNullOrEmpty(ResponseEncryptionMode) == false) {
                            if (ResponseEncryptionMode == (CryptoHelper.RESPONSE_ENCRYPTION_TYPES.OPENSSL).ToString("D")) {
                                BantamMain.Shells[hostTarget].ResponseEncryptionMode = (int)CryptoHelper.RESPONSE_ENCRYPTION_TYPES.OPENSSL;
                            } else if (ResponseEncryptionMode == CryptoHelper.RESPONSE_ENCRYPTION_TYPES.MCRYPT.ToString("D")) {
                                BantamMain.Shells[hostTarget].ResponseEncryptionMode = (int)CryptoHelper.RESPONSE_ENCRYPTION_TYPES.MCRYPT;
                            }
                        }

                        try {
                            BantamMain.Instance.InitializeShellData(hostTarget);
                        } catch (Exception e) {
                            LogHelper.AddGlobalLog("Exception caught in XmlHelper.LoadShells ( " + e.Message + " )", "XML Load file Exception", LogHelper.LOG_LEVEL.INFO);
                        }
                    }
                }
            } else {
                LogHelper.AddGlobalLog("Config file (" + configFile + ") is missing.", "Failed to located XML File", LogHelper.LOG_LEVEL.ERROR);
            }
        }

        /// <summary>
        /// Saves Shells from the UI into an XML file
        /// </summary>
        /// <param name="configFile"></param>
        public static void SaveShells(string configFile)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("servers");
            xmlDoc.AppendChild(rootNode);

            foreach (KeyValuePair<String, ShellInfo> host in BantamMain.Shells) {
                ShellInfo shellInfo = host.Value;

                XmlNode serverNode = xmlDoc.CreateElement("server");

                XmlAttribute hostAttribute = xmlDoc.CreateAttribute("host");
                hostAttribute.Value = host.Key;

                serverNode.Attributes.Append(hostAttribute);

                XmlAttribute requestArgAttribute = xmlDoc.CreateAttribute("request_arg");
                requestArgAttribute.Value = shellInfo.RequestArgName;

                serverNode.Attributes.Append(requestArgAttribute);

                XmlAttribute requestMethod = xmlDoc.CreateAttribute("request_method");
                requestMethod.Value = (shellInfo.SendDataViaCookie ? "cookie" : "post");
                serverNode.Attributes.Append(requestMethod);

                XmlAttribute ResponseEncryption = xmlDoc.CreateAttribute("response_encryption");
                ResponseEncryption.Value = (shellInfo.ResponseEncryption ? "1" : "0");
                serverNode.Attributes.Append(ResponseEncryption);

                XmlAttribute responseEncrpytionMode = xmlDoc.CreateAttribute("response_encryption_mode");
                responseEncrpytionMode.Value = shellInfo.ResponseEncryptionMode.ToString();
                serverNode.Attributes.Append(responseEncrpytionMode);

                XmlAttribute gzipRequest = xmlDoc.CreateAttribute("gzip_request");
                gzipRequest.Value = (shellInfo.GzipRequestData ? "1" : "0");
                serverNode.Attributes.Append(gzipRequest);

                XmlAttribute requestEncryption = xmlDoc.CreateAttribute("request_encryption");
                requestEncryption.Value = (shellInfo.RequestEncryption ? "1" : "0");
                serverNode.Attributes.Append(requestEncryption);

                XmlAttribute requestEncryptionKey = xmlDoc.CreateAttribute("request_encryption_key");
                requestEncryptionKey.Value = shellInfo.RequestEncryptionKey;
                serverNode.Attributes.Append(requestEncryptionKey);

                XmlAttribute requestEncryptionIV = xmlDoc.CreateAttribute("request_encryption_iv");
                requestEncryptionIV.Value = shellInfo.RequestEncryptionIV;
                serverNode.Attributes.Append(requestEncryptionIV);

                XmlAttribute requestEncryptionIVVarName = xmlDoc.CreateAttribute("request_encryption_iv_var_name");
                requestEncryptionIVVarName.Value = shellInfo.RequestEncryptionIVRequestVarName;
                serverNode.Attributes.Append(requestEncryptionIVVarName);

                rootNode.AppendChild(serverNode);
            }
            xmlDoc.Save(configFile);
        }
    }
}

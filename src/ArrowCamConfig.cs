using System;
using System.IO;
using System.Text;
using System.Xml;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace ArrowCam
{
    public static class ArrowCamConfig
    {
        private static PlatformFilePath _configFile;

        public static float StopWatchingDelay { get; set; } = 0.2f;
        public static bool EnableSlowMotion { get; set; } = true;
        public static float SlowMotionTime { get; set; } = 3.0f;

        public static void Initialize()
        {
            // Get the config file path
            ArrowCamConfig._configFile = new PlatformFilePath(EngineFilePaths.ConfigsPath, "ArrowCamConfig.xml");

            // If it does not exist, create it by an initial save
            if (!FileHelper.FileExists(ArrowCamConfig._configFile))
            {
                ArrowCamConfig.Save();
            }
            else
            {
                // Read it
                var content = FileHelper.GetFileContentString(ArrowCamConfig._configFile);
                var stringReader = new System.IO.StringReader(content);
                XmlTextReader textReader = new XmlTextReader(stringReader);

                bool found_version = false;
                while (textReader.Read())
                {
                    if (textReader.IsStartElement())
                    {
                        if (textReader.Name == "version")
                        {
                            found_version = true;
                        }
                    }
                }

                textReader.Close();

                if (!found_version)
                {
                    // Reset
                    ArrowCamConfig.Save();
                }

                content = FileHelper.GetFileContentString(ArrowCamConfig._configFile);
                stringReader = new System.IO.StringReader(content);
                textReader = new XmlTextReader(stringReader);

                while (textReader.Read())
                {
                    if (textReader.IsStartElement())
                    {
                        if (textReader.Name == "stopWatchingDelay")
                        {
                            ArrowCamConfig.StopWatchingDelay = float.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "enableSlowMotion")
                        {
                            ArrowCamConfig.EnableSlowMotion = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "slowMotionTime")
                        {
                            ArrowCamConfig.SlowMotionTime = float.Parse(textReader.ReadString());
                        }
                    }
                }
            }
        }

        public static void Save()
        {
            // Open writer and write settings
            XmlDocument xmlDocument = new XmlDocument();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.NewLineChars = "\r\n";
            xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
            StringBuilder sb = new StringBuilder();

            //using (XmlTextWriter textWriter = XmlWriter.Create(xmlDocument.CreateNavigator().AppendChild(), xmlWriterSettings))
            using (XmlWriter textWriter = XmlWriter.Create(sb, xmlWriterSettings))
            {
                textWriter.WriteStartElement("config");
                textWriter.WriteComment("If you want to reset the configuration to default values just delete this file.");
                textWriter.WriteComment("How long the camera will stay active when lifting the ArrowCam key.");
                textWriter.WriteElementString("stopWatchingDelay", ArrowCamConfig.StopWatchingDelay.ToString());
                textWriter.WriteComment("Enables (True) / Disables (False) the slow motion after a hit.");
                textWriter.WriteElementString("enableSlowMotion", ArrowCamConfig.EnableSlowMotion.ToString());
                textWriter.WriteComment("Maximum time that the slow motion will be active.");
                textWriter.WriteElementString("slowMotionTime", ArrowCamConfig.SlowMotionTime.ToString());

                textWriter.WriteEndElement();
            }

            FileHelper.SaveFileString(ArrowCamConfig._configFile, sb.ToString());
        }
    }
}

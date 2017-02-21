using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using PcapDotNet.Core;

using PcapDotNet.Packets;
using System.Collections;
using Listener = GamesListener.Game.Listener;

namespace GamesListener.Util
{
    class GLOLoader
    {
        private List<Listener> Listeners;
        private List<LivePacketDevice> EthDevices = new List<LivePacketDevice>();
        
        private static string DebugReadConfig(string rawData) 
        {
            string jsonContents = "";
            JsonTextReader jsonReader = new JsonTextReader(new StringReader(rawData));
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    jsonContents += String.Format("Pair: {0}, Value: {1}\n", jsonReader.TokenType, jsonReader.Value);
                }
                else
                {
                    jsonContents += String.Format("Pair: {0}\n", jsonReader.TokenType);
                }
            }
            return jsonContents;
        }

        public void ReadConfig(string fileName, bool debug = false) 
        {
            // relative to the executable, config.json must reside therein.
            string __baseDir = Environment.CurrentDirectory + "\\";
            string __portsConf = __baseDir + fileName;

            // if file exists, get contents
            try
            {
                string json = File.ReadAllText(__portsConf);
                if (debug) Console.WriteLine(json);
                // http://www.newtonsoft.com/json/help/html/DeserializeCollection.htm
                Listeners = JsonConvert.DeserializeObject<List<Listener>>(json);                
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message + " " + ex.StackTrace);
            }

            foreach (Listener glo in Listeners)
            {
                glo.FindAppPID();
            }
        }

        // only run the processpacket method, on each listener, if
        // we have an ethernet device in the list. (see calls above).
        public void SurveyEthernetDevices()
        {
            IList<LivePacketDevice> Devices = LivePacketDevice.AllLocalMachine;
            int index = 0;
            var input = "";
            int choice = 0;

            foreach (LivePacketDevice dev in Devices)
            {
                Console.WriteLine(String.Format("[{0}] {1}", index, dev.Description));
                index++;
            }

            Console.WriteLine("Select index of device to listen on...");
            input = Console.ReadLine();
            if (input == "") input = "0";
            choice = Convert.ToInt16(input);

            using (PacketCommunicator pm = Devices[choice].Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                EthDevices.Add(Devices[choice]);
            }
        }            

        public void NetstatListeners()
        {
            foreach (Listener glo in Listeners)
            {
                glo.NetstatAppPID();
            }
        }

        public void InterceptListeners()
        {
            foreach (Listener glo in Listeners)
            {
                glo.InterceptPortPackets(EthDevices);
            }
        }
    }
}

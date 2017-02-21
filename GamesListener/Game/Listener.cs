using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Core;

namespace GamesListener.Game
{
    // provide basic info - printable name, and exectuble's filename.
    // get process information from executable, the rest follows from there.
    public class Listener
    {
        public string Title;
        public string Executable;
        private int PID = -1;

        private List<Port> Ports = new List<Port>();
        private string RemoteAddress;
                
        public void FindAppPID() 
        {
            try
            {
                using (Process p = Process.GetProcessesByName(Executable).Last())
                {
                    PID = p.Id;
                    Console.WriteLine(String.Format("PID = {0}: [{1}] '{2}' ", PID, Executable, Title));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("No PID: [{0}] '{1}'", Executable, Title));
            }
        }
        
        public void NetstatAppPID(string prot = "UDP") {
            if (PID < 0)
            {
                PrintAppPIDInvalid();
                return;
            }
            try
            {
                using (Process p = new Process())
                {
                    // new process to run netstat, get UDP traffic
                    ProcessStartInfo ps = new ProcessStartInfo();
                    ps.Arguments = "-aonp " + prot;
                    ps.FileName = "netstat.exe";
                    ps.UseShellExecute = false;
                    ps.WindowStyle = ProcessWindowStyle.Hidden;
                    ps.RedirectStandardInput = true;
                    ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;

                    p.StartInfo = ps;
                    p.Start();

                    // read output/error text
                    StreamReader stdOutput = p.StandardOutput;
                    StreamReader stdError = p.StandardError;

                    string rawData = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    string exitStatus = p.ExitCode.ToString();

                    // set up objects for output parsing
                    string line;
                    StringReader reader = new StringReader(rawData);
                    while ((line = reader.ReadLine()) != null)
                    {                        
                        if (line.Contains(PID.ToString()))
                        {
                            AddPort(line, prot);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[{0}] {1}", ex.TargetSite, ex.Message));
            }
        }

        private void AddPort(string line, string prot = "UDP")
        {
            string[] netData = Regex.Split(line, "\\s+");
                       
            if (netData.Length > 4 && (netData[1].Equals("UDP") || netData[1].Equals("TCP")))
            {
                Ports.Add(
                    new Game.Port {
                        Number = netData[2].Split(':')[1].Trim(),
                        PID = PID.ToString(),
                        Executable = Executable,
                        Protocol = netData[1]
                    }
                );
            }
        }

        public void InterceptPortPackets(List<LivePacketDevice> devices)
        {
            if (devices.Count == 0)
            {
                Console.WriteLine("No ethernet devices found");
                return;
            }

            try 
            {
                // set filter using ports
                // just use first port found for now
                if (Ports.Count != 0)
                {
                    foreach (LivePacketDevice dev in devices)
                    {
                        using (PacketCommunicator pm = dev.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 500))
                        {
                            string filter = Ports[0].Protocol.ToLower() + " port " + Ports[0].Number;
                            Console.WriteLine(filter);
                            using (BerkeleyPacketFilter f = pm.CreateFilter(filter))
                            {
                                pm.SetFilter(f);
                            }

                            Console.Write("Listening on device " + dev.Description);
                            pm.ReceivePackets(0, PrintPacketInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[{0}] {1}", ex.TargetSite, ex.Message));
            }
        }

        // public helper methods
        public void PrintAppPID()
        {
            if (PID < 0)
            {
                PrintAppPIDInvalid();
                return;
            }
            Console.WriteLine(String.Format("'{0}' [{1}]: {2}", Title, Executable, PID));
        }

        // private helper methods
        private void PrintAppPIDInvalid()
        {
            Console.WriteLine(String.Format("No process found for: '{0}' [{1}]", Title, Executable));
        }

        private void PrintPacketInfo(Packet packet)
        {
            IpV4Datagram ip = packet.Ethernet.IpV4;
            UdpDatagram udp = ip.Udp;

            string info = String.Format(
                "[time:{0}] [length:{1}] [source:{2}] [destination:{3}]", packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff"), packet.Length, ip.Source, ip.Destination);

            Console.WriteLine(info);
        }

        private void FilterPortPackets(PacketCommunicator pm, string prot = "UDP")
        {
            string f = prot.ToLower() + " port " + Ports[0].Number;
            Console.WriteLine(f);
            using (BerkeleyPacketFilter filter = pm.CreateFilter(f))
            {
                pm.SetFilter(filter);
            }
            pm.ReceivePackets(5, PrintPacketInfo);
        }

        // rewrite of the former method, implemented using ReceivePacket()
        // use this method in a timer, filter based on timer (active or not)
        private void FilterPortPacket(PacketCommunicator pm, string prot = "UDP")
        {
            Packet packet;
            PacketCommunicatorReceiveResult result = pm.ReceivePacket(out packet);

            switch (result)
            {
                case PacketCommunicatorReceiveResult.Ok:
                    Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                    break;
                default:
                    throw new InvalidOperationException("The result " + result + " should never be reached here");
            }
        }
    }
}

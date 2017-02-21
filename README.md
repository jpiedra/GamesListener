# GamesWatcher
Ascertains TCP/UDP connection data, for integration with external, web-based service(s)

# Status
Project is currently under development, features on the way:
- Scan each port, for each detected process, every N seconds (configurable)
- Ability to send remote connection data (ex., what multiplayer server you're on) to external services (Twitch, HitBox, etc.)
- Graphic User Interface, systray presence

# Usage
Clone the project to your computer (developed/tested on Windows 8.1, YMMV). 
The following dependencies are required:
- Json.NET Library, to read configuration info (config.json) [http://www.newtonsoft.com/json]
- Pcap.Net Library, to parse TCP/UDP/Etc. packet data [https://github.com/PcapDotNet/Pcap.Net/wiki]

Once cloned, build the project to your desired settings (Any CPU/x86/etc.) and add a file, 'config.json' to the same directory
where the built executable (.exe) can be found. An example configuration to use is included in the Github project root folder,
('example_config.json') and will need to be renamed to 'config.json'.

Finally, run the console program generated from the build (must run as administrator, "Allow" once prompt appears). You'll be 
asked the select which network device to listen on. At this current point, the program will only listen on the FIRST
application you specified that was found running, and connected, prior to running GamesListener.exe.

# Sample Output
![Sample output](http://i.imgur.com/JNo0HFs.png)
Packet activity monitored after obtaining UDP ports for detected process, remote server address highlighted.

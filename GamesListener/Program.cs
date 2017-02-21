using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLOLoader = GamesListener.Util.GLOLoader;

namespace GamesListener
{
    class Program
    {
        static void Main(string[] args)
        {
            GLOLoader Handle = new GLOLoader();

            string gamesConfig = "config.json";
            Handle.ReadConfig(gamesConfig);
            Handle.SurveyEthernetDevices();
            Handle.NetstatListeners();
            Handle.InterceptListeners();
                        
            Console.ReadKey();                
        }
    }
}

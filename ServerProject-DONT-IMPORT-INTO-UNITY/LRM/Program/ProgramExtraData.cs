using System;
using System.Collections.Generic;
using System.Linq;

namespace LightReflectiveMirror
{
    partial class Program
    {
        public List<Room> GetRooms() => _relay.rooms;
        public int GetConnections() => _currentConnections.Count;
        public TimeSpan GetUptime() => DateTime.Now - _startupTime;
        public int GetPublicRoomCount() => _relay.rooms.Where(x => x.isPublic).Count();

        public static void WriteLogMessage(string message, ConsoleColor color = ConsoleColor.White, bool oneLine = false)
        {
            Console.ForegroundColor = color;
            if (oneLine)
                Console.Write(message);
            else
                Console.WriteLine(message);
        }

        private static void GetPublicIP()
        {
            try
            {
                // easier to just ping an outside source to get our public ip
                publicIP = webClient.DownloadString("https://api.ipify.org/").Replace("\\r", "").Replace("\\n", "").Trim();
            }
            catch
            {
                WriteLogMessage("Failed to reach public IP endpoint! Using loopback address.", ConsoleColor.Yellow);
                publicIP = "127.0.0.1";
            }
        }

        void WriteTitle()
        {
            string t = @"  
           ##             
           ###            
          ## ##           
         ## # ##          
         #  #  ##         Echpochmak Games
        ##  #   #         
        #   #   ##        
       #   ##    ##       
      ##    #     #       Relay for Mirror Networking version 67.1.0
      #           ##      
     ##     ##     ##     
     #    #####     #     
    ##   #  ## #    ##    
    #     #   ##     #    
    #      ###  #    ##   
   ##   #        #    #   
   #   #         ###  ##  
   #  #            #   #  
  ## #             ### ## 
  ##                   ## 
  ####################### 
";

            string load = $"Chimp Event Listener Initializing... OK";

            WriteLogMessage(t, ConsoleColor.White);
            WriteLogMessage(load, ConsoleColor.Green);
        }
    }
}

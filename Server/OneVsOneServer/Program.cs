using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MyNet;
using MyNet.ServerFramework;
using OneVsOneServer.GameLogic;

namespace OneVsOneServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MyNetServer.Instance.Start("127.0.0.1", 2500, 5);
            MyNetServer.Instance.onNewClient += Login.Instance.Assign;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorKingServer
{
    public class MainClass
    {
        public static void Main()
        {
            Gateway.Connect("127.0.0.1", 8888);
        }
    }
}

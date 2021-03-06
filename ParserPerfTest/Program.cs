﻿using System;
using System.IO;
using RedChess.ParserFactory;

namespace ParserPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = args[0];
            string inputData = File.ReadAllText(filename);
            Console.WriteLine("Parsing games in {0}", filename);
            ParserFactory.GetParser().Parse(inputData, null, null, null, playGame:true);
            Console.ReadLine();
        }
    }
}

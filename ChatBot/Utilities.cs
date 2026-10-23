using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBot
{
    internal class Utilities
    {
        internal static void SetEnvironmentVariable()
        {
            foreach (var line in File.ReadAllLines(".env"))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0], parts[1]);
                }
            }
        }
    }
}

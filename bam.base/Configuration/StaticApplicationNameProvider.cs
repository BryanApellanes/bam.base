﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Configuration
{
    public class StaticApplicationNameProvider : IApplicationNameProvider
    {
        public const string DefaultApplicationName = "UNKNOWN-APPLICATION";
        public StaticApplicationNameProvider() : this(DefaultApplicationName) { }
        public StaticApplicationNameProvider(string applicationName)
        {
            ApplicationName = applicationName;
        }
        public string ApplicationName { get; set; }
        public string GetApplicationName()
        {
            return ApplicationName;
        }
    }
}

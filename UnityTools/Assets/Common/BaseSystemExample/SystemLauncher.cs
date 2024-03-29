﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common.Example
{
    public class SystemLauncher : Launcher<SystemLauncher.Data, Environment>
    {
        [Serializable]
        public class Data
        {
            public PCConfigure pcConfigure;
        }

        protected override void ConfigureEnvironment()
        {
            base.ConfigureEnvironment();

            this.data.pcConfigure.Init();

            #if !DEBUG
            this.environment.runtime = Environment.Runtime.Production;
            #endif
        }

    }
}

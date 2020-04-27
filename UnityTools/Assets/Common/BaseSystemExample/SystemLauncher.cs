using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common.Example
{
    public class SystemLauncher : Launcher<SystemLauncher.Data>
    {
        [Serializable]
        public class Data
        {
            public PCConfigure pcConfigure;
        }

        protected override void ConfigureEnvironment()
        {
            base.ConfigureEnvironment();

            this.data.pcConfigure.Initialize();

            #if !DEBUG
            this.environment.runtime = Environment.Runtime.Production;
            #endif
        }

    }
}

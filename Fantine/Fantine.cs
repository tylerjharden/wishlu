using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squid.Log;

namespace Fantine
{
    public partial class Fantine : ServiceBase
    {
        public Fantine()
        {
            InitializeComponent();
            Logger.Log("Fantine service starting...");
        }

        private bool stopping = false;

        protected override void OnStart(string[] args)
        {
            //this.EventLog.WriteEntry("Fantine in OnStart.");
            try
            {
                Logger.Log("Fantine service started.");

                ThreadStart ts = new ThreadStart(this.ServiceMain);

                Thread t = new Thread(ts);

                t.Start();
            }
            catch { }

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            //this.EventLog.WriteEntry("Fantine is OnStop");
            //Logger.Log("Fantine service stopping...");

            try
            {
                this.stopping = true;
            }
            catch { }
        }                

        protected void ServiceMain()
        {
            while (!this.stopping)
            {
                try
                {
                    Housekeeping.Run();
                }
                catch { }
            }
        }
    }
}

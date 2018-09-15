using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler.Utils
{
    public class Settings
    {
        public static string JOBID { get { return "JOBID"; } }
        public static string ACTIONID { get { return "ACTIONID"; } }
        public static string JOBACTION { get { return "JOBACTION"; } }
        public static string ACTIONACTION { get { return "ACTIONACTION"; } }

        public static string JOBNAME { get { return "JOBNAME"; } }
        public static string JOBCRON { get { return "JOBCRON"; } }

        public class Actions
        {
            public static int CREATE { get { return 0; } }
            public static int EDIT { get { return 1; } }
        }
    }
}

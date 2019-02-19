using System;
using System.Collections.Generic;
using System.Text;

namespace EgmFramework
{
    /// <summary>
    /// These are some pre-defined port numbers that have been 
    /// used during the development of this framework. They've 
    /// been left in the code to make the rest of the code 
    /// easier to read, but they aren't required for anything.
    /// Feel free to use whatever port numbers tickle your fancy.
    /// </summary>
    public enum DemoEgmPortNumbers
    {
        POS_STREAM_PORT = 6510,
        POS_GUIDE_PORT = 6511,
        PATH_CORR_PORT = 6512,
        EGM_ENDPOINT_PORT = 8080,
        SENSOR_PORT = 8081,
        LINE_SENSOR_PORT = 8082,
        TEST_PORT = 12345
    }
}

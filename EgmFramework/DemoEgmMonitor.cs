using abb.egm;
using lth.egm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EgmFramework
{

    /// <summary>
    /// This is an example implementation of an EgmMonitor. It was used for the demonstration of 
    /// the framework for my thesis project. This EgmMonitor contains the communication logic for 
    /// handeling sensor controlled EGM position guidance. In the example, a helmet with a line 
    /// sensor is attached to a robot arm. A ball is used to represent a moving head in the 
    /// simulation. As the head moves back and forth, the line sensor detects the position of the 
    /// head and sends this information to an EgmUdpThread that is listening on port 
    /// DemoEgmPortNumbers.LINE_SENSOR_PORT. This position information is used to calculate the 
    /// next position the robot will need to move to in order to keep following the head. There is 
    /// a EgmUdpThread communicating with the robot controller via EGM on port 
    /// DemoEgmPortNumbers.POS_GUIDE_PORT. A simple switch case on read and write checks which thread 
    /// is trying to access the monitor and the appropriate Google Protocol Buffer messages are either 
    /// derialized (in read) or de-serialized (in write).
    /// </summary>
    /// <seealso cref="EgmFramework.IEgmUdpThread"/>
    public class DemoEgmMonitor : IEgmMonitor
    {
        private int seqNbr = 0;
        private double offset = 145.0;  //the demo head has a radius of 145mm 
        private double[] feedback = new double[] {0.0 ,0.1, 0.2};
        private double[] sensedPoint = new double[] { 0.0, 0.1, 0.2 };

        /// <summary>
        /// If the EgmUdpThread on port DemoEgmPortNumbers.POS_GUIDE_PORT calls read: 
        /// then build a Google Protocol Buffer message of type EgmSensor and calculate 
        /// the next position required to follow the head.
        /// Default, return null because there is no other EgmUdpThread than needs to send 
        /// data back to its client. 
        /// </summary>
        /// <param name="udpPortNbr"></param>
        /// <returns></returns>
        public byte[] Read(int udpPortNbr)
        {

            byte[] data;

            switch (udpPortNbr)
            {
                case (int)DemoEgmPortNumbers.POS_GUIDE_PORT:
                    // builder for an EgmSensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    // builder for the header
                    EgmHeader.Builder hdr = new EgmHeader.Builder();
                    // data for the header
                    hdr.SetSeqno((uint)seqNbr++)
                        .SetTm((uint)DateTime.Now.Ticks)
                        .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION);
                    // set the data into the header 
                    sensor.SetHeader(hdr);
                    // create some builders for the body of the EgmSensor message
                    EgmPlanned.Builder planned = new EgmPlanned.Builder();
                    EgmPose.Builder pos = new EgmPose.Builder();
                    EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
                    EgmCartesian.Builder pc = new EgmCartesian.Builder();
                    // calculate the next Y position to send to the robot controller
                    // i.e. current position + ((sensed position + offset) - current position)*(some overshot for control)
                    double nextY = feedback[1] + ((sensedPoint[1] + offset) - feedback[1]) * 1.6;
                    // set the data
                    pc.SetX(922.868225097656)
                        .SetY(nextY)
                        .SetZ(1407.03857421875);
                    pq.SetU0(1.0)
                        .SetU1(0.0)
                        .SetU2(0.0)
                        .SetU3(0.0);
                    pos.SetPos(pc)
                        .SetOrient(pq);
                    planned.SetCartesian(pos); 
                    sensor.SetPlanned(planned);
                    EgmSensor sensorMessage = sensor.Build();
                    using(MemoryStream memoryStream = new MemoryStream())
                    {
                        sensorMessage.WriteTo(memoryStream);
                        data = memoryStream.ToArray();
                    }
                    break;

                default:
                    Debug.WriteLine($"No defined Read() case for data going to port {udpPortNbr}.");
                    data = null;
                    break;
            }

            return data;
        }

        /// <summary>
        /// Data from DemoEgmPortNumbers.POS_GUIDE_PORT is a Google Protocol Buffer message of type EgmRobot.
        /// Data from DemoEgmPortNumbers.LINE_SENSOR_PORT is a Google Protocol Buffer message of type LineSensor
        /// Data from any other port is unknown and not handeled by this monitor.
        /// </summary>
        /// <param name="udpPortNbr"></param>
        /// <param name="data"></param>
        public void Write(int udpPortNbr, byte[] data)
        {
            switch (udpPortNbr)
            {
                case (int)DemoEgmPortNumbers.POS_GUIDE_PORT:
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();
                    feedback = new double[] {
                        robot.FeedBack.Cartesian.Pos.X,
                        robot.FeedBack.Cartesian.Pos.Y,
                        robot.FeedBack.Cartesian.Pos.Z
                    };
                    break;

                case (int)DemoEgmPortNumbers.LINE_SENSOR_PORT:
                    LineSensor state = LineSensor.CreateBuilder().MergeFrom(data).Build();
                    if (state.SensorID == 42)
                    {
                        sensedPoint = new double[]
                        {
                            state.SensedPoint.X,
                            state.SensedPoint.Y,
                            state.SensedPoint.Z
                        };
                    }
                    break;

                default:
                    Debug.WriteLine($"No defined Write() case for data coming from port {udpPortNbr}.");
                    break;
            }
        }
    }
}

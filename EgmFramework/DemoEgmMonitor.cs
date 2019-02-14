using abb.egm;
using lth.egm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EgmFramework
{
    public class DemoEgmMonitor : IEgmMonitor
    {
        private int seqNbr = 0;
        private double offset = 145.0;  //the demo head has a radius of 145mm 
        private double[] feedback = new double[] {0.0 ,0.1, 0.2};
        private double[] sensedPoint = new double[] { 0.0, 0.1, 0.2 };


        public byte[] Read(int udpPortNbr)
        {
            

            byte[] data;

            switch (udpPortNbr)
            {
                case (int)EgmPortNumbers.POS_GUIDE_PORT:
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
            //if(data != null)
            //{
            //    Debug.WriteLine($"Sending data to {udpPortNbr}");
            //}

            return data;
        }

        public void Write(int udpPortNbr, byte[] data)
        {
            //Debug.WriteLine($"Received data from {udpPortNbr}");
            switch (udpPortNbr)
            {
                case (int)EgmPortNumbers.POS_GUIDE_PORT:
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();
                    feedback = new double[] {
                        robot.FeedBack.Cartesian.Pos.X,
                        robot.FeedBack.Cartesian.Pos.Y,
                        robot.FeedBack.Cartesian.Pos.Z
                    };
                    break;

                case (int)EgmPortNumbers.SENSOR_PORT:
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

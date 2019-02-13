using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

using ABB.Robotics.Math;
using ABB.Robotics.RobotStudio.Stations;
using lth.egm;

namespace EgmLineSensor
{
    /// <summary>
    /// Code-behind class for the EgmLineSensor Smart Component.
    /// </summary>
    /// <remarks>
    /// The code-behind class should be seen as a service provider used by the 
    /// Smart Component runtime. Only one instance of the code-behind class
    /// is created, regardless of how many instances there are of the associated
    /// Smart Component.
    /// Therefore, the code-behind class should not store any state information.
    /// Instead, use the SmartComponent.StateCache collection.
    /// </remarks>
    public class CodeBehind : SmartComponentCodeBehind
    {
        //GRAPHICS STUFF
        public override void OnInitialize(SmartComponent component)
        {
            RebuildGraphic(component);
        }

        public override void OnSimulationStart(SmartComponent component)
        {
            base.OnSimulationStart(component);
        }

        //GRAPHICS STUFF
        public override void OnLibraryReplaced(SmartComponent component)
        {
            RebuildGraphic(component);
        }


        //START HERE: IO SIGNAL VALUE CHANGED -> UPDATE SENSOR (COMPONENT)
        public override void OnIOSignalValueChanged(SmartComponent component, IOSignal signal)
        {
            if (signal.Name == "Active")
            {
                UpdateSensor(component);
            }
        }

        //ALSO START HERE: SIMULATION STEP -> UPDATE SENSOR (COMPONENT)
        public override void OnSimulationStep(SmartComponent component, double dSimTime, double dPrevSimTime)
        {
            UpdateSensor(component);
        }

        //STEP 2: UPDATE SENSOR (COMPONENT) -> SENSE (COMPONENT)
        protected void UpdateSensor(SmartComponent component)
        {
            int active = (int)component.IOSignals["Active"].Value;
            // WHAT THE HELL DOES THIS DO? I WAS JUST STARTING TO FEEL AT HOME IN C#
            int result = active != 0 ? Sense(component) : 0;
            component.IOSignals["SensorOut"].Value = result;
        }

        //GRAPHICS STUFF: ?something? -> CreateGraphic()
        protected void RebuildGraphic(SmartComponent component)
        {
            GraphicComponent gc = null;
            if (component.GraphicComponents.TryGetGraphicComponent("_sensorPart_", out gc))
            {
                component.GraphicComponents.Remove(gc);
            }

            Part part = new Part(false);
            part.UIVisible = false;
            part.PickingEnabled = false;
            part.Detectable = false;
            part.Name = "_sensorPart_";
            component.GraphicComponents.Add(part);

            MeshPart mp = new MeshPart();
            CreateGraphic(component, mp);
            part.Mesh[DetailLevels.Medium] = mp;
            part.Mesh.Rebuild();
            part.Color = System.Drawing.Color.FromArgb(64, 255, 255, 0);
        }
        //GRAPHICS STUFF if property start, end, radius changes -> CreateGraphic()
        public override void OnPropertyValueChanged(SmartComponent component, DynamicProperty changedProperty, object oldValue)
        {
            if (changedProperty.Name == "Start" ||
                changedProperty.Name == "End" ||
                changedProperty.Name == "Radius")
            {
                RebuildGraphic(component);
            }

            if (changedProperty.Name == "SensedPoint" || changedProperty.Name == "SensedPart")
            {
                sendState(component);
            }
        }
        //GRAPHICS STUFF -> draws the sensor area based on start, end, and radius
        protected void CreateGraphic(SmartComponent component, MeshPart part)
        {
            Vector3 start = (Vector3)component.Properties["Start"].Value;
            Vector3 end = (Vector3)component.Properties["End"].Value;
            double radius = (double)component.Properties["Radius"].Value;

            if (start != end)
            {
                if (radius > 0)
                {
                    int numSides = 12;
                    double height = (end - start).Length();

                    MeshFace bottom = new MeshFace();
                    MeshFace top = new MeshFace();
                    MeshFace sides = new MeshFace();
                    bottom.Vertices.Add(new Vector3(0, 0, 0));
                    bottom.Normals.Add(new Vector3(0, 0, -1));
                    top.Vertices.Add(new Vector3(0, 0, height));
                    top.Normals.Add(new Vector3(0, 0, 1));

                    for (int i = 0; i <= numSides; i++)
                    {
                        double angle = (2 * Math.PI * i) / (double)numSides;
                        Vector3 v = new Vector3(Math.Cos(angle), Math.Sin(angle), 0);
                        Vector3 bv = radius * v;
                        Vector3 tv = new Vector3(bv.x, bv.y, height);

                        bottom.Vertices.Add(bv);
                        bottom.Normals.Add(new Vector3(0, 0, -1));
                        if (i > 0) bottom.TriangleIndices.AddRange(new int[] { 0, i + 1, i });

                        top.Vertices.Add(tv);
                        top.Normals.Add(new Vector3(0, 0, 1));
                        if (i > 0) top.TriangleIndices.AddRange(new int[] { 0, i, i + 1 });

                        sides.Vertices.AddRange(new Vector3[] { bv, tv }); ;
                        sides.Normals.AddRange(new Vector3[] { v, v });
                        if (i < numSides) sides.TriangleIndices.AddRange(new int[] { 2 * i, 2 * i + 3, 2 * i + 1,
                                                                                   2 * i, 2 * i + 2, 2 * i + 3 });
                    }

                    part.Bodies.Add(new MeshBody(new MeshFace[] { bottom, top, sides }));

                    Vector3 z = end - start; z.Normalize();
                    Vector3 x = z.Normal();
                    Vector3 y = z.Cross(x);
                    part.Transform(new Matrix4(x, y, z, start));
                }

                MeshFace line = new MeshFace();
                line.Vertices.AddRange(new Vector3[] { start, end });
                line.WireIndices.AddRange(new int[] { 0, 1 });
                part.Bodies.Add(new MeshBody(new MeshFace[] { line }));
            }
        }

        //STEP 3: SENSE (COMPONENT) -> EXTRACT FROM COMPONENT: 
        // 1) STATION 
        // 2) START_VECTOR
        // 3) END_VECTOR 
        // 4) RADIUS 
        // 5) GLOBAL TRANSFORMATION MATRIX 
        // -> USE MATRIX ON START_VECTOR AND END_VECTOR TO MAKE GLOBAL_START_VECTOR & GLOBAL_END_VECTOR
        // MAKE NEW NULL PART
        // MAKE A POINT OBJECT AS ZERO-LENGTH-VECTOR (WHAT'S A ZERO-LENGTH-VECTOR?)
        // MAKE NEW NULL GRAPHICS COMPONENT -> (ASSUMPTION: TRY TO ALLOCATE A GRAPHICS COMPONENT VIA component.GraphicComponents.TryGetGraphicComponent())
        // CHECK FOR LINE INTERSECTION: 
        //            IF INTERSECTION -> ALLOCATE PART AND POINT TO COMPONENT -> RETURN 1
        //            ELSE -> ALLOCATE NULL AND ZERO_POINT TO COMPONENT       -> RETURN 0
        protected int Sense(SmartComponent component)
        {
            Station stn = component.ContainingProject as Station;
            if (stn == null) return 0; //??

            Vector3 start = (Vector3)component.Properties["Start"].Value;
            Vector3 end = (Vector3)component.Properties["End"].Value;
            double radius = (double)component.Properties["Radius"].Value;

            // Convert to global (i.e. translate start and end point from component frame coorinates to global coordinates)
            Matrix4 mat = component.Transform.GlobalMatrix;
            start = mat.MultiplyPoint(start);
            end = mat.MultiplyPoint(end);

            Part part = null;
            Vector3 point = Vector3.ZeroVector;

            GraphicComponent gc = null;
            component.GraphicComponents.TryGetGraphicComponent("_sensorPart_", out gc);//<- WHAT'S ALL THIS THEN?!

            int result;
            if (CollisionDetector.CheckLineIntersection(stn, start, end, radius, out part, out point))
            {
                component.Properties["SensedPart"].Value = part;
                component.Properties["SensedPoint"].Value = point;
                result = 1;
            }
            else
            {
                component.Properties["SensedPart"].Value = null;
                component.Properties["SensedPoint"].Value = Vector3.ZeroVector;
                result = 0;
            }

            return result;
        }

        protected void sendState(SmartComponent component)
        {
            int PORT = (int)component.Properties["PortNumber"].Value;

            using (var sock = new UdpClient())
            {
                LineSensor.Builder sensorData = LineSensor.CreateBuilder();
                Point.Builder sensedPoint = new Point.Builder();
                Point.Builder start = new Point.Builder();
                Point.Builder end = new Point.Builder();

                UInt32 sensorIDProperty = Convert.ToUInt32(component.Properties["SensorID"].Value);
                Vector3 sensedPointProperty = (Vector3)component.Properties["SensedPoint"].Value;
                String sensedPartProperty = Convert.ToString(component.Properties["SensedPart"].Value);
                Vector3 startProperty = (Vector3)component.Properties["Start"].Value;
                Vector3 endProperty = (Vector3)component.Properties["End"].Value;
                double radiusProperty = (double)component.Properties["Radius"].Value;

                // convert point from m to mm 
                sensedPointProperty = sensedPointProperty.Multiply(1000);

                sensedPoint.SetX(sensedPointProperty.x)
                    .SetY(sensedPointProperty.y)
                    .SetZ(sensedPointProperty.z);

                start.SetX(startProperty.x)
                    .SetY(startProperty.y)
                    .SetZ(startProperty.z);

                end.SetX(endProperty.x)
                    .SetY(endProperty.y)
                    .SetZ(endProperty.z);

                sensorData.SetSensedPoint(sensedPoint)
                    .SetStart(start)
                    .SetEnd(end)
                    .SetRadius(radiusProperty)
                    .SetSensedPart(sensedPartProperty)
                    .SetSensorID(sensorIDProperty);
                //if(sensorData.SensorID == 42)
                //{
                //    Debug.WriteLine($"sensor nbr {sensorData.SensorID} numbers: ({sensorData.SensedPoint.X}, {sensorData.SensedPoint.Y}, {sensorData.SensedPoint.Z})");
                //}
                LineSensor data = sensorData.Build();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    data.WriteTo(memoryStream);
                    var bytesSent = sock.SendAsync(memoryStream.ToArray(), (int)memoryStream.Length, "localhost", PORT);
                    //int bytesSent = sock.Send(memoryStream.ToArray(), (int)memoryStream.Length, "localhost", PORT);
                    //if (bytesSent < 0)
                    //{
                    //    Console.WriteLine("ERROR");
                    //}
                }

                //byte[] bytes = Encoding.UTF8.GetBytes("hello, world!");
                //sock.Send(bytes, bytes.Length, "localhost", PORT);
            }


        }
    }
}

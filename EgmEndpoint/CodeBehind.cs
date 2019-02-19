using System;
using System.Collections.Generic;
using System.Text;

using ABB.Robotics.Math;
using ABB.Robotics.RobotStudio;
using ABB.Robotics.RobotStudio.Stations;
using EgmFramework;

namespace EgmEndpoint
{
    
    public class CodeBehind : SmartComponentCodeBehind
    {

        IEgmMonitor monitor = null;
        IEgmUdpThread egmPositionGuidance = null;
        IEgmUdpThread egmLineSensor = null;

        public override void OnPropertyValueChanged(SmartComponent component, DynamicProperty changedProperty, Object oldValue)
        {
        }
        
        public override void OnIOSignalValueChanged(SmartComponent component, IOSignal changedSignal)
        {
        }
        
        public override void OnSimulationStep(SmartComponent component, double simulationTime, double previousTime)
        {
        }

        public override void OnSimulationStart(SmartComponent component)
        {
            base.OnSimulationStart(component);
            if(monitor != null)
            {
                egmPositionGuidance.Stop();
                egmLineSensor.Stop();
                
                egmPositionGuidance = null;
                egmLineSensor = null;
                monitor = null;
            }
            monitor = new DemoEgmMonitor();
            egmPositionGuidance = new EgmUdpThread((int)DemoEgmPortNumbers.POS_GUIDE_PORT, 4, 50);
            egmLineSensor = new EgmUdpThread((int)DemoEgmPortNumbers.LINE_SENSOR_PORT, 4, 50);
            egmPositionGuidance.StartUdp(monitor);
            egmLineSensor.StartUdp(monitor);
        }

        public override void OnSimulationStop(SmartComponent component)
        {
            base.OnSimulationStop(component);
            egmPositionGuidance.Stop();
            egmLineSensor.Stop();

            egmPositionGuidance = null;
            egmLineSensor = null;
            monitor = null;
        }
    }
}

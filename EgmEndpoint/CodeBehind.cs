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

        IEgmMonitor monitor;
        IEgmUdpThread egmPositionGuidance;
        IEgmUdpThread egmLineSensor;

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
            egmPositionGuidance = new EgmUdpThread((int)EgmPortNumbers.POS_GUIDE_PORT, 4, 50);
            egmLineSensor = new EgmUdpThread((int)EgmPortNumbers.SENSOR_PORT, 4, 50);
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

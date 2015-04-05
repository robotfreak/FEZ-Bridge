/*
Copyright 2010 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;

namespace GHIElectronics.NETMF.FEZ
{
    public static partial class FEZ_Components
    {
        public class ServoMotor : IDisposable
        {
            OutputCompare oc;
            uint[] timings = new uint[5];
            
            public void Dispose()
            {
                oc.Dispose();
            }
            
            public ServoMotor(FEZ_Pin.Digital pin)
            {
                oc = new OutputCompare((Cpu.Pin)pin, true, 5);
            }
            
            public void SetPosition(byte angle_degree)
            {
                uint pos = (uint)(((float)((2500-400) / 180) * (angle_degree)) + 400);
                timings[0] = pos;
                timings[1] = 50000;
                oc.Set(true, timings, 0, 2, true);
            }
        }
    }
}
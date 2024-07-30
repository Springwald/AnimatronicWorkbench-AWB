using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awb.Core.Export.ExporterParts.CustomCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awb.Core.Export.ExporterParts.CustomCode.Tests
{
    [TestClass()]
    public class CustomCodeReaderWriterTests
    {
        [TestMethod()]
        public void ReadRegionsTest()
        {
            var content = """
                                #ifndef _CUSTOM_CODE_H_
                #define _CUSTOM_CODE_H_

                #include <Arduino.h>
                #include <String.h>
                #include <Actuators/NeopixelManager.h>

                /*
                    Enter your custom code in this header file and the corresponding cpp file.
                    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
                */

                class CustomCode
                {
                protected:
                    NeopixelManager *neopixelManager;

                    /* cc-start-protected - insert your protected code here before the end-protected comment: */

                    void setButtonLightByTouch(uint16_t btnIndex, uint8_t touchPin);

                    /* cc-end-protected  */

                public:
                    CustomCode(NeopixelManager *neopixelManager) : neopixelManager(neopixelManager)
                    {
                        /* cc-start-constructor - insert your protected code here before the end-constructor comment: */
                        /* cc-end-constructor  */
                    }

                    ~CustomCode()
                    {
                        /* cc-start-destrucutor - insert your protected code here before the end-destrucutor comment: */
                        /* cc-end-destrucutor  */
                    }

                    void setup();
                    void loop();
                };

                #endif
                """;

            var regionsReader = new CustomCodeReaderWriter();
            var regions = regionsReader.ReadRegions("CustomCode.h", content);

            Assert.IsNotNull(regions);

            Assert.AreEqual(3, regions.Regions.Length);

            Assert.AreEqual("protected", regions.Regions[0].Key);
            Assert.AreEqual("constructor", regions.Regions[1].Key);
            Assert.AreEqual("destrucutor", regions.Regions[2].Key);

            Assert.AreEqual(4, regions.Regions[0].Content.Split(Environment.NewLine).Length);
            Assert.AreEqual(1, regions.Regions[1].Content.Split(Environment.NewLine).Length);
            Assert.AreEqual(1, regions.Regions[2].Content.Split(Environment.NewLine).Length);
        }
    }
}
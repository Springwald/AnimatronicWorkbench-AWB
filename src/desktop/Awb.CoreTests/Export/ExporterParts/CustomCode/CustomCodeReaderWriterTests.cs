// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.CustomCode.Tests
{
    [TestClass()]
    public class CustomCodeReaderWriterTests
    {

        [TestMethod()]
        public void ReadRegionsTestFilledRegions()
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

                        void testMethod1a();
                        void testMethod1b();

                        /* cc-end-constructor  */
                    }

                    ~CustomCode()
                    {
                        /* cc-start-destrucutor - insert your protected code here before the end-destrucutor comment: */

                        void testMethod2();

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

            Assert.AreEqual(1, regions.Regions[0].Content.Split(Environment.NewLine).Length);
            Assert.AreEqual(2, regions.Regions[1].Content.Split(Environment.NewLine).Length);
            Assert.AreEqual(1, regions.Regions[2].Content.Split(Environment.NewLine).Length);
        }

        [TestMethod()]
        public void ReadRegionsTestEmptyRegions()
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

            Assert.AreEqual(1, regions.Regions.Length);

            Assert.AreEqual("protected", regions.Regions[0].Key);

            Assert.AreEqual(1, regions.Regions[0].Content.Split(Environment.NewLine).Length);
        }
    }
}
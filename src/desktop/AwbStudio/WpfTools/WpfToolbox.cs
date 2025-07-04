﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.IO;

namespace AwbStudio.Tools
{
    internal static class WpfToolbox
    {
        public static T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            using StringReader stringReader = new StringReader(savedObject);
            using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;
using System;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    public class DeleteObjectEventArgs(IProjectObjectListable objectToDelete) : EventArgs
    {
        public IProjectObjectListable ObjectToDelete { get; private set; } = objectToDelete;
    }
}

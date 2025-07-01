// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Various;
using System.Collections.Generic;
using System.Linq;

namespace AwbStudio.ProjectConfiguration
{
    internal class IdCreator(ProjectConfigViewModel projectConfigViewModel, AwbProject awbProject)
    {
        private readonly ProjectConfigViewModel _viewModel = projectConfigViewModel;
        private readonly AwbProject _awbProject = awbProject;

        public int GetNewInputId()
        {
            int id = 1;
            while (true)
            {
                if (!_viewModel.Inputs.Select(ic => ic as InputConfig).Any(ic => ic?.Id == id)) return id;
                id++;
            }
        }

        public int GetNewTimelineStateId()
        {
            int id = 1;
            while (true)
            {
                if (!_viewModel.TimelineStates.Select(tst => tst as TimelineState).Any(tst => tst?.Id == id))
                    return id;
                id++;
            }
        }

        public string CreateNewObjectId(string prefix)
        {
            int idCount = 1;
            while (true)
            {
                var id = prefix + "-" + idCount.ToString();
                if (!GetAllIds().Any(x => x == id)) return id;
                idCount++;
            }
        }

        private IEnumerable<string> GetAllIds()
        {
            _viewModel.WriteToProject(_awbProject);
            foreach (var item in _awbProject.Pca9685PwmServos) yield return item.Id;
            foreach (var item in _awbProject.StsServos) yield return item.Id;
            foreach (var item in _awbProject.ScsServos) yield return item.Id;
            foreach (var item in _awbProject.Mp3PlayersYX5300) yield return item.Id;
            foreach (var item in _awbProject.Mp3PlayersDFPlayerMini) yield return item.Id;
        }
    }
}

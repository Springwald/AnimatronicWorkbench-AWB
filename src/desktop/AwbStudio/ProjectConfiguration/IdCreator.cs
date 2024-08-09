// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Various;
using System.Collections.Generic;
using System.Linq;

namespace AwbStudio.ProjectConfiguration
{
    internal class IdCreator
    {
        private readonly ProjectConfigViewModel _viewModel;
        private readonly AwbProject _awbProject;

        public IdCreator(ProjectConfigViewModel projectConfigViewModel, AwbProject awbProject)
        {
            _viewModel = projectConfigViewModel;
            _awbProject = awbProject;
        }
        public int GetNewInputId()
        {
            int id = 1;
            while (true)
            {
                if (!_viewModel.Inputs.Select(x => x as InputConfig).Any(x => x.Id == id)) return id;
                id++;
            }
        }

        public int GetNewTimelineStateId()
        {
            int id = 1;
            while (true)
            {
                if (!_viewModel.TimelineStates.Select(x => x as TimelineState).Any(x => x.Id == id)) return id;
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
        }


    }
}

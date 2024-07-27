//// Animatronic WorkBench
//// https://github.com/Springwald/AnimatronicWorkBench-AWB
////
//// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
//// https://daniel.springwald.de - daniel@springwald.de
//// All rights reserved   -  Licensed under MIT License

//using Awb.Core.Project;
//using Awb.Core.Project.Various;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AwbStudio.ProjectConfiguration
//{
//    internal class IdCreator
//    {
//        private readonly AwbProject _awbProject;

//        public IdCreator(ProjectConfigViewModel projectConfig)
//        {
//            _projectConfig = projectConfig;
//        }
//        public int GetNewInputId()
//        {
//            int id = 1;
//            while (true)
//            {
//                if (!_awbProject.Inputs.Any(x => x.Id == id)) return id;
//                id++;
//            }
//        }

//        public int GetNewTimelineStateId()
//        {
//            int id = 1;
//            while (true)
//            {
//                if (!_awbProject.TimelinesStates.Any(x => x.Id == id)) return id;
//                id++;
//            }
//        }

//        public string CreateNewObjectId(string prefix)
//        {
//            int idCount = 1;
//            while (true)
//            {
//                var id = prefix + "-" + idCount.ToString();
//                if (!_awbProject.GetAllIds().Any(x => x == id)) return id;
//                idCount++;
//            }
//        }
//    }
//}

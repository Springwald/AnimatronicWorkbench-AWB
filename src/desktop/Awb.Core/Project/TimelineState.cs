// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class TimelineState: IProjectObjectListable
    {
        public int Id { get; internal set; }
        public string Title { get; internal set; }

        /// <summary>
        /// Export timelines with this state to the client project souurce code
        /// </summary>
        public bool Export { get; internal set; } = true;

        /// <summary>
        /// Play the timeline automatically when the state is active.
        /// If false: The timeline will be played when the user activates it manually by remote control
        /// </summary>
        public bool AutoPlay { get; internal set; } = true;

        /// <summary>
        /// The state is only available when one of this inputs are on
        /// </summary>
        public int[] PositiveInputs { get; internal set; }

        /// <summary>
        /// The state not available when one of this inputs are on
        /// </summary>
        public int[] NegativeInputs { get; internal set; }

        public string TitleShort =>  Title ?? $"TimelineState has no title set '{Id}'";

        public string TitleDetailled => $"TimelineState {TitleShort}";

        public TimelineState(int id, string title, bool export, bool autoPlay, int[]? positiveInputs = null, int[]? negativeInputs = null)
        {
            Id = id;
            Title = title;
            PositiveInputs = positiveInputs ?? Array.Empty<int>();
            NegativeInputs = negativeInputs ?? Array.Empty<int>();
            Export = export;
            AutoPlay = autoPlay;
        }

        
        //todo!
        public override string ToString() => $"[{Id}] {Title}"; 
    }
}
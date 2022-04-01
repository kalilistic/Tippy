using System.Collections.Generic;

#pragma warning disable CS1591
namespace Tippy
{
    public class TippyAnimData {
        public TippyAnimData(int start, int stop) {
            Start = start;
            Stop = stop;
        }

        public int Start { get; set; }
        public int Stop { get; set; }

        public static readonly Dictionary<TippyAnimState, TippyAnimData> tippyAnimDatas =
            new()
            {
                {TippyAnimState.GetAttention, new TippyAnimData(199, 223)},
                {TippyAnimState.Idle, new TippyAnimData(233, 267)},
                {TippyAnimState.Banger, new TippyAnimData(343, 360)},
                {TippyAnimState.Sending, new TippyAnimData(361, 412)},
                {TippyAnimState.Reading, new TippyAnimData(443, 493)},
                {TippyAnimState.GetAttention2, new TippyAnimData(522, 535)},
                {TippyAnimState.PointLeft, new TippyAnimData(545, 554)},
                {TippyAnimState.WritingDown, new TippyAnimData(555, 624)},
                {TippyAnimState.CheckingYouOut, new TippyAnimData(718, 791)},
            };
    }
}

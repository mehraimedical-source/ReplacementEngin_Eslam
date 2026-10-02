using System.Collections.Generic;

namespace MEHR_PACSViewer
{
    /// <summary>
    /// Test stub. Replace the bodies with the real PACS/database implementation.
    /// Signatures are kept for uc_VoiceRecorder compatibility.
    /// </summary>
    internal static class VoiceCommand
    {
        internal static List<VoiceResult.VoiceInfo> Count(ulong pid)
        {
            // TODO: load voice records/count from database.
            return new List<VoiceResult.VoiceInfo>();
        }

        internal static VoiceResult.VoiceInfo Add(VoiceResult.VoiceInfo voiceInfo, int sourceIndex)
        {
            // TODO: insert voiceInfo in database and set the generated voiceID.
            return voiceInfo;
        }
    }
}

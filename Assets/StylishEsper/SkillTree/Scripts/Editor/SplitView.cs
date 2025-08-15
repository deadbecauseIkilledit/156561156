//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class SplitView : TwoPaneSplitView
    {
#endif
#if UNITY_2022
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
#endif
    }
}
#endif
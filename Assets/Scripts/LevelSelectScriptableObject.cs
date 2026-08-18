using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for the level select screen.
///
/// Held the owl's "already disappeared" flag until Phase 5. That was runtime state written onto a
/// project asset, so it survived in the editor, reset itself in a build, and was never really
/// per-player. It lives in the save profile now. Phase 6 gives this asset the authored level list.
/// </summary>
[CreateAssetMenu(menuName = "LevelSelectScreen")]
public class LevelSelectScriptableObject : ScriptableObject
{
}

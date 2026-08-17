# Audit notes

Working notes for the revival. Started as a Phase 1 code audit and grew to cover the
cleanup and the URP move as those landed.

The first pass of this audit was done against `Version1.0.3`, which turned out to be
content-stripped and about 35% smaller than the real base. Everything here was re-derived
against `release/launch`. Line numbers are valid as of the start of Phase 1a on `develop`.

## 1. Where the plan was wrong

Four assumptions in the approved plan did not hold on this base.

| Plan said | Reality | What changed |
|---|---|---|
| `com.unity.cinemachine` has zero scene references, remove it | Used in all 6 gameplay scenes (Brain, VirtualCamera, FramingTransposer) | Keep it. It drives the gameplay camera |
| Standardize on DOTween, migrate 3 LeanTween sites | DOTween's only call site is `DotWeenPath.cs`, which is dead. LeanTween is the only live tweener | Decision re-confirmed below |
| `main` is v1.0.3 / build 13 / May 2023 | `origin/main` is v1.1.2 with Firebase, forked before release/launch's 158 commits | Resetting main is a force-push, so it is deferred to Phase 10 |
| Phase 7: fix `CameraFollow`'s frame-rate-dependent Lerp | `CameraFollow.cs` is dead, superseded by Cinemachine | Delete it rather than fix it |

### Branch state

`origin/main` is deliberately left at the v1.1.2 Firebase tip. All revival work happens on
`develop`. `main` is reset to the release commit only at Phase 10, when v1.2.0 is ready to
merge and tag. Until then main is stale, and that is accepted rather than an oversight.

Local `main` sits at the revival base and so reports "ahead 158, behind 3" against
`origin/main`. Leave it, do not reconcile it mid-project.

`origin/Version1.0.3` still exists remotely. It is fully preserved by the
`archive/version-1.0.3` tag and can be deleted whenever.

Tags on origin: `archive/version-1.0.3`, `archive/release-launch`,
`archive/feature-post-processing`, `v1.0.3-main`, `v1.1.1-playstore`.

### Do not delete the GabrielAguiar pack wholesale

`SpecialMonsters.cs`, the Level 6 boss, subscribes to
`ProjectileMoveScript.DeactivateAllActiveBullets`, and `ProjectileMoveScript` lives inside
`Assets/GabrielAguiarProductions/Unique_Projectiles_Volume_1/`. A GUID scan shows most of
that pack's assets unreferenced, so an aggressive cleanup pass would happily delete it and
break the boss fight at compile time. Keep `Scripts/UniqueProjectiles/` and prune only
verified-unreferenced art and prefabs inside the pack.

## 2. Dead code

Verified two independent ways: a GUID index across every `.unity`, `.prefab`, `.asset`,
`.controller` and `.mat`, plus a symbol grep across every `.cs`.

| File | Lines | Note |
|---|---|---|
| `Scripts/CameraFollow.cs` | 23 | Superseded by Cinemachine |
| `Scripts/DotWeenPath.cs` | 26 | The only DOTween usage in the project |
| `Scripts/Projectile.cs` | 18 | Previously unaudited |
| `Scripts/StickyPlatform.cs` | 24 | Previously unaudited, so the plan's moving-platform concern is moot |
| `Scripts/WayPointFolloweActivator.cs` | 15 | Previously unaudited |

Four of the six scripts the plan listed as never reviewed turned out to be dead, so they
are deleted rather than audited or refactored.

This proves no static reference only. A script added via `AddComponent` from a string, or
living in a Resources prefab, would not show up. None of these appear in any `AddComponent`
call and the project has no `Resources.Load` of them.

## 3. Save and persistence, inputs for Phase 5

9 distinct keys, 52 call sites, and zero `PlayerPrefs.Save()` calls anywhere in the project.

| Key | Type | Sites | Note |
|---|---|---|---|
| `CoinsCollected` | string | 12 | `"CollectedAll"` / `"Collected Half"` / `"Collected Quater"` (sic) |
| `LevelClearedCount` | int | 9 | |
| `IsLastSceneMainMenu` | int | 7 | used as a bool |
| `PreviousBall` | int | 5 | |
| `Current Level` | int | 5 | contains a space, quote it exactly |
| `CoinsCollectedQuantity` | int | 5 | |
| `LevelCleared` | int | 4 | stores a build index |
| `IsMainMenuChnagedAtLeastOnce` | int | 3 | misspelled, baked into shipped saves, read it verbatim |
| `GameOverLevel` | int | 2 | sentinel for "final level" |

`SaveManager.cs` (139 lines) on this base:

- `SaveJson` and `OverrideJson` are byte-identical and both `File.AppendAllText`, so the
  file grows unboundedly and keys repeat.
- `int.Parse(parts[0])` with no `TryParse` and no length guard, so one malformed line is an
  unhandled exception on load.
- `LoadJson` returns null on first run and not all callers null-check.
- Writes tab-delimited text despite the `Json` naming.

## 4. Tweening

The complete live tween surface:

| Site | Call |
|---|---|
| `MainMenu.cs:98` | `LeanTween.alpha(...)` fade in |
| `MainMenu.cs:104` | `LeanTween.alpha(...)` fade out plus `setOnComplete` |
| `SawRotate.cs:9` | `LeanTween.rotateAroundLocal(...).setRepeat(-1)`, infinite and never cancelled |

Two UI alpha fades and one constant spin. DOTween is currently unused, so the approved
decision was made believing DOTween had a live call site and LeanTween had three.

Decision stands anyway: keep DOTween, port the 3 LeanTween sites, then delete
`Assets/LeanTween/` (6.8 MB, 34 scripts). Keep PathCreator, since `Follower.cs` samples
`path.GetPointAtDistance` and `DOPath` cannot replace that.

The port is a fix as well as a swap. Every migrated tween gets `DOTween.Kill` in `OnDisable`,
`.SetLink(gameObject)` as a second net, and `.SetUpdate(true)` on anything drawn while
`Time.timeScale` is 0. `SawRotate.cs:9` is the one that is broken today.

## 5. CameraManager.cs (71 lines)

Previously unaudited.

- `Update()` line 34 compares a float for exact equality:
  `_level6Camera.transform.position.x == _maxXdistance` (116.1f). That is essentially never
  true for an animated transform, so the level 6 intro camera may never hand back to the
  main camera and the UI may never reappear.
- `async void DeactivateCameraDelay()` at line 59 uses `Task.Delay(500)`. It is not tied to
  the Unity lifecycle, so if the scene unloads mid-delay the continuation resumes against
  destroyed objects. It also runs on wall-clock time and ignores `Time.timeScale`. Should be
  a coroutine.
- `Start()` line 18 unconditionally forces the level-6 camera on and hides the UI. That is
  correct only if `CameraManager` exists solely in Level 6, which needs confirming before
  anyone changes it.
- `_maxXdistance` is a hardcoded non-serialized magic number.

## 6. SpecialMonsters.cs (221 lines), the boss fight

Previously unaudited. This file has the highest defect density in the project.

- `RetriveBullets()` returns null at line 201 when all 5 pooled bullets are active or the
  alien is dead, and `FireBullets()` line 176 dereferences it immediately.
- `Debug.Log("Hit" + _enemyHealth[_hit].name)` at line 86 runs before any bounds or tag
  check, so once `_hit` reaches 3 with 3 entries the next collision throws.
- `async void SpawnBulletsOnAInterval()` at line 106 loops on `Task.Delay` with nothing
  cancelling it on disable, destroy or scene change. It outlives the GameObject destroyed at
  line 168, keeps calling `ShootBullet()` against destroyed objects, and dereferences
  `GameManager.instance` after a scene swap.
- Pooled bullets leak. Five `Instantiate(_bullets)` at line 61 with no parent, and `Dead()`
  destroys the alien but never the bullets.
- `Debug.Log` in `Update()` at line 71 is a string concat and box every frame, per instance,
  in a shipping build. Also logs at 86, 110 and 182.
- `Animator.SetBool` is written every frame rather than on change, lines 75 and 79.
- Public mutable statics `_isAlienDead`, `EndBlockPosition` and `_startPos` survive scene
  loads and are reset from `OnDisable`, so ordering decides correctness. `CameraManager`
  reads `_isAlienDead`, so the two files' bugs interact.
- `_bulletInitalVelocity` at line 57 reads velocity off an inactive template and is always
  `Vector3.zero`, making line 180 a no-op reset. Harmless, but the name lies.
- Naming is inconsistent throughout: `firedBullets` is private without the underscore,
  `_bulletsList` is public, and the public `Animator` field shadows the type. Delays are
  float milliseconds cast to int.

## 7. Legacy API migration

The plan's count of 9 `.velocity` sites was low. Actual, split by ownership:

First-party, must migrate (10 occurrences):
`PlayerController.cs` lines 74, 126, 136, 138, 139 (twice), 209, 211, and
`SpecialMonsters.cs` lines 57, 180.

Third-party in demo folders that Phase 2 deletes (5 occurrences):
`ExplodingProjectileExplosion1.cs` 54, 89 and `ECExplodingProjectile.cs` 72, 100, 128.

False positives, do not touch: `ParticleSystem.velocityOverLifetime` in
`ParticleSystemController.cs` at 83, 444 and 568 is not deprecated.

Order matters. `Rigidbody.linearVelocity` does not exist before Unity 6, so this has to land
after the editor upgrade or the project will not compile.

## 8. Carried forward from the first audit, re-confirmed here

- `PlayerController.cs:139` assigns a `Vector2` to a `Vector3` property, silently zeroing Z.
- `GameManager.cs:130,134` use `Mathf.Ceil(_coinCount / 2)` and `/ 4`, where the integer
  division happens before `Ceil` and makes it a no-op. Phase 6's absolute `starThresholds`
  removes the division entirely.
- `GameManager.cs:75` has an empty `if (PlayerPrefs.HasKey("LevelCleared")) { }` body.
- `GameManager.cs:126-137` has a non-exhaustive star chain, so `CoinsCollected` keeps the
  previous level's value when a player collects less than a quarter.
- No `Screen.safeArea` handling anywhere, while the manifest advertises
  `android.notch_support`.
- Every singleton lacks `else { Destroy(gameObject); return; }` and an `OnDestroy` null-out.
  `SaveManager.Awake` assigns `Instance` but never destroys a duplicate.

## 9. Packages after the Unity 6 upgrade

Removed, each with zero hits across scenes, prefabs, `.asset` and C#, and zero dependents in
`packages-lock.json`: `2d.sprite`, `2d.tilemap`, `ai.navigation`, `ide.vscode`,
`multiplayer.center`, `visualscripting`, `modules.unityanalytics`.

Kept, where that differs from the plan:

| Package | Version | Reason |
|---|---|---|
| `cinemachine` | 2.10.5 | The plan scheduled removal on the claim of zero scene references. Wrong: Brain, VirtualCamera and FramingTransposer appear in all 6 gameplay scenes. The upgrader stayed on 2.x, and a 3.x bump must not be accepted since it renames `CinemachineVirtualCamera` and breaks every scene at compile time. |
| `recorder` | 5.1.4 | Unreferenced, kept deliberately for capturing portfolio and store-listing footage. |
| `timeline` | 1.8.10 | Unused directly, no `.playable` assets and no `PlayableDirector`. Retained only because `recorder` depends on it. If Recorder goes, this goes too. |

URP 17.3.0 is installed through the package manager rather than by hand-editing
`manifest.json`, so the version resolves against the editor instead of being pinned by guess.

## 10. Ball selection is stored as the active flag on prefab assets

Found when Unity 6 re-serialized `FootBall.prefab` and flipped its root `m_IsActive` 0 to 1.

`ChooseBall.cs:9` exposes `public GameObject[] BallPool` on a ScriptableObject holding the
six ball prefab assets, and selection is written onto those assets:

- `BallManager.cs:36,42` calls `_ballPool.BallPool[i].SetActive(true/false)`
- `BallManager.cs:27` calls `_ballPool.PreviousBall.SetActive(true)`
- `GameManager.cs:230-234` loops the pool and instantiates every ball whose `activeSelf` is true

Three consequences:

1. It mutates project assets from the editor. Unity's in-memory copy of a prefab can diverge
   from disk, and a later save writes the runtime flag back into the `.prefab` file and into
   version control. This happened twice during Phase 2. Corrections have to go through the
   AssetDatabase (`SetActive`, `SetDirty`, `SaveAssets`), not a text edit, or Unity simply
   overwrites them.
2. The selection is not really a selection. `GameManager` instantiates every active ball, so
   any state with two active prefabs spawns two stacked balls on the player.
3. It behaves differently in a build, where prefab assets are read-only. The flag lives only
   in memory and resets each launch, so editor and device disagree.

Committed baseline: all six ball roots are `m_IsActive: 0`. No ball is active on disk, and
`BallManager.Start` activates one at runtime from `PlayerPrefs["PreviousBall"]`. Any diff
showing a ball root becoming 1 is editor state leaking in, not a design change.

Phase 5 moves this to `SaveData.selectedBallId`, after which `GameManager` instantiates
exactly one ball looked up by id.

Until then, guard with `tools/ballcheck.py`, or otherwise match each prefab's root GameObject
by name before reading `m_IsActive`. Do not use
`grep -m1 m_IsActive Assets/Prefab/Balls/*.prefab`: these prefabs serialize child GameObjects
ahead of the root, so `-m1` reports a child's flag and gives the wrong answer silently. That
mistake produced an earlier and incorrect claim that `poke bola` was committed active.

## 11. GetIsOwlTriggeredOnce throws on MainMenu startup

Found by the Phase 1 play-mode gate. Entering play mode on MainMenu throws immediately:

```
FormatException: String was not recognized as a valid Boolean.
  SaveManager.GetIsOwlTriggeredOnce ()  at SaveManager.cs:131
  AllSceneManager.Start ()              at AllSceneManager.cs:34
```

```csharp
// SaveManager.cs:121-134
public void IsOwlTriggeredSO(bool isTriggered) {
    string readFromFilePath = Application.persistentDataPath + "/isOwlTriggered.txt";
    File.AppendAllText(readFromFilePath, isTriggered.ToString());   // APPEND
}
public bool GetIsOwlTriggeredOnce() {
    string readFromFilePath = Application.persistentDataPath + "/isOwlTriggered.txt";
    bool val = bool.Parse(File.ReadAllText(readFromFilePath));      // no guards
    ...
}
```

`bool.Parse` runs on raw file text with no `File.Exists` check, no `TryParse` and no
try/catch, and the writer appends. So the read succeeds in exactly one case:

| File state | How it arises | Result |
|---|---|---|
| missing | fresh install, owl never triggered | `FileNotFoundException` |
| empty | the state on this machine, 0 bytes, dated 2023 | `FormatException`, observed |
| `True` | `IsOwlTriggeredSO` called exactly once | works |
| `TrueTrue` | called twice, and `LevelSelect.cs:308` can fire repeatedly | `FormatException` |

It therefore throws for effectively every player. Because it throws from `Start()`, the rest
of `AllSceneManager.Start()` never runs and `_owlSO.IsOwlDisappereadOnce` is never restored,
so the owl reappears every launch for anyone past the trigger.

This is a second instance of the append-instead-of-overwrite defect the plan flagged on
`SaveManager.OverrideJson`, and unlike that one it is already crashing. Phase 5 subsumes it:
the flag becomes `SaveData.isOwlDisappearedOnce`, written through the atomic `File.Replace`
path, and `Load()` never throws. Until then, expect this exception on every MainMenu entry.
It is pre-existing, not an upgrade regression.

Two smaller things found in the same pass:

`Alien_Tall` animator: `AnyState -> MonsterArmature_Bite_Front` has no Exit Time and no
condition, so Unity ignores the transition and the Level 6 boss's bite animation never plays.
Pre-existing content bug, relevant to Phase 8's Arena 2 verification.

`AllSceneManager.cs:23-26`: the duplicate-singleton branch calls `Destroy(this)`, which
destroys only the component and leaves the GameObject, and `DontDestroyOnLoad(this)` then
runs unconditionally on that same just-destroyed duplicate.

## 12. The EffectCore ShaderGraph subtree could not import

Installing URP surfaced a hard error:

```
Asset import failed, "Assets/EffectCore/packs/StylizedExplosionPack1/shader/
alphaBlend_glow_shadergraph.ShaderGraph" > InvalidOperationException:
Failed to add object of type `UniversalMetadata`
```

The graph predates Shader Graph 17 and cannot be upgraded. Seven materials referenced it
(`empty_glow`, `decal_explode_glow`, `fire_glow`, `flarespark_glow`, `MeltingFire_glow`,
`circle_glow`, `cartoonSmoke_glow`), and a GUID scan of the 28 assets under
`prefabs/ShaderGraphVersion/` against all 11 scenes returned zero hits, with none of the 7
materials referenced outside `EffectCore/` itself.

Deleted rather than repaired: 66 files, done before the Phase 3 material conversion so the
converter never had to touch an asset that cannot import.

## 13. Cleanup outcome and what Phase 3 inherited

Cleanup removed roughly 1,000 files. All 11 scenes open with zero errors and zero warnings
apart from the `Alien_Tall` animator transition above.

Kept deliberately, against a first reading of "unreferenced":

| Kept | Why |
|---|---|
| `Platformer Game Kit/**/*.blend` (65 MB) | The editable sources for the game's entire art identity. Crab, Bee, Bouncer, Cannon, Hazard_Saw, SpikeTrap, Tower, platforms, rocks, trees and Coin are all live FBX from this kit, so deleting the sources means any future model tweak starts from scratch. |
| `Free Game Menu Music Pack` (294 MB, 21 WAVs) | Licensed tracks. Only one ships today, but new arenas need music and re-sourcing licensed audio is worse than carrying it. |
| `CustomBall/organic-ball/orgballex.fbx` | Unreachable only because its prefab is unwired, see below. |
| GabrielAguiar textures and prefabs, JMO Cartoon FX, EffectCore mobile and legacy prefabs | Effect libraries for future bosses and hazards. |

Two finished ball skins are not wired up. `BallPool.asset` is live and feeds 4 skins, but
`Assets/Prefab/Balls/orgballex.prefab` and `WaterMelon.prefab` are not in the pool, which is
why their meshes scan as unreachable. Adding them is a minutes-long job and gets two extra
skins free. Worth doing alongside Phase 5, when ball selection moves to
`SaveData.selectedBallId`.

`*.unitypackage` is gitignored (`.gitignore:61`). The three GabrielAguiar installers (112 MB)
are local-only and were never in the repo, so deleting them would be irreversible for zero
repo benefit and they were left alone.

Deleted vector and archive sources, about 254 MB: 50 `.eps` and `.ai` files Unity cannot
read, and duplicate `.zip` and `.obj` copies of models that already exist as FBX. Deleting
the `.eps` and `.ai` files stranded 28 folder `.meta` files, which made Unity recreate all 28
empty folders on the next refresh. When deleting a folder's entire contents outside Unity,
remove the folder's `.meta` too.

## 14. URP made 10 shaders render invisible rather than magenta

The expected failure mode of a pipeline swap is magenta. This project's was the opposite and
much easier to miss: materials that silently disappeared.

URP draws a pass only if its `LightMode` tag is `UniversalForward`, `SRPDefaultUnlit` or
`UniversalForwardOnly`, or if it has no `LightMode` tag at all. A pass tagged
`"LightMode"="ForwardBase"`, the Built-in RP name, matches nothing in URP's forward loop, so
URP renders nothing for it. No error, no warning, no magenta, and the shader compiles clean.

Diagnosis used `Shader.FindPassTagValue` over every pass of every shader reachable from the
build scenes. Neither the console nor the material inspector reports this, so there is no
easier way to see it.

Two causes, two different fixes.

**Eight shaders carried a vestigial ForwardBase tag.** Six GAP projectile shaders plus
`EffectCore/alphaBlend_glow` and `alphaBlend_depthBlend_glow`. Each is a single-SubShader,
single-Pass, pure unlit vert/frag shader with zero Built-in lighting macros
(`LIGHTING_COORDS`, `UNITY_LIGHTING`, `SHADOW_COORDS`, `LIGHT_ATTENUATION`, `_LightColor0`,
`AutoLight`), so they never consumed the tag and it was inherited boilerplate. Deleting the
one line changes nothing and leaves them valid under Built-in too.

**Two were surface shaders, which URP cannot run at all.**
`GAP/ParticlesAdditiveMobile_Scroll` and `GAP/ParticlesABMobile_Scroll`. Here the
`FORWARDBASE` tag is generated by the surface compiler and absent from the source, so there
was nothing to edit out. The `#pragma surface` path has no URP equivalent.

That one was gameplay-breaking. `ParticlesAdditiveMobile_Scroll` backs
`Projectile_Fireball_AddScroll_Mobile` and `Trail03_AddScroll_Mobile`, the two materials on
`Assets/Prefab/Vfx/vfx_Projectile_Fireball01Blue_Mobile.prefab`, which is the Arena 2 boss
projectile referenced by `Scenes/Arena2/Level2-1.unity` and nothing else. The boss was firing
completely invisible projectiles. Relevant to Phase 8's Arena 2 verification.

Both were ported by hand to explicit vertex/fragment passes, and the port preserves behaviour
rather than reinterpreting it. Both declared a custom lighting model returning
`half4(0,0,0,s.Alpha)`, contributing no light, and emitted all visible colour through
`o.Emission`. They were unlit shaders wearing a surface-shader costume. The fragment maths is
the original `surf()` body verbatim, and render states are spelled out to match what the
surface compiler had been generating (`alpha:fade` becomes `Blend SrcAlpha OneMinusSrcAlpha`
plus `ZWrite Off`). Property names and defaults are unchanged, so no material, prefab or
scene edits were needed.

Two details worth carrying forward:

- Vector uniforms are declared `float4` and swizzled, not `float2`. Unity's Vector properties
  are `float4` on the C# side, and a bare `float2` inside `CBUFFER_START(UnityPerMaterial)`
  silently shifts every member after it and breaks SRP Batcher compatibility.
- The ASE node graph and `CustomEditor "ASEMaterialInspector"` were dropped, since Amplify
  Shader Editor is not installed here and the custom editor was already a dead reference. The
  original graph is recoverable from git history.

`UniqueProjectilesVol1_2020.3_URP_v1.7.unitypackage` was kept on disk during Phase 2 on the
assumption it would supply URP versions of these shaders. It was not used, and importing it
would have been the worse option: it also carries `Scripts/UniqueProjectiles/`, which
`SpecialMonsters` depends on, so the import risked overwriting live gameplay code to fix two
materials. The hand port touched two files. Keep the package as a fallback, but it is off the
critical path.

Verification: a full sweep of all 11 enabled build scenes found 105 material references
across 12 distinct shaders, with zero that URP will not draw.

Two earlier claims from this document were wrong and are corrected here. `whYKnot/glass` was
never at risk, since it has no `LightMode` tag and URP draws it normally. And the roughly 116
legacy particle materials (`Legacy Shaders/Particles/*`, `Mobile/Particles/*`) already render
correctly for the same reason, so converting them to `Particles/Unlit` is optional
modernization rather than a fix, and is not blocking anything.

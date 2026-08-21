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

## 15. The assembly split exposed a dependency cycle

Phase 4 put the game code in `Boltz.Runtime`. Three things about that were not in the plan.

**`Assets/Joystick Pack/Scripts/Editor/` needed its own assembly.** An assembly definition
overrides special folder behaviour, so once the pack has an asmdef the four `[CustomEditor]`
scripts under `Editor/` stop being treated as editor-only and land in the runtime assembly,
where `using UnityEditor` does not compile. Any third-party folder that gets an asmdef and
has an `Editor` subfolder needs a second, editor-only asmdef alongside it.

**DOTween's modules are loose `.cs` files, not part of `DOTween.dll`.** They live in
`Assets/Plugins/Demigiant/DOTween/Modules/` and so compile into `Assembly-CSharp-firstpass`,
which nothing with an asmdef can reference. That matters because `Image.DOFade` is defined
there rather than in the DLL, and `MainMenu.cs` calls it.

Do not hand-write an asmdef into that folder. `ASMDEFManager` inside `DOTweenEditor.dll`
owns it and deletes any file it finds there while `createASMDEF` is `0` in
`Assets/Resources/DOTweenSettings.asset`. Setting that flag is the fix, and DOTween then
generates and maintains the file itself, which is why it looks nothing like the hand-written
ones. A file written by hand disappears on the next domain reload with no error.

**`SpecialMonsters` and `ProjectileMoveScript` reference each other.**

| Direction | Sites |
|---|---|
| `SpecialMonsters.cs` 50 and 217 | `ProjectileMoveScript.DeactivateAllActiveBullets` |
| `ProjectileMoveScript.cs` 60 | `SpecialMonsters._startPos` |
| `ProjectileMoveScript.cs` 71 | `GameManager.instance.GameOver()` |

Two assemblies cannot reference each other, so giving the projectile pack its own asmdef is
not possible while those upward references exist. The pack's
`Scripts/UniqueProjectiles/` folder is folded into `Boltz.Runtime` by an `.asmref` instead.
That changes no code and moves no files, and the cycle simply never forms.

The sibling `Scripts/ParticleSystemController/` folder was checked in both directions, shares
nothing with `UniqueProjectiles/`, and stays in `Assembly-CSharp`.

The real fix is to delete the two upward references so the pack knows nothing about game
code, which means changing how the Level 6 boss reports a game over and where the arena's
start position is read from. That is a gameplay change to the file with the highest defect
density in the project, so it was left out of a phase whose whole point was that the compiler
answers whether anything broke.

`Boltz.Editor` and `Boltz.Tests.PlayMode` still hold no scripts and Unity warns about each on
every compile. Expected, and it stops once either gets its first file.

## 16. What the save rewrite covers, and what it does not

The 29 EditMode tests pass. They cover the corruption path against real malformed JSON, both
the fall back to `.bak` and the fall back to defaults, that repeated saves do not grow the
file, that a weaker replay does not overwrite a better result, and that a repeated ordinal in
the legacy star file takes the best value rather than the last.

Two gaps worth knowing about.

`Boltz.Tests.PlayMode` is empty, so nothing exercises `SaveFlushBehaviour.OnApplicationPause`.
Android kills backgrounded apps without firing `OnApplicationQuit`, so that hook is the one
that saves most players' progress and it is the only part of the save system with no
automated coverage.

Migration is tested against synthetic input, not against real data. The nine shipped
`PlayerPrefs` keys and an actual `levelAndStar.txt` from a store install have not been run
through `LegacySaveMigrator`. Only a device upgrading from the old build proves that, and it
is one shot, because the migrator deletes the legacy keys once it has read them.

## 17. The customize screen is built for exactly four skins

`ChooseBall.BallPool` now holds six. `orgballex` and `WaterMelon` were appended rather than
inserted, so the four existing indices keep their meaning and no migrated `SelectedBallId`
moves. Neither is reachable yet.

Selection lives in `Assets/Scenes/Customize.unity`, and four is baked into it four separate
ways.

- `Customize.SpotLights` has four entries. `Customize.cs:31` runs
  `SpotLights[i].SetActive(i == ballId)`, so an index the list does not contain matches
  nothing and switches every spotlight off. A button reaching skin 4 or 5 before this list
  grows would blank the selection screen rather than fail loudly.
- Four `Image` cells under `HorizontalGrid`, with four `onClick` entries into
  `ActivateParticularBall`. Nothing can send 4 or 5 today.
- `Assets/Animation/BallChoose.anim` binds its curves by child name, `Image (2)` and
  `Image (3)` among them. New cells inherit no curves and sit still while the others move.
- The cells pair with `SpotLight1` through `SpotLight4` in `Assets/Prefab/SpotLight/`.

One trap. `ChooseBall.SpotLight`, the array directly below `BallPool` in the same asset, is
dead. Nothing reads it. The live list is `Customize.SpotLights` in the scene, and extending
the asset's array looks like the fix while doing nothing at all.

## 18. Four level select tiles opened the wrong scenes

Found while mapping the level select screen for Phase 6. The screen lays out ten tiles across
two arenas, but only six levels exist. The four spare tiles in arena 2 were not empty
placeholders. Each carried a live `onClick` calling `LevelToBeOpened` with a build index.

| Tile | Argument | Scene at that build index |
|---|---|---|
| Level7 | 7 | GameOver |
| Level8 | 8 | LevelSelect |
| Level9 | 9 | GameCompleted |
| Level10 | 10 | Customize |

They were reachable. On returning from a level the screen ran
`_levelsToUnlock[levelClearedCount]` to open the next tile, and once all six levels were
cleared that index is 6, which is Level7. The guard in front of it was
`levelClearedCount % 5 != 0`, and 6 % 5 is 1, so it passed.

The route in: clear all six levels, relaunch so the static `_previousLevelClearedCount` is
empty again, replay any level that is not the last, then return to the level select screen. A
seventh tile unlocks and opens the GameOver scene.

An earlier note in this document described that line as indexing past the end of the array.
That was wrong. Only six levels exist so the cleared count never exceeds 6 and the array holds
ten, meaning it never overruns. It indexes into the bogus tiles instead, which is worse than a
crash because nothing reports it.

Related, `FindIfArenaCompleted` summed a six wide window, `levelCompleted - 5` through
`levelCompleted` inclusive, against a hardcoded 15. At a cleared count of 6 that window covers
levels 1-2 through 2-1 rather than arena 1, so "arena 1 fully starred" could be satisfied by
the wrong five levels. The window also counted the arena 2 level towards the total that
unlocks arena 2.

Fixed by giving the tiles no build index at all. `LevelButtonView` holds a `LevelDefinition` or
null, a null tile renders permanently locked and cannot be tapped, and the unlock rule moved to
`LevelProgression.IsUnlocked`, which checks that every earlier level is cleared and that the
arena's star requirement is met by the arenas ahead of it only.

## 19. What Phase 6 replaced, and what is still unproven

The level order, the end of the game and the star thresholds are authored now. Coin counts were
read from each scene's `CoinBag`, the same source `GameManager` counts at runtime, and the
thresholds reproduce the old ratio behaviour exactly. That was checked by running the previous
formula against the new one for every coin count on every level rather than by reading them
side by side.

| Level | Coins | 1 star | 2 stars | 3 stars |
|---|---|---|---|---|
| Level1-1 | 15 | 4 | 8 | 15 |
| Level1-2 | 15 | 4 | 8 | 15 |
| Level1-3 | 15 | 4 | 8 | 15 |
| Level1-4 | 15 | 4 | 8 | 15 |
| Level1-5 | 20 | 5 | 10 | 20 |
| Level2-1 | 15 | 4 | 8 | 15 |

`GameManager` now warns when a level's authored coin count disagrees with the number of coins
actually in the scene. Authored thresholds go stale the moment somebody adds or removes a coin,
and nothing else would notice.

Three things worth carrying forward.

The level select entry animation is unexercised. Binding was verified in play mode against a
real profile, but the star pop, the arena crossing and the owl only run on arriving from a
level with 15 stars in arena 1. The code is reviewed, not run.

`Lever.cs:26` still reads `GameSession.CurrentLevelBuildIndex == 5`. It is the last hardcoded
build index in the project. It gates activating a waypoint list that is probably only populated
on that level, so testing whether the list is empty would likely remove the check outright,
but that needs confirming against all six scenes.

Re-saving `Level1.prefab` migrated it to the Unity 6 serialization format, dropping
`m_RootOrder`, moving `Animator` to serializedVersion 7 and renaming TextMeshPro's
`m_enableWordWrapping` to `m_TextWrappingMode`. That accounts for most of that file's diff and
is not a behaviour change. Expect the same the first time any other old prefab is re-saved.

## 20. Input stays on the legacy Input Manager

Recorded here rather than in a README because the project does not have one.

The whole shipping input path is uGUI plus an EventSystem: the joystick from the Joystick Pack
and every button in the game are pointer handlers, and those are backend agnostic. After this
phase there is exactly one raw `Input.` call left in the project, the debug jump key at
`PlayerController.cs:119`, and it is now behind `#if UNITY_EDITOR || DEVELOPMENT_BUILD` so it
cannot reach a release build.

Moving to the Input System would cost a package, an input module swap across eleven scenes and a
fresh class of "nothing responds to touch" bugs, to buy rebinding that nothing asks for, gamepad
support on a touch only title, and hotplug. The editor's deprecation warning for the Input
Manager is noise in that light. Revisit only if the game gains a control scheme it does not have
today.

The standard's "no raw pixel thresholds" rule still applies and has not been checked against the
Joystick Pack's handle range. That is open.

## 21. Where the plan's Phase 7 was wrong

Five items on the plan's Phase 7 list did not survive contact with this base.

| Plan said | Reality |
|---|---|
| `EnemyController` polls a static singleton every frame; delete `Update` | It has no `Update` at all and is already event driven. Nothing to do |
| `UIManager`'s `_isGamePaused &= false` is the pause desync | That line is ugly but sets the flag false correctly. The desync is that `ResumeGame` never cleared the flag, so the next pause press took the resume branch and it took two presses to pause again |
| `FallingBricks` gates on wall clock `Time.time % 3` | It is `% 1`, which is zero for every integer, so that gate was always true. All three of its conditions were broken, not one |
| `CameraManager`'s float equality means the camera may never hand back | It does hand back. `Level6Record`'s final keyframe is bit for bit the same value as the literal in the code, so the comparison is true once the clip holds. It is fragile, not broken |
| Moving platforms: `WayPointFollower`, `Follower`, `StickyPlatform` | `StickyPlatform` was deleted in Phase 2. `Follower` is not a platform at all: its only user is the music button on the main menu, a Canvas element with no collider |

### Things the audit did not have

- `ShootTrigger` raises `StartShooting` on every trigger entry, so walking out of the boss arena
  and back in started a second attack loop alongside the first. The unused `int called = 0;
  called++;` and its log in the old loop look like an attempt to find this.
- Six of the nine `FallingBricks` in the game have no Animator, and the old code wrapped both the
  animation call and the fall trigger in a null check on it, so the script never made those six
  fall. They are dynamic bodies with gravity already on, which is why they behaved like bricks
  anyway.
- The four patrolling `Cube_Bricks` were dynamic rigidbodies with gravity enabled, held in place
  only by `WayPointFollower` overwriting their position every frame.
- `Bouncer`'s impulse works out to 9.78, 6.72, 3.95 and 9.95 on the four pads in the game. There
  is one pad per level and the value is different in each, so there was no single number to
  replace the formula with.
- `SpecialMonsters.EndBlockPosition` and the `ProjectileEnd` field feeding it are written and
  never read anywhere.

## 22. What Phase 7 changed, and what is still unproven

Ten commits, behaviour first and naming last so a rename never hid a logic change.

The three crash paths in the enemy kill are gone, replaced by an `Enemy` component on the body
object of the three enemy prefabs. The magic `for (i = 0; i < 3; i++)` was encoding the fact that
`Enemy.prefab` has three meshes with colliders while Bee and Crab have one. Every enemy in all
six levels is a prefab instance, so no scene needed touching for it.

Sixteen waypoint driven objects moved from `transform.position` writes to kinematic rigidbodies
moved with `MovePosition` on the physics step, with interpolation so they still render smoothly.
The nine enemy bodies and five saws had no rigidbody at all before this.

`Time.timeScale` has one owner. It had eight.

**None of this has been run.** There is still no device build, and there has not been one for six
phases now. Specifically unproven:

- The boss fight. The attack pattern's timing is preserved on paper, by keeping the shot count
  raised when the bullet leaves rather than when it is scheduled, but nobody has watched it.
- The camera handback, which now triggers 200ms earlier because the alien dead flag is set when
  the boss takes its final hit rather than from a delayed callback.
- Every one of the sixteen objects converted to physics movement. Enemies and saws never carried
  the player, so the conversion was about correctness rather than a symptom anyone reported.
- The four patrolling bricks, which are kinematic now. The two that wait for a lever no longer
  sit there being pulled down by gravity, which is a visible change.
- Safe area on a real device with a cutout. The arithmetic is tested, the wiring is not.
- `Bouncer` feel. The number is preserved exactly, so this should be a no-op, but it is computed
  in `Start` now rather than per bounce.

### Re-saving scenes did not migrate them

Section 19 expected the Unity 6 format migration to inflate the diff of anything re-saved. That
held for prefabs but not for scenes. Saving Level1-5, Level2-1, MainMenu, LevelSelect and
Customize left their `m_RootOrder` counts untouched and grew each file only by the lines actually
added. Scene diffs in this phase are between 29 and 64 lines and all of it is real content.

`ProjectSettings/TimeManager.asset` is the opposite case. Unity 6 rewrites the fixed timestep as
a rational the first time anything saves project settings, and it came back after every save
during this phase, so it has its own commit.

### The naming sweep, and the four names left alone

The sweep went last so a rename could not hide a logic change, and every serialized rename
carries a `[FormerlySerializedAs]`. To prove nothing reset, the 208 affected serialized values
across all 11 scenes and every prefab were dumped before the rename and again after. The two
dumps are identical, and no scene or prefab was dirtied by the sweep.

`Item.objectName`, `Item.stackable`, `Item.item` and `Consumables.item` are still lower case.
They are the only naming violations left. Renaming them would touch every collectable asset in
the game for no behavioural gain, and nothing in Phase 7 went near them, so they are left for
whenever the collectables are worked on properly.

`SpecialMonsters` had a public field called `Animator`, of type `Animator`, which shadowed the
type name inside the class. It is `_animator` now.

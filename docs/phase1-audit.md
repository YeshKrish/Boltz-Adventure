# Phase 1 Audit — re-baselined on `release/launch`

All prior findings were verified against the `Version1.0.3` branch, which turned out to be
content-stripped and ~35% smaller. This document replaces them. Line numbers below are valid
as of commit `1fc79b04`+ on `develop`.

Severity per `unity-standards`: **Critical** / **Major** / **Minor** / **Observation**.

---

## 0. Corrections to the approved plan

Four plan assumptions were wrong on this base. These matter before Phase 2 runs.

| Plan said | Reality on `release/launch` | Impact |
|---|---|---|
| `com.unity.cinemachine` has **zero** scene references — remove it | **Used in all 6 gameplay scenes** (CinemachineBrain + VirtualCamera + FramingTransposer + Pipeline) | **Do not remove.** Cinemachine drives the gameplay camera |
| Standardize on DOTween; migrate 3 LeanTween sites | DOTween's **only** call site is in `DotWeenPath.cs`, which is **dead code**. LeanTween is the only *live* tweener | Decision needs revisiting — see §3 |
| `main` is v1.0.3 / build 13 / May 2023 | `origin/main` is **v1.1.2 with Firebase**, forked *before* release/launch's 158 commits | Resetting main is a force-push, not a fast-forward. **DECIDED: deferred** — see below |
| Phase 7: fix `CameraFollow` frame-rate-dependent Lerp | `CameraFollow.cs` is **dead code** — superseded by Cinemachine | Delete it; don't fix it |

### Branch state after Phase 0

`origin/main` was **deliberately left untouched** — it still points at the v1.1.2 Firebase tip.
All revival work happens on `develop` (pushed to origin). `main` is reset to the release commit
only at Phase 10, when v1.2.0 is ready to merge and tag. Until then `main` is stale; that is a
known, accepted state, not an oversight.

Local `main` sits at the revival base (`bf53ac27`) and therefore reports "ahead 158, behind 3"
against `origin/main`. Leave it; do not reconcile it mid-project.

`origin/Version1.0.3` also still exists remotely. It is fully preserved by the
`archive/version-1.0.3` tag on origin and can be removed at any time with
`git push origin --delete Version1.0.3`.

Tags on origin: `archive/version-1.0.3`, `archive/release-launch`,
`archive/feature-post-processing`, `v1.0.3-main`, `v1.1.1-playstore`.

## 0b. Phase 2 hazard — do not delete this pack

`SpecialMonsters.cs` (the Level 6 boss) subscribes to
`ProjectileMoveScript.DeactivateAllActiveBullets`, and `ProjectileMoveScript` lives in
**`Assets/GabrielAguiarProductions/Unique_Projectiles_Volume_1/`**.

Deleting that pack wholesale — which the "aggressive cleanup of unused art packs" step would
do, since a GUID scan shows most of its *assets* unreferenced — **breaks the boss fight at
compile time**. Keep the `Scripts/UniqueProjectiles/` subtree; prune only verified-unreferenced
art/prefabs within the pack.

`Assets/ShootTrigger.cs` and `Assets/Invulnerable.cs` are first-party gameplay scripts sitting
loose in the `Assets/` root. They belong under `Assets/Scripts/`.

---

## 1. Dead code — zero scene/prefab GUID refs AND zero code refs

Verified two independent ways (GUID index across all `.unity`/`.prefab`/`.asset`/`.controller`/`.mat`,
plus a symbol grep across all `.cs`).

| File | Lines | Note |
|---|---|---|
| `Assets/Scripts/CameraFollow.cs` | 23 | Superseded by Cinemachine |
| `Assets/Scripts/DotWeenPath.cs` | 26 | The **only** DOTween usage in the project |
| `Assets/Scripts/Projectile.cs` | 18 | Previously unaudited |
| `Assets/Scripts/StickyPlatform.cs` | 24 | Previously unaudited — Phase 7's "moving platform" concern is moot |
| `Assets/Scripts/WayPointFolloweActivator.cs` | 15 | Previously unaudited |

**Four of the six "previously unaudited" scripts are dead.** Delete in Phase 2 rather than
auditing or refactoring them.

> Caveat: this proves no *static* reference. A script added purely via `AddComponent` from a
> string, or living in an addressable/Resources prefab, would not show up. None of these five
> appear in any `AddComponent` call, and the project has no `Resources.Load` of them.

---

## 2. Save / persistence — inputs for Phase 5

**9 distinct keys, 52 call sites, and zero `PlayerPrefs.Save()` calls in the entire project.**

| Key | Type | Sites | Note |
|---|---|---|---|
| `CoinsCollected` | string | 12 | `"CollectedAll"` / `"Collected Half"` / `"Collected Quater"` (sic) |
| `LevelClearedCount` | int | 9 | |
| `IsLastSceneMainMenu` | int | 7 | used as a bool |
| `PreviousBall` | int | 5 | |
| `Current Level` | int | 5 | **contains a space** — quote exactly |
| `CoinsCollectedQuantity` | int | 5 | |
| `LevelCleared` | int | 4 | stores a build index |
| `IsMainMenuChnagedAtLeastOnce` | int | 3 | **misspelled — baked into shipped saves, read verbatim** |
| `GameOverLevel` | int | 2 | sentinel for "final level" |

`SaveManager.cs` (139 lines) confirmed on this base:
- `SaveJson` / `OverrideJson` are **byte-identical** and both `File.AppendAllText` — the file
  grows unboundedly and keys repeat. *(Critical)*
- `int.Parse(parts[0])` with no `TryParse` and no length guard — one malformed line is an
  unhandled exception on load. *(Critical)*
- `LoadJson` returns `null` on first run; callers do not all null-check. *(Critical)*
- Writes tab-delimited text despite the `Json` naming. *(Minor)*

---

## 3. Tweening — decision needs re-confirming

Live call sites, complete:

| Site | Call |
|---|---|
| `MainMenu.cs:98` | `LeanTween.alpha(...)` fade in |
| `MainMenu.cs:104` | `LeanTween.alpha(...)` fade out + `setOnComplete` |
| `SawRotate.cs:9` | `LeanTween.rotateAroundLocal(...).setRepeat(-1)` — infinite, never cancelled *(Major)* |

That is the entire tween surface: two UI alpha fades and one constant spin. DOTween is
currently **unused**. The approved decision ("standardize on DOTween, delete LeanTween") was
made believing DOTween had a live call site and LeanTween had three.

**DECIDED: keep DOTween.** Port the 3 LeanTween sites to DOTween, then delete `Assets/LeanTween/`
(6.8 MB, 34 scripts). DOTween (835 KB) stays as the library for future polish work. Keep
PathCreator — `Follower.cs` samples `path.GetPointAtDistance`, which `DOPath` cannot replace.

The port is a **fix as well as a swap**. Every migrated tween gets `DOTween.Kill` in `OnDisable`,
`.SetLink(gameObject)` as a second net, and `.SetUpdate(true)` on anything drawn while
`Time.timeScale == 0`. `SawRotate.cs:9` is the one that is currently broken.

`DotWeenPath.cs` is dead code and is deleted rather than ported — but it is the reference for
how `DOPath` was configured here, so read it before deleting.

---

## 4. `CameraManager.cs` (71 lines) — previously unaudited

- **`Update()` line 34** compares a float for **exact equality**:
  `_level6Camera.transform.position.x == _maxXdistance` (116.1f). This is essentially never
  true for an animated transform, so the level 6 intro camera may never hand back to the main
  camera and the UI may never re-appear. *(Critical)*
- **`async void DeactivateCameraDelay()`** (line 59) uses `Task.Delay(500)`. Not tied to the
  Unity lifecycle: if the scene unloads mid-delay the continuation resumes against destroyed
  objects. Also runs on wall-clock time, so it ignores `Time.timeScale`. Should be a coroutine.
  *(Major — also violates the standard's "coroutines over async")*
- **`Start()` line 18**: `if (!_level6Camera.activeSelf) { _level6Camera.SetActive(true); ... }`
  unconditionally forces the level-6 camera on and hides the UI. Correct only if `CameraManager`
  exists solely in Level 6 — **verify before changing**. *(Major, needs confirmation)*
- `_maxXdistance` is a hardcoded non-serialized magic number. *(Minor)*

## 5. `SpecialMonsters.cs` (221 lines) — previously unaudited, the boss fight

- **`RetriveBullets()` returns `null`** (line 201) when all 5 pooled bullets are active or the
  alien is dead; `FireBullets()` line 176 dereferences it immediately →
  `NullReferenceException`. *(Critical)*
- **`Debug.Log("Hit" + _enemyHealth[_hit].name)` at line 86 runs before any bounds or tag
  check.** Once `_hit` reaches 3 and `_enemyHealth` has 3 entries, the next collision throws
  `IndexOutOfRangeException`. *(Critical)*
- **`async void SpawnBulletsOnAInterval()`** (line 106) runs `while (true && !GameManager.instance.IsPlayerDead)`
  with `Task.Delay`. Nothing cancels it on disable, destroy, or scene change — it outlives the
  GameObject (`Destroy` at line 168) and keeps calling `ShootBullet()` against destroyed
  objects, and dereferences `GameManager.instance` after a scene swap. *(Critical)*
- **Pooled bullets leak.** `Instantiate(_bullets)` ×5 at line 61 with no parent; `Dead()`
  destroys the alien but never the bullets, so they persist for the scene's lifetime. *(Major)*
- **`Debug.Log` in `Update()`** (line 71) — a string concat + box every frame, per instance,
  in a shipping build. Also logs at lines 86, 110, 182. *(Major)*
- `Animator.SetBool` written every frame rather than on change (lines 75/79). *(Minor)*
- Public mutable statics `_isAlienDead`, `EndBlockPosition`, `_startPos` survive scene loads and
  are reset from `OnDisable`, so ordering determines correctness. `CameraManager` reads
  `_isAlienDead` — the two bugs interact. *(Major)*
- `_bulletInitalVelocity` (line 57) reads velocity off an inactive template, so it is always
  `Vector3.zero`; line 180 is effectively `velocity = Vector3.zero`. Harmless as a reset, but
  the name does not describe it. *(Observation)*
- Naming: `firedBullets` (private, no `_`), public `_bulletsList`, public fields `ProjectileEnd`
  / `Animator` (the latter shadows the `Animator` type). Delays are float milliseconds cast to
  int. *(Minor)*

---

## 6. Legacy API migration (`.velocity` → `.linearVelocity`)

The plan's "×9" over-counted. Actual, split by ownership:

**First-party — must migrate (10 occurrences, 2 files):**
- `PlayerController.cs` lines 74, 126, 136, 138, 139 (×2), 209, 211
- `SpecialMonsters.cs` lines 57, 180

**Third-party, in demo/WebDemo folders that Phase 2 deletes (5 occurrences):**
- `EffectCore/packs/StylizedExplosionPack1/WebDemo/scripts/ExplodingProjectileExplosion1.cs` 54, 89
- `EffectCore/packs/WebDemoAssets/scripts/ECExplodingProjectile.cs` 72, 100, 128

**False positives — `ParticleSystem.velocityOverLifetime` is not deprecated, do not touch:**
- `GabrielAguiarProductions/.../ParticleSystemController.cs` 83, 444, 568

> Order matters: `Rigidbody.linearVelocity` does not exist before Unity 6. This migration must
> land **after** the editor upgrade, or the project will not compile.

---

## 7. Carried forward from the original audit — re-confirmed present

Line numbers re-derived on this base.

- `PlayerController.cs:139` — `_rb.velocity = new Vector2(...)` assigned to a `Vector3`
  property, silently zeroing Z. *(Critical)*
- `GameManager.cs:130,134` — `Mathf.Ceil(_coinCount / 2)` and `/ 4`: integer division happens
  *before* `Ceil`, making it a no-op. Phase 6's absolute `starThresholds` removes the division
  entirely. *(Critical)*
- `GameManager.cs:75` — `if (PlayerPrefs.HasKey("LevelCleared")) { }` empty body. *(Minor)*
- `GameManager.cs:126-137` — the star chain is non-exhaustive, so `CoinsCollected` keeps the
  *previous* level's value when a player collects less than a quarter. *(Critical)*
- `SawRotate.cs:9` — infinite tween, no `OnDisable` cancellation. *(Major)*
- No `Screen.safeArea` handling anywhere, while the manifest advertises `android.notch_support`. *(Major)*
- All singletons lack `else { Destroy(gameObject); return; }` and an `OnDestroy` null-out.
  `SaveManager.Awake` assigns `Instance` but never destroys a duplicate. *(Major)*

## 8. Package inventory after the Unity 6 upgrade

Section 6's `.velocity` migration was applied automatically by Unity's API updater
during the upgrade — all 10 first-party and 5 third-party sites, with the three
`ParticleSystem.velocityOverLifetime` false positives correctly untouched.

**Removed** (each: zero hits across scenes/prefabs/`.asset`/C#, zero dependents in
`packages-lock.json`):
`2d.sprite`, `2d.tilemap`, `ai.navigation`, `ide.vscode`, `multiplayer.center`,
`visualscripting`, `modules.unityanalytics`.

**Kept, and why it differs from the approved plan:**

| Package | Version | Reason |
|---|---|---|
| `cinemachine` | 2.10.5 | The plan said "zero scene references — confirmed" and scheduled removal. **Wrong.** `CinemachineBrain`, `CinemachineVirtualCamera` and `CinemachineFramingTransposer` appear in all 6 gameplay scenes. The upgrader stayed on 2.x; **do not accept a 3.x bump** — it renames `CinemachineVirtualCamera` and breaks every scene at compile time. |
| `recorder` | 5.1.4 | Unreferenced, but kept by the user's decision for capturing portfolio and store-listing footage. |
| `timeline` | 1.8.10 | Unused directly — no `.playable` assets, no `PlayableDirector`. Retained only because `recorder` depends on it. If Recorder is ever dropped, this goes too. |

**Still open (Phase 1b):** `com.unity.render-pipelines.universal` is not installed yet.
`Assets/UniversalRenderPipelineGlobalSettings.asset` and `Assets/DefaultVolumeProfile.asset`
were emitted by the upgrader and are inert until it is (their `m_Script` is null).
`GraphicsSettings.m_CustomRenderPipeline` is still `{fileID: 0}`, so the project renders
Built-in — activation is Phase 3, which also relocates those two assets into `Assets/Settings/`.

Install URP through the package manager rather than hand-editing `manifest.json`, so the
version resolves against the editor instead of being pinned by guess.

**Done.** URP 17.3.0 is installed and *not* activated —
`GraphicsSettings.m_CustomRenderPipeline` is `{fileID: 0}` and all 6 quality levels have
null pipeline overrides, so the project still renders Built-in.

## 9. Ball selection is stored as the active flag on prefab *assets* — Critical

Found when Unity 6 re-serialized `FootBall.prefab` and flipped its root
`m_IsActive` from 0 to 1.

`ChooseBall.cs:9` exposes `public GameObject[] BallPool` on a ScriptableObject
(`Assets/Scripts/ScriptableObjects/BallPool.asset`) holding the six ball **prefab assets**.
Selection is then written *onto those assets*:

- `BallManager.cs:36,42` — `_ballPool.BallPool[i].SetActive(true/false)`
- `BallManager.cs:27` — `_ballPool.PreviousBall.SetActive(true)`
- `GameManager.cs:230-234` — loops the pool and `Instantiate`s **every** ball whose
  `activeSelf` is true

Three consequences:

1. **It mutates project assets from the editor.** Unity's in-memory copy of a prefab can
   diverge from disk and a later save writes the runtime flag back into the `.prefab` file
   and into version control. Observed twice in Phase 2: `FootBall.prefab` was flipped to
   active by an editor save, reverted on disk, and flipped back again — because the fix
   had been applied to the file while Unity still held the dirty object. Correcting it has
   to go through the AssetDatabase (`SetActive` + `SetDirty` + `SaveAssets`), not a text
   edit, or Unity simply overwrites it.
2. **The selection is not actually a selection.** `GameManager` instantiates every active
   ball, so any state with two active prefabs spawns two stacked balls on the player.
3. **It behaves differently in a build**, where prefab assets are read-only — the flag lives
   only in memory and resets each launch, so editor and device disagree.

**Committed baseline: all six ball roots are `m_IsActive: 0`.** No ball is active on disk;
`BallManager.Start` activates one at runtime from `PlayerPrefs["PreviousBall"]`. That is the
clean state and any diff showing a ball root becoming `1` is editor state leaking in, not a
design change.

This is the same violation the plan flagged as "`ChooseBall` stores selected ball on the
asset itself", but the mechanism is worse than described. Phase 5 moves it to
`SaveData.selectedBallId`; `GameManager` then instantiates exactly one ball, looked up by id.

**Guard until then:** run `tools/ballcheck.py`, or otherwise match each prefab's root
GameObject *by name* before reading `m_IsActive`. Do **not** use
`grep -m1 m_IsActive Assets/Prefab/Balls/*.prefab` — these prefabs have child GameObjects
serialized ahead of the root, so `-m1` reports a child's flag and silently gives the wrong
answer. That mistake is what produced the earlier claim that `poke bola` was committed active.

## 10. Phase 2 target — `EffectCore` ShaderGraphVersion subtree fails to import

Installing URP surfaced a hard import error:

```
Asset import failed, "Assets/EffectCore/packs/StylizedExplosionPack1/shader/
alphaBlend_glow_shadergraph.ShaderGraph" > InvalidOperationException:
Failed to add object of type `UniversalMetadata`
```

The graph predates Shader Graph 17 and cannot be upgraded. Seven materials reference it
(`empty_glow`, `decal_explode_glow`, `fire_glow`, `flarespark_glow`, `MeltingFire_glow`,
`circle_glow`, `cartoonSmoke_glow`).

**Not worth repairing.** A GUID scan of the 28 assets under
`EffectCore/packs/StylizedExplosionPack1/prefabs/ShaderGraphVersion/` against all 11 scenes
returns zero hits, and none of the 7 materials is referenced outside `EffectCore/` itself.
The whole subtree is dead weight. Deleting it in Phase 2 removes the error; do that before
the Phase 3 material conversion so the converter never has to touch it.

**Done in Phase 2b.** 66 files removed, import error cleared.

## 12. Phase 2 outcome and carry-forward for Phase 3

Cleanup removed ~1,000 files. All 11 scenes open with zero errors and zero warnings apart
from the `Alien_Tall` animator transition above.

**Kept deliberately, against a first reading of "unreferenced":**

| Kept | Why |
|---|---|
| `Platformer Game Kit/**/*.blend` (65 MB) | These are the editable sources for the game's entire art identity — Crab, Bee, Bouncer, Cannon, Hazard_Saw, SpikeTrap, Tower, platforms, rocks, trees, Coin are all live FBX from this kit. Deleting the sources means any future model tweak starts from scratch. |
| `Free Game Menu Music Pack` (294 MB, 21 WAVs) | Licensed tracks; only one ships today, but new arenas need music and re-sourcing licensed audio is worse than carrying it. |
| `CustomBall/organic-ball/orgballex.fbx` | Unreachable *because* its prefab is unwired — see below. |
| GabrielAguiar textures/prefabs, JMO Cartoon FX, EffectCore mobile/legacy prefabs | Effect libraries for future bosses and hazards. |

**Two finished ball skins are not wired up.** `BallPool.asset` is live and feeds 4 ball skins,
but `Assets/Prefab/Balls/orgballex.prefab` and `WaterMelon.prefab` are **not in the pool** —
which is why their meshes scan as unreachable. Adding them to `BallPool` is a minutes-long job
and gets two extra skins for free. Worth doing alongside Phase 5, when ball selection moves to
`SaveData.selectedBallId`.

**`*.unitypackage` is gitignored** (`.gitignore:61`). The three GabrielAguiar installers
(112 MB) are local-only and were never in the repo, so deleting them would be irreversible for
zero repo benefit — they were left in place. Of these,
`UniqueProjectilesVol1_2020.3_URP_v1.7.unitypackage` is worth keeping on disk for **Phase 3**:
importing it yields ready-made URP materials for that pack instead of hand-converting them.

**Deleted vector/archive sources** (~254 MB): 50 `.eps`/`.ai` files Unity cannot read, and
duplicate `.zip`/`.obj` copies of models that already exist as FBX. Note that deleting the
`.eps`/`.ai` files stranded 28 folder `.meta` files, which made Unity recreate all 28 empty
folders on the next refresh — when deleting a folder's entire contents outside Unity, remove
the folder's `.meta` too.

## 11. `GetIsOwlTriggeredOnce` throws on MainMenu startup — Critical, live

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

`bool.Parse` is called on raw file text with no `File.Exists` check, no `TryParse`, and no
try/catch — and the writer **appends**. So the read succeeds only in one narrow case:

| File state | How it arises | Result |
|---|---|---|
| missing | fresh install, owl never triggered | `FileNotFoundException` |
| empty | the state on this machine (0 bytes, dated 2023) | `FormatException` ← observed |
| `True` | `IsOwlTriggeredSO` called exactly once | works |
| `TrueTrue` | called twice — `LevelSelect.cs:308` can fire repeatedly | `FormatException` |

So it throws for effectively every player. Because it throws from `Start()`, the rest of
`AllSceneManager.Start()` never runs and `_owlSO.IsOwlDisappereadOnce` is never restored —
**the owl reappears every launch** for anyone past the trigger.

This is the same append-instead-of-overwrite defect the plan flagged on
`SaveManager.OverrideJson`; this is a second instance, and unlike that one it is already
crashing. Phase 5 subsumes it: the flag becomes `SaveData.isOwlDisappearedOnce`, written
through the atomic `File.Replace` path, and `Load()` never throws. Until Phase 5 lands,
expect this exception on every MainMenu entry — it is pre-existing, not an upgrade
regression.

### Minor, same pass

`Alien_Tall` animator: `AnyState -> MonsterArmature_Bite_Front` has no Exit Time and no
condition, so Unity ignores the transition — the Level 6 boss's bite animation never plays.
Pre-existing content bug, relevant to Phase 8's Arena 2 verification. *(Minor)*

`AllSceneManager.cs:23-26` — the duplicate-singleton branch calls `Destroy(this)`, which
destroys only the component and leaves the GameObject, and `DontDestroyOnLoad(this)` then
runs unconditionally on that same just-destroyed duplicate. Same singleton class of bug as
section 7. *(Major)*

## 13. URP made 10 shaders render INVISIBLE, not magenta — resolved in Phase 3

The expected failure mode of a pipeline swap is magenta. This project's was the opposite and
much easier to miss: **materials that silently disappeared.**

URP only draws a pass whose `LightMode` tag is `UniversalForward`, `SRPDefaultUnlit` or
`UniversalForwardOnly` — or that has **no** `LightMode` tag at all. A pass tagged
`"LightMode"="ForwardBase"` (the Built-in RP name) matches nothing in URP's forward loop, so
URP renders **nothing** for it. No error, no warning, no magenta. The shader compiles clean.

Diagnosis used `Shader.FindPassTagValue` over every pass of every shader reachable from the
build scenes — the only reliable way to see this, since neither the console nor the material
inspector reports it.

### Two distinct causes, two distinct fixes

**(a) Eight shaders carried a vestigial `ForwardBase` tag.** Six GAP projectile shaders plus
`EffectCore/alphaBlend_glow` and `alphaBlend_depthBlend_glow`. Each was verified to be a
single-SubShader, single-Pass, pure unlit vert/frag shader containing **zero** Built-in
lighting macros (`LIGHTING_COORDS`, `UNITY_LIGHTING`, `SHADOW_COORDS`, `LIGHT_ATTENUATION`,
`_LightColor0`, `AutoLight`). They never consumed the tag; it was inherited boilerplate.
Deleting the one line is a zero-behaviour-change fix and leaves them valid under Built-in too.
*(commit `7f2bf5f7`)*

**(b) Two shaders were surface shaders, which URP cannot run at all.**
`GAP/ParticlesAdditiveMobile_Scroll` and `GAP/ParticlesABMobile_Scroll`. Here the
`FORWARDBASE` tag is *generated by the surface compiler* and is absent from the source, so
there is nothing to edit out — the `#pragma surface` path itself has no URP equivalent.

**This one was gameplay-breaking.** `ParticlesAdditiveMobile_Scroll` backs
`Projectile_Fireball_AddScroll_Mobile` and `Trail03_AddScroll_Mobile`, the two materials on
`Assets/Prefab/Vfx/vfx_Projectile_Fireball01Blue_Mobile.prefab` — the Arena 2 boss projectile,
referenced by `Scenes/Arena2/Level2-1.unity` and nothing else. **The boss was firing invisible
projectiles.** Relevant to Phase 8's Arena 2 verification.

Both were ported by hand to explicit vertex/fragment passes *(commit `50f83dcd`)*. The port is
behaviour-preserving rather than a reinterpretation: both declared a custom lighting model
returning `half4(0,0,0,s.Alpha)` — zero light contribution — and emitted all visible colour
through `o.Emission`. They were unlit shaders wearing a surface-shader costume. The fragment
maths is the original `surf()` body verbatim; render states are spelled out to match what the
surface compiler had been generating (`alpha:fade` → `Blend SrcAlpha OneMinusSrcAlpha` +
`ZWrite Off`). Property names and defaults are unchanged, so no `.mat`, prefab or scene edits
were needed.

Two details worth carrying forward:

- **Vector uniforms are declared `float4` and swizzled, not `float2`.** Unity's Vector
  properties are `float4` on the C# side; a bare `float2` inside `CBUFFER_START(UnityPerMaterial)`
  silently shifts every member after it and breaks SRP Batcher compatibility.
- **The ASE node graph and `CustomEditor "ASEMaterialInspector"` were dropped.** Amplify Shader
  Editor is not installed in this project, so the custom editor was already a dead reference.
  The original graph is recoverable from git history.

### The unitypackage was not needed

Section 12 flagged `UniqueProjectilesVol1_2020.3_URP_v1.7.unitypackage` as worth keeping for
Phase 3. **It was not used**, and importing it would have been the worse option: it also
carries `Scripts/UniqueProjectiles/`, which `SpecialMonsters` depends on via
`ProjectileMoveScript.DeactivateAllActiveBullets`, so the import risked overwriting live
gameplay code to fix two materials. The hand port touched two files and nothing else. Keep the
package on disk as a fallback, but it is no longer on the critical path.

### Verification

Full sweep of all **11 enabled build scenes**: 105 material references across 12 distinct
shaders, **zero** that URP will not draw. Both ported shaders compile with zero errors and
zero warnings, report an untagged pass, and their live materials resolve with all property
values intact.

**Two earlier claims corrected by this pass:**

1. `whYKnot/glass` was flagged as at-risk. It is not — it has no `LightMode` tag, so URP draws
   it normally.
2. The ~116 legacy particle materials (`Legacy Shaders/Particles/*`, `Mobile/Particles/*`)
   **already render correctly** under URP for the same reason. Converting them to
   `Particles/Unlit` is therefore optional modernization, **not** a fix, and is not blocking.

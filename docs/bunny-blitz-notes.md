# Bunny Blitz notes

What we take from Unity's "3D as 2D" sample, what we leave, and why. Reviewed
against the project at `E:\Unity\Projects\Bunny Blitz - 2D & 3D Sample Project`,
82 C# files, editor `6000.3.9f1`.

The conclusions here are drawn from reading their source. Nothing is copied. Their
project ships under the Asset Store EULA, and no file, asset or code block moves
from it into ours.

## Why the architecture does not transfer

The two games look alike in a screenshot and are built on opposite foundations.

| | Bunny Blitz | Boltz |
|---|---|---|
| Physics | Physics2D, `OnTriggerEnter2D`, `gravityScale` | 3D `Rigidbody` and `SphereCollider` |
| Renderer | URP 2D Renderer | URP Universal renderer, 3 mobile tiers |
| World | Sprites, Tilemap, Sprite Shape | 3D models plus 2D parallax backdrops |
| Character | Animated humanoid | A physics ball driven by momentum |

Adopting the 2D Renderer means adopting `Rigidbody2D`, which means rewriting the
rolling-ball feel the game is built on and every script that touches it. That is
not a port, it is a different game. So the useful reading of "get inspired by
Bunny Blitz" is their structure and their polish, re-implemented against our own
setup.

Two things from Unity's blog turned out to be wrong once the project was open. It
is authored on `6000.3.9f1`, the same editor we are on, not 6.5, so nothing here
needs an upgrade. And their depth of field is not a custom pass, it is a stock URP
Volume forced to Bokeh mode by `DynamicDepthOfFieldController`. The custom pass is
a separate background blur, and it is 2D Renderer only.

## Per-level save records

Their `SaveSystem` keys progress by level name, a `Dictionary<string, LevelData>`
serialized through `ISerializationCallbackReceiver` into parallel key and value
lists, because `JsonUtility` cannot serialize a dictionary.

That shape is the structural fix for the bug in `phase1-audit.md` section 8. Our
`CoinsCollected` is one global PlayerPrefs string, so finishing a level with less
than a quarter of its coins leaves the previous level's rating in place. Holding a
record per level makes that state unrepresentable rather than merely corrected.

Their storage is the part to leave. One JSON blob in PlayerPrefs is weaker than
the atomic `File.Replace` path Phase 5 already targets, and that path also
subsumes the `GetIsOwlTriggeredOnce` crash in section 11.

Phase 5 is rewriting the format anyway, so it is the moment to reserve fields that
cost nothing now and a migration later: per-level secret flags, and a checkpoint
index and life count even though neither feature is being built yet.

## Level data in the scene

Their `LevelData` is a MonoBehaviour sitting in each level scene holding the level
name, a kill plane, and a block of per-level overrides for player speed and
gravity that get applied on load and reverted on exit. `GameManager` finds it with
`FindAnyObjectByType` and generates a fallback when a scene has none.

This replaces our worst structural wart. `PlayerController` resolves levels
through `GetScenePathByBuildIndex(5)` and `(6)`, then string-compares scene names
every frame to decide whether tower and boss logic should run. Per-level behaviour
belongs on a component in the level, not in hardcoded indices and name compares.

Their scene template is worth taking wholesale, and it is Unity's stock Scene
Template feature, so it costs no code. Their `LevelLoader` is not: it stores a raw
scene-name string per trigger, which trades our build-index coupling for
stringly-typed names and is no better. Levels should be referenced through the
existing `LevelSelectScriptableObject`.

## Graphics

Their shadows are a good idea implemented at a budget we do not have.
`ShadowCameraFarClipSetter` runs a second camera into a full-screen ARGB32 render
texture, rebuilds it whenever resolution changes, and publishes it as a global
`_BackShadowTex` for the tilemap and sprite shaders to sample. An extra
full-screen camera pass is not affordable on our low tier.

What we want from it is the readability, not the technique. Boltz has no ground
shadow at all, and in a platformer that shadow is not decoration, it is how the
player knows where they will land. A downward raycast and a quad with a soft
circle texture, scaled by distance, gets the whole benefit.

Their `LightManager` solves the mismatch we have, where our parallax layers are
flat-lit while our models take real URP lighting, by pushing the key light's
forward vector into the sprite material so the 2D art shades with the 3D
character. Take the idea and not the code: theirs writes to a shared material
asset from an `[ExecuteInEditMode]` `Update`, which is the same defect as section
10 of the audit, where editor state leaked into the ball prefabs and dirtied
version control. `Shader.SetGlobalVector` does the same job without touching an
asset.

Tinting each parallax layer toward the level's key-light colour, and fading the
far layers toward the sky colour, costs nothing on the GPU and recovers most of
the depth separation their background blur provides. Porting the blur itself would
mean writing a `ScriptableRendererFeature`, since `RenderPassEvent2D` does not
exist on the Universal renderer, and it is not worth that.

Matcap is their cheap-shading workhorse, used across the blocks, the bomb, the
bricks and the checkpoint. One texture lookup, no lights, and it reads as polished
metal or glass. That suits our six ball skins on mobile exactly. ToonyColorsPro is
gone from `Assets/`, leaving only a stale `.csproj` at the repo root, so this is a
small hand-written shader rather than an import.

Their layer and sorting-group scheme is the 2D equivalent of Z discipline. Ours is
just a convention worth writing down: the gameplay plane at Z=0, with named near,
mid and far bands so props and parallax stop fighting.

## Left alone

The 2D Renderer and Physics2D migration, for the reason at the top. The shadow
camera and Bokeh depth of field, on mobile cost. VFX Graph, which is compute-driven
and a risk on low-end GLES Android, where we are well served by Shuriken through
JMO Cartoon FX and EffectCore. Their thrown pomegranates, because a ball does not
throw and Boltz reads as a momentum game. Their enemy stomp, which we already have
in `_enemyDeadJumpHeight`. Their art, which clashes with the Platformer Game Kit
identity and would be recognisable to anyone who has seen the sample.

Also worth not copying: their `GameManager.Update` reads `Keyboard.current` for a
debug teleport with no guard, and it shipped that way.

## Checkpoints, noted but not scheduled

`Checkpoint` and `SpawnPoint` together are about forty lines. The checkpoint is a
trigger that hands a spawn point to the game manager, and the spawn point is a
marker with an `IsStartSpawn` flag.

Boltz has no checkpoint, respawn or player health. One hit goes to the GameOver
scene and replays the level from the beginning. That is the largest gameplay gap
between the two projects and the cheapest to close, which is the opposite of what
we assumed when scoping this pass. It is not scheduled, but the save fields
reserved in Phase 5 leave it a drop-in with no migration.

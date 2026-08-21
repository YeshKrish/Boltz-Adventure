using System.Collections;
using System.Collections.Generic;
using Boltz.Levels;
using Boltz.Save;
using Boltz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The level select screen.
///
/// This used to work out what to show from a cleared count and the build index order, in two nearly
/// identical forty line branches, with a static list carried between scene loads deciding which of
/// them ran. It also polled the save every frame from Update to notice when an arena was finished.
///
/// It is now a straight read of the level database and the save profile: bind every tile once, then
/// play the entry animation if the player arrived by finishing a level. Nothing here writes progress
/// apart from the owl's one time flag.
/// </summary>
public class LevelSelect : MonoBehaviour
{
    [Tooltip("Every tile, in play order. Slots past the last authored level render permanently locked.")]
    [SerializeField]
    private LevelButtonView[] _buttons;

    [Tooltip("One page per arena, matching the database's arena order.")]
    [SerializeField]
    private List<GameObject> _arenaPages;

    [Tooltip("Next and previous arena buttons, in that order.")]
    [SerializeField]
    private List<Button> _nextAndPreviousArenaButtons;

    [SerializeField]
    private Animator _owlDisappearingAnimation;

    [SerializeField]
    private GameObject _owl;

    [SerializeField]
    private GameObject _owlTextPrompt;

    /// <summary>Beat before the owl reacts to a newly opened arena, so the page change reads first.</summary>
    private const float OwlReactionDelay = 0.3f;

    /// <summary>How long the owl stays visible after reacting, before leaving for good.</summary>
    private const float OwlDepartureSeconds = 4f;

    private const float OwlPromptSeconds = 2.5f;

    private LevelDatabase _database;
    private int _currentPage;
    private Coroutine _promptRoutine;

    private void Awake()
    {
        _database = LevelFlow.Database;

        if (_owl != null && SaveService.IsOwlDisappearedOnce)
            _owl.SetActive(false);

        foreach (var button in Buttons())
            button.Clicked += OnLevelChosen;
    }

    private void OnDestroy()
    {
        foreach (var button in Buttons())
            button.Clicked -= OnLevelChosen;
    }

    private void Start()
    {
        BindAll();

        var justPlayed = _database == null ? null : _database.GetById(GameSession.LastRunLevelId);
        bool arrivedFromALevel = !GameSession.CameFromMainMenu && justPlayed != null;

        ShowPage(arrivedFromALevel ? PageOf(justPlayed) : FurthestOpenPage());

        if (arrivedFromALevel)
            StartCoroutine(PlayEntrySequence(justPlayed));
    }

    /// <summary>Shows every tile's current state, with no animation.</summary>
    private void BindAll()
    {
        if (_database == null || _buttons == null)
            return;

        var levels = _database.All;

        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null)
                continue;

            var level = i < levels.Count ? levels[i] : null;
            int stars = level == null ? 0 : SaveService.GetStars(level.LevelId);
            _buttons[i].Bind(level, stars, level != null && LevelProgression.IsUnlocked(_database, i));
        }
    }

    /// <summary>
    /// Pops the stars just earned, then opens the next level, crossing to a new arena if that is
    /// where it lives. Deliberately the same order and timing the screen has always had.
    /// </summary>
    private IEnumerator PlayEntrySequence(LevelDefinition justPlayed)
    {
        if (GameSession.LastRunStars > 0)
        {
            var tile = ButtonFor(justPlayed);
            if (tile != null)
                tile.ShowStars(GameSession.LastRunStars);
        }

        var next = _database.Next(justPlayed);
        if (next == null)
            yield break;

        int nextIndex = _database.IndexOf(next);
        if (!LevelProgression.IsUnlocked(_database, nextIndex))
            yield break;

        if (nextIndex < 0 || nextIndex >= _buttons.Length)
            yield break;

        var tileToOpen = _buttons[nextIndex];
        if (tileToOpen == null || tileToOpen.IsUnlocked)
            yield break;

        if (_database.ArenaOf(next) != _database.ArenaOf(justPlayed))
        {
            ShowPage(PageOf(next));
            yield return StartCoroutine(SendTheOwlAway());
        }

        yield return StartCoroutine(tileToOpen.PlayUnlock());
    }

    /// <summary>
    /// The owl's one appearance, when a new arena opens. Its flag is the only thing this screen
    /// writes to the save profile.
    /// </summary>
    private IEnumerator SendTheOwlAway()
    {
        if (SaveService.IsOwlDisappearedOnce)
            yield break;

        SaveService.IsOwlDisappearedOnce = true;
        SaveService.Flush();

        if (_owlTextPrompt != null)
            _owlTextPrompt.SetActive(false);

        yield return new WaitForSeconds(OwlReactionDelay);

        if (_owlDisappearingAnimation != null)
            _owlDisappearingAnimation.SetBool("isNewArenaUnlocked", true);

        StartCoroutine(HideOwlAfterItLeaves());
    }

    private IEnumerator HideOwlAfterItLeaves()
    {
        yield return new WaitForSeconds(OwlDepartureSeconds);

        if (_owl != null)
            _owl.SetActive(false);
    }

    /// <summary>Called by the owl's own button.</summary>
    public void ActivateOwlPrompt()
    {
        if (_owlTextPrompt == null || _owlTextPrompt.activeSelf)
            return;

        _owlTextPrompt.SetActive(true);

        if (_promptRoutine != null)
            StopCoroutine(_promptRoutine);

        _promptRoutine = StartCoroutine(HidePromptAfterAWhile());
    }

    private IEnumerator HidePromptAfterAWhile()
    {
        yield return new WaitForSeconds(OwlPromptSeconds);

        if (_owlTextPrompt != null)
            _owlTextPrompt.SetActive(false);

        _promptRoutine = null;
    }

    public void NextArena()
    {
        ShowPage(_currentPage + 1 >= _arenaPages.Count ? 0 : _currentPage + 1);
    }

    public void PreviousArena()
    {
        ShowPage(_currentPage - 1 < 0 ? _arenaPages.Count - 1 : _currentPage - 1);
    }

    public void MainMenu()
    {
        NavigationManager.Instance.MainMenu();
    }

    private void ShowPage(int page)
    {
        if (_arenaPages == null || _arenaPages.Count == 0)
            return;

        _currentPage = Mathf.Clamp(page, 0, _arenaPages.Count - 1);

        for (int i = 0; i < _arenaPages.Count; i++)
        {
            if (_arenaPages[i] != null)
                _arenaPages[i].SetActive(i == _currentPage);
        }

        if (_nextAndPreviousArenaButtons != null && _nextAndPreviousArenaButtons.Count >= 2)
        {
            if (_nextAndPreviousArenaButtons[0] != null)
                _nextAndPreviousArenaButtons[0].interactable = _currentPage > 0;

            if (_nextAndPreviousArenaButtons[1] != null)
                _nextAndPreviousArenaButtons[1].interactable = _currentPage < _arenaPages.Count - 1;
        }
    }

    /// <summary>The furthest arena the player has opened, so arriving from the menu lands there.</summary>
    private int FurthestOpenPage()
    {
        if (_database == null)
            return 0;

        int page = 0;
        var levels = _database.All;

        for (int i = 0; i < levels.Count; i++)
        {
            if (LevelProgression.IsUnlocked(_database, i))
                page = Mathf.Max(page, PageOf(levels[i]));
        }

        return page;
    }

    private int PageOf(LevelDefinition level)
    {
        var arena = _database.ArenaOf(level);
        var arenas = _database.Arenas;

        for (int i = 0; i < arenas.Length; i++)
        {
            if (arenas[i] == arena)
                return i;
        }

        return 0;
    }

    private LevelButtonView ButtonFor(LevelDefinition level)
    {
        int index = _database.IndexOf(level);
        return index >= 0 && index < _buttons.Length ? _buttons[index] : null;
    }

    private IEnumerable<LevelButtonView> Buttons()
    {
        if (_buttons == null)
            yield break;

        foreach (var button in _buttons)
        {
            if (button != null)
                yield return button;
        }
    }

    private void OnLevelChosen(LevelDefinition level)
    {
        MusicManager.Instance.ButtonClickSound();
        MusicManager.Instance.GameMusic();
        MusicManager.Instance.MainMenuMusicStop();

        SceneManager.LoadScene(level.SceneName);
    }
}

using System.Collections;
using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Объединённый кондишен ожидания: таймер, аудио или анимация.
/// Флаги режима комбинируются, ожидания выполняются последовательно:
/// секунды → аудио → анимация.
/// ВАЖНО: ShowIf указан полным именем Sirenix.OdinInspector.ShowIf, потому что
/// в проекте есть самописный глобальный ShowIfAttribute, который затеняет
/// Odin'овский (типы глобального namespace выигрывают у using без ошибок).
/// </summary>
public class WaitCondition : ConditionBase
{
    [System.Flags]
    public enum WaitMode
    {
        None = 0,
        Seconds = 1,
        Audio = 2,
        Animation = 4
    }

    [BoxGroup("General"), PropertyOrder(100)]
    [Tooltip("Что ждёт кондишен. Флаги комбинируются, ожидания выполняются последовательно")]
    [SerializeField, EnumToggleButtons] private WaitMode mode = WaitMode.Seconds;

    [BoxGroup("General"), PropertyOrder(101), Sirenix.OdinInspector.ShowIf(nameof(IsSeconds))]
    [Tooltip("Время ожидания в секундах")]
    [SerializeField, Min(0f)] private float waitSeconds = 1f;

    [BoxGroup("General"), PropertyOrder(102), Sirenix.OdinInspector.ShowIf(nameof(IsAudio))]
    [Tooltip("Аудиоклип, который нужно проиграть и дождаться окончания")]
    [SerializeField] private AudioClip waitAudioClip;

    [BoxGroup("General"), PropertyOrder(103), Sirenix.OdinInspector.ShowIf(nameof(IsAnimation))]
    [Tooltip("Animator, на котором нужно запустить анимацию")]
    [SerializeField] private Animator animator;

    [BoxGroup("General"), PropertyOrder(104), Sirenix.OdinInspector.ShowIf(nameof(IsAnimation))]
    [Tooltip("Имя состояния аниматора. Клип не должен быть зациклен — иначе ожидание завершится только по переходу из состояния")]
    [SerializeField] private string stateName;

    private const int LayerIndex = 0;
    private const float EnterStateTimeout = 5f;

    private IEnumerator _waitRoutine;
    private bool _waitIsStopped;
    private bool _animationFailed;

    public bool IsSeconds => mode.HasFlag(WaitMode.Seconds);
    public bool IsAudio => mode.HasFlag(WaitMode.Audio);
    public bool IsAnimation => mode.HasFlag(WaitMode.Animation);

    public override void SetupCondition()
    {
        _waitIsStopped = false;

        if (IsAudio && waitAudioClip == null)
            Debug.LogError($"WaitCondition: waitAudioClip == null; StepName - {parentStep.name}");

        if (IsAnimation && !ValidateAnimator())
            return;

        _waitRoutine = WaitRoutine();
        CoroutineRunner.RunCoroutine(_waitRoutine, parentStep.gameObject);
    }

    private bool ValidateAnimator()
    {
        //if (animator == null)
        //{
        //    Debug.LogError($"WaitCondition: animator == null; StepName - {parentStep.name}");
        //    return false;
        //}

        //if (!animator.enabled || !animator.gameObject.activeInHierarchy)
        //{
        //    Debug.LogError($"WaitCondition: Animator или его объект неактивен ({animator.gameObject.name}); StepName - {parentStep.name}");
        //    return false;
        //}

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"WaitCondition: у Animator нет контроллера ({animator.gameObject.name}); StepName - {parentStep.name}");
            return false;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(LayerIndex, stateHash))
        {
            Debug.LogError($"WaitCondition: у Animator нет состояния '{stateName}'; StepName - {parentStep.name}");
            return false;
        }

        return true;
    }

    private IEnumerator WaitRoutine()
    {
        if (IsSeconds)
            yield return new WaitForSeconds(waitSeconds);

        if (_waitIsStopped)
            yield break;

        if (IsAudio)
        {
            AudioSource audioSource = parentStep.gameObject.AddComponent<AudioSource>();
            audioSource.clip = waitAudioClip;
            audioSource.Play();
            yield return new WaitForSeconds(waitAudioClip.length);
            UnityEngine.Object.Destroy(audioSource); // не оставляем компонент после проигрывания
        }

        if (_waitIsStopped)
            yield break;

        if (IsAnimation)
        {
            int stateHash = Animator.StringToHash(stateName);
            _animationFailed = false;
            yield return WaitForAnimation(stateHash);

            // анимация не запустилась — не закрываем кондишен успехом
            if (_animationFailed)
                yield break;
        }

        if (_waitIsStopped)
            yield break;

        State = State.Success;
        DisableCondition();
        OnConditionResolved?.Invoke();
        OnResolved?.Invoke(this, CanResolveAsSuccess());
    }

    private IEnumerator WaitForAnimation(int stateHash)
    {
        animator.Play(stateHash, LayerIndex);

        // кадр на обновление стейт-машины после Play
        yield return null;

        // ждём входа в целевое состояние; если аниматор так и не переключился — сообщаем причину
        float elapsed = 0f;
        while (animator.IsInTransition(LayerIndex) ||
               animator.GetCurrentAnimatorStateInfo(LayerIndex).shortNameHash != stateHash)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= EnterStateTimeout)
            {
                Debug.LogError($"WaitCondition: аниматор не вошёл в состояние '{stateName}' за {EnterStateTimeout} сек " +
                               $"(проверь: объект активен, Animator включён, состояние на слое {LayerIndex}); StepName - {parentStep.name}");
                _animationFailed = true;
                yield break;
            }

            yield return null;
        }

        // ждём завершения: конец клипа, переход по Exit Time или смену состояния
        while (true)
        {
            var info = animator.GetCurrentAnimatorStateInfo(LayerIndex);

            if (info.shortNameHash != stateHash)
                break;

            if (info.normalizedTime >= 1f)
                break;

            if (animator.IsInTransition(LayerIndex))
                break;

            yield return null;
        }
    }

    public override void DisableCondition()
    {
        // защита от позднего завершения: SkipCondition/сброс шага останавливают ожидание
        _waitIsStopped = true;
        _waitRoutine = null;

        base.DisableCondition();
    }
}
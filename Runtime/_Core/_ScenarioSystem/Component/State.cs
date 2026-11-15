namespace _Core._ScenarioSystem.Component
{
    public enum State
    {
        //None - не инициализирован
        None,
        //Init - проинициализирован
        Init,
        //Success - шаг выполнен успешно
        Success,
        //Failure - шаг выполнен с ошибкой
        Failure,
        //Skipped - шаг пропущен
        Skipped,
    }
}

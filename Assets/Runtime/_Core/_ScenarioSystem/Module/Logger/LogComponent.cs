using System;
using _Core._ScenarioSystem.Component;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Logger
{
    [Serializable]
    public class LogComponent
    {
        [SerializeReference, ReadOnly] private int _guidId;
        //тип (группа/шаг/кондишн)
        [SerializeReference, ReadOnly] private ComponentType _type;
        //статус Инициализирован/Выполнен/Провален/Пропущен
        [SerializeReference, ReadOnly] private State _state;
        //статус тип ошибки TODO кривая реализация нужно через родителей детей делать чтобы было КРАСИВО
        [SerializeReference, ReadOnly] private EnumMistakeType _mistakeType;
        //Количество штрафных очков за ошибку
        [SerializeReference, ReadOnly] private float _penaltyPoints;
        //описание instruction text
        [SerializeReference, ReadOnly] private string _info;
        //описание ошибки
        [SerializeReference, ReadOnly] private string _mistake;
        //описание саксеса
        [SerializeReference, ReadOnly] private string _success;
        //время прилёта компонета
        private DateTime _date;
        //время для дебага в инспекторе
        [SerializeReference, ReadOnly] private string _time;

        private bool _excludeFromReport;

        // количество совпадений
        private int _count;

        // для отображения в инспекторе
        public string Label => $"{State} | {Info}";
        
        public int GuidId => _guidId;
        public ComponentType Type => _type;
        public State State => _state;
        public EnumMistakeType MistakeType => _mistakeType;
        public float PenaltyPoints => _penaltyPoints;
        public string Info => _info;
        public string Mistake => _mistake;
        public string Success => _success;
        public DateTime Date => _date;
        public int Count { get => _count; set => _count = value; }

        public bool ExcludeFromReport => _excludeFromReport;

        public LogComponent(int guidId, ComponentType type, State state, EnumMistakeType mistakeType,
            float penaltyPoints, string info, string mistake, string success,
            DateTime date, bool excludeFromReport)
        {
            _guidId = guidId;
            _type = type;
            _state = state;
            _mistakeType = mistakeType;
            _penaltyPoints = penaltyPoints;
            _info = info;
            _mistake = mistake;
            _success = success;
            _date = date;

            _count = 1;

            _time = _date.ToShortTimeString();
            _excludeFromReport = excludeFromReport;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Alex.Utils
{
	/// <summary>
	/// Класс-обёртка для преобразования значимого типа в ссылочный
	/// Имеет публичный Action<T> OnValueChanged который вызывается при изменении значения
	/// При помощи этой обёртки и интерфейса можно реализовать логику передачи значения из приватного поля класса
	/// в класс-обработчик. А так же подписаться на обновления приватного поля класса
	/// </summary>
	/// <typeparam name="T"></typeparam>

	public class StateWrapper<T>
	{
		public event Action<T> OnValueChanged;

		private T _value;
		public T Value
		{
			get => _value;
			set
			{
				_value = value;
				OnValueChanged?.Invoke(_value);
				Debug.Log("VALUE INCOME:" + _value);
			}
		}

		public StateWrapper(T value)
		{
			_value = value;
		}

		public void SetValue(T value)
		{
			Value = value; // это вызовет setter, который уже вызовет событие
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem.Utils
{
    /// <summary>
    /// 通用对象池实现
    /// 参考Unity的ObjectPool设计
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDestroy;
        private readonly int _maxSize;

        private int _countAll;

        /// <summary>
        /// 池中对象总数（已分配 + 可用）
        /// </summary>
        public int CountAll => _countAll;

        /// <summary>
        /// 池中可用对象数
        /// </summary>
        public int CountInactive => _stack.Count;

        /// <summary>
        /// 正在使用的对象数
        /// </summary>
        public int CountActive => _countAll - _stack.Count;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            _createFunc = createFunc ?? (() => new T());
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _maxSize = maxSize;

            _stack = new Stack<T>(defaultCapacity);
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            T element;

            if (_stack.Count == 0)
            {
                element = _createFunc();
                _countAll++;
            }
            else
            {
                element = _stack.Pop();
            }

            _actionOnGet?.Invoke(element);
            return element;
        }

        /// <summary>
        /// 获取对象（使用out参数）
        /// </summary>
        public PooledObject<T> Get(out T v)
        {
            v = Get();
            return new PooledObject<T>(v, this);
        }

        /// <summary>
        /// 释放对象回池中
        /// </summary>
        public void Release(T element)
        {
            if (element == null)
            {
                Debug.LogError("[ObjectPool] 尝试释放空对象");
                return;
            }

            _actionOnRelease?.Invoke(element);

            if (_stack.Count < _maxSize)
            {
                _stack.Push(element);
            }
            else
            {
                _actionOnDestroy?.Invoke(element);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            if (_actionOnDestroy != null)
            {
                foreach (var item in _stack)
                {
                    _actionOnDestroy(item);
                }
            }

            _stack.Clear();
            _countAll = 0;
        }
    }

    /// <summary>
    /// 池化对象包装器 - 使用using语句自动释放
    /// </summary>
    public readonly struct PooledObject<T> : IDisposable where T : class, new()
    {
        private readonly T _value;
        private readonly ObjectPool<T> _pool;

        public T Value => _value;

        public PooledObject(T value, ObjectPool<T> pool)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value)); // 防止传入 null 值
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));   // 防止传入 null 池
        }

        public void Dispose()
        {
            _pool?.Release(_value);
        }
    }
}

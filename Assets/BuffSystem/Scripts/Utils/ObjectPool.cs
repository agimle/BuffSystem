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
        private readonly Stack<T> stack;
        private readonly Func<T> createFunc;
        private readonly Action<T> actionOnGet;
        private readonly Action<T> actionOnRelease;
        private readonly Action<T> actionOnDestroy;
        private readonly int maxSize;

        private int countAll;

        /// <summary>
        /// 池中对象总数（已分配 + 可用）
        /// </summary>
        public int CountAll => countAll;

        /// <summary>
        /// 池中可用对象数
        /// </summary>
        public int CountInactive => stack.Count;

        /// <summary>
        /// 正在使用的对象数
        /// </summary>
        public int CountActive => countAll - stack.Count;

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
            this.createFunc = createFunc ?? (() => new T());
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
            this.maxSize = maxSize;

            stack = new Stack<T>(defaultCapacity);
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            T element;

            if (stack.Count == 0)
            {
                element = createFunc();
                countAll++;
            }
            else
            {
                element = stack.Pop();
            }

            actionOnGet?.Invoke(element);
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

            actionOnRelease?.Invoke(element);

            if (stack.Count < maxSize)
            {
                stack.Push(element);
            }
            else
            {
                actionOnDestroy?.Invoke(element);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            if (actionOnDestroy != null)
            {
                foreach (var item in stack)
                {
                    actionOnDestroy(item);
                }
            }

            stack.Clear();
            countAll = 0;
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

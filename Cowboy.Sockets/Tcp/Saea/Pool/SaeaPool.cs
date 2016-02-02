﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cowboy.Sockets
{
    public class SaeaPool : QueuedObjectPool<SaeaAwaitable>
    {
        private Func<SaeaAwaitable> _saeaCreator;
        private Action<SaeaAwaitable> _saeaCleaner;

        public SaeaPool(int batchCount, int maxFreeCount, Func<SaeaAwaitable> saeaCreator, Action<SaeaAwaitable> saeaCleaner)
        {
            if (batchCount <= 0)
                throw new ArgumentOutOfRangeException("batchCount");
            if (maxFreeCount <= 0)
                throw new ArgumentOutOfRangeException("maxFreeCount");
            if (saeaCreator == null)
                throw new ArgumentNullException("saeaCreator");

            _saeaCreator = saeaCreator;
            _saeaCleaner = saeaCleaner;

            if (batchCount > maxFreeCount)
            {
                batchCount = maxFreeCount;
            }

            Initialize(batchCount, maxFreeCount);
        }

        public override bool Return(SaeaAwaitable saea)
        {
            if (_saeaCleaner != null)
            {
                _saeaCleaner(saea);
            }

            if (!base.Return(saea))
            {
                CleanupItem(saea);
                return false;
            }

            return true;
        }

        protected override void CleanupItem(SaeaAwaitable item)
        {
            item.Dispose();
        }

        protected override SaeaAwaitable Create()
        {
            return _saeaCreator();
        }
    }
}

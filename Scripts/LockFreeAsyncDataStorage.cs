using System;
using Networking;
using UnityEngine;

namespace LockFreeAsyncDataStorage
{
    public interface IDataStoragePusher<TInnerData> where TInnerData : struct
    {
        bool IsFree();
        void PushData(TInnerData data);
    }

    public interface IDataStoragePopper<TInnerData> where TInnerData : struct
    {
        bool IsReady();
        TInnerData PopData();
    }

    public interface IDataStorage<TData> where TData : struct
    {
        IDataStoragePusher<TData> Pusher { get; }
        IDataStoragePopper<TData> Popper { get; }
    }


    public class ConstantCapacityDataStorage<TData> :
        IDataStorage<TData> where TData : struct
    {
        public class ConstantCapacityDataStoragePusher<TInnerData> :
            IDataStoragePusher<TInnerData> where TInnerData : struct
        {
            ConstantCapacityDataStorage<TInnerData> storage;

            internal ConstantCapacityDataStoragePusher(ConstantCapacityDataStorage<TInnerData> dataStorage)
            {
                storage = dataStorage;
            }

            public bool IsFree()
            {
                return storage.popIndex != (storage.pushIndex + 1) % storage.capacity;
            }

            public void PushData(TInnerData data)
            {
                if (!IsFree())
                {
                    throw new InvalidOperationException("DataStorage is fully occupied.");
                }
                Debug.Log($"Pushing {data}.");

                storage.Data[storage.pushIndex] = data;
                storage.pushIndex = (storage.pushIndex + 1) % storage.capacity;
            }
        }

        public class ConstantCapacityDataStoragePopper<TInnerData> :
            IDataStoragePopper<TInnerData> where TInnerData : struct
        {
            ConstantCapacityDataStorage<TInnerData> storage;

            internal ConstantCapacityDataStoragePopper(ConstantCapacityDataStorage<TInnerData> dataStorage)
            {
                storage = dataStorage;
            }

            public bool IsReady()
            {
                return storage.popIndex != storage.pushIndex;
            }
            public TInnerData PopData()
            {
                if (!IsReady())
                {
                    throw new InvalidOperationException("DataStorage is empty.");
                }
                Debug.Log($"Popping {storage.Data.ToString()}.");

                TInnerData returnData = storage.Data[storage.popIndex];
                storage.popIndex = (storage.popIndex + 1) % storage.capacity;
                return returnData;
            }
        }

        int capacity;
        int popIndex, pushIndex;
        bool wasPusherGenerated = false;
        bool wasPopperGenerated = false;

        TData[] Data;

        public ConstantCapacityDataStorage(int maximalCapacity)
        {
            if (maximalCapacity <= 0)
            {
                throw new ArgumentException("Storage capacity must be a natural number.");
            }
            capacity = maximalCapacity + 1;
            Data = new TData[capacity]; 
        }

        public int Capacity { get {  return capacity - 1; } }

        public IDataStoragePusher<TData> Pusher
        {
            get
            {
                if (wasPusherGenerated)
                {
                    throw new InvalidOperationException("DataStorage is thread safe only when used by one Pusher.");
                }
                wasPusherGenerated = true;
                return new ConstantCapacityDataStoragePusher<TData>(this);
            }
        }

        public IDataStoragePopper<TData> Popper
        {
            get
            {
                if (wasPopperGenerated)
                {
                    throw new InvalidOperationException("DataStorage is thread safe only when used by one Popper.");
                }
                wasPopperGenerated = true;
                return new ConstantCapacityDataStoragePopper<TData>(this);
            }
        }
    }


    public class RewritableDataStorage<TData> :
        IDataStorage<TData> where TData : struct
    {
        public class RewritableDataStoragePusher<TInnerData> :
            IDataStoragePusher<TInnerData> where TInnerData : struct
        {
            RewritableDataStorage<TInnerData> storage;

            internal RewritableDataStoragePusher(RewritableDataStorage<TInnerData> dataStorage)
            {
                storage = dataStorage;
            }

            public bool IsFree()
            {
                return !storage.isOccupied;
            }

            public void PushData(TInnerData data)
            {
                Debug.Log($"Pushing {data}.");

                storage.Data = data;
                storage.isOccupied = true;
            }
        }
        public class RewritableDataStoragePopper<TInnerData> :
            IDataStoragePopper<TInnerData> where TInnerData : struct
        {
            RewritableDataStorage<TInnerData> storage;

            internal RewritableDataStoragePopper(RewritableDataStorage<TInnerData> dataStorage)
            {
                storage = dataStorage;
            }

            public bool IsReady()
            {
                return storage.isOccupied;
            }
            public TInnerData PopData()
            {
                if (!IsReady())
                {
                    throw new InvalidOperationException("DataStorage is empty.");
                }
                Debug.Log($"Popping {storage.Data}.");

                TInnerData returnData = storage.Data;
                storage.isOccupied = false;
                return returnData;
            }
        }

        TData Data;
        bool isOccupied = false;
        bool wasPusherGenerated = false;
        bool wasPopperGenerated = false;

        public IDataStoragePusher<TData> Pusher
        {
            get
            {
                if (wasPusherGenerated)
                {
                    throw new InvalidOperationException("DataStorage is thread safe only when used by one Pusher.");
                }
                wasPusherGenerated = true;
                return new RewritableDataStoragePusher<TData>(this);
            }
        }

        public IDataStoragePopper<TData> Popper
        {
            get
            {
                if (wasPopperGenerated)
                {
                    throw new InvalidOperationException("DataStorage is thread safe only when used by one Popper.");
                }
                wasPopperGenerated = true;
                return new RewritableDataStoragePopper<TData>(this);
            }
        }
    }
}

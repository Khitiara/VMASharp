﻿using System.Diagnostics;
using Silk.NET.Vulkan;

namespace VMASharp;

internal class CurrentBudgetData
{
    public readonly InternalBudgetStruct[] BudgetData  = new InternalBudgetStruct[Vk.MaxMemoryHeaps];
    public readonly ReaderWriterLockSlim   BudgetMutex = new();
    public          int                    OperationsSinceBudgetFetch;

    public CurrentBudgetData() { }

    public void AddAllocation(int heapIndex, long allocationSize) {
        if ((uint)heapIndex >= Vk.MaxMemoryHeaps) {
            throw new ArgumentOutOfRangeException(nameof(heapIndex));
        }

        Interlocked.Add(ref BudgetData[heapIndex].AllocationBytes, allocationSize);
        Interlocked.Increment(ref OperationsSinceBudgetFetch);
    }

    public void RemoveAllocation(int heapIndex, long allocationSize) {
        ref InternalBudgetStruct heap = ref BudgetData[heapIndex];

        Debug.Assert(heap.AllocationBytes >= allocationSize);

        Interlocked.Add(ref heap.AllocationBytes, -allocationSize); //Subtraction

        Interlocked.Increment(ref OperationsSinceBudgetFetch);
    }

    internal struct InternalBudgetStruct
    {
        public long BlockBytes;
        public long AllocationBytes;
        public long VulkanUsage;
        public long VulkanBudget;
        public long BlockBytesAtBudgetFetch;
    }
}
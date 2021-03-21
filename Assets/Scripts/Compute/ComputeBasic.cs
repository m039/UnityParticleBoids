using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using m039.Common;
using Unity.Jobs;

namespace GP4
{

    public class ComputeBasic : MonoBehaviour
    {
        void OnEnable()
        {
            Invoke(nameof(TestCompute2), 0.5f);
        }

        struct TestCompute1Job : IJob
        {
            [ReadOnly]
            public NativeArray<long> numbers;

            public NativeArray<long> result;

            public void Execute()
            {
                long sum = 0;

                for (int i = 0; i < numbers.Length; i++)
                {
                    var number = numbers[i];
                    sum += number * number;
                }

                result[0] = sum;
            }
        }

        void TestCompute1()
        {
            DebugUtils.ClearLog();

            int arraySize = 1024 * 1024 * 128 ; // 1 + 2 + 3 + 4 + 5
            long result = (1L + arraySize) * arraySize / 2L;

            // Init array.

            var timer = Time.realtimeSinceStartup;

            NativeArray<long> numbers = new NativeArray<long>(arraySize, Allocator.TempJob);

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = (i + 1L);
            }

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("Inited array in {0} ms.", timer));

            // Do without jobs.

            timer = Time.realtimeSinceStartup;
            long sum = 0;

            for (int i = 0; i < numbers.Length; i++)
            {
                var number = numbers[i];
                sum += number * number;
            }

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("No job, result = {0}[{1}] in {2} ms.", sum, result, timer));

            // Do with job

            timer = Time.realtimeSinceStartup;
            NativeArray<long> jobResult = new NativeArray<long>(1, Allocator.TempJob);

            var job = new TestCompute1Job
            {
                numbers = numbers,
                result = jobResult
            };

            job.Schedule().Complete();

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("With job, result = {0}[{1}] in {2} ms.", job.result[0], result, timer));

            numbers.Dispose();
            jobResult.Dispose();
        }


        /// TestCompute2
        ///

        struct TestCompute2Job : IJob
        {
            public NativeArray<long> numbers;

            public void Execute()
            {
                for (int i = 0; i < numbers.Length; i++)
                {
                    numbers[i] = numbers[i] * 2;
                }
            }
        }

        struct TestCompute2JobParallel : IJobParallelFor
        {
            public NativeArray<long> numbers;

            public void Execute(int i)
            {
                numbers[i] = numbers[i] * 2;
            }
        }

        void TestCompute2()
        {
            DebugUtils.ClearLog();

            int arraySize = 1024 * 1024 * 128; // 1 + 2 + 3 + 4 + 5
            long result = (1L + arraySize) * arraySize / 2L;

            var timer = Time.realtimeSinceStartup;

            NativeArray<long> numbers = new NativeArray<long>(arraySize, Allocator.TempJob);

            // Init array.

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = (i + 1L);
            }

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("[1] Inited array in {0} ms.", timer));

            // Do without jobs.

            timer = Time.realtimeSinceStartup;
 
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] *= 2;
            }

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("No job, result = {0} in {1} ms.", numbers[1024], timer));

            // Init array.

            timer = Time.realtimeSinceStartup;

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = (i + 1L);
            }

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("[2] Inited array in {0} ms.", timer));

            // Do with job

            timer = Time.realtimeSinceStartup;

            var job = new TestCompute2JobParallel
            {
                numbers = numbers,
            };

            job.Schedule(numbers.Length, 1024).Complete();

            timer = Time.realtimeSinceStartup - timer;

            Debug.Log(string.Format("With job, result = {0} in {1} ms.", numbers[1024], timer));

            numbers.Dispose();
        }
    }

}

using UnityEngine;

namespace Replica.Structures {
    public struct StdDev {
        float mean;
        float varianceSum;

        int index;
        float[] samples;

        int maxWindowSize;
        public int Count => samples.Length;
        public float Mean => mean;
        public float Variance => varianceSum / (maxWindowSize - 1);
        public float Value => Mathf.Sqrt(Variance);

        public void Initialize(int windowSize) {
            maxWindowSize = windowSize;
            samples = new float[maxWindowSize];
        }

        public void Integrate(float sample) {
            index = (index + 1) % maxWindowSize;
            float samplePrev = samples[index];
            float meanPrev = mean;

            mean += (sample - samplePrev) / maxWindowSize;
            varianceSum += (sample + samplePrev - mean - meanPrev) * (sample - samplePrev);

            samples[index] = sample;
        }
    }
}

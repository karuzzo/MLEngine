using Microsoft.ML.Data;

namespace MLEngine
{
    public class BarLoadMultiPrediction
    {
        [ColumnName("PredictedLabel")]
        public float Prediction { get; set; }

        public float[] Score { get; set; }
    }
}
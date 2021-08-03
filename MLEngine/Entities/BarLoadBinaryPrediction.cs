using Microsoft.ML.Data;

namespace MLEngine
{
    public class BarLoadBinaryPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }
}
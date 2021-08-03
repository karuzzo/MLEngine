using Microsoft.ML.Data;

namespace MLEngine
{
    public class BarLoadRegressionPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }

        [ColumnName("Prediction")]
        public float Prediction { get; set; }
    }
}
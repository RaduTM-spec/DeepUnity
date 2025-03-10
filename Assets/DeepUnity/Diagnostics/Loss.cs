using System;
using System.Linq;
using Unity.VisualScripting;

namespace DeepUnity
{
    /// <summary>
    /// A tool for computing a loss function for predictions and targets. There are 3 properties: <br></br> <br></br>
    /// <b>Item</b>: Returns the mean of all loss values in the tensor. (scalar) <br></br>
    /// <b>Value</b>: Returns the loss <see cref="Tensor"/> applied element-wisely over predictions and targets.<br></br>
    /// <b>Gradient</b>: Returns the derivative of the loss function <see cref="Tensor"/> applied element-wisely over predictions and targets. 
    /// <b>Used for backpropagation.</b>
    /// 
    /// <br></br><br></br>
    /// <i>Note: The Loss reduction is always <b>mean</b> (layers compute the mean gradients)</i>
    /// </summary>
    public class Loss
    {
        private LossType lossType;
        private Tensor predicts;
        private Tensor targets;

        private Loss(LossType type, Tensor predicts, Tensor targets)
        {
            if(!predicts.Shape.SequenceEqual(targets.Shape))
                throw new ArgumentException($"Predicts shape ({predicts.Shape.ToCommaSeparatedString()}) must be the same with Targets shape ({targets.Shape.ToCommaSeparatedString()})");
            
            lossType = type;
            this.predicts = predicts;
            this.targets = targets;
        }
        /// <summary>
        /// Mean Squared Error loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss MSE(Tensor predicts, Tensor targets) => new Loss(LossType.MSE, predicts, targets);
        /// <summary>
        /// Mean Absolute Error loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss MAE(Tensor predicts, Tensor targets) => new Loss(LossType.MAE, predicts, targets);
        /// <summary>
        /// Root Mean Squared Error loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// </summary>
        public static Loss RMSE(Tensor predicts, Tensor targets) => new Loss(LossType.RMSE, predicts, targets);
        /// <summary>
        /// Cross Entropy loss. (note: the predicts must be probabilities) <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss CE(Tensor predicts, Tensor targets) => new Loss(LossType.CE, predicts, targets);
        /// <summary>
        /// Hinge Hmbedded loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss HE(Tensor predicts, Tensor targets) => new Loss(LossType.HE, predicts, targets);
        /// <summary>
        /// Binary Cross Entropy loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss BCE(Tensor predicts, Tensor targets) => new Loss(LossType.BCE, predicts, targets);
        /// <summary>
        /// Kullback-Liebler Divergence loss. <br></br>
        /// Predicts: (B, *) or (*) for unbatched input <br></br>
        /// Targets: (B, *) or (*) for unbatched input <br></br>
        /// where * = input Shape
        /// </summary>
        public static Loss KLD(Tensor predicts, Tensor targets) => new Loss(LossType.KLD, predicts, targets);

        /// <summary>
        /// Returns the mean loss magnitude value.
        /// </summary>
        public float Item { get
            {
                Tensor lossItem = Value;
                return lossItem.Average();

            }
        }
        /// <summary>
        /// Returns the computed loss function.
        /// </summary>
        public Tensor Value { get
            {
                switch (lossType)
                {
                    case LossType.MSE:
                        return Tensor.Pow(predicts - targets, 2);
                    case LossType.MAE:
                        return Tensor.Abs(predicts - targets);
                    case LossType.RMSE:
                        return Tensor.Sqrt(Tensor.Pow(predicts - targets, 2));

                    case LossType.CE:
                        return -targets * Tensor.Log(predicts + Utils.EPSILON);
                    case LossType.BCE:
                        return - (targets * Tensor.Log(predicts + Utils.EPSILON) + (1f - targets) * Tensor.Log(1f - predicts + Utils.EPSILON));

                    case LossType.HE:
                        return predicts.Zip(targets, (p, t) => MathF.Max(0f, 1f - p * t));
                    case LossType.KLD:
                        return targets * Tensor.Log(targets / (predicts + Utils.EPSILON));
                    default:
                        throw new NotImplementedException("Unhandled loss type.");
                }
            }
        }
        /// <summary>
        /// Returns the partial derivative of the <see cref="Loss"/>(pred, targ) w.r.t. the prediction `y`.
        /// </summary>
        public Tensor Grad { get
            {
                switch (lossType)
                {
                    case LossType.MSE:
                        return 2f * (predicts - targets);
                    case LossType.MAE:
                        return predicts.Zip(targets, (p, t) => p - t > 0 ? 1f : -1f);
                    case LossType.RMSE:
                        Tensor diff = predicts - targets; return diff / diff.Pow(2f).Sqrt();
                    case LossType.CE:
                        return -targets / (predicts + Utils.EPSILON);
                    case LossType.BCE:
                        return (predicts - targets) / (predicts * (1f - predicts) + Utils.EPSILON);

                    case LossType.HE:
                        return predicts.Zip(targets, (p, t) => 1f - p * t > 0f ? -t : 0f);
                    case LossType.KLD:
                        return -targets / (predicts + Utils.EPSILON);
                    default:
                        throw new NotImplementedException("Unhandled loss type.");
                }
            }
        }
        private enum LossType
        {
            MSE,
            MAE,
            RMSE,
            CE,
            BCE,
            HE,            
            KLD
        }
    }
}
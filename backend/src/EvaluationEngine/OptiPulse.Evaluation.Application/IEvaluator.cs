using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Evaluation.Application;

public interface IEvaluator
{
    EvaluationResult Evaluate(EvaluationContext context);
}
